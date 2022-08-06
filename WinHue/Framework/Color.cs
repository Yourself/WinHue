using System;

namespace WinHue.Framework
{
    /// <summary>
    /// Represents an 8-bit per channel RGB color.
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Computes a color blended between two others by the specified amount.
        /// </summary>
        /// 
        /// <param name="alpha">The amount to blend by.</param>
        /// <param name="a">The first color to blend.</param>
        /// <param name="b">The second color to blend.</param>
        /// 
        /// <returns>A linearly blended combination of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static Color Blend(double alpha, Color a, Color b)
        {
            return new(
                ToByte((b.R - a.R) * alpha + a.R),
                ToByte((b.G - a.G) * alpha + a.G),
                ToByte((b.B - a.B) * alpha + a.B)
                );
        }

        public static explicit operator Color(int color) { return new() { R = (byte)(color >> 16), G = (byte)(color >> 8), B = (byte)(color) }; }

        /// <summary>
        /// Constructs a new instance of <see cref="Color"/> with the specified channel values.
        /// </summary>
        /// 
        /// <param name="r">The value of the red channel.</param>
        /// <param name="g">The value of the green channel.</param>
        /// <param name="b">The value of the blue channel.</param>
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="Color"/> with the specified normalized channel values.
        /// </summary>
        /// 
        /// <param name="r">The value of the red channel from 0 to 1.</param>
        /// <param name="g">The value of the green channel from 0 to 1.</param>
        /// <param name="b">The value of the blue channel from 0 to 1.</param>
        public Color(double r, double g, double b)
        {
            R = ToByte(255 * r);
            G = ToByte(255 * g);
            B = ToByte(255 * b);
        }

        /// <summary>
        /// Copies this color into the specified span.
        /// </summary>
        /// 
        /// <param name="span">The span describing the range in which to copy this color as bytes.</param>
        public readonly void CopyTo(in Span<byte> span)
        {
            span[0] = R;
            span[1] = G;
            span[2] = B;
        }

        /// <summary>
        /// Whether this color is equivalent to the color serialized in the specified span.
        /// </summary>
        /// 
        /// <param name="span">The span describing the range to compare with.</param>
        /// 
        /// <returns>true if the color in the first 3 bytes of <paramref name="span"/> matches this color; otherwise, false.</returns>
        public readonly bool Equals(in ReadOnlySpan<byte> span)
        {
            return span[0] == R && span[1] == G && span[2] == B;
        }

        public readonly override string ToString()
        {
            return $"0x{R:x2}{G:x2}{B:x2}";
        }

        public byte R;
        public byte G;
        public byte B;

        private static byte ToByte(double x)
        {
            return (byte)Math.Clamp(Math.Round(x), 0, 255);
        }
    }
}
