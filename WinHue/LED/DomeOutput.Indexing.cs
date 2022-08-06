using System.Linq;

namespace WinHue.LED
{
    public sealed partial class DomeOutput
    {
        private static class LEDCount
        {
            public const int Yellow = 34;
            public const int Red = 40;
            public const int Blue = 40;
            public const int Orange = 40;
            public const int Green = 42;
            public const int Purple = 44;

            public static readonly int[][] ByControlBoxAndStrut = new[]
            {
                new[]{ Green, Blue, Orange, Orange, Yellow },
                new[]{ Orange, Blue, Purple, Blue, Red },
                new[]{ Red, Blue, Green, Green, Blue },
                new[]{ Green, Blue, Red, Yellow, Yellow },
                new[]{ Green, Purple, Blue, Red },
                new[]{ Green, Purple, Purple, Green, Green },
                new[]{ Orange, Yellow, Yellow, Red, Red },
                new[]{ Blue, Blue, Blue, Yellow }
            };

            public static readonly int[] ByStrut = ByControlBoxAndStrut.SelectMany(_ => _).ToArray();

            public static readonly int StrandStride = ByControlBoxAndStrut.Select(counts => counts.Sum()).Max();
            public static readonly int ControlBoxStride = ByControlBoxAndStrut.Length * StrandStride;
        }

        private static readonly int StrutsPerControlBox = LEDCount.ByStrut.Length;
        private static readonly int[] ControlBoxStrutOffset;

        private static int GetOpcPixelIndex(int strut, int led)
        {
            int index = LEDCount.ControlBoxStride * (strut / StrutsPerControlBox)
                + ControlBoxStrutOffset[strut % StrutsPerControlBox]
                + led;
            return index;
        }

        private static int GetStrutLEDCount(int strut)
        {
            return LEDCount.ByStrut[strut % StrutsPerControlBox];
        }

        private static void InitializeIndexing(out int[] strutOffset)
        {
            strutOffset = new int[StrutsPerControlBox];
            int strut = 0;
            int strandOffset = 0;
            foreach (var strutLedCounts in LEDCount.ByControlBoxAndStrut)
            {
                int offset = strandOffset;
                foreach (var ledCount in strutLedCounts)
                {
                    strutOffset[strut++] = offset;
                    offset += ledCount;
                }
                strandOffset += LEDCount.StrandStride;
            }
        }
    }
}
