// See LICENSE.txt for license information.

using System.Buffers.Binary;
using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Utilities for dealing with Mach-O executables.
/// </summary>
internal class MachOHelper
{
	private const uint MachO32 = 0xFEEDFACE;
	private const uint MachO64 = 0xFEEDFACF;
	private const int SegmentSectionNameSize = 16;
	private const uint LoadCommandTypeSegment = 0x01;
	private const uint LoadCommandTypeSegment64 = 0x19;

	/// <summary>
	/// Identify whether the stream is the expected file type.
	/// </summary>
	/// <returns>True if the stream is the expected file type.</returns>
	public static bool Identify(Stream stream)
	{
		Span<byte> buffer = stackalloc byte[4];
		stream.ReadExactly(buffer);
		var identifier = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
		stream.Seek(-4, SeekOrigin.Current);
		// No support for MACHO_FAT and MACHO_FAT_CIGAM
		return identifier is MachO32 or MachO64;
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
			throw new ArgumentException("Invalid Mach-O identifier.");
		}

		// Loop commands
		using var br = new EndianBinaryReader(stream, true);
		var magic = br.ReadUInt32();
		br.BaseStream.Seek(12, SeekOrigin.Current);
		var numCommands = br.ReadUInt32();
		br.BaseStream.Seek(magic == MachO32 ? 0x08 : 0x0C, SeekOrigin.Current);
		Span<byte> nameBuffer = stackalloc byte[SegmentSectionNameSize];
		for (var i = 0u; i < numCommands; ++i)
		{
			var commandType = br.ReadUInt32();
			var commandSize = br.ReadUInt32();
			uint numSectionsOffset;
			uint sectionSize;
			uint sectionSizeOffset;
			switch (commandType)
			{
				case LoadCommandTypeSegment:
					numSectionsOffset = 40;
					sectionSize = 68;
					sectionSizeOffset = 36;
					break;
				case LoadCommandTypeSegment64:
					numSectionsOffset = 56;
					sectionSize = 80;
					sectionSizeOffset = 40;
					break;
				default:
					br.BaseStream.Seek(commandSize - sizeof(uint) * 2, SeekOrigin.Current);
					continue;
			}

			br.BaseStream.Seek(numSectionsOffset, SeekOrigin.Current);
			var numSections = br.ReadUInt32();
			br.BaseStream.Seek(sizeof(uint), SeekOrigin.Current);

			// Search for the section name
			for (var j = 0u; j < numSections; ++j)
			{
				// Check section name
				br.BaseStream.ReadExactly(nameBuffer);
				var thisSectionName = StringHelper.TryReadNullTerminatedAscii(nameBuffer);
				if (thisSectionName != sectionName)
				{
					br.BaseStream.Seek(sectionSize - nameBuffer.Length, SeekOrigin.Current);
					continue;
				}

				// Get section data info
				br.BaseStream.Seek(sectionSizeOffset - nameBuffer.Length, SeekOrigin.Current);
				var sectionDataSize = commandType == LoadCommandTypeSegment ? br.ReadUInt32() : br.ReadUInt64();
				var sectionOffset = br.ReadUInt32();
				return (sectionOffset, sectionDataSize);
			}
		}

		// Didn't find it
		return null;
	}
}
