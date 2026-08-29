using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PvzLauncherRemake.Utils.Network
{
    public class DownloadService : IDisposable
    {
        public string Url { get; init; } = "";
        public string SavePath { get; init; } = "";
        public Action<double, double>? Progress { get; init; } // (progress%, speed KB/s)
        public Action<bool, string?>? Completed { get; init; } // (success, error)
        public int ReportIntervalMs { get; init; } = 200;     // 进度回调频率（毫秒）
        public bool IgnoreSslErrors { get; init; } = false;    // 是否忽略 SSL 证书验证错误（仅用于开发/测试）
        public int ThreadCount { get; init; } = 8;   // 线程数1~16

        private readonly HttpClient _httpClient;
        private CancellationTokenSource? _cts;
        private Task? _task;

        // 内部常量
        private const int MinSizeForMulti = 2 * 1024 * 1024;   // 小于 2MB 不启用多线程
        private const int MaxAllowedThreads = 16;

        // 多线程进度累加
        private long _downloadedBytes;

        public DownloadService()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression =
                    System.Net.DecompressionMethods.GZip |
                    System.Net.DecompressionMethods.Deflate
            };

            if (IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            _httpClient = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public void StartDownload()
        {
            if (string.IsNullOrWhiteSpace(Url))
                throw new InvalidOperationException("Url 未设置");
            if (string.IsNullOrWhiteSpace(SavePath))
                throw new InvalidOperationException("SavePath 未设置");
            if (_task?.IsCompleted == false)
                throw new InvalidOperationException("已在下载");

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _task = DownloadAsync(_cts.Token);
        }

        public void StopDownload() => _cts?.Cancel();

        private async Task DownloadAsync(CancellationToken ct)
        {
            string[]? tempFiles = null;
            try
            {
                using var probe = await _httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, ct);
                probe.EnsureSuccessStatusCode();

                var total = probe.Content.Headers.ContentLength ?? -1;
                bool acceptRanges = probe.Headers.AcceptRanges.Contains("bytes");

                Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
                if (File.Exists(SavePath)) File.Delete(SavePath);

                // 计算实际使用的线程数
                int segmentCount = Math.Clamp(ThreadCount, 1, MaxAllowedThreads);

                // 不满足多线程条件则回退单线程
                if (total < MinSizeForMulti || !acceptRanges || segmentCount <= 1)
                {
                    await DownloadSingleAsync(probe, total, ct);
                    Progress?.Invoke(100.0, 0.0);
                    Completed?.Invoke(true, null);
                    return;
                }

                // 释放探测响应，开始多线程分段
                probe.Dispose();

                tempFiles = new string[segmentCount];
                _downloadedBytes = 0;

                var tasks = new Task[segmentCount];
                long segmentSize = total / segmentCount;

                for (int i = 0; i < segmentCount; i++)
                {
                    long start = i * segmentSize;
                    long end = (i == segmentCount - 1) ? total - 1 : (start + segmentSize - 1);

                    string tempPath = Path.Combine(Path.GetTempPath(),
                        $"pvz_dl_{Guid.NewGuid():N}.part{i}");
                    tempFiles[i] = tempPath;

                    tasks[i] = DownloadSegmentAsync(start, end, tempPath, ct);
                }

                var downloadTask = Task.WhenAll(tasks);
                var monitorTask = MonitorProgressAsync(downloadTask, total, ct);

                await downloadTask;
                await monitorTask;

                await MergeSegmentsAsync(tempFiles, SavePath, ct);

                Progress?.Invoke(100.0, 0.0);
                Completed?.Invoke(true, null);
            }
            catch (OperationCanceledException)
            {
                Completed?.Invoke(false, "已取消");
            }
            catch (Exception ex)
            {
                Completed?.Invoke(false, ex.Message);
            }
            finally
            {
                if (tempFiles != null)
                {
                    foreach (var f in tempFiles)
                    {
                        try { if (File.Exists(f)) File.Delete(f); } catch { /* ignore */ }
                    }
                }
            }
        }

        private async Task DownloadSegmentAsync(long start, long end, string tempPath, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Url);
            request.Headers.Range = new RangeHeaderValue(start, end);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode != HttpStatusCode.PartialContent &&
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"分段下载失败，状态码: {(int)response.StatusCode}");
            }

            await using var file = new FileStream(tempPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 65536, true);
            using var stream = await response.Content.ReadAsStreamAsync(ct);

            var buffer = new byte[65536];
            int bytes;
            while ((bytes = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                await file.WriteAsync(buffer.AsMemory(0, bytes), ct);
                Interlocked.Add(ref _downloadedBytes, bytes);
            }
        }

        private async Task DownloadSingleAsync(HttpResponseMessage response, long total, CancellationToken ct)
        {
            var canReport = total > 0 && Progress != null;

            await using var file = new FileStream(SavePath, FileMode.Create, FileAccess.Write,
                FileShare.None, 65536, true);
            using var stream = await response.Content.ReadAsStreamAsync(ct);

            var buffer = new byte[65536];
            long readTotal = 0;
            var sw = Stopwatch.StartNew();
            long lastBytes = 0;
            double lastTime = 0;

            int bytes;
            while ((bytes = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                await file.WriteAsync(buffer.AsMemory(0, bytes), ct);
                readTotal += bytes;

                if (canReport && sw.Elapsed.TotalMilliseconds - lastTime >= ReportIntervalMs)
                {
                    var percent = readTotal * 100.0 / total;
                    var speed = (readTotal - lastBytes) / ((sw.Elapsed.TotalMilliseconds - lastTime) / 1000.0) / 1024.0;
                    Progress?.Invoke(percent, speed);

                    lastBytes = readTotal;
                    lastTime = sw.Elapsed.TotalMilliseconds;
                }
            }
        }

        private async Task MonitorProgressAsync(Task downloadTask, long total, CancellationToken ct)
        {
            if (Progress == null)
            {
                await downloadTask;
                return;
            }

            var sw = Stopwatch.StartNew();
            long lastBytes = 0;
            double lastTime = 0;

            while (!downloadTask.IsCompleted)
            {
                try
                {
                    await Task.Delay(ReportIntervalMs, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                long current = Interlocked.Read(ref _downloadedBytes);
                double elapsedSec = (sw.Elapsed.TotalMilliseconds - lastTime) / 1000.0;
                if (elapsedSec <= 0) continue;

                double percent = total > 0 ? Math.Min(99.9, current * 100.0 / total) : 0;
                double speed = (current - lastBytes) / elapsedSec / 1024.0;

                Progress.Invoke(percent, speed);

                lastBytes = current;
                lastTime = sw.Elapsed.TotalMilliseconds;
            }
        }

        private static async Task MergeSegmentsAsync(string[] tempFiles, string savePath, CancellationToken ct)
        {
            await using var output = new FileStream(savePath, FileMode.Create, FileAccess.Write,
                FileShare.None, 131072, true);

            foreach (var temp in tempFiles)
            {
                await using var input = new FileStream(temp, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 131072, true);
                await input.CopyToAsync(output, 131072, ct);
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _httpClient.Dispose();
        }
    }
}