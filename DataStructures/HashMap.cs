using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public class HashMap<T> : ICollection<T>
    {
        public int Count { get; private set; }

        public bool IsReadOnly => false;

        private int size = 1000;
        private List<T>[] data;

        public HashMap()
        {
            data = new List<T>[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = new List<T>();
            }
        }

        public void Add(T item)
        {
            int id = item.GetHashCode()%size;
            if (!data[id].Contains(item))
            {
                data[id].Add(item);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < size; i++)
            {
                data[i].Clear();
            }
        }

        public bool Contains(T item)
        {
            int id = item.GetHashCode()%size;
            return data[id].Contains(item);
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
            int id = item.GetHashCode()%size;
            if (data[id].Contains(item))
            {
                data[id].Remove(item);
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
}
