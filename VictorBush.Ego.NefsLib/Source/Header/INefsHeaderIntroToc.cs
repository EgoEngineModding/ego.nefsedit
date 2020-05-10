// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;

    /// <summary>
    /// Header intro table of contents. Contains offsets to other header parts.
    /// </summary>
    public interface INefsHeaderIntroToc
    {
        /// <summary>
        /// Offset the header part 1.
        /// </summary>
        UInt32 OffsetToPart1 { get; }

        /// <summary>
        /// Offset to header part 2.
        /// </summary>
        UInt32 OffsetToPart2 { get; }

        /// <summary>
        /// Offset to header part 3 (the filename/directory strings list).
        /// </summary>
        UInt32 OffsetToPart3 { get; }

        /// <summary>
        /// Offset to header part 4.
        /// </summary>
        UInt32 OffsetToPart4 { get; }

        /// <summary>
        /// Offset to header part 5.
        /// </summary>
        UInt32 OffsetToPart5 { get; }

        /// <summary>
        /// Offset to header part 6.
        /// </summary>
        UInt32 OffsetToPart6 { get; }

        /// <summary>
        /// Offset to header part 7.
        /// </summary>
        UInt32 OffsetToPart7 { get; }

        /// <summary>
        /// Offset to header part 8.
        /// </summary>
        UInt32 OffsetToPart8 { get; }

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
        UInt32 ComputeNumChunks(uint extractedSize);
    }
}
