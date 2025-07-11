// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Utilities for dealing with PE executables.
/// </summary>
internal class PeHelper
{
	/// <summary>
	/// Expected DOS header signature.
	/// </summary>
	public const uint DosHeaderSignature = 0x5a4d;

	/// <summary>
	/// Expected PE header signature.
	/// </summary>
	public const uint PeHeaderSignature = 0x4550;

	/// <summary>
	/// Offset to field that specifies number of sections.
	/// </summary>
	public const uint PeNumberOfSectionsOffset = 6;

	/// <summary>
	/// Offset to value that specifies the start of the PE header.
	/// </summary>
	public const uint PeOffsetOffset = 0x3c;

	/// <summary>
	/// Offset to start of optional header in PE.
	/// </summary>
	public const uint PeOptionalHeaderOffset = 0x18;

	/// <summary>
	/// Offset in a PE section table to the value that specifies the offset to raw data.
	/// </summary>
	public const uint PeSectionRawDataOffset = 20;
	private const uint PeSectionRawDataSizeOffset = 16;

	/// <summary>
	/// Size of a PE section table.
	/// </summary>
	public const uint PeSectionSize = 40;

	/// <summary>
	/// Offset to value in PE header that specifies size of optional header.
	/// </summary>
	public const uint PeSizeOfOptionalHeaderOffset = 0x14;

	/// <summary>
	/// Identify whether the stream is the expected file type.
	/// </summary>
	/// <returns>True if the stream is the expected file type.</returns>
	public static bool Identify(Stream stream)
	{
		Span<byte> buffer = stackalloc byte[2];
		stream.ReadExactly(buffer);
		var identifier = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
		stream.Seek(-2, SeekOrigin.Current);
		return identifier == DosHeaderSignature;
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
		// Verify DOS header stub
		if (!Identify(stream))
		{
			throw new ArgumentException("Invalid DOS header signature.");
		}

		// Get PE header offset
		using var br = new EndianBinaryReader(stream, true);
		var peOffset = ReadUInt32(br, (int)PeOffsetOffset);

		// Verify PE signature
		if (ReadUInt32(br, peOffset) != PeHeaderSignature)
		{
			throw new ArgumentException("Invalid PE header signature.");
		}

		// Get optional header size
		var optionalHeaderSize = ReadUInt16(br, peOffset + PeSizeOfOptionalHeaderOffset);

		// Get offset to section table
		var sectionTableOffset = peOffset + PeOptionalHeaderOffset + optionalHeaderSize;

		// Get number of sections
		var numSections = ReadUInt16(br, peOffset + PeNumberOfSectionsOffset);

		// Search for the section name
		Span<byte> nameBuffer = stackalloc byte[8];
		for (var i = 0; i < numSections; ++i)
		{
			var sectionHeaderOffset = sectionTableOffset + (i * PeSectionSize);

			// Check section name
			br.BaseStream.Seek(sectionHeaderOffset, SeekOrigin.Begin);
			br.BaseStream.ReadExactly(nameBuffer);
			var thisSectionName = StringHelper.TryReadNullTerminatedAscii(nameBuffer);
			if (thisSectionName != sectionName)
			{
				continue;
			}

			// Get section data info
			var sectionOffset = ReadUInt32(br, sectionHeaderOffset + PeSectionRawDataOffset);
			var sectionSize = ReadUInt32(br, sectionHeaderOffset + PeSectionRawDataSizeOffset);
			return (sectionOffset, sectionSize);
		}

		// Didn't find it
		return null;
	}

	private static ushort ReadUInt16(EndianBinaryReader reader, long offset)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		return reader.ReadUInt16();
	}

	private static uint ReadUInt32(EndianBinaryReader reader, long offset)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		return reader.ReadUInt32();
	}
}
