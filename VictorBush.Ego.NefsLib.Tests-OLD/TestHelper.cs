using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.Tests
{
    class TestHelper
    {
        public static string TestFileDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "../../TestData");

        public static string EmptyFile = Path.Combine(
            TestFileDir,
            "EmptyFile.dat"
            );

        public static string DataTypeTestsFile = Path.Combine(
            TestFileDir,
            "DataTypeTestsFile.dat"
            );
    }
}
