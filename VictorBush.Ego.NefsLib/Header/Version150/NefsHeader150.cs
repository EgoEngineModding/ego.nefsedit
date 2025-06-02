// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <inheritdoc cref="INefsHeader" />
public sealed class NefsHeader150 : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public NefsTocHeader150 Intro { get; }
	public NefsHeaderEntryTable150 EntryTable { get; }
	public NefsHeaderSharedEntryInfoTable150 SharedEntryInfoTable { get; }
	public NefsHeaderPart3 Part3 { get; }
	public NefsHeaderBlockTable150 BlockTable { get; }
	public NefsHeaderPart5 Part5 { get; }

	/// <inheritdoc />
	public NefsVersion Version => (NefsVersion)Intro.Version;

	/// <inheritdoc />
	public bool IsLittleEndian => WriterSettings.IsLittleEndian;

	/// <inheritdoc />
	public bool IsEncrypted => WriterSettings.IsEncrypted;

	/// <inheritdoc />
	public byte[] AesKey => Intro.AesKeyBuffer.GetAesKey();

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
		NefsHeaderPart3 part3,
		NefsHeaderBlockTable150 blockTable,
		NefsHeaderPart5 part5)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable ?? throw new ArgumentNullException(nameof(entryTable));
		SharedEntryInfoTable = sharedEntryInfoTable ?? throw new ArgumentNullException(nameof(sharedEntryInfoTable));
		Part3 = part3 ?? throw new ArgumentNullException(nameof(part3));
		BlockTable = blockTable ?? throw new ArgumentNullException(nameof(blockTable));
		Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));

		Volumes =
		[
			new VolumeInfo
			{
				Name = Part3.FileNamesByOffset[Part5.DataFileNameStringOffset],
				DataOffset = Part5.FirstDataOffset,
				Size = Part5.DataSize
			}
		];
	}

	/// <inheritdoc />
	public string GetFileName(uint nameOffset)
	{
		return Part3.FileNamesByOffset[nameOffset];
	}
}
