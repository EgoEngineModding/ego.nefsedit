// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version020;

/// <inheritdoc cref="INefsHeader" />
public sealed class NefsHeader020 : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public NefsTocHeader020 Intro { get; }
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
	public byte[] AesKey => Intro.AesKey.GetAesKey();

	/// <inheritdoc />
	public Sha256Hash Hash => new();

	/// <inheritdoc />
	public uint Size => Intro.TocSize;

	/// <inheritdoc />
	public uint BlockSize => Intro.BlockSize;

	/// <inheritdoc />
	public uint NumEntries => (uint)EntryTable.Entries.Count;

	/// <inheritdoc />
	public IReadOnlyList<VolumeInfo> Volumes { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeader020"/> class.
	/// </summary>
	public NefsHeader020(
		NefsWriterSettings writerSettings,
		NefsTocHeader020 header,
		NefsHeaderEntryTable010 entryTable,
		NefsHeaderLinkTable010 sharedEntryInfoTable,
		NefsHeaderNameTable nameTable,
		NefsHeaderBlockTable010 blockTable,
		NefsHeaderVolumeSizeTable010 volumeInfoTable)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable;
		LinkTable = sharedEntryInfoTable;
		NameTable = nameTable;
		BlockTable = blockTable;
		VolumeSizeTable = volumeInfoTable;

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
