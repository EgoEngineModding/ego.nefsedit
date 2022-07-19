// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header intro table of contents. Contains offsets to other header parts.
/// </summary>
public interface INefsHeaderIntroToc
{
	/// <summary>
	/// Offset the header part 1.
	/// </summary>
	uint OffsetToPart1 { get; }

	/// <summary>
	/// Offset to header part 2.
	/// </summary>
	uint OffsetToPart2 { get; }

	/// <summary>
	/// Offset to header part 3 (the filename/directory strings list).
	/// </summary>
	uint OffsetToPart3 { get; }

	/// <summary>
	/// Offset to header part 4.
	/// </summary>
	uint OffsetToPart4 { get; }

	/// <summary>
	/// Offset to header part 5.
	/// </summary>
	uint OffsetToPart5 { get; }

	/// <summary>
	/// Offset to header part 6.
	/// </summary>
	uint OffsetToPart6 { get; }

	/// <summary>
	/// Offset to header part 7.
	/// </summary>
	uint OffsetToPart7 { get; }

	/// <summary>
	/// Offset to header part 8.
	/// </summary>
	uint OffsetToPart8 { get; }

	/// <summary>
	/// The size of header part 1.
	/// </summary>
	uint Part1Size { get; }

	/// <summary>
	/// The size of header part 2.
	/// </summary>
	uint Part2Size { get; }

	/// <summary>
	/// The size of header part 3.
	/// </summary>
	uint Part3Size { get; }

	/// <summary>
	/// The size of header part 4.
	/// </summary>
	uint Part4Size { get; }

	/// <summary>
	/// Computes how many chunks a file will be split up into based on extracted size.
	/// </summary>
	/// <param name="extractedSize">The extracted size of the file.</param>
	/// <returns>The number of data chunks to expect.</returns>
	uint ComputeNumChunks(uint extractedSize);
}
