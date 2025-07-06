// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version130;

/// <inheritdoc cref="INefsHeader" />
public sealed class NefsHeader130 : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public NefsTocHeader130 Intro { get; }
	public NefsHeaderEntryTable010 EntryTable { get; }
	public NefsHeaderLinkTable010 LinkTable { get; }
	public NefsHeaderNameTable NameTable { get; }
	public NefsHeaderBlockTable010 BlockTable { get; }
	public NefsHeaderVolumeSizeTable010 VolumeSizeTable { get; }
	public NefsHeaderVolumeNameStartTable130 VolumeNameStartTable { get; }
	public NefsHeaderNameTable VolumeNameTable { get; }

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
	public uint SplitSize => Intro.SplitSize;

	/// <inheritdoc />
	public uint NumEntries => (uint)EntryTable.Entries.Count;

	/// <inheritdoc />
	public IReadOnlyList<VolumeInfo> Volumes { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeader130"/> class.
	/// </summary>
	public NefsHeader130(
		NefsWriterSettings writerSettings,
		NefsTocHeader130 header,
		NefsHeaderEntryTable010 entryTable,
		NefsHeaderLinkTable010 sharedEntryInfoTable,
		NefsHeaderNameTable nameTable,
		NefsHeaderBlockTable010 blockTable,
		NefsHeaderVolumeSizeTable010 volumeSizeTable,
		NefsHeaderVolumeNameStartTable130 volumeNameStartTable,
		NefsHeaderNameTable volumeNameTable)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable;
		LinkTable = sharedEntryInfoTable;
		NameTable = nameTable;
		BlockTable = blockTable;
		VolumeSizeTable = volumeSizeTable;
		VolumeNameStartTable = volumeNameStartTable;
		VolumeNameTable = volumeNameTable;

		var volumes = new VolumeInfo[Intro.NumVolumes];
		Volumes = volumes;
		for (var i = 0; i < volumes.Length; ++i)
		{
			// Perhaps a bug in CM's code?
			var nameStart = volumeNameStartTable.Entries[i].Start;
			if (!VolumeNameTable.FileNamesByOffset.TryGetValue(nameStart, out var name))
			{
				name = VolumeNameTable.FileNamesByOffset[nameStart - 1];
			}

			volumes[i] = new VolumeInfo
			{
				Size = VolumeSizeTable.Entries[i].Size,
				Name = name,
				DataOffset = i == 0 ? Intro.TocSize : 0
			};
		}
	}

	/// <inheritdoc />
	public string GetFileName(uint nameOffset)
	{
		return NameTable.FileNamesByOffset[nameOffset];
	}
}
