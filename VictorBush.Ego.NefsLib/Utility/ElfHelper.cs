// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Utilities for dealing with ELF executables.
/// </summary>
internal class ElfHelper
{
	// \x7FELF
	private static ReadOnlySpan<byte> FourCc => [0x7F, 0x45, 0x4C, 0x46];
	private const ulong SectionOffsetOffset32 = 16;
	private const ulong SectionOffsetOffset64 = 24;
	private const ulong SectionSizeOffset32 = 20;
	private const ulong SectionSizeOffset64 = 32;

	/// <summary>
	/// Identify whether the stream is the expected file type.
	/// </summary>
	/// <returns>True if the stream is the expected file type.</returns>
	public static bool Identify(Stream stream)
	{
		Span<byte> buffer = stackalloc byte[4];
		stream.ReadExactly(buffer);
		stream.Seek(-4, SeekOrigin.Current);
		return buffer.SequenceEqual(FourCc);
	}

	/// <summary>
	/// Gets the offset and size of the section data with the given name.
	/// </summary>
	/// <param name="stream">The input stream.</param>
	/// <param name="sectionName">The section name to get.</param>
	/// <returns>The section data offset and size, or null if it was not found.</returns>
	public static (ulong Position, ulong Size)? GetSectionDataInfo(
		Stream stream,
		string sectionName)
	{
		if (!Identify(stream))
		{
			throw new ArgumentException("Invalid ELF identifier.");
		}

		stream.Seek(4, SeekOrigin.Current);
		var elfClass = stream.ReadByte();
		var bit32 = elfClass == 1;

		var elfDataType = stream.ReadByte();
		var isLittleEndian = elfDataType == 1;

		// Setup endian binary reader
		using var br = new EndianBinaryReader(stream, isLittleEndian);

		// Read header data
		var sectionHeaderTableOffset = ReadElfOffset(br, bit32 ? 0x20u : 0x28u, bit32);
		var sectionHeaderTableEntrySize = ReadElfHalf(br, bit32 ? 0x2Eu : 0x3Au);
		var sectionHeaderTableEntryCount = ReadElfHalf(br, bit32 ? 0x30u : 0x3Cu);
		var sectionHeaderStringTableIndex = ReadElfHalf(br, bit32 ? 0x32u : 0x3Eu);

		// Find the header name location
		var sectionHeaderStringTableOffset =
			sectionHeaderTableOffset + (uint)sectionHeaderTableEntrySize * sectionHeaderStringTableIndex;
		var sectionHeaderStringTableDataOffset = ReadElfOffset(br,
			bit32
				? sectionHeaderStringTableOffset + SectionOffsetOffset32
				: sectionHeaderStringTableOffset + SectionOffsetOffset64, bit32);

		// Search for the section name in section header table
		for (var i = 0u; i < sectionHeaderTableEntryCount; ++i)
		{
			var sectionHeaderOffset = sectionHeaderTableOffset + i * sectionHeaderTableEntrySize;

			// Check section name
			var sectionNameOffset = ReadElfWord(br, sectionHeaderOffset);
			br.BaseStream.Seek((long)sectionHeaderStringTableDataOffset + sectionNameOffset, SeekOrigin.Begin);
			var thisSectionName = br.ReadNullTerminatedAscii();
			if (thisSectionName != sectionName)
			{
				continue;
			}

			// Get section data info
			var sectionOffset = ReadElfOffset(br,
				bit32 ? sectionHeaderOffset + SectionOffsetOffset32 : sectionHeaderOffset + SectionOffsetOffset64,
				bit32);
			var sectionSize = ReadElfXWord(br,
				bit32 ? sectionHeaderOffset + SectionSizeOffset32 : sectionHeaderOffset + SectionSizeOffset64, bit32);
			return (sectionOffset, sectionSize);
		}

		// Didn't find it
		return null;
	}

	private static ushort ReadElfHalf(EndianBinaryReader reader, ulong offset)
	{
		// same between 32-bit and 64-bit
		reader.BaseStream.Seek(Convert.ToInt64(offset), SeekOrigin.Begin);
		return reader.ReadUInt16();
	}

	private static uint ReadElfWord(EndianBinaryReader reader, ulong offset)
	{
		// same between 32-bit and 64-bit
		reader.BaseStream.Seek(Convert.ToInt64(offset), SeekOrigin.Begin);
		return reader.ReadUInt32();
	}

	private static ulong ReadElfXWord(EndianBinaryReader reader, ulong offset, bool bit32)
	{
		reader.BaseStream.Seek(Convert.ToInt64(offset), SeekOrigin.Begin);
		return bit32 ? reader.ReadUInt32() : reader.ReadUInt64();
	}

	private static ulong ReadElfOffset(EndianBinaryReader reader, ulong offset, bool bit32)
	{
		return ReadElfXWord(reader, offset, bit32);
	}
}
