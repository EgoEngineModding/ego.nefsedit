using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes
{
    [TestClass]
    public class UInt64TypeTest
    {
        [TestMethod]
        public void TestUInt64Type()
        {
            FileStream file;
            UInt64Type data;

            /*
             * Verify size is 8 bytes
             */
            data = new UInt64Type(0);
            Assert.AreEqual(8, data.Size);

            /*
             * 0x0 offset
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new UInt64Type(0x0);
            data.Read(file, 0);
            Assert.AreEqual((UInt64)0x0102030405060708, data.Value);
            file.Close();

            /*
             * Negative offset
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new UInt64Type(-8);
            data.Read(file, 8);
            Assert.AreEqual((UInt64)0x0102030405060708, data.Value);
            file.Close();
            
            /*
             * Positive offset
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new UInt64Type(8);
            data.Read(file, 0);
            Assert.AreEqual((UInt64)0x1112131415161718, data.Value);
            file.Close();

            /* Test ToString() */
            Assert.AreEqual("1112131415161718", data.ToString());
        }
    }
}
