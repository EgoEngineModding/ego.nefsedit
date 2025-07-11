﻿// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// A table in the NeFS table of contents header.
/// </summary>
public interface INefsTocTable<T> where T : unmanaged, INefsTocData<T>
{
	/// <summary>
	/// The table's entries.
	/// </summary>
	IReadOnlyList<T> Entries { get; }

	/// <summary>
	/// The size of the table in bytes.
	/// </summary>
	int ByteCount => Entries.Count * T.ByteCount;

	/// <summary>
	/// The size of an entry in bytes.
	/// </summary>
	static int EntryByteCount => T.ByteCount;
}

/// <summary>
/// A table in the NeFS table of contents header.
/// </summary>
public interface INefsTocTable<T, TData> : INefsTocTable<TData>
	where T : INefsTocTable<T, TData>
	where TData : unmanaged, INefsTocData<TData>
{
	/// <summary>
	/// Creates a table with the given entries.
	/// </summary>
	internal static abstract T Create(IReadOnlyList<TData> entries);
}

public static class NefsTocTableExtensions
{
	/// <summary>
	/// The size of the table in bytes.
	/// </summary>
	public static int ByteCount<T>(this INefsTocTable<T> table)
		where T : unmanaged, INefsTocData<T>
	{
		return table.ByteCount;
	}
}
