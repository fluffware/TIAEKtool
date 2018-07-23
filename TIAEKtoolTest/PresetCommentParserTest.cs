using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TIAEKtool;

namespace TIAEKtoolTest
{
    [TestClass]
    public class PresetCommentParserTest
    {
        [TestMethod]
        public void TestParser()
        {
            PresetTag tag = new PresetTag();
            PresetCommentParser.Parse("@{preset: Preset label } @{preset_default 2} @{preset_nostore}", "en_US", tag);
            Assert.AreEqual("Preset label", tag.labels["en_US"]);
            Assert.AreEqual("2", tag.defaultValue);
            Assert.AreEqual(true, tag.noStore);
        }
    }
}
