using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIAEKtool.Plc;

namespace TIAEKtoolTest
{

    [TestClass]
    public class PlcValueTest
    {
        [TestMethod]
        public void TestStringEscape()
        {
            Assert.AreEqual("abc$$$'$R$L$P$Tdef", PlcValue.EscapeStringValue("abc$'\r\n\f\tdef"));
        }

        [TestMethod]
        public void TestValueToString()
        {
            Assert.AreEqual("2834", PlcValue.ValueToString(2834));
            Assert.AreEqual("T#23m6s", PlcValue.ValueToString(new TimeSpan(0, 23, 6)));
            Assert.AreEqual("'abc$Rdef'", PlcValue.ValueToString("abc\rdef"));
            Assert.AreEqual("87.25", PlcValue.ValueToString(87.25));
        }
    }

}
