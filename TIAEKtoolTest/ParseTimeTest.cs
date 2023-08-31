using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TIAEKtool;
using TIAEKtool.Plc;

namespace TIAEKtoolTest
{
    [TestClass]
    public class ParseTimeTest
    {
        [TestMethod]
        public void ParseTime()
        {
            Assert.AreEqual(new TimeSpan(0, 0, 1), PlcValue.ParseTimeValue("T#1s"));
            Assert.AreEqual(new TimeSpan(10, 20, 30, 20, 630), PlcValue.ParseTimeValue("T#10d_20h_30m_20s_630ms"));
            Assert.AreEqual(new TimeSpan(10, 20, 30, 20, 630), PlcValue.ParseTimeValue("TIME#10d_20h_30m_20s_630ms"));
            Assert.AreEqual(new TimeSpan(20, 0,0), PlcValue.ParseTimeValue("T#20h"));
        }

        public void TimespanToPLCValue()
        {
            Assert.AreEqual(PresetDocument.TimespanToPLCValue( new TimeSpan(0, 0, 1)), "T#1s");
            Assert.AreEqual(PresetDocument.TimespanToPLCValue(new TimeSpan(3, 0, 1)), "T#3h");
            Assert.AreEqual(PresetDocument.TimespanToPLCValue(new TimeSpan(34, 6, 17)), "T#34h6m17s");
            Assert.AreEqual(PresetDocument.TimespanToPLCValue(new TimeSpan(12,0, 0, 0)), "T#12d");
            Assert.AreEqual(PresetDocument.TimespanToPLCValue(new TimeSpan(12, 0, 0, 0,932)), "T#12d932ms");
        }
    }
}
