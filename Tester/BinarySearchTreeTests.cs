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
    public class BinarySearchTreeTests
    {
        readonly BinarySearchTree<int> test = new BinarySearchTree<int>();

        [TestMethod()]
        public void ContainsTest()
        {
            test.Insert(3);
            Assert.IsTrue(test.Contains(3));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            test.Clear();
            test.Insert(5);
            test.Insert(10);
            test.Insert(3);
            test.Insert(11);
            test.Insert(9);
            test.Remove(10);
            Assert.IsFalse(test.Contains(10));
        }

        [TestMethod()]
        public void ToStringTest()
        {
            test.Clear();
            test.Insert(5);
            test.Insert(4);
            test.Insert(7);
            string str = test.ToString();
            Assert.AreEqual("4 5 7 ", str);
        }

        [TestMethod()]
        public void ClearTest()
        {
            test.Insert(5);
            test.Clear();
            Assert.IsFalse(test.Contains(5));
        }

        [TestMethod()]
        public void InsertTest()
        {
            test.Insert(5);
            Assert.IsTrue(test.Contains(5));
        }

        [TestMethod()]
        public void ToArrayTest()
        {
            test.Clear();
            test.Insert(5);
            test.Insert(4);
            test.Insert(7);
            var arr = test.ToArray();
            Assert.IsTrue(arr.SequenceEqual(new int[] { 4, 5, 7 }));
        }
    }
}