using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.DataTypes
{
    /// <summary>
    /// Provides a helper for defining the layout of data in a file. Also provides functions
    /// for reading and writing this data.
    /// </summary>
    /// <remarks>
    /// The [FileData] attribute can be placed above private fields of type DataType in a class.
    /// Listing the fields in the order they appear in the file is not necessary (although it may
    /// be helpful, for example, to document a file layout).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    class FileData : Attribute
    {
        /// <summary>
        /// Gets a list of all private DataType fields in the specified object that have the [FileData] attribute.
        /// </summary>
        /// <param name="obj">The object to get [FileData] fields from.</param>
        /// <returns>List of DataType objects.</returns>
        public static IEnumerable<DataType> GetDataList(object obj)
        {
            var fields = from f in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                         where (f.IsDefined(typeof(FileData), false)
                             && f.FieldType.BaseType == typeof(DataType))
                         select f.GetValue(obj) as DataType;

            return fields;
        }

        /// <summary>
        /// Gets all DataType fields with the [FileData] attribute and reads their data from the file stream.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        /// <param name="base_offset">Base offset in the file.</param>
        /// <param name="obj">The object whose [FileData] fields to read.</param>
        public static void ReadData(FileStream file, UInt32 baseOffset, object obj)
        {
            foreach (var data in GetDataList(obj))
            {
                data.Read(file, baseOffset);
            }
        }

        /// <summary>
        /// Gets all DataType fields with the [FileData] attribute and writes their data to the file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="base_offset">Base offset in the file.</param>
        /// <param name="obj">The object whose [FileData] fields to write.</param>
        public static void WriteData(FileStream file, UInt32 baseOffset, object obj)
        {
            foreach (var data in GetDataList(obj))
            {
                data.Write(file, baseOffset);
            }
        }
    }
}
