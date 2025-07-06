// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <inheritdoc cref="INefsHeader" />
public sealed class NefsHeader151 : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public NefsTocHeader151 Intro { get; }
	public NefsHeaderEntryTable150 EntryTable { get; }
	public NefsHeaderSharedEntryInfoTable150 SharedEntryInfoTable { get; }
	public NefsHeaderNameTable NameTable { get; }
	public NefsHeaderBlockTable151 BlockTable { get; }
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
	public uint SplitSize => Intro.SplitSize;

	/// <inheritdoc />
	public uint NumEntries => Intro.NumEntries;

	/// <inheritdoc />
	public IReadOnlyList<VolumeInfo> Volumes { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeader151"/> class.
	/// </summary>
	/// <param name="writerSettings">Writer settings.</param>
	/// <param name="header">Header intro.</param>
	/// <param name="entryTable">Header part 1.</param>
	/// <param name="sharedEntryInfoTable">Header part 2.</param>
	/// <param name="nameTable">Header part 3.</param>
	/// <param name="blockTable">Header part 4.</param>
	/// <param name="volumeInfoTable">Header part 5.</param>
	public NefsHeader151(
		NefsWriterSettings writerSettings,
		NefsTocHeader151 header,
		NefsHeaderEntryTable150 entryTable,
		NefsHeaderSharedEntryInfoTable150 sharedEntryInfoTable,
		NefsHeaderNameTable nameTable,
		NefsHeaderBlockTable151 blockTable,
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
