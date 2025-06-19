// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <inheritdoc cref="INefsHeader" />
public sealed class NefsHeader150 : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public NefsTocHeader150 Intro { get; }
	public NefsHeaderEntryTable150 EntryTable { get; }
	public NefsHeaderSharedEntryInfoTable150 SharedEntryInfoTable { get; }
	public NefsHeaderNameTable NameTable { get; }
	public NefsHeaderBlockTable010 BlockTable { get; }
	public NefsHeaderVolumeInfoTable150 VolumeInfoTable { get; }

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
	public uint NumEntries => Intro.NumEntries;

	/// <inheritdoc />
	public IReadOnlyList<VolumeInfo> Volumes { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeader151"/> class.
	/// </summary>
	public NefsHeader150(
		NefsWriterSettings writerSettings,
		NefsTocHeader150 header,
		NefsHeaderEntryTable150 entryTable,
		NefsHeaderSharedEntryInfoTable150 sharedEntryInfoTable,
		NefsHeaderNameTable nameTable,
		NefsHeaderBlockTable010 blockTable,
		NefsHeaderVolumeInfoTable150 volumeInfoTable)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable ?? throw new ArgumentNullException(nameof(entryTable));
		SharedEntryInfoTable = sharedEntryInfoTable ?? throw new ArgumentNullException(nameof(sharedEntryInfoTable));
		NameTable = nameTable ?? throw new ArgumentNullException(nameof(nameTable));
		BlockTable = blockTable ?? throw new ArgumentNullException(nameof(blockTable));
		VolumeInfoTable = volumeInfoTable ?? throw new ArgumentNullException(nameof(volumeInfoTable));

		Volumes = VolumeInfoTable.Entries.Select(x => new VolumeInfo
		{
			Size = x.Size,
			Name = NameTable.FileNamesByOffset[x.NameOffset],
			DataOffset = x.DataOffset
		}).ToArray();
	}

	/// <inheritdoc />
	public string GetFileName(uint nameOffset)
	{
		return NameTable.FileNamesByOffset[nameOffset];
	}
}
