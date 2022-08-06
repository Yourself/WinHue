using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinHue.Utilities;

namespace WinHue.Tests.Utilities
{
    [TestClass()]
    public class ProjectionTests
    {
        [TestMethod()]
        public void GetAzimuthalEquidistantTest()
        {
            Assert.AreEqual(Projection.GetAzimuthalEquidistant(new Vector3(0, 0, 1)).Radius, 0);

            Assert.AreEqual(Projection.GetAzimuthalEquidistant(new Vector3(1, 0, 0)).Radius, Math.PI / 2);

            Assert.AreEqual(Projection.GetAzimuthalEquidistant(new Vector3(1, 0, 0)).Azimuth, 0);
        }
    }
}
