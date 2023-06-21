// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Source.Utility;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// Header introduction. Contains size, encryption, and verification info.
/// </summary>
public record Nefs151HeaderIntro : INefsHeaderIntro, INefsHeaderIntroToc
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderIntro"/> class.
	/// </summary>
	public Nefs151HeaderIntro(bool isEncrypted = false)
	{
		Data_MagicNumber.Value = NefsHeaderIntro.NefsMagicNumber;
		IsEncrypted = isEncrypted;
	}

	/// <summary>
	/// Whether the header is encrypted.
	/// </summary>
	public bool IsEncrypted { get; init; }

	/// <inheritdoc />
	public uint MagicNumber
	{
		get => Data_MagicNumber.Value;
		init => Data_MagicNumber.Value = value;
	}

	/// <inheritdoc />
	public uint HeaderSize
	{
		get => Data_HeaderSize.Value;
		init => Data_HeaderSize.Value = value;
	}

	/// <inheritdoc />
	public uint NefsVersion
	{
		get => Data_NefsVersion.Value;
		init => Data_NefsVersion.Value = value;
	}

	/// <summary>
	/// Unknown value.
	/// </summary>
	public uint Unknown0x0C
	{
		get => Data0x0C_Unknown.Value;
		init => Data0x0C_Unknown.Value = value;
	}

	/// <inheritdoc />
	public uint NumberOfItems
	{
		get => Data_NumberOfItems.Value;
		init => Data_NumberOfItems.Value = value;
	}

	/// <summary>
	/// Block size (chunk size). The size of chunks data is split up before any transforms are applied.
	/// </summary>
	public uint BlockSize
	{
		get => Data_BlockSize.Value;
		init => Data_BlockSize.Value = value;
	}

	/// <summary>
	/// Unknown value.
	/// </summary>
	public uint Unknown0x18
	{
		get => Data0x18_Unknown.Value;
		init => Data0x18_Unknown.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart1
	{
		get => Data_OffsetToPart1.Value;
		init => Data_OffsetToPart1.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart2
	{
		get => Data_OffsetToPart2.Value;
		init => Data_OffsetToPart2.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart3
	{
		get => Data_OffsetToPart3.Value;
		init => Data_OffsetToPart3.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart4
	{
		get => Data_OffsetToPart4.Value;
		init => Data_OffsetToPart4.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart5
	{
		get => Data_OffsetToPart5.Value;
		init => Data_OffsetToPart5.Value = value;
	}

	/// <inheritdoc/>
	public uint Part1Size => OffsetToPart2 - OffsetToPart1;

	/// <inheritdoc/>
	public uint Part2Size => OffsetToPart3 - OffsetToPart2;

	/// <inheritdoc/>
	public uint Part3Size => OffsetToPart4 - OffsetToPart3;

	/// <inheritdoc/>
	public uint Part4Size => OffsetToPart5 - OffsetToPart4;

	/// <summary>
	/// Unknown value.
	/// </summary>
	public uint Unknown0x30
	{
		get => Data0x30_Unknown.Value;
		init => Data0x30_Unknown.Value = value;
	}

	/// <summary>
	/// Unknown value.
	/// </summary>
	public uint Unknown0x34
	{
		get => Data0x34_Unknown.Value;
		init => Data0x34_Unknown.Value = value;
	}

	/// <summary>
	/// Unknown value.
	/// </summary>
	public uint Unknown0x38
	{
		get => Data0x38_Unknown.Value;
		init => Data0x38_Unknown.Value = value;
	}

	/// <inheritdoc />
	public byte[] AesKeyHexString
	{
		get => Data_AesKeyHexString.Value;
		init => Data_AesKeyHexString.Value = value;
	}

	/// <summary>
	/// Unknown value.
	/// </summary>
	public uint Unknown0x7C
	{
		get => Data0x7C_Unknown.Value;
		init => Data0x7C_Unknown.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart6 => throw new NotSupportedException("Part6 not supported in 1.5.1.");

	/// <inheritdoc/>
	public uint OffsetToPart7 => throw new NotSupportedException("Part7 not supported in 1.5.1.");

	/// <inheritdoc/>
	public uint OffsetToPart8 => throw new NotSupportedException("Part8 not supported in 1.5.1.");

	[FileData]
	private UInt32Type Data_MagicNumber { get; } = new(0x0000);

	[FileData]
	private UInt32Type Data_HeaderSize { get; } = new(0x0004);

	[FileData]
	private UInt32Type Data_NefsVersion { get; } = new(0x0008);

	[FileData]
	private UInt32Type Data0x0C_Unknown { get; } = new(0x000C);

	[FileData]
	private UInt32Type Data_NumberOfItems { get; } = new(0x0010);

	[FileData]
	private UInt32Type Data_BlockSize { get; } = new(0x0014);

	[FileData]
	private UInt32Type Data0x18_Unknown { get; } = new(0x0018);

	[FileData]
	private UInt32Type Data_OffsetToPart1 { get; } = new(0x001C);

	[FileData]
	private UInt32Type Data_OffsetToPart2 { get; } = new(0x0020);

	[FileData]
	private UInt32Type Data_OffsetToPart3 { get; } = new(0x0024);

	[FileData]
	private UInt32Type Data_OffsetToPart4 { get; } = new(0x0028);

	[FileData]
	private UInt32Type Data_OffsetToPart5 { get; } = new(0x002C);

	[FileData]
	private UInt32Type Data0x30_Unknown { get; } = new(0x0030);

	[FileData]
	private UInt32Type Data0x34_Unknown { get; } = new(0x0034);

	[FileData]
	private UInt32Type Data0x38_Unknown { get; } = new(0x0038);

	[FileData]
	private ByteArrayType Data_AesKeyHexString { get; } = new(0x003C, 0x40);

	[FileData]
	private UInt32Type Data0x7C_Unknown { get; } = new(0x007C);

	/// <inheritdoc />
	public byte[] GetAesKey()
	{
		var asciiKey = Encoding.ASCII.GetString(AesKeyHexString);
		return StringHelper.FromHexString(asciiKey);
	}

	/// <inheritdoc/>
	public uint ComputeNumChunks(uint extractedSize) =>
		(uint)Math.Ceiling(extractedSize / (double)BlockSize);
}
