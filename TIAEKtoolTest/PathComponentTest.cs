using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using TIAEktool.Plc.Types;
using TIAEKtool.Plc;

namespace TIAEKtoolTest
{
    [TestClass]
    public class PathComponentTest
    {
        [TestMethod]
        public void TestPrefixType()
        {
            PathComponent a1 = new MemberComponent("a1", new STRUCT());
            PathComponent a2 = new MemberComponent("a2", new STRUCT(),a1);
            PathComponent a3 = new MemberComponent("a3", UINT.Type, a2);
            PathComponent b3 = new MemberComponent("b3", new STRUCT(), a2);
            PathComponent b4 = new MemberComponent("b4", new STRING(), b3);

            Assert.AreEqual(a3.ToString(), "a1.a2.a3");
            Assert.AreEqual(b4.ToString(), "a1.a2.b3.b4");
            Assert.AreEqual(b4.MatchPrefix(a3), 2);
            Assert.AreEqual(a3.MatchPrefix(b4), 1);
            Assert.AreEqual(b4.MatchPrefix(a2), 2);
            Assert.AreEqual(b4.MatchPrefix(a1), 3);
            Assert.AreEqual(b4.MatchPrefix(b4), 0);
        }
    }
}
