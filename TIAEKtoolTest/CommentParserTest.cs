using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TIAEKtoolTest
{
    [TestClass]
    public class CommentParserTest
    {
        class ParseCtxt
        {
            public string foo;
            public string bar;
            public string foo_bar3;

            public void Handle(string type, string data)
            {
                if (type == "foo") foo = data;
                else if (type == "bar") bar = data;
                else if (type == "foo_bar3") foo_bar3 = data;
            }
        }
        [TestMethod]
        public void TestParser()
        {
            ParseCtxt ctxt = new ParseCtxt();
            TIAEKtool.CommentParser.Parse(" @{foo:kls}jlkd@{foo_bar3} @{bar sjkjk}", ctxt.Handle);
            Assert.AreEqual(ctxt.foo, ":kls");
            Assert.AreEqual(ctxt.bar, " sjkjk");
            Assert.AreEqual(ctxt.foo_bar3, "");
        }
    }
}
