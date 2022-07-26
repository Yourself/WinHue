using System;
using System.Diagnostics;
using WinHue.Framework;

namespace WinHue.Input
{
    internal class ClockInput : IInput
    {
        [Configurable]
        public double FrequencyHz { get; set; } = 1;

        public double Time { get; private set; }

        public double Delta { get; private set; }

        bool IInput.AlwaysActive => false;

        void IInput.Activate()
        {
            mStopwatch.Restart();
            mLastTime = 0;
        }

        void IInput.Deactivate()
        {
            mStopwatch.Stop();
        }

        void IInput.Update()
        {
            Time = mStopwatch.Elapsed.TotalSeconds * FrequencyHz;
            Delta = Time - mLastTime;
            mLastTime = Time;
        }

        private readonly Stopwatch mStopwatch = new();
        private double mLastTime;
    }
}
