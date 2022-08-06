using System.Collections.Generic;

namespace WinHue.LED
{
    public sealed partial class DomeOutput
    {
        public readonly struct Strut
        {
            public Strut(DomeOutput dome, int index)
            {
                mDome = dome;
                mStrutIdx = index;
            }

            public int LEDCount => GetStrutLEDCount(mStrutIdx);

            public (Vertex, Vertex) Ends
            {
                get
                {
                    var (v1, v2) = StrutVertices[mStrutIdx];
                    return (new Vertex(v1), new Vertex(v2));
                }
            }

            public IEnumerable<Pixel> Pixels
            {
                get
                {
                    for (int i = 0; i < LEDCount; ++i)
                    {
                        yield return new(mDome, mStrutIdx, i);
                    }
                }
            }

            private readonly DomeOutput mDome;
            private readonly int mStrutIdx;
        }
    }
}
