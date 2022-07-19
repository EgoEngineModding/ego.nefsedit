﻿// See LICENSE.txt for license information.

using System.Reflection;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// Provides a helper for defining the layout of data in a file. Also provides functions for reading and writing this data.
/// </summary>
/// <remarks>
/// The [FileData] attribute can be placed above fields or properties of type DataType in a class. Listing the fields in
/// the order they appear in the file is not necessary (although it may be helpful, for example, to document a file layout).
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class FileData : Attribute
{
	/// <summary>
	/// Gets a list of all public and private DataType fields and properties in the specified object that have the
	/// [FileData] attribute.
	/// </summary>
	/// <param name="obj">The object to get [FileData] fields from.</param>
	/// <returns>List of DataType objects.</returns>
	public static IEnumerable<DataType> GetDataList(object obj)
	{
		var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Where(f => f.IsDefined(typeof(FileData), false)
					&& f.PropertyType.BaseType == typeof(DataType))
			.Select(f => (DataType)f.GetValue(obj));

		var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Where(f => f.IsDefined(typeof(FileData), false)
					&& f.FieldType.BaseType == typeof(DataType))
			.Select(f => (DataType)f.GetValue(obj));

		return props.Concat(fields);
	}

	/// <summary>
	/// Gets all DataType fields with the [FileData] attribute and reads their data from the file stream.
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
