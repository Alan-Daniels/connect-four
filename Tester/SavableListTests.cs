using Microsoft.VisualStudio.TestTools.UnitTesting;
using Connect_Four;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect_Four.Tests
{
    [TestClass()]
    public class SavableListTests
    {
        SavableList<int> test = new SavableList<int>(new System.IO.FileInfo("SaveTest.txt"));

        [TestMethod()]
        public void LoadTest()
        {
            test.Add(123);
            test.Save();
            test.Clear();
            test.Load();
            Assert.IsTrue(test.Contains(123));
        }

        [TestMethod()]
        public void SaveTest()
        {
            test.Add(456);
            test.Save();
            test.Clear();
            test.Load();
            Assert.IsTrue(test.Contains(456));
        }
    }
}