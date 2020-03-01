using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes
{
    [TestClass]
    public class FileDataTest
    {
        class TestClass
        {
            [FileData]
            UInt32Type _data_0x0 = new UInt32Type(0x0);

            [FileData]
            UInt32Type _data_0x4 = new UInt32Type(0x4);

            int _other_variable;

            public DataType Data_0x0
            { get { return _data_0x0; } }

            public DataType Data_0x4
            { get { return _data_0x4; } }
        }

        [TestMethod]
        public void Test_GetDataList()
        {
            var test = new TestClass();
            var data = FileData.GetDataList(test).ToList();
            Assert.AreEqual(2, data.Count);
            Assert.AreSame(test.Data_0x0, data[0]);
            Assert.AreSame(test.Data_0x4, data[1]);
        }
    }
}
