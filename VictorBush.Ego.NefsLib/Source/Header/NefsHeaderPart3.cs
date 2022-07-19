// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header part 3.
/// </summary>
public sealed class NefsHeaderPart3
{
	private readonly SortedDictionary<uint, string> fileNamesByOffset = new();

	private readonly Dictionary<string, uint> offsetsByFileName = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart3"/> class.
	/// </summary>
	/// <param name="entries">A unique list of strings.</param>
	internal NefsHeaderPart3(IEnumerable<string> entries)
	{
		Init(entries);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart3"/> class.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	internal NefsHeaderPart3(NefsItemList items)
	{
		// Add the archive file name to the list and sort strings alphabetically
		var strings = items.EnumerateById().Select(i => i.FileName)
			.Append(items.DataFileName)
			.Distinct()
			.OrderBy(i => i);

		Init(strings);
	}

	/// <summary>
	/// Gets the list of file names sorted in correct order.
	/// </summary>
	public IEnumerable<string> FileNames => this.fileNamesByOffset.Values;

	/// <summary>
	/// The dictionary of strings in the strings table, keyed by offset. The key is the offset to the string relative to
	/// the beginning of header part 3. The value is the string from the table.
	/// </summary>
	public IReadOnlyDictionary<uint, string> FileNamesByOffset => this.fileNamesByOffset;

	/// <summary>
	/// The dictionary of strings in the strings table, keyed by string. The key is the string from the table. The value
	/// is the offset to the string relative to the beginning of header part 3.
	/// </summary>
	public IReadOnlyDictionary<string, uint> OffsetsByFileName => this.offsetsByFileName;

	/// <summary>
	/// The current size of header part 3.
	/// </summary>
	public int Size { get; private set; }

	/// <summary>
	/// Rebuilds the string table from a list of strings. The strings must be unique.
	/// </summary>
	/// <param name="strings">A unique list of strings.</param>
	private void Init(IEnumerable<string> strings)
	{
		var offset = 0;
		foreach (var s in strings)
		{
			this.fileNamesByOffset.Add((uint)offset, s);
			this.offsetsByFileName.Add(s, (uint)offset);

			// Increase offset by string length plus a null terminator
			offset += s.Length + 1;
		}

		// Update header size
		Size = offset;
	}
}
