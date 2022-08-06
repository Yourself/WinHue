using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WinHue.Tests
{
    internal static class AssertExtentions
    {
        public static void AreClose(this Assert _, double x, double y, double eps = 1e-15)
        {
            Assert.IsTrue(Math.Abs(x - y) < eps);
        }

        public static void AreClose(this Assert _, float x, float y, float eps = 1e-6f)
        {
            Assert.IsTrue(Math.Abs(x - y) < eps);
        }
    }
}
