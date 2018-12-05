using Microsoft.VisualStudio.TestTools.UnitTesting;
using Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection.Tests
{
    [TestClass()]
    public class JsonConvertTests
    {
        [Serializable]
        class TestClass
        {
            public int num;
            public int num2;

            public override string ToString()
            {
                return $"{{\"num\":{num},\"num2\":{num2}}}";
            }
        }

        TestClass a = new TestClass() { num = 1, num2 = 2 };

        [TestMethod()]
        public void SerialiseTest()
        {
            Assert.AreEqual(a.ToString(), JsonConvert.Serialise(a));
        }

        [TestMethod()]
        public void DeserialiseTest()
        {
            Assert.AreEqual(a.ToString(), JsonConvert.Deserialise<TestClass>(a.ToString()).ToString());
        }
    }
}