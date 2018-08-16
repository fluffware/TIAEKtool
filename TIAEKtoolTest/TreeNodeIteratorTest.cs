using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TIAEKtool;
using System.Windows.Forms;

namespace TIAEKtoolTest
{
    [TestClass]
    public class TreeNodeIteratorTest
    {
        [TestMethod]
        public void TestIterator()
        {
            TreeView view = new TreeView();
            TreeNode node1 = new TreeNode("1");
            view.Nodes.Add(node1);

            TreeNode node11 = new TreeNode("11");
            node11.Nodes.Add("111");
            node11.Nodes.Add("112");
            node1.Nodes.Add(node11);
            node1.Nodes.Add("12");
            node1.Nodes.Add("13");
            TreeNode node14 = new TreeNode("14");
            node1.Nodes.Add(node14);
            node14.Nodes.Add("141");

            TreeNodeDepthFirstEnumerator iter = new TreeNodeDepthFirstEnumerator(node1.Nodes);
            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("111", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("112", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("11", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("12", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("13", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("141", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("14", iter.Current.Text);

            Assert.AreEqual(true, iter.MoveNext());
            Assert.AreEqual("1", iter.Current.Text);

            Assert.AreEqual(false, iter.MoveNext());


        }
    }
}
