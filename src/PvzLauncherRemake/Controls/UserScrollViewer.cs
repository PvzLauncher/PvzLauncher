using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

//已修改: 支持按住Shift水平滚动
// https://github.com/TwilightLemon/FluentScrollViewer
//原始许可证文本，来自储存库 ↑ :
/*
 MIT License

Copyright (c) [year] [fullname]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

namespace PvzLauncherRemake.Controls
{

    public class UserScrollViewer : ScrollViewer
    {
        #region 模型参数
        /// <summary>
        /// 缓动模型的叠加速度力度，数值越大，滚动起始速率越快，滚得越远
        /// </summary>
        private const double VelocityFactor = 2.0;
        /// <summary>
        /// 缓动模型的速度衰减系数，数值越小，越快停下来
        /// </summary>
        private const double Friction = 0.92;

        /// <summary>
        /// 精确模型的插值系数，数值越大，滚动越快接近目标
        /// </summary>
        private const double LerpFactor = 0.5;

        /// <summary>
        /// 目标帧时间
        /// </summary>
        private const double TargetFrameTime = 1.0d / 144;
        #endregion

        public UserScrollViewer()
        {
            this.IsManipulationEnabled = true;
            this.PanningMode = PanningMode.VerticalOnly;
            this.IsManipulationEnabled = true;
            //使用此触屏滚动会导致闪屏，先不用了..
            // this.PanningDeceleration = 0; // 禁用默认惯性
            //StylusTouchDevice.SetSimulate(this, true);
            Unloaded += ScrollViewer_Unloaded;
        }
        //记录参数
        private int _lastScrollingTick = 0, _lastScrollDelta = 0;
        //private double _lastTouchVelocity = 0;
        private double _targetOffset = 0;
        private double _targetVelocity = 0;
        private long _lastTimestamp = 0;

        private double _targetHorizontalVelocity = 0;

        //标志位
        private bool _isRenderingHooked = false;
        private bool _isAccuracyControl = false;

        private void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_isRenderingHooked)
            {
                CompositionTarget.Rendering -= OnRendering;
                _isRenderingHooked = false;
            }
        }

        /* protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
         {
             base.OnManipulationDelta(e);    //如果没有这一行则不会触发ManipulationCompleted事件??
             e.Handled = true;
             //手还在屏幕上，使用精确滚动
             _isAccuracyControl = true;
             double deltaY = -e.DeltaManipulation.Translation.Y;
             _targetOffset = Math.Clamp(_currentOffset + deltaY, 0, ScrollableHeight);
             // 记录最后一次速度
             _lastTouchVelocity = -e.Velocities.LinearVelocity.Y;

             if (!_isRenderingHooked)
             {
                 _lastTimestamp = Stopwatch.GetTimestamp();
                 CompositionTarget.Rendering += OnRendering;
                 _isRenderingHooked = true;
             }
         }

         protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
         {
             base.OnManipulationCompleted(e);
             e.Handled = true;
             Debug.WriteLine("vel: " + _lastTouchVelocity);
             _targetVelocity = _lastTouchVelocity; // 用系统识别的速度继续滚动
             _isAccuracyControl = false;

             if (!_isRenderingHooked)
             {
                 _lastTimestamp = Stopwatch.GetTimestamp();
                 CompositionTarget.Rendering += OnRendering;
                 _isRenderingHooked = true;
             }
         }*/

        /// <summary>
        /// 判断MouseWheel事件由鼠标触发还是由触控板触发
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsTouchpadScroll(MouseWheelEventArgs e)
        {
            var tickCount = Environment.TickCount;
            var isTouchpadScrolling =
                    e.Delta % Mouse.MouseWheelDeltaForOneLine != 0 ||
                    (tickCount - _lastScrollingTick < 100 && _lastScrollDelta % Mouse.MouseWheelDeltaForOneLine != 0);
            //Debug.WriteLine(e.Delta + "  " + e.Timestamp + "  ==>" + isTouchpadScrolling);
            _lastScrollDelta = e.Delta;
            _lastScrollingTick = e.Timestamp;
            return isTouchpadScrolling;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            bool horizontal = Keyboard.IsKeyDown(Key.LeftShift) ||
                      Keyboard.IsKeyDown(Key.RightShift);


            if (horizontal)
            {
                _targetHorizontalVelocity += -e.Delta * VelocityFactor;

                if (!_isRenderingHooked)
                {
                    _lastTimestamp = Stopwatch.GetTimestamp();
                    CompositionTarget.Rendering += OnRendering;
                    _isRenderingHooked = true;
                }

                return;
            }

            _isAccuracyControl = IsTouchpadScroll(e);

            if (_isAccuracyControl)
            {
                _targetVelocity = 0;
                _targetOffset = Math.Clamp(
                    VerticalOffset - e.Delta,
                    0,
                    ScrollableHeight);
            }
            else
            {
                _targetVelocity += -e.Delta * VelocityFactor;
            }


            if (!_isRenderingHooked)
            {
                _lastTimestamp = Stopwatch.GetTimestamp();
                CompositionTarget.Rendering += OnRendering;
                _isRenderingHooked = true;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            // 计算时间增量
            long currentTimestamp = Stopwatch.GetTimestamp();
            double deltaTime = (double)(currentTimestamp - _lastTimestamp) / Stopwatch.Frequency;
            _lastTimestamp = currentTimestamp;

            double timeFactor = deltaTime / TargetFrameTime;
            double _currentOffset = VerticalOffset;
            if (_isAccuracyControl)
            {
                // 精确滚动：Lerp 逼近目标（使用时间因子调整）
                double lerpAmount = 1.0 - Math.Pow(1.0 - LerpFactor, timeFactor);
                _currentOffset += (_targetOffset - _currentOffset) * lerpAmount;

                // 如果已经接近目标，就停止
                if (Math.Abs(_targetOffset - _currentOffset) < 0.5)
                {
                    _currentOffset = _targetOffset;
                    StopRendering();
                }
            }
            else
            {
                // 缓动滚动：速度衰减模拟（使用时间因子调整）
                if (Math.Abs(_targetVelocity) < 0.1 &&
                    Math.Abs(_targetHorizontalVelocity) < 0.1)
                {
                    _targetVelocity = 0;
                    _targetHorizontalVelocity = 0;

                    StopRendering();
                    return;
                }

                // 使用时间因子调整摩擦力衰减
                _targetVelocity *= Math.Pow(Friction, timeFactor);

                // 根据实际时间计算偏移量
                _currentOffset = Math.Clamp(_currentOffset + _targetVelocity * (timeFactor / 24), 0, ScrollableHeight);
            }

            ScrollToVerticalOffset(_currentOffset);


            //横向惯性滚动
            if (Math.Abs(_targetHorizontalVelocity) > 0.1)
            {
                _targetHorizontalVelocity *= Math.Pow(Friction, timeFactor);

                double horizontal =
                    HorizontalOffset +
                    _targetHorizontalVelocity * (timeFactor / 24);

                ScrollToHorizontalOffset(
                    Math.Clamp(horizontal, 0, ScrollableWidth));
            }
            else
            {
                _targetHorizontalVelocity = 0;
            }
        }

        private void StopRendering()
        {
            CompositionTarget.Rendering -= OnRendering;
            _isRenderingHooked = false;
        }
    }
}