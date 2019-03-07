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
        public void ToString_CorrectStringReturned()
        {
            var file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            var data = new UInt32Type(8);
            data.Read(file, 0);
            Assert.AreEqual((UInt32)0x15161718, data.Value);
            file.Close();

            /* Test ToString() */
            Assert.AreEqual("15161718", data.ToString());
        }

        [TestMethod]
        public void UInt32Type_Size_4Bytes()
        {
            /* Verify size is 4 bytes */
            var data = new UInt32Type(0);
            Assert.AreEqual((UInt32)4, data.Size);
        }

        [TestMethod]
        public void UInt32Type_NegativeOffset()
        {
            /* Negative offset */
            var file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            var data = new UInt32Type(-8);
            data.Read(file, 8);
            Assert.AreEqual((UInt32)0x05060708, data.Value);
            file.Close();
        }

        [TestMethod]
        public void UInt32Type_NoOffset()
        {
            /* 0x0 offset */
            var file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            var data = new UInt32Type(0x0);
            data.Read(file, 0);
            Assert.AreEqual((UInt32)0x05060708, data.Value);
            file.Close();
        }

        [TestMethod]
        public void UInt32Type_PositiveOffset()
        {
            /* Positive offset */
            var file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            var data = new UInt32Type(8);
            data.Read(file, 0);
            Assert.AreEqual((UInt32)0x15161718, data.Value);
            file.Close();
        }
    }
}
