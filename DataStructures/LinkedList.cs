using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    class SkeletonLinkedList<T> where T : IComparable<T>
    {
        private Node head, tail;
        public int Length { get; private set; }

        public SkeletonLinkedList()
        {
            Clear();
        }

        private class Node
        {
            public Node(T data) { this.data = data; }
            public T data;
            public Node next, prev;
        }

        private struct Vector
        {
            public Vector(int magnetude, bool direction)
            { Magnetude = magnetude; Direction = direction; }
            public int Magnetude { get; private set; }
            public bool Direction { get; private set; }
        }

        public void Add(T data)
        {
            try
            {
                tail = tail.next = new Node(data) { prev = tail };
            }
            catch (Exception)
            {
                tail = head = new Node(data);
            }
            Length++;
        }

        public void InsertBefore(int index, T data)
        {
            Node prev, current, target;
            current = new Node(data);
            target = Get(index);
            prev = target.prev;

            current.prev = prev;
            current.next = target;

            prev.next = current;
            target.prev = current;
            Length++;
        }

        public void InsertAfter(int index, T data)
        {
            Node next, current, target;
            current = new Node(data);
            target = Get(index);
            next = target.next;

            current.prev = target;
            current.next = next;

            target.next = current;
            next.prev = current;
            Length++;
        }

        private Node Get(int index)
        {
            Vector location = OptimiseIndex(index);
            if (!InRange(location))
            {
                throw new IndexOutOfRangeException();
            }
            else
            {
                Node current = location.Direction ? head : tail;
                int count = 0;

                if (location.Direction)
                {
                    while (location.Magnetude > count)
                    {
                        count++;
                        current = current.next;
                    }
                }
                else
                {
                    while (location.Magnetude > count)
                    {
                        count++;
                        current = current.prev;
                    }
                }

                return current;
            }
        }

        private Node Find(T data)
        {
            Node current = head;
            for (int i = 0; i < Length; i++)
            {
                if (current.data.CompareTo(data) == 0)
                {
                    return current;
                }
                current = current.next;
            }
            return null;
        }

        public T this[int index]
        {
            get
            {
                if (InRange(index))
                { return Get(index).data; }
                else
                { return default(T); }

            }
            set
            {
                if (InRange(index))
                { Get(index).data = value; }
                else
                { Add(value); }
            }
        }

        public T[] this[int index, int num]
        {
            get
            {
                T[] data = new T[num];
                if (InRange(index))
                {
                    Node current = Get(index);
                    for (int i = 0; i < num; i++)
                    {
                        try
                        {
                            data[i] = Get(index + i).data;
                            current = current.next;
                            Node a = current.next;
                        }
                        catch (Exception)
                        { return data; }
                    }
                }

                return data;
            }
            set
            {
                Node current = head;
                if (InRange(index)) { current = Get(index); }
                num = (num < value.Length) ? num : value.Length;

                for (int i = 0; i < num; i++)
                {
                    if (InRange(index + i))
                    {
                        current.data = value[i];
                        current = current.next;
                    }
                    else
                    {
                        Add(value[i]);
                    }
                }
            }
        }

        public bool Contains(T data)
        {
            Node n = Find(data);
            return n != null;
        }

        private bool InRange(int index)
        {
            return index >= 0 - Length && index < Length;
        }

        private bool InRange(Vector location)
        {
            return location.Magnetude < Length;
        }

        private Vector OptimiseIndex(int index)
        {
            Vector current = index >= 0 ? new Vector(index, true) : new Vector(-(index + 1), false);
            return (current.Magnetude > Length / 2) ?
                new Vector(current.Magnetude - (Length - 1), !current.Direction) :
                current;
        }

        public void Clear()
        {
            Length = 0;
            head = tail = null;
        }

        public bool RemoveNext(T data)
        {
            Node n = Find(data);
            if (n != null)
            {
                RemoveNode(n);
                return true;
            }
            return false;
        }

        public T RemoveAt(int index)
        {
            Node n = Get(index);
            RemoveNode(n);
            return n.data;
        }

        private void RemoveNode(Node reference)
        {
            try
            { reference.prev.next = reference.next; }
            catch (Exception)// triggers when trying to remove the head.
            { head = head.next; }

            try
            { reference.next.prev = reference.prev; }
            catch (Exception)// triggers when trying to remove the tail
            { tail = tail.prev; }
            Length--;
        }

        public void Sort()
        {
            Node current;
            for (int x = 0; x < Length; x++)
            {
                current = head;
                for (int y = 0; y < Length - (x + 1); y++)
                {
                    if (current.data.CompareTo(current.next.data) > 0)
                    {
                        Swap(current, current.next);
                    }
                    current = current.next;
                }
            }
        }

        private void Swap(Node a, Node b)
        {
            T temp = a.data;
            a.data = b.data;
            b.data = temp;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("|");
            Node current = head;
            for (int i = 0; i < Length; i++)
            {
                sb.Append(current.data + "|");
                current = current.next;
            }
            return sb.ToString();
        }
    }
}
