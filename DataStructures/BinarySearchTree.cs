using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public class BinarySearchTree<T> : ICollection<T> where T : IComparable<T>
    {
        public int Count { get; private set; }

        public bool IsReadOnly{get{ return false; } }

        private BinaryNode<T> root;

        private ref BinaryNode<T> Find(ref BinaryNode<T> search, T match)
        {
            if (search == null || search.CompareTo(match) == 0)
            {
                return ref search;
            }
            else if (search.CompareTo(match)==1)
            {
                if (search.right != null)
                {
                    return ref Find(ref search, match);
                }
                return ref search.right;
            }
            else
            {
                if (search.left != null)
                {
                    return ref Find(ref search, match);
                }
                return ref search.left;
            }
        }

        public void Add(T item)
        {
            var node = Find(ref root, item);
            if (node == null)
                node = new BinaryNode<T>(item);
            else
                node.Data = item;
        }

        public void Clear()
        {
            root = null;
        }

        public bool Contains(T item)
        {
            return Find(ref root, item) != null;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            var node = Find(ref root, item);
            if (node != null)
            {
                node = null;
                return true;
            }
            else
                return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    class BinaryNode<T> : IComparable<T> where T : IComparable<T>
    {
        public T Data;
        public BinaryNode<T> right, left;

        public BinaryNode(T data)
        {
            Data = data;
        }

        public int CompareTo(T other)
        {
            try
            {
                return Data.CompareTo(other);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
