using System;
using System.Collections.Generic;

namespace DataStructures
{
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        private Node root;

        private class Node
        {
            public T data;
            public Node left;
            public Node right;

            public Node(T data, Node left, Node right)
            {
                this.data = data;
                this.left = left;
                this.right = right;
            }
        }

        public void Insert(T data)
        {
            Search(ref root, data, out Node n) = new Node(data, null, null);
        }

        public bool Contains(T data)
        {
            return (Search(ref root, data, out Node n) != null);
        }

        private ref Node SearchClosest(ref Node from, T data, out Node prev)
        {
            int compare = from == null ? 0 : from.data.CompareTo(data);
            if (compare == 0)
            {
                //found
                prev = root;
                return ref from;
            }
            else if (compare < 0)
            {
                //move right
                int comp = from.right == null ? 0 : from.right.data.CompareTo(data);
                if (comp == 0)
                {
                    prev = null;
                    return ref from;
                }
                else if (comp > 0)
                {
                    if (from.right.right == null)
                    {
                        prev = from;
                        return ref from.right;
                    }
                    else
                        return ref Search(ref from.right, data, out prev);
                }
                else
                {
                    if (from.right.left == null)
                    {
                        prev = from;
                        return ref from.right;
                    }
                    else
                        return ref Search(ref from.right, data, out prev);
                }
            }
            else
            {
                //move left
                int comp = from.left == null ? 0 : from.left.data.CompareTo(data);

                if (comp == 0)
                {
                    prev = null;
                    return ref from;
                }
                else if (comp > 0)
                {
                    if (from.left.right == null)
                    {
                        prev = from;
                        return ref from.left;
                    }
                    else
                        return ref Search(ref from.left, data, out prev);
                }
                else
                {
                    if (from.left.left == null)
                    {
                        prev = from;
                        return ref from.left;
                    }
                    else
                        return ref Search(ref from.left, data, out prev);
                }
            }
        }

        private ref Node Search(ref Node from, T data, out Node prev)
        {

            int compare = from == null ? 0 : from.data.CompareTo(data);
            if (compare == 0)
            {
                //found
                prev = root;
                return ref from;
            }
            else if (compare < 0)
            {
                //move right
                if (from.right == null || from.right.data.CompareTo(data) == 0)
                {
                    prev = from;
                    return ref from.right;
                }
                else
                {
                    return ref Search(ref from.right, data, out prev);
                }
            }
            else
            {
                //move left
                if (from.left == null || from.left.data.CompareTo(data) == 0)
                {
                    prev = from;
                    return ref from.left;
                }
                else
                {
                    return ref Search(ref from.left, data, out prev);
                }
            }
        }

        public void Remove(T data)
        {
            Node node = Search(ref root, data, out Node prev);
            int compare = prev.data.CompareTo(node.data);
            Node right = node.right;
            Node left = node.left;
            if (right != null && left != null)//two children
            {
                Node newNode = SearchClosest(ref right, data, out Node nPrev);
                if (nPrev == null)
                {
                    nPrev = node;
                }
                int comp = nPrev.data.CompareTo(newNode.data);

                if (comp > 0)
                {
                    nPrev.left = null;
                }
                else
                {
                    nPrev.right = null;
                }

                if (compare > 0)
                {
                    prev.left = newNode;
                    newNode.left = left;
                }
                else
                {
                    prev.right = newNode;
                    newNode.left = left;
                }
            }
            else if (right != null || left != null)//one child
            {
                if (compare > 0)
                {
                    prev.left = left ?? right;// replace this node with the valid child node
                }
                else
                {
                    prev.right = left ?? right;
                }
            }
            else//leaf node
            {
                if (compare > 0)
                {
                    prev.left = null;
                }
                else
                {
                    prev.right = null;
                }
            }

            /*
             * case 1: Is a leaf node
             * case 2: Has one child
             * case 3: Has two children
             */
        }

        public void Clear()
        {
            root = null;
        }

        private interface IVisitor
        {
            void Visit(Node node);
        }

        private class PrintVisitor : IVisitor
        {
            private String output;
            public List<T> Ts { get; } = new List<T>();

            void IVisitor.Visit(Node node)
            {
                output += node.data.ToString() + " ";
                Ts.Add(node.data);
            }
            public override string ToString()
            {
                return output;
            }
        }

        public T[] ToArray()
        {
            PrintVisitor pv = new PrintVisitor();
            Traverse(root, pv);
            return pv.Ts.ToArray();
        }

        public override string ToString()
        {
            PrintVisitor pv = new PrintVisitor();
            Traverse(root, pv);
            return pv.ToString();
        }

        private void Traverse(Node from, IVisitor visitor)
        {
            if (from == null) return;
            Traverse(from.left, visitor);
            visitor.Visit(from);
            Traverse(from.right, visitor);
        }
    }
}
