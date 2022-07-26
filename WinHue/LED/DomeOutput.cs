using System;
using System.Collections.Generic;
using WinHue.Framework;

namespace WinHue.LED
{
    internal sealed partial class DomeOutput : IOutput, IDisposable
    {
        public DomeOutput()
        {
            mBatcher = mDriver.GetPixelBatcher(0);
        }

        [Configurable]
        public string? RemoteAddress { get; set; }

        [Configurable]
        public ushort? DebugPort { get; set; }

        public IEnumerable<Pixel> Pixels
        {
            get
            {
                for (int strut = 0; strut < StrutVertices.Length; ++strut)
                {
                    int n = GetStrutLEDCount(strut);
                    for (int i = 0; i < n; ++i)
                    {
                        yield return new Pixel(this, strut, i);
                    }
                }
            }
        }

        public void SetPixel(int strutIndex, int ledIndex, Color color)
        {
            mBatcher.SetPixel(GetOpcPixelIndex(strutIndex, ledIndex), color);
        }

        public void Update()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(DomeOutput));

            mBatcher.Send();
        }

        public void Dispose()
        {
            if (mDisposed) return;
            ((IOutput)this).Deactivate();
            mDisposed = true;
        }

        static DomeOutput()
        {
            InitializeIndexing(out ControlBoxStrutOffset);
            InitializeGeometry(out StrutVertices, out VertexPositions);
        }

        void IOutput.Activate()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(DomeOutput));

            mDriver.Start();
        }

        void IOutput.Deactivate()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(DomeOutput));

            mDriver.Stop();
        }

        private readonly OpcDriver mDriver = new();
        private readonly IPixelBatcher mBatcher;

        private bool mIsActive;
        private bool mDisposed;
    }
}
