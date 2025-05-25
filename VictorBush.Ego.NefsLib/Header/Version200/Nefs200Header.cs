// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Source.Utility;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version200;

/// <inheritdoc cref="INefsHeader" />
public sealed class Nefs200Header : INefsHeader
{
	/// <summary>
	/// Offset to the first data item used in most archives.
	/// </summary>
	public const uint DataOffsetDefault = 0x10000U;

	/// <summary>
	/// Offset to the first data item used in large archives where header needs more room.
	/// </summary>
	public const uint DataOffsetLarge = 0x50000U;

	/// <summary>
	/// Offset to the header intro.
	/// </summary>
	public const uint IntroOffset = 0x0;

	public NefsWriterSettings WriterSettings { get; }
	public Nefs160TocHeaderA Intro { get; }
	public Nefs200TocHeaderB TableOfContents { get; }
	public Nefs160HeaderEntryTable EntryTable { get; }
	public Nefs160HeaderSharedEntryInfoTable SharedEntryInfoTable { get; }
	public NefsHeaderPart3 Part3 { get; }
	public Nefs200HeaderBlockTable BlockTable { get; }
	public NefsHeaderPart5 Part5 { get; }
	public Nefs160HeaderWriteableEntryTable WriteableEntryTable { get; }
	public Nefs160HeaderWriteableSharedEntryInfo WriteableSharedEntryInfo { get; }
	public Nefs160HeaderHashDigestTable HashDigestTable { get; }

	/// <inheritdoc />
	public bool IsEncrypted => WriterSettings.IsEncrypted;

	/// <inheritdoc />
	public byte[] AesKey => Intro.AesKey.GetAesKey();

	/// <inheritdoc />
	public Sha256Hash Hash => Intro.Hash;

	/// <inheritdoc />
	public uint Size => Intro.TocSize;

	/// <inheritdoc />
	public uint BlockSize => 0x10000;

