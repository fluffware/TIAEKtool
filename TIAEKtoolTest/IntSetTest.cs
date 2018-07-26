using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TIAEKtool;

namespace TIAEKtoolTest
{
    [TestClass]
    public class IntSetTest
    {
        [TestMethod]
        public void SetTest()
        {
            var a = new IntSet();
            a.Add(1);
            a.Add(3, 6);
            a.Add(2);

            var b = new IntSet();
            b.Add(1, 6);

            Assert.AreEqual(b, a);

            a.Add(-2, -1);
            a.Add(0, 1);

            var c = new IntSet();
            c.Add(0, 6);
            c.Add(-2);
          
            Assert.AreEqual(c, a);

            a.Add(-1, 9);
            var d = new IntSet();
            d.Add(-2, 9);

            Assert.AreEqual(d, a);

            Assert.AreEqual(-3, c.LowestFree(-3));
            Assert.AreEqual(-1, c.LowestFree(-2));
            Assert.AreEqual(-1, c.LowestFree(-1));
            Assert.AreEqual(6, c.LowestFree(0));
            Assert.AreEqual(6, c.LowestFree(5));
            Assert.AreEqual(12, c.LowestFree(12));
        }
    }
}
