// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Source.Utility;

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
	public NefsHeaderPart5 Part5 { get; }

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
	/// <param name="writerSettings">Writer settings.</param>
	/// <param name="header">Header intro.</param>
	/// <param name="entryTable">Header part 1.</param>
	/// <param name="sharedEntryInfoTable">Header part 2.</param>
	/// <param name="nameTable">Header part 3.</param>
	/// <param name="blockTable">Header part 4.</param>
	/// <param name="part5">Header part 5.</param>
	public NefsHeader151(
		NefsWriterSettings writerSettings,
		NefsTocHeader151 header,
		NefsHeaderEntryTable150 entryTable,
		NefsHeaderSharedEntryInfoTable150 sharedEntryInfoTable,
		NefsHeaderNameTable nameTable,
		NefsHeaderBlockTable151 blockTable,
		NefsHeaderPart5 part5)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable ?? throw new ArgumentNullException(nameof(entryTable));
		SharedEntryInfoTable = sharedEntryInfoTable ?? throw new ArgumentNullException(nameof(sharedEntryInfoTable));
		NameTable = nameTable ?? throw new ArgumentNullException(nameof(nameTable));
		BlockTable = blockTable ?? throw new ArgumentNullException(nameof(blockTable));
		Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));

		Volumes =
		[
			new VolumeInfo
			{
				Name = NameTable.FileNamesByOffset[Part5.DataFileNameStringOffset],
				DataOffset = Part5.FirstDataOffset,
				Size = Part5.DataSize
			}
		];
	}

	/// <inheritdoc />
	public string GetFileName(uint nameOffset)
	{
		return NameTable.FileNamesByOffset[nameOffset];
	}
}
