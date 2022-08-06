using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace WinHue.Core
{
    internal class RuntimeExecutor
    {
        public event EventHandler<ProgressEventArgs>? Progress;
        public event EventHandler<Exception> ExceptionRaised;

        public RuntimeExecutor()
        {
            mThread = new Thread(Run) { IsBackground = true };
            mCancellationSource = new();
            ExceptionRaised += OnException;
            mDispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Start()
        {
            mThread.Start(new ThreadData(this));
        }

        public void Stop()
        {
            mCancellationSource.Cancel();
            mThread.Join();
        }

        public double Framerate => mFramerate;

        private class ThreadData
        {
            public ThreadData(RuntimeExecutor parent)
            {
                CancellationToken = parent.mCancellationSource.Token;
            }

            public CancellationToken CancellationToken { get; }
        }

        private void Run(object? arg)
        {
            try
            {
                if (arg == null)
                {
                    return;
                }
                var data = (ThreadData)arg;
                var token = data.CancellationToken;
                RaiseProgress("Initialized...", 100);
                var sw = Stopwatch.StartNew();
                var frameQueue = new Queue<TimeSpan>(1000);
                frameQueue.Enqueue(sw.Elapsed);
                while (!token.IsCancellationRequested)
                {
                    var t = sw.Elapsed;
                    frameQueue.Enqueue(t);
                    var elapsed = t - frameQueue.Peek();
                    Interlocked.Exchange(ref mFramerate, frameQueue.Count / elapsed.TotalSeconds);
                    while (frameQueue.Count > 1 && frameQueue.Peek() < t - TimeSpan.FromSeconds(1)) frameQueue.Dequeue();
                    Thread.Yield();
                }
            }
            catch (Exception e)
            {
                RaiseException(e);
            }
        }

        private void OnException(object? sender, Exception e)
        {
            mThread.Join();
        }

        private void RaiseException(Exception e)
        {
            mDispatcher.Invoke(() => ExceptionRaised(this, e));
        }

        private void RaiseProgress(string message, double progress)
        {
            mDispatcher.Invoke(() => Progress?.Invoke(this, new ProgressEventArgs(message, progress)));
        }

        private readonly Thread mThread;
        private readonly CancellationTokenSource mCancellationSource;
        private readonly Dispatcher mDispatcher;

        private double mFramerate;
    }
}
