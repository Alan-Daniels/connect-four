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
        readonly BinarySearchTree<string> test = new BinarySearchTree<string>();

        [TestMethod()]
        public void AddTest()
        {
            test.Add("add");
            Assert.IsTrue(test.Contains("add"));
        }

        [TestMethod()]
        public void ClearTest()
        {
            test.Clear();
            Assert.IsFalse(test.Contains("add"));
        }

        [TestMethod()]
        public void ContainsTest()
        {
            test.Add("contains");
            Assert.IsTrue(test.Contains("contains"));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            test.Add("remove");
            test.Remove("remove");
            Assert.IsFalse(test.Contains("remove"));
        }
    }
}