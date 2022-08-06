using System;
using System.Collections.Generic;
using System.Linq;
using WinHue.Framework;

namespace WinHue.LED
{
    public sealed partial class DomeOutput : IOutput, IDisposable
    {
        public DomeOutput()
        {
            mBatcher = mDriver.GetPixelBatcher(0);
        }

        [Configurable]
        public string? RemoteAddress { get; set; }

        [Configurable]
        public ushort? DebugPort { get; set; }

        public IEnumerable<Pixel> Pixels => Struts.SelectMany(strut => strut.Pixels);

        public IEnumerable<Strut> Struts => Enumerable.Range(0, StrutVertices.Length).Select(idx => new Strut(this, idx));

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

        private bool mDisposed;
    }
}
