// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Source.Utility;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header introduction. Contains size, encryption, and verification info.
/// </summary>
public record NefsHeaderIntro
{
	/// <summary>
	/// Expected first four bytes of a NeFS archive.
	/// </summary>
	public const uint NefsMagicNumber = 0x5346654E;

	/// <summary>
	/// The size of this section.
	/// </summary>
	public const int Size = 0x80;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderIntro"/> class.
	/// </summary>
	public NefsHeaderIntro(bool isEncrypted = false)
	{
		Data0x00_MagicNumber.Value = NefsMagicNumber;
		IsEncrypted = isEncrypted;
	}

	/// <summary>
	/// 256-bit AES key stored as a hex string.
	/// </summary>
	public byte[] AesKeyHexString
	{
		get => Data0x24_AesKeyHexString.Value;
		init => Data0x24_AesKeyHexString.Value = value;
	}

	/// <summary>
	/// Expected hash of header.
	/// </summary>
	public Sha256Hash ExpectedHash
	{
		get => new Sha256Hash(Data0x04_ExpectedHash.Value);
		init => Data0x04_ExpectedHash.Value = value.Value;
	}

	/// <summary>
	/// Size of header in bytes.
	/// </summary>
	public uint HeaderSize
	{
		get => Data0x64_HeaderSize.Value;
		init => Data0x64_HeaderSize.Value = value;
	}

	/// <summary>
	/// Whether the header is encrypted.
	/// </summary>
	public bool IsEncrypted { get; init; } = false;

	/// <summary>
	/// File magic number; "NeFS" or 0x5346654E.
	/// </summary>
	public uint MagicNumber
	{
		get => Data0x00_MagicNumber.Value;
		init => Data0x00_MagicNumber.Value = value;
	}

	/// <summary>
	/// The NeFS format version.
	/// </summary>
	public uint NefsVersion
	{
		get => Data0x68_NefsVersion.Value;
		init => Data0x68_NefsVersion.Value = value;
	}

	/// <summary>
	/// The number of items in the archive.
	/// </summary>
	public uint NumberOfItems
	{
		get => Data0x6c_NumberOfItems.Value;
		init => Data0x6c_NumberOfItems.Value = value;
	}

	/// <summary>
	/// 8 bytes; Another constant; the last four bytes are "zlib" in ASCII.
	/// </summary>
	public ulong Unknown0x70zlib
	{
		get => Data0x70_UnknownZlib.Value;
		init => Data0x70_UnknownZlib.Value = value;
	}

	/// <summary>
	/// Unknown value.
	/// </summary>
	public ulong Unknown0x78
	{
		get => Data0x78_Unknown.Value;
		init => Data0x78_Unknown.Value = value;
	}

	[FileData]
	private UInt32Type Data0x00_MagicNumber { get; } = new UInt32Type(0x0000);

	[FileData]
	internal ByteArrayType Data0x04_ExpectedHash { get; } = new ByteArrayType(0x0004, 0x20);

	[FileData]
	private ByteArrayType Data0x24_AesKeyHexString { get; } = new ByteArrayType(0x0024, 0x40);

	[FileData]
	private UInt32Type Data0x64_HeaderSize { get; } = new UInt32Type(0x0064);

	[FileData]
	private UInt32Type Data0x68_NefsVersion { get; } = new UInt32Type(0x0068);

	[FileData]
	private UInt32Type Data0x6c_NumberOfItems { get; } = new UInt32Type(0x006c);

	[FileData]
	private UInt64Type Data0x70_UnknownZlib { get; } = new UInt64Type(0x0070);

	[FileData]
	private UInt64Type Data0x78_Unknown { get; } = new UInt64Type(0x0078);

	/// <summary>
	/// Gets the AES-256 key for this header.
	/// </summary>
	/// <returns>A byte array with the AES key.</returns>
	public byte[] GetAesKey()
	{
		var asciiKey = Encoding.ASCII.GetString(AesKeyHexString);
		return StringHelper.FromHexString(asciiKey);
	}
}
