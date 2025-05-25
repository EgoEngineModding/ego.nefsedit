// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header;

public sealed class NefsInjectHeader
{
	public const int ExpectedMagicNumber = 4484;
	public const int Size = 0x20;

	public NefsInjectHeader()
	{
	}

	public NefsInjectHeader(long primaryOffset, int primarySize, long secondaryOffset, int secondarySize)
	{
		Data0x00_MagicNum.Value = ExpectedMagicNumber;
		Data0x04_Version.Value = 1;
		Data0x08_PrimaryOffset.Value = (ulong)primaryOffset;
		Data0x10_PrimarySize.Value = (uint)primarySize;
		Data0x14_SecondaryOffset.Value = (ulong)secondaryOffset;
		Data0x1C_SecondarySize.Value = (uint)secondarySize;
	}

	/// <summary>
	/// The NefsInject header magic number.
	/// </summary>
	public int MagicNumber => (int)Data0x00_MagicNum.Value;

	/// <summary>
	/// Offset to the primary header section. This section contains header intro/toc, parts 1-5 and 8.
	/// </summary>
	public long PrimaryOffset => (long)Data0x08_PrimaryOffset.Value;

	/// <summary>
	/// Size of the primary header section.
	/// </summary>
	public int PrimarySize => (int)Data0x10_PrimarySize.Value;

	/// <summary>
	/// Offset to the secondary header section. This section contains header parts 6 and 7.
	/// </summary>
	public long SecondaryOffset => (long)Data0x14_SecondaryOffset.Value;

	/// <summary>
	/// Size of the secondary header section.
	/// </summary>
	public int SecondarySize => (int)Data0x1C_SecondarySize.Value;

	/// <summary>
	/// NefsInject header version.
	/// </summary>
	public int Version => (int)Data0x04_Version.Value;

	[FileData]
	private UInt32Type Data0x00_MagicNum { get; } = new UInt32Type(0x00);

	[FileData]
	private UInt32Type Data0x04_Version { get; } = new UInt32Type(0x04);

	[FileData]
	private UInt64Type Data0x08_PrimaryOffset { get; } = new UInt64Type(0x08);

	[FileData]
	private UInt32Type Data0x10_PrimarySize { get; } = new UInt32Type(0x10);

	[FileData]
	private UInt64Type Data0x14_SecondaryOffset { get; } = new UInt64Type(0x14);

	[FileData]
	private UInt32Type Data0x1C_SecondarySize { get; } = new UInt32Type(0x1C);
}
