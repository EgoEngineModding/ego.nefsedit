// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// Provides a helper for defining the layout of data in a file. Also provides functions for
    /// reading and writing this data.
    /// </summary>
    /// <remarks>
    /// The [FileData] attribute can be placed above fields or properties of type DataType in a
    /// class. Listing the fields in the order they appear in the file is not necessary (although it
    /// may be helpful, for example, to document a file layout).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FileData : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileData"/> class.
        /// </summary>
        public FileData()
        {
        }

        /// <summary>
        /// Gets a list of all public and private DataType fields and properties in the specified
        /// object that have the [FileData] attribute.
        /// </summary>
        /// <param name="obj">The object to get [FileData] fields from.</param>
        /// <returns>List of DataType objects.</returns>
        public static IEnumerable<DataType> GetDataList(object obj)
        {
            var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(FileData), false)
                        && f.PropertyType.BaseType == typeof(DataType))
                .Select(f => f.GetValue(obj) as DataType);

            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(FileData), false)
                        && f.FieldType.BaseType == typeof(DataType))
                .Select(f => f.GetValue(obj) as DataType);

            return props.Concat(fields);
        }

        /// <summary>
        /// Gets all DataType fields with the [FileData] attribute and reads their data from the
        /// file stream.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        /// <param name="baseOffset">Base offset in the file.</param>
        /// <param name="obj">The object whose [FileData] fields to read.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ReadDataAsync(Stream file, long baseOffset, object obj, NefsProgress p)
        {
            foreach (var data in GetDataList(obj))
            {
                await data.ReadAsync(file, baseOffset, p);
            }
        }

        /// <summary>
        /// Gets all DataType fields with the [FileData] attribute and writes their data to the file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="baseOffset">Base offset in the file.</param>
        /// <param name="obj">The object whose [FileData] fields to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteDataAsync(Stream file, long baseOffset, object obj, NefsProgress p)
        {
            foreach (var data in GetDataList(obj))
            {
                await data.WriteAsync(file, baseOffset, p);
            }
        }
    }
}
