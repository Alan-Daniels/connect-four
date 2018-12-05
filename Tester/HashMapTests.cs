using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.Tests
{
    [TestClass()]
    public class HashMapTests
    {
        public HashMap<int> hash = new HashMap<int>();

        [TestMethod()]
        public void AddTest()
        {
            hash.Add(1234);
            Assert.IsTrue(hash.Contains(1234));
        }

        [TestMethod()]
        public void ClearTest()
        {
            hash.Add(12);
            hash.Clear();
            Assert.IsFalse(hash.Contains(12));
        }

        [TestMethod()]
        public void ContainsTest()
        {
            hash.Add(33);
            Assert.IsTrue(hash.Contains(33));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            hash.Clear();
            hash.Add(45);
            hash.Remove(45);
            Assert.IsFalse(hash.Contains(45));
        }
    }
}