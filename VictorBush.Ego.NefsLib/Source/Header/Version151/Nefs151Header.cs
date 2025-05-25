// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <inheritdoc cref="INefsHeader" />
public sealed class Nefs151Header : INefsHeader
{
	public NefsWriterSettings WriterSettings { get; }
	public Nefs151TocHeader Intro { get; }
	public Nefs150HeaderEntryTable EntryTable { get; }
	public Nefs150HeaderSharedEntryInfoTable SharedEntryInfoTable { get; }
	public NefsHeaderPart3 Part3 { get; }
	public Nefs151HeaderBlockTable BlockTable { get; }
	public NefsHeaderPart5 Part5 { get; }

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

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151Header"/> class.
	/// </summary>
	/// <param name="writerSettings">Writer settings.</param>
	/// <param name="header">Header intro.</param>
	/// <param name="entryTable">Header part 1.</param>
	/// <param name="sharedEntryInfoTable">Header part 2.</param>
	/// <param name="part3">Header part 3.</param>
	/// <param name="blockTable">Header part 4.</param>
	/// <param name="part5">Header part 5.</param>
	public Nefs151Header(
		NefsWriterSettings writerSettings,
		Nefs151TocHeader header,
		Nefs150HeaderEntryTable entryTable,
		Nefs150HeaderSharedEntryInfoTable sharedEntryInfoTable,
		NefsHeaderPart3 part3,
		Nefs151HeaderBlockTable blockTable,
		NefsHeaderPart5 part5)
	{
		WriterSettings = writerSettings;
		Intro = header;
		EntryTable = entryTable ?? throw new ArgumentNullException(nameof(entryTable));
		SharedEntryInfoTable = sharedEntryInfoTable ?? throw new ArgumentNullException(nameof(sharedEntryInfoTable));
		Part3 = part3 ?? throw new ArgumentNullException(nameof(part3));
		BlockTable = blockTable ?? throw new ArgumentNullException(nameof(blockTable));
		Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));
	}

	/// <inheritdoc />
	public string GetFileName(uint nameOffset)
	{
		return Part3.FileNamesByOffset[nameOffset];
	}
}
