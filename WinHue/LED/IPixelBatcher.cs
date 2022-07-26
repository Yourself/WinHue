using WinHue.Framework;

namespace WinHue.LED
{
    /// <summary>
    /// Batches pixel changes together to minimize transmitted messages.
    /// </summary>
    internal interface IPixelBatcher
    {
        void SetPixel(int index, Color color);

        Color GetPixel(int index);

        void Send();

        byte Channel { get; }
    }
}
