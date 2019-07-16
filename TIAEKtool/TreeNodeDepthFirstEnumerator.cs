using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIAEKtool
{
    public class TreeNodeDepthFirstEnumerator : IEnumerator<TreeNode>, IEnumerable<TreeNode>
    {
        TreeNode current;
        bool first = true;
        public TreeNode Current { get => current; }

        object IEnumerator.Current { get => current; }

        public TreeNodeDepthFirstEnumerator(TreeNodeCollection nodes)
        {
            if (nodes.Count > 0)
            {
                current = nodes[0];
                while (current.Nodes.Count > 0) current = current.Nodes[0];
            }
            else
            {
                current = null;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            if (current == null) return false;
            if (first)
            {
                first = false;
                return true;
            }
            if (current.NextNode != null)
            {
                current = current.NextNode;
                while (current.Nodes.Count > 0) current = current.Nodes[0];
                return true;
            }
            current = current.Parent;
            return current != null;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                   
                }
                current = null;
                disposedValue = true;
            }
        }

        #endregion
    }
}
