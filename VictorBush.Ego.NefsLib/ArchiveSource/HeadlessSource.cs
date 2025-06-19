// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource;

/// <summary>
/// Header and file data are stored in separate files. Usually the header is split into two sections, a primary section
/// (header parts 1-5, 8) and a secondary section (parts 6-7).
/// </summary>
public sealed class HeadlessSource : NefsArchiveSource
{
	internal HeadlessSource(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize)
		: base(dataFilePath)
	{
		HeaderFilePath = headerFilePath ?? throw new ArgumentNullException(nameof(headerFilePath));
		PrimaryOffset = primaryOffset;
		PrimarySize = primarySize;
		SecondaryOffset = secondaryOffset;
		SecondarySize = secondarySize;
	}

	/// <summary>
	/// Path to the file containing the file data (i.e., game.dat, game.bin, etc.).
	/// </summary>
	public string DataFilePath => FilePath;

	/// <summary>
	/// Path to the file containing the archive header (i.e., the game executable).
	/// </summary>
	public string HeaderFilePath { get; }

	/// <summary>
	/// Offset to the beginning of the header from the beginning of the file specified by <see cref="HeaderFilePath"/>.
	/// </summary>
	public long PrimaryOffset { get; }

	/// <summary>
	/// The size of the first chunk of header data. This includes the intro, table of contents, parts 1-5, and part 8.
	/// This size is not the same value as the total header size as stored within the header itself. Providing this
	/// value is optional. If not provided, the size of header part 8 will be deduced based on the total size of file
	/// data. The extra data from part 8 will not be loaded.
	/// </summary>
	public int? PrimarySize { get; }

	/// <summary>
	/// The offset to header part 6.
	/// </summary>
	public long SecondaryOffset { get; }

	/// <summary>
	/// The size of the part 6 and 7 chunk. Part 6 and 7 are stored together separately from the rest of the header
	/// data. This is optional. If not provided, the size of parts 6 and 7 will be deduced from the other header data
	/// and any extra data at the end will be ignored.
	/// </summary>
	public int? SecondarySize { get; }
}
