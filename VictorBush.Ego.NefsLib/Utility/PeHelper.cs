// See LICENSE.txt for license information.

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

	/// <summary>
	/// Size of a PE section table.
	/// </summary>
	public const uint PeSectionSize = 40;

	/// <summary>
	/// Offset to value in PE header that specifies size of optional header.
	/// </summary>
	public const uint PeSizeOfOptionalHeaderOffset = 0x14;

	/// <summary>
	/// Gets the offset to a PE section data in an exectuable file.
	/// </summary>
	/// <param name="exeBytes">The executable.</param>
	/// <param name="sectionName">The section name to get.</param>
	/// <param name="offset">The output address to section data.</param>
	/// <returns>Whether the offset was found successfully.</returns>
	public static bool GetRawOffsetToSection(byte[] exeBytes, string sectionName, out ulong offset)
	{
		// Verify DOS header stub
		if (BitConverter.ToUInt16(exeBytes, 0) != DosHeaderSignature)
		{
			throw new ArgumentException("Invalid DOS header signature.");
		}

		// Get PE header offset
		var peOffset = BitConverter.ToUInt32(exeBytes, (int)PeOffsetOffset);

		// Verify PE signature
		if (BitConverter.ToUInt32(exeBytes, (int)peOffset) != PeHeaderSignature)
		{
			throw new ArgumentException("Invalid PE header signature.");
		}

		// Get optional header size
		var optionalHeaderSize = BitConverter.ToUInt16(exeBytes, (int)(peOffset + PeSizeOfOptionalHeaderOffset));

		// Get offset to section table
		var sectionTableOffset = peOffset + PeOptionalHeaderOffset + optionalHeaderSize;

		// Get nubmer of sections
		var numSections = BitConverter.ToUInt16(exeBytes, (int)(peOffset + PeNumberOfSectionsOffset));

		// Search for the section name
		for (var i = 0; i < numSections; ++i)
		{
			var sectionOffset = sectionTableOffset + (i * PeSectionSize);

			// Check section name
			var thisSectionName = StringHelper.TryReadNullTerminatedAscii(exeBytes, (int)sectionOffset, 8);
			if (thisSectionName != sectionName)
			{
				continue;
			}

			// Get address
			offset = BitConverter.ToUInt32(exeBytes, (int)(sectionOffset + PeSectionRawDataOffset));
			return true;
		}

		// Didn't find it
		offset = 0;
		return false;
	}
}
