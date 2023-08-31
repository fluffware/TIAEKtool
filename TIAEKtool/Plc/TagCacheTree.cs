using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool.Plc
{

    public class TagCacheTree : IEnumerable<Tag>, ITagCache
    {


        protected class TagNode
        {
            public PathComponent path;
            public Tag tag;
            public SortedList<PathComponent, TagNode> children;
        }

        private readonly TagNode _tags;
        public TagCacheTree()
        {
            _tags = new TagNode()
            {
                tag = null,
                children = new SortedList<PathComponent, TagNode>()
            };
        }

        protected TagNode CreatNode(TagNode node, PathComponent path)
        {
            if (path.Parent != null) node = CreatNode(node, path.Parent);
            if (node.children.TryGetValue(path, out TagNode child_node))
            {
                return child_node;
            }
            else
            {
                TagNode new_node = new TagNode() { path = path, tag = null, children = new SortedList<PathComponent, TagNode>() };
                node.children.Add(path, new_node);
                return new_node;
            }
        }
        protected TagNode FindNode(TagNode node, PathComponent path)
        {
            if (path.Parent != null) node = FindNode(node, path.Parent);
            if (node.children.TryGetValue(path, out TagNode child_node))
            {
                return child_node;
            }
            else
            {
                return null;
            }
        }

        public void Add(Tag tag)
        {
            TagNode node = CreatNode(_tags, tag.Path);
            node.tag = tag;
        }

        public Tag Find(PathComponent path)
        {
            TagNode node = FindNode(_tags, path);
            return node?.tag;
        }

        private string NodeToString(TagNode node, int indent)
        {

            StringBuilder str = new StringBuilder(new string(' ', indent));

            foreach (var tag in this) {
                str.Append(tag.Path != null ? tag.Path.ToString() : "/");
                if (tag.StartValue != null)
                    str.Append(" =" + tag.StartValue);
                if (tag.Comment != null && tag.Comment.TryGetAnyText(out string comment))
                    str.Append(" // " + comment);

                str.Append("\n");
            }
            return str.ToString();
        }

        public override string ToString()
        {
            return NodeToString(_tags, 0);
        }


        class DepthFirstEnumerator : IEnumerator<Tag>
        {
            TagNode node;
            Stack<IEnumerator<TagNode>> iter_stack;

            public DepthFirstEnumerator(TagNode n)
            {
                node = n;
            }
            public Tag Current
            {
                get
                {
                    return node.tag;

                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return node.tag;
                }
            }

            public void Dispose()
            {
                node = null;
                iter_stack = null;
            }

            public bool MoveNext()
            {
                while (node != null)
                {

                    if (iter_stack == null)
                    {
                        iter_stack = new Stack<IEnumerator<TagNode>>();
                        iter_stack.Push(node.children.Values.GetEnumerator());
                    }
                    else
                    {
                        // Move down to a leaf node
                        while (node.children != null && node.children.Count > 0)
                        {
                            var iter = node.children.Values.GetEnumerator();
                            iter.MoveNext();
                            iter_stack.Push(iter);

                            node = iter.Current;
                            if (node.tag != null)
                            {
                                return true;
                            }
                        }
                        // Move up the tree until we can advance the iterator
                        while (true)
                        {
                            if (iter_stack.First().MoveNext())
                            {
                                node = iter_stack.First().Current;
                                if (node.tag != null)
                                {
                                    return true;
                                }

                            }
                            else
                            {

                                if (iter_stack.Count == 1)
                                {
                                    node = null;
                                    iter_stack = null;
                                    return false;
                                }
                                iter_stack.Pop();

                            }
                        }
                      
                       
                    }
                }
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }


        public IEnumerator<Tag> GetEnumerator()
        {
            return new DepthFirstEnumerator(_tags);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DepthFirstEnumerator(_tags);
        }

        public IEnumerator<Tag> GetEnumerator(PathComponent prefix)
        {
            TagNode node = FindNode(_tags, prefix);
            return new DepthFirstEnumerator(node);
        }
    }
   
}