	/// <inheritdoc />
	public uint NumEntries => Intro.NumEntries;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs200Header"/> class.
	/// </summary>
	public Nefs200Header(
		NefsWriterSettings writerSettings,
		Nefs160TocHeaderA intro,
		Nefs200TocHeaderB toc,
		Nefs160HeaderEntryTable entryTable,
		Nefs160HeaderSharedEntryInfoTable sharedEntryInfoTable,
		NefsHeaderPart3 part3,
		Nefs200HeaderBlockTable blockTable,
		NefsHeaderPart5 part5,
		Nefs160HeaderWriteableEntryTable writeableEntryTable,
		Nefs160HeaderWriteableSharedEntryInfo writeableSharedEntryInfo,
		Nefs160HeaderHashDigestTable hashDigestTable)
	{
		WriterSettings = writerSettings;
		Intro = intro;
		TableOfContents = toc;
		EntryTable = entryTable ?? throw new ArgumentNullException(nameof(entryTable));
		SharedEntryInfoTable = sharedEntryInfoTable ?? throw new ArgumentNullException(nameof(sharedEntryInfoTable));
		Part3 = part3 ?? throw new ArgumentNullException(nameof(part3));
		BlockTable = blockTable ?? throw new ArgumentNullException(nameof(blockTable));
		Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));
		WriteableEntryTable = writeableEntryTable ?? throw new ArgumentNullException(nameof(writeableEntryTable));
		WriteableSharedEntryInfo = writeableSharedEntryInfo ?? throw new ArgumentNullException(nameof(writeableSharedEntryInfo));
		HashDigestTable = hashDigestTable ?? throw new ArgumentNullException(nameof(hashDigestTable));
	}

	/// <inheritdoc/>
	public string GetFileName(uint nameOffset)
	{
		return Part3.FileNamesByOffset[nameOffset];
	}

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		if (format is null)
		{
			return ToString() ?? string.Empty;
		}

		if (format is not "DBG")
		{
			throw new ArgumentException("Format is not supported.", nameof(format));
		}

		var headerPart1String = new StringBuilder();
		foreach (var entry in EntryTable.Entries)
		{
			headerPart1String.Append($"0x{entry.Start.ToString("X", formatProvider)}".PadRight(20));
			headerPart1String.Append($"0x{entry.SharedInfo.ToString("X", formatProvider)}".PadRight(20));
			headerPart1String.Append($"0x{entry.FirstBlock.ToString("X", formatProvider)}".PadRight(20));
			headerPart1String.Append($"0x{entry.NextDuplicate.ToString("X", formatProvider)}".PadRight(20));
			headerPart1String.AppendLine();
		}

		var headerPart2String = new StringBuilder();
		foreach (var entry in SharedEntryInfoTable.Entries)
		{
			headerPart2String.Append($"0x{entry.Parent.ToString("X", formatProvider)}".PadRight(20));
			headerPart2String.Append($"0x{entry.FirstChild.ToString("X", formatProvider)}".PadRight(20));
			headerPart2String.Append($"0x{entry.NameOffset.ToString("X", formatProvider)}".PadRight(20));
			headerPart2String.Append($"0x{entry.Size.ToString("X", formatProvider)}".PadRight(20));
			headerPart2String.Append($"0x{entry.FirstDuplicate.ToString("X", formatProvider)}".PadRight(20));
			headerPart1String.AppendLine();
		}

		var headerPart3String = new StringBuilder();
		foreach (var s in Part3.FileNames)
		{
			headerPart3String.AppendLine(s);
		}

		var headerPart6String = new StringBuilder();
		foreach (var entry in WriteableEntryTable.Entries)
		{
			headerPart6String.Append($"0x{entry.Volume.ToString("X", formatProvider)}".PadRight(20));
			headerPart6String.Append($"0x{entry.Flags.ToString("X", formatProvider)}".PadRight(20));
			headerPart6String.AppendLine();
		}

		var headerPart7String = new StringBuilder();
		foreach (var entry in WriteableSharedEntryInfo.Entries)
		{
			headerPart7String.Append($"0x{entry.NextSibling.ToString("X", formatProvider)}".PadRight(20));
			headerPart7String.Append($"0x{entry.PatchedEntry.ToString("X", formatProvider)}".PadRight(20));
			headerPart7String.AppendLine();
		}

		var headerPart8String = new StringBuilder();
		foreach (var hash in HashDigestTable.Entries)
		{
			headerPart8String.Append(hash);
			headerPart8String.AppendLine();
		}

		return $"""
		        General Info
		        -----------------------------------------------------------
		        Archive Size:               {Part5.DataSize.ToString("X", formatProvider)}
		        Is Header Encrypted?        {WriterSettings.IsEncrypted}

		        Header size:                {Intro.TocSize.ToString("X", formatProvider)}

		        Header Intro
		        -----------------------------------------------------------
		        Magic Number:               {Intro.Magic.ToString("X", formatProvider)}
		        Expected SHA-256 hash:      {Intro.Hash}
		        AES 256 key hex string:     {StringHelper.ByteArrayToString(Intro.AesKey.GetAesKey())}
		        Header size:                {Intro.TocSize.ToString("X", formatProvider)}
		        NeFS version:               {Intro.Version.ToString("X", formatProvider)}
		        Number of items:            {Intro.NumEntries.ToString("X", formatProvider)}
		        User Value:                 {Intro.UserValue.ToString("X", formatProvider)}
		        Random Padding:             {Convert.ToHexString(Intro.RandomPadding)}
		        Unused:                     {Intro.Unused.ToString("X", formatProvider)}

		        Header Table of Contents
		        -----------------------------------------------------------
		        Offset to Part 1:           {TableOfContents.EntryTableStart.ToString("X", formatProvider)}
		        Offset to Part 2:           {TableOfContents.SharedEntryInfoTableStart.ToString("X", formatProvider)}
		        Offset to Part 3:           {TableOfContents.NameTableStart.ToString("X", formatProvider)}
		        Offset to Part 4:           {TableOfContents.BlockTableStart.ToString("X", formatProvider)}
		        Offset to Part 5:           {TableOfContents.VolumeInfoTableStart.ToString("X", formatProvider)}
		        Offset to Part 6:           {TableOfContents.WritableEntryTableStart.ToString("X", formatProvider)}
		        Offset to Part 7:           {TableOfContents.WritableSharedEntryInfoTableStart.ToString("X", formatProvider)}
		        Offset to Part 8:           {TableOfContents.HashDigestTableStart.ToString("X", formatProvider)}
		        Num Volumes:                {TableOfContents.NumVolumes.ToString("X", formatProvider)}
		        Hash Block Size (<< 15):    {TableOfContents.HashBlockSize.ToString("X", formatProvider)}
		        Random Padding:             {Convert.ToHexString(TableOfContents.RandomPadding)}

		        Entry Table
		        -----------------------------------------------------------
		        Data Offset        Shared Entry        First Block        Id
		        {headerPart1String}
		        Shared Entry Info Table
		        -----------------------------------------------------------
		        Directory Id        First child Id        Name offset        Extracted size        Id
		        {headerPart2String}
		        Name Table
		        -----------------------------------------------------------
		        {headerPart3String}
		        Block Table
		        -----------------------------------------------------------
		        Number of entries:          {BlockTable.Entries.Count.ToString("X", formatProvider)}

		        Volume Info Table (Count: 1)
		        -----------------------------------------------------------
		        Archive size:               {Part5.DataSize.ToString("X", formatProvider)}
		        First data offset:          {Part5.FirstDataOffset.ToString("X", formatProvider)}
		        Archive name string offset: {Part5.DataFileNameStringOffset.ToString("X", formatProvider)}

		        Writeable Entry Table (Count: {WriteableEntryTable.Entries.Count})
		        -----------------------------------------------------------
		        {headerPart6String}
		        Writeable Shared Entry Info Table (Count: {WriteableSharedEntryInfo.Entries.Count})
		        -----------------------------------------------------------
		        {headerPart7String}
		        Hash Digest Table (Count: {HashDigestTable.Entries.Count})
		        -----------------------------------------------------------
		        {headerPart8String}
		        """;
	}
}
