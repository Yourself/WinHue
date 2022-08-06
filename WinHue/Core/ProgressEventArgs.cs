using System;

namespace WinHue.Core
{
    internal sealed class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(string message, double progress = double.NaN)
        {
            Message = message;
            Progress = progress;
        }

        public string Message { get; }

        public double Progress { get; }
    }
}
