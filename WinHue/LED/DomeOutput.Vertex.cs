using System.Numerics;

namespace WinHue.LED
{
    public sealed partial class DomeOutput
    {
        public readonly struct Vertex
        {
            public Vertex(int idx)
            {
                Index = idx;
            }

            public int Index { get; }

            public Vector3 Position => VertexPositions[Index];
        }
    }
}
