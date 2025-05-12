// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// Header introduction. Contains size, encryption, and verification info.
/// </summary>
public record Nefs150HeaderIntro : INefsHeaderIntro, INefsHeaderIntroToc
{
	private readonly Nefs150TocHeader data;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderIntro"/> class.
	/// </summary>
	public Nefs150HeaderIntro(Nefs150TocHeader data)
	{
		this.data = data;
	}

	/// <inheritdoc />
	public bool IsLittleEndian { get; init; }

	/// <inheritdoc />
	public bool IsEncrypted { get; init; }

	/// <inheritdoc />
	public bool IsXorEncoded { get; init; }

	/// <inheritdoc />
	public uint MagicNumber
	{
		get => this.data.Magic;
		init => this.data.Magic = value;
	}

	/// <inheritdoc />
	public uint HeaderSize
	{
		get => this.data.TocSize;
		init => this.data.TocSize = value;
	}

	/// <inheritdoc />
	public uint NefsVersion
	{
		get => this.data.Version;
		init => this.data.Version = value;
	}

	/// <summary>
	/// The number of volumes.
	/// </summary>
	public uint NumVolumes
	{
		get => this.data.NumVolumes;
		init => this.data.NumVolumes = value;
	}

	/// <inheritdoc />
	public uint NumberOfItems
	{
		get => this.data.NumEntries;
		init => this.data.NumEntries = value;
	}

	/// <summary>
	/// Block size (chunk size). The size of chunks data is split up before any transforms are applied.
	/// </summary>
	public uint BlockSize
	{
		get => this.data.BlockSize;
		init => this.data.BlockSize = value;
	}

	/// <summary>
	/// The split size.
	/// </summary>
	public uint SplitSize
	{
		get => this.data.SplitSize;
		init => this.data.SplitSize = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart1
	{
		get => this.data.EntryTableStart;
		init => this.data.EntryTableStart = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart2
	{
		get => this.data.SharedEntryInfoTableStart;
		init => this.data.SharedEntryInfoTableStart = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart3
	{
		get => this.data.NameTableStart;
		init => this.data.NameTableStart = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart4
	{
		get => this.data.BlockTableStart;
		init => this.data.BlockTableStart = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart5
	{
		get => this.data.VolumeInfoTableStart;
		init => this.data.VolumeInfoTableStart = value;
	}

	/// <inheritdoc/>
	public uint Part1Size => OffsetToPart2 - OffsetToPart1;

	/// <inheritdoc/>
	public uint Part2Size => OffsetToPart3 - OffsetToPart2;

	/// <inheritdoc/>
	public uint Part3Size => OffsetToPart4 - OffsetToPart3;

	/// <inheritdoc/>
	public uint Part4Size => OffsetToPart5 - OffsetToPart4;

	/// <inheritdoc />
	public ReadOnlySpan<byte> AesKeyHexString
	{
		get => this.data.AesKeyBuffer;
		init => value.CopyTo(this.data.AesKeyBuffer);
	}

	/// <inheritdoc/>
	public uint OffsetToPart6 => throw new NotSupportedException("Part6 not supported in 1.5.1.");

	/// <inheritdoc/>
	public uint OffsetToPart7 => throw new NotSupportedException("Part7 not supported in 1.5.1.");

	/// <inheritdoc/>
	public uint OffsetToPart8 => throw new NotSupportedException("Part8 not supported in 1.5.1.");

	/// <inheritdoc />
	public byte[] GetAesKey()
	{
		var asciiKey = Encoding.ASCII.GetString(AesKeyHexString);
		return StringHelper.FromHexString(asciiKey);
	}

	/// <inheritdoc/>
	public uint ComputeNumChunks(uint extractedSize) =>
		(extractedSize + (BlockSize - 1)) / BlockSize;
}
