using System;
using System.Collections.Generic;
using System.Linq;
using WinHue.Framework;

namespace WinHue.LED
{
    internal sealed partial class OpcDriver
    {
        private const int BytesPerPixel = 3;
        private const int MaxMessageLength = (1 << 16) - 1;
        private const int MaxPixelsPerChannel = MaxMessageLength / BytesPerPixel;
        private const int HeaderBytes = 4;

        private class PixelBatcher : IPixelBatcher
        {
            public PixelBatcher(OpcDriver parent, byte channel)
            {
                mParent = parent;
                Channel = channel;
            }

            public void SetPixel(int index, Color color)
            {
                if (index >= MaxPixelsPerChannel || index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Pixel index {index} out of range, must be in [0, ${MaxPixelsPerChannel})");
                }

                if (color.Equals(GetPixelSpan(index))) return;

                mMaxIndex = Math.Max(mMaxIndex, index);
                mPixelColors.TryAdd(index, color);
            }

            public Color GetPixel(int index)
            {
                if (index >= MaxPixelsPerChannel || index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Pixel index {index} out of range, must be in [0, ${MaxPixelsPerChannel})");
                }

                int offset = HeaderBytes + BytesPerPixel * index;
                return index + BytesPerPixel <= mBuffer.Length ? new Color(mBuffer[offset], mBuffer[offset + 1], mBuffer[offset + 2]) : new();
            }

            public void Send()
            {
                if (mPixelColors.Count == 0) return;

                ushort numDataBytes = (ushort)(BytesPerPixel * (mMaxIndex + 1));
                EnsureCapacity(numDataBytes + HeaderBytes);
                mBuffer[0] = Channel;
                mBuffer[1] = 0; // Command 0 - set 8-bit RGB
                var lengthSpan = new Span<byte>(mBuffer, 2, sizeof(ushort));
                BitConverter.TryWriteBytes(lengthSpan, numDataBytes);
                if (BitConverter.IsLittleEndian) { lengthSpan.Reverse(); }
                foreach (var (index, color) in mPixelColors)
                {
                    color.CopyTo(GetPixelSpan(index));
                }
                mParent.Send(new Span<byte>(mBuffer, 0, HeaderBytes + numDataBytes));

                mMaxIndex = 0;
                mPixelColors.Clear();
            }

            public byte Channel { get; }

            private void EnsureCapacity(int capacity)
            {
                if (mBuffer.Length >= capacity) return;

                var newBuffer = new byte[capacity];
                Array.Copy(mBuffer, newBuffer, mBuffer.Length);
                mBuffer = newBuffer;
            }

            private Span<byte> GetPixelSpan(int index)
            {
                return new(mBuffer, HeaderBytes + BytesPerPixel * index, BytesPerPixel);
            }

            private readonly OpcDriver mParent;
            private readonly Dictionary<int, Color> mPixelColors = new();
            private byte[] mBuffer = new byte[HeaderBytes + BytesPerPixel * 1000];
            private int mMaxIndex = 0;
        }
    }
}
