using System.Numerics;
using WinHue.Framework;

namespace WinHue.LED
{
    internal sealed partial class DomeOutput
    {
        public readonly struct Pixel
        {
            public Pixel(DomeOutput dome, int strut, int led)
            {
                mParent = dome;
                Strut = strut;
                LED = led;
                mOpcIndex = GetOpcPixelIndex(Strut, LED);
            }

            public readonly int Strut { get; }
            public readonly int LED { get; }
            public readonly Vector3 Position { get => GetLedPosition(Strut, LED); }

            public readonly Color Color
            {
                get => mParent.mBatcher.GetPixel(mOpcIndex);
                set => mParent.mBatcher.SetPixel(mOpcIndex, value);
            }

            private readonly DomeOutput mParent;
            private readonly int mOpcIndex;
        }
    }
}
