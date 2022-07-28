using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace WinHue.Core
{
    internal class Runtime
    {
        public event EventHandler<Exception> ExceptionRaised;

        public Runtime()
        {
            mThread = new Thread(Run) { IsBackground = true };
            mCancellationSource = new();
            ExceptionRaised += OnException;
            mDispatcher = Dispatcher.CurrentDispatcher;
        }

        public double Framerate => mFramerate;

        private class ThreadData
        {
            public ThreadData(Runtime parent)
            {
                CancellationToken = parent.mCancellationSource.Token;
            }

            public CancellationToken CancellationToken { get; }
        }

        private void Start()
        {
            mThread.Start(new ThreadData(this));
        }

        private void Stop()
        {
            mCancellationSource.Cancel();
            mThread.Join();
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
                mDispatcher.Invoke(() => ExceptionRaised(this, e));
            }
        }

        private void OnException(object? sender, Exception e)
        {
            mThread.Join();
        }

        private readonly Thread mThread;
        private readonly CancellationTokenSource mCancellationSource;
        private readonly Dispatcher mDispatcher;

        private double mFramerate;
    }
}
