using System.Numerics;
using WinHue.Framework;

namespace WinHue.LED
{
    public sealed partial class DomeOutput
    {
        public readonly struct Pixel
        {
            public Pixel(DomeOutput dome, int strut, int led)
            {
                mDome = dome;
                Strut = strut;
                LED = led;
                mOpcIndex = GetOpcPixelIndex(Strut, LED);
            }

            public readonly int Strut { get; }
            public readonly int LED { get; }
            public readonly Vector3 Position { get => GetLedPosition(Strut, LED); }

            public readonly Color Color
            {
                get => mDome.mBatcher.GetPixel(mOpcIndex);
                set => mDome.mBatcher.SetPixel(mOpcIndex, value);
            }

            private readonly DomeOutput mDome;
            private readonly int mOpcIndex;
        }
    }
}
