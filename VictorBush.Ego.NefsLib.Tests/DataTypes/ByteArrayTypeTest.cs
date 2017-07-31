using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes
{
    [TestClass]
    public class ByteArrayTypeTest
    {
        [TestMethod]
        public void Test_ByteArrayType()
        {
            FileStream file;
            ByteArrayType data;

            /*
             * Verify size is stored and retrieved correctly
             */
            data = new ByteArrayType(0, 6);
            Assert.AreEqual(6, data.Size);

            /*
             * Test 0 size array (not allowed)
             */
            try
            {
                data = new ByteArrayType(0, 0);

                /* Fail if this is reached */
                Assert.AreEqual(0, 1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                /* Pass if this exception was raised */
                Assert.AreEqual(1, 1);
            }
        }

        [TestMethod]
        public void Test_GetBytes()
        {
            ByteArrayType data;
            FileStream file;

            /*
             * Test getting byte array
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new ByteArrayType(0, 6);
            data.Read(file, 0);

            Assert.IsTrue(new byte[] 
                { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03 }.SequenceEqual(
                data.GetBytes()));
            file.Close();

            /*
             * Test getting byte array before reading data
             */
            data = new ByteArrayType(0, 2);
            Assert.IsTrue(new byte[]
                { 0x00, 0x00 }.SequenceEqual(
                data.GetBytes()));
        }

        [TestMethod]
        public void Test_GetUInt32()
        {
            ByteArrayType data;
            FileStream file;

            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new ByteArrayType(0, 6);
            data.Read(file, 0);

            /*
             * Test retrieval of UInt32 value from byte array
             */
            Assert.AreEqual((UInt32)0x05060708, data.GetUInt32(0));

            /*
             * Test retrieval of UInt32 using an offset
             */
            Assert.AreEqual((UInt32)0x03040506, data.GetUInt32(2));

            /*
             * Test when offset is 2 bytes away from end of array
             */
            try
            {
                data.GetUInt32(4);

                /* Fail if this is reached */
                Assert.AreEqual(0, 1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                /* Pass if this exception was raised */
                Assert.AreEqual(1, 1);
            }

            /*
             * Test when offset is outside bounds of array
             */
            try
            {
                data.GetUInt32(8);

                /* Fail if this is reached */
                Assert.AreEqual(0, 1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                /* Pass if this exception was raised */
                Assert.AreEqual(1, 1);
            }

            file.Close();
        }

        [TestMethod]
        public void Test_Read()
        {
            ByteArrayType data;
            FileStream file;

            /*
             *  Data size: 0x3
             *  Data offset: 0x2
             *  Base offset: 0x10
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new ByteArrayType(0x2, 0x3);
            data.Read(file, 0x10);

            Assert.IsTrue(new byte[] 
                { 0x26, 0x25, 0x24 }.SequenceEqual(
                data.Value));

            file.Close();

            /*
             * Data size: 5
             * Data offset: -4
             * Base offset: 0x10
             */
            file = new FileStream(TestHelper.DataTypeTestsFile, FileMode.Open);
            data = new ByteArrayType(-4, 5);
            data.Read(file, 0x10);

            Assert.IsTrue(new byte[]
                { 0x14, 0x13, 0x12, 0x11, 0x28 }.SequenceEqual(
                data.Value));

            file.Close();
        }
    }
}
