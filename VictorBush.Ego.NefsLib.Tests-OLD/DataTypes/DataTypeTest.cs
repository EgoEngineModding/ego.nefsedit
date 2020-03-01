using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes
{
    [TestClass]
    public class DataTypeTest
    {
        /// <summary>
        /// Bare implementation of DataType to use in tests.
        /// </summary>
        class TestDataType : DataType
        {
            public TestDataType(int offset) : base(offset)
            { }

            public override UInt32 Size { get { return 0x04; } }

            public override byte[] GetBytes()
            {
                throw new NotImplementedException();
            }

            public override void Read(Stream file, uint baseOffset)
            {
                readFile(file, baseOffset);
            }
        }

        [TestMethod]
        public void Test_Offset()
        {
            TestDataType data;

            /*
             *  Test 0 offset
             */
            data = new TestDataType(0);
            Assert.AreEqual(0, data.Offset);

            /*
             * Test positive offset (+123456)
             */
            data = new TestDataType(123456);
            Assert.AreEqual(123456, data.Offset);

            /*
             * Test negative offset (-123456);
             */
            data = new TestDataType(-123456);
            Assert.AreEqual(-123456, data.Offset);
        }

        [TestMethod]
        public void Test_Write()
        {
            TestDataType data;
            FileStream file;

            /*
             * Test null file stream
             */
            data = new TestDataType(0);
            try
            {
                data.Write(null, 0);
            }
            catch (ArgumentNullException ex)
            {
                /* Passes if this exception is caught */
                Assert.AreEqual(1, 1);
            }

            /* 
             * Try to write from a negative actual offset
             */
            file = new FileStream(TestHelper.EmptyFile, FileMode.Open);
            data = new TestDataType(-4);

            try
            {
                data.Read(file, 0);

                /* Fail if this is reached */
                Assert.AreEqual(1, 0);
            }
            catch (InvalidOperationException ex)
            {
                /* Pass if this exception is caught */
                Assert.AreEqual(1, 1);
            }

            file.Close();
        }

        [TestMethod]
        public void Test_readFile()
        {
            FileStream file;
            TestDataType data;

            /*
             * Test null file stream
             */
            data = new TestDataType(0);
            try
            {
                data.Read(null, 0);
            }
            catch (ArgumentNullException ex)
            {
                /* Passes if this exception is caught */
                Assert.AreEqual(1, 1);
            }

            /* 
             * Try to read from a negative actual offset
             */
            file = new FileStream(TestHelper.EmptyFile, FileMode.Open);
            data = new TestDataType(-4);

            try
            {
                data.Read(file, 0);

                /* Fail if this is reached */
                Assert.AreEqual(1, 0);
            }
            catch (InvalidOperationException ex)
            {
                /* Pass if this exception is caught */
                Assert.AreEqual(1, 1);
            }

            file.Close();

            /*
             * Try to read past end of file
             */
            file = new FileStream(TestHelper.EmptyFile, FileMode.Open);
            data = new TestDataType(0);

            try
            {
                data.Read(file, 0);

                /* Fail if this is reached */
                Assert.AreEqual(1, 0);
            }
            catch (Exception ex)
            {
                /* Pass if this exception is caught */
                Assert.AreEqual(1, 1);
            }

            /* 
             * Valid read tests are handled by classes that inherit DataType.
             */
        }
    }
}
