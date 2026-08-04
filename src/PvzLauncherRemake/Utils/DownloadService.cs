using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace PvzLauncherRemake.Utils
{
    public class DownloadService : IDisposable
    {
        public string Url { get; init; } = "";
        public string SavePath { get; init; } = "";
        public Action<double, double>? Progress { get; init; } // (progress%, speed KB/s)
        public Action<bool, string?>? Completed { get; init; } // (success, error)
        public int ReportIntervalMs { get; init; } = 200;     // 进度回调频率（毫秒）
        public bool IgnoreSslErrors { get; init; } = false;    // 是否忽略 SSL 证书验证错误（仅用于开发/测试）

        private readonly HttpClient _httpClient;
        private CancellationTokenSource? _cts;
        private Task? _task;

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
            try
            {
                using var response = await _httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                var total = response.Content.Headers.ContentLength ?? -1;
                var canReport = total > 0 && Progress != null;

                Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
                if (File.Exists(SavePath)) File.Delete(SavePath);

                await using var file = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None, 32768, true);
                using var stream = await response.Content.ReadAsStreamAsync(ct);

                var buffer = new byte[32768];
                long readTotal = 0;
                var sw = Stopwatch.StartNew();
                long lastBytes = 0;
                double lastTime = 0;

                int bytes;
                while ((bytes = await stream.ReadAsync(buffer, ct)) > 0)
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
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _httpClient.Dispose();
        }
    }
}
