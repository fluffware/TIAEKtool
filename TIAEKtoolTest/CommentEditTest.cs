using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TIAEKtoolTest
{
    [TestClass]
    public class CommentEditTest
    {
        class ParseCtxt
        {
            public string foo;
            public string bar;
            public string foo_bar3;

            public bool Handle(string type, ref string data)
            {
                if (type == "foo") foo = data;
                else if (type == "bar") bar = data;
                else if (type == "foo_bar3") foo_bar3 = data;
                return true;
            }
            public static bool Edit(string type, ref string data)
            {
                if (type == "foo") data = ":foo";
                else if (type == "bar") data = " bar";
                else if (type == "foo_bar3") data = " foo_bar3";
                else return false;
                return true;
            }
        }
        [TestMethod]
        public void TestParser()
        {
            ParseCtxt ctxt = new ParseCtxt();
            StringBuilder comment = new StringBuilder(" @{foo:kls}jlkd@{foo_bar3} @{bar sjkjk}");
            TIAEKtool.CommentEditor.Edit(ref comment, new HashSet<string>(new string[] { "foo", "bar", "foo_bar3" }), ctxt.Handle);
            Assert.AreEqual(ctxt.foo, ":kls");
            Assert.AreEqual(ctxt.bar, " sjkjk");
            Assert.AreEqual(ctxt.foo_bar3, "");
        }

        [TestMethod]
        public void TestEdit()
        {
            ParseCtxt ctxt = new ParseCtxt();
            StringBuilder comment = new StringBuilder(" @{foo:kls}jlkd@{foo_bar3} @{bar sjkjk}t");
            TIAEKtool.CommentEditor.Edit(ref comment, new HashSet<string>(new string[] { "foo", "bar", "foo_bar3" }), ParseCtxt.Edit);
            Assert.AreEqual(comment.ToString(), " @{foo:foo}jlkd@{foo_bar3 foo_bar3} @{bar bar}t");
            comment = new StringBuilder("@{ksl slll}jlkd@{foo_bar3} @{bar sjkjk}t");
            TIAEKtool.CommentEditor.Edit(ref comment, new HashSet<string>(new string[] { "foo", "bar", "foo_bar3" }), ParseCtxt.Edit);
            Assert.AreEqual(comment.ToString(), "@{ksl slll}jlkd@{foo_bar3 foo_bar3} @{bar bar}t@{foo:foo}");

            comment = new StringBuilder("@{ksl slll}jlkd@{foo_bar3} @{bar sjkjk}t");
            TIAEKtool.CommentEditor.Edit(ref comment, new HashSet<string>(new string[] { "foo", "bar","ksl", "foo_bar3" }), ParseCtxt.Edit);
            Assert.AreEqual(comment.ToString(), "jlkd@{foo_bar3 foo_bar3} @{bar bar}t@{foo:foo}");
        }
    }
}
