using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinHue.LED;

namespace WinHue.Tests.LED
{
    [TestClass]
    public class DomeTests
    {
        [TestMethod]
        public void TopologyTest()
        {
            var dome = new DomeOutput();

            var adjacency = new Dictionary<int, HashSet<int>>();
            int eCount = 0;
            foreach (var strut in dome.Struts)
            {
                var (front, back) = strut.Ends;

                adjacency.TryAdd(front.Index, new());
                adjacency[front.Index].Add(back.Index);

                adjacency.TryAdd(back.Index, new());
                adjacency[back.Index].Add(front.Index);
                ++eCount;
            }

            int vCount = adjacency.Count;
            int fCount = 2 + eCount - vCount;
            Assert.AreEqual(121, fCount);

            for (int i = 0; i < vCount; ++i)
            {
                Assert.IsTrue(adjacency.ContainsKey(i));
            }
        }

        [TestMethod]
        public void PositionsTest()
        {
            var dome = new DomeOutput();
            var vertices = dome.Struts.SelectMany(s => new[] { s.Ends.Item1, s.Ends.Item2 }).DistinctBy(v => v.Index).OrderBy(v => v.Index).ToList();
            foreach (var vertex in vertices)
            {
                Assert.That.AreClose(vertex.Position.Length(), 1.0f);
            }
        }
    }
}
