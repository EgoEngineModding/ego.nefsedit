// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version010;

/// <inheritdoc cref="INefsHeader" />
public sealed class NefsHeader010 : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public NefsTocHeader010 Intro { get; }
	public NefsHeaderEntryTable010 EntryTable { get; }
	public NefsHeaderLinkTable010 LinkTable { get; }
	public NefsHeaderNameTable NameTable { get; }
	public NefsHeaderBlockTable010 BlockTable { get; }
	public NefsHeaderVolumeSizeTable010 VolumeSizeTable { get; }

	/// <inheritdoc />
	public NefsVersion Version => (NefsVersion)Intro.Version;

	/// <inheritdoc />
	public bool IsLittleEndian => WriterSettings.IsLittleEndian;

	/// <inheritdoc />
	public bool IsEncrypted => WriterSettings.IsEncrypted;

	/// <inheritdoc />
	public byte[] AesKey => [];

	/// <inheritdoc />
	public Sha256Hash Hash => new();

	/// <inheritdoc />
	public uint Size => Intro.TocSize;

	/// <inheritdoc />
	public uint BlockSize => Intro.BlockSize;

	/// <inheritdoc />
	public uint SplitSize => 0;

	/// <inheritdoc />
	public uint NumEntries => (uint)EntryTable.Entries.Count;

	/// <inheritdoc />
	public IReadOnlyList<VolumeInfo> Volumes { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeader010"/> class.
	/// </summary>
	public NefsHeader010(
		NefsWriterSettings writerSettings,
		NefsTocHeader010 header,
		NefsHeaderEntryTable010 entryTable,
		NefsHeaderLinkTable010 sharedEntryInfoTable,
		NefsHeaderNameTable nameTable,
		NefsHeaderBlockTable010 blockTable,
		NefsHeaderVolumeSizeTable010 volumeSizeTable)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable;
		LinkTable = sharedEntryInfoTable;
		NameTable = nameTable;
		BlockTable = blockTable;
		VolumeSizeTable = volumeSizeTable;

		Volumes = VolumeSizeTable.Entries.Select(x => new VolumeInfo
		{
			Size = x.Size,
			Name = string.Empty,
			DataOffset = Intro.TocSize
		}).ToArray();
	}

	/// <inheritdoc />
	public string GetFileName(uint nameOffset)
	{
		return NameTable.FileNamesByOffset[nameOffset];
	}
}
