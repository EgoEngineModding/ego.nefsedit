using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes
{
    [TestClass]
    public class UInt32TypeTest
    {
        [TestMethod]
        public void TestUInt32Type()
        {
            FileStream file;
            UInt32Type data;

            /*
             * Verify size is 4 bytes
             */
            data = new UInt32Type(0);
            Assert.AreEqual(4, data.Size);

            /*
             * 0x0 offset
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new UInt32Type(0x0);
            data.Read(file, 0);
            Assert.AreEqual((UInt32)0x05060708, data.Value);
            file.Close();

            /*
             * Negative offset
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new UInt32Type(-8);
            data.Read(file, 8);
            Assert.AreEqual((UInt32)0x05060708, data.Value);
            file.Close();

            /*
             * Positive offset
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new UInt32Type(8);
            data.Read(file, 0);
            Assert.AreEqual((UInt32)0x15161718, data.Value);
            file.Close();

            /* Test ToString() */
            Assert.AreEqual("15161718", data.ToString());
        }
    }
}
