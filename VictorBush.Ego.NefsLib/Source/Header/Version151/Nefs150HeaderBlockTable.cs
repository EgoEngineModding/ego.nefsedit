// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// The block table.
/// </summary>
public sealed class Nefs150HeaderBlockTable : INefsTocTable<Nefs150TocBlock>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs150TocBlock> Entries { get; }

	/// <summary>
	/// There is a 4-byte value at the end of header part 4. Purpose unknown.
	/// </summary>
	public uint UnkownEndValue { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderBlockTable"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	/// <param name="unkownEndValue">Last four bytes of part 4.</param>
	internal Nefs150HeaderBlockTable(IReadOnlyList<Nefs150TocBlock> entries, uint unkownEndValue)
	{
		Entries = entries;
		UnkownEndValue = unkownEndValue;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class from a list of items.
	// /// </summary>
	// /// <param name="items">The items to initialize from.</param>
	// /// <param name="unkownEndValue">Last four bytes of part 4.</param>
	// internal Nefs150HeaderBlockTable(NefsItemList items, uint unkownEndValue)
	// {
	// 	var entries = new List<Nefs150TocBlock>();
	// 	UnkownEndValue = unkownEndValue;
	//
	// 	var nextStartIdx = 0U;
	//
	// 	foreach (var item in items.EnumerateById())
	// 	{
	// 		if (item.Type == NefsItemType.Directory || item.DataSource.Size.Chunks.Count == 0)
	// 		{
	// 			// Item does not have a part 4 entry
	// 			continue;
	// 		}
	//
	// 		// Create entry for each data chunk
	// 		foreach (var chunk in item.DataSource.Size.Chunks)
	// 		{
	// 			// Create entry
	// 			var entry = new Nefs150TocBlock
	// 			{
	// 				End = chunk.CumulativeSize,
	// 				Transformation = (uint)GetTransformType(chunk.Transform),
	// 			};
	//
	// 			entries.Add(entry);
	// 			nextStartIdx++;
	// 		}
	// 	}
	//
	// 	Entries = entries;
	// }
	//
	// private NefsDataTransformType GetTransformType(NefsDataTransform transform)
	// {
	// 	// Can have both aes and zlib simulatneously?
	// 	if (transform.IsAesEncrypted && transform.IsZlibCompressed)
	// 	{
	// 		Log.LogWarning("Found multiple data transforms for header part 4 entry.");
	// 	}
	//
	// 	if (transform.IsAesEncrypted)
	// 	{
	// 		return NefsDataTransformType.Aes;
	// 	}
	// 	else if (transform.IsZlibCompressed)
	// 	{
	// 		return NefsDataTransformType.Zlib;
	// 	}
	// 	else if (transform.IsLzssCompressed)
	// 	{
	// 		return NefsDataTransformType.Lzss;
	// 	}
	//
	// 	return NefsDataTransformType.None;
	// }
}
