// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header intro table of contents. Contains offsets to other header parts.
/// </summary>
public sealed class Nefs16HeaderIntroToc : INefsHeaderIntroToc
{
	/// <summary>
	/// Offset to the table of contents in the header.
	/// </summary>
	public const uint Offset = NefsHeaderIntro.Size;

	/// <summary>
	/// The size of this section.
	/// </summary>
	public const int Size = 0x80;

	/// <summary>
	/// Block size (chunk size). The size of chunks data is split up into before any transforms are applied.
	/// </summary>
	public uint BlockSize
	{
		get => (uint)Data0x04_BlockSize.Value << 15;
		init => Data0x04_BlockSize.Value = (ushort)(value >> 15);
	}

	/// <summary>
	/// Hash block size. The size of block the archive file data is split up into to be hashed and stored in <see cref="NefsHeaderPart8"/>.
	/// </summary>
	public uint HashBlockSize
	{
		get => (uint)Data0x02_HashBlockSize.Value << 15;
		init => Data0x02_HashBlockSize.Value = (ushort)(value >> 15);
	}

	/// <summary>
	/// Number of volumes.
	/// </summary>
	public ushort NumVolumes
	{
		get => Data0x00_NumVolumes.Value;
		init => Data0x00_NumVolumes.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart1
	{
		get => Data0x08_OffsetToPart1.Value;
		init => Data0x08_OffsetToPart1.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart2
	{
		get => Data0x10_OffsetToPart2.Value;
		init => Data0x10_OffsetToPart2.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart3
	{
		get => Data0x18_OffsetToPart3.Value;
		init => Data0x18_OffsetToPart3.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart4
	{
		get => Data0x1c_OffsetToPart4.Value;
		init => Data0x1c_OffsetToPart4.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart5
	{
		get => Data0x20_OffsetToPart5.Value;
		init => Data0x20_OffsetToPart5.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart6
	{
		get => Data0x0c_OffsetToPart6.Value;
		init => Data0x0c_OffsetToPart6.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart7
	{
		get => Data0x14_OffsetToPart7.Value;
		init => Data0x14_OffsetToPart7.Value = value;
	}

	/// <inheritdoc/>
	public uint OffsetToPart8
	{
		get => Data0x24_OffsetToPart8.Value;
		init => Data0x24_OffsetToPart8.Value = value;
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
	/// Split size. Usage unknown.
	/// </summary>
	public uint SplitSize
	{
		get => (uint)Data0x06_SplitSize.Value << 15;
		init => Data0x06_SplitSize.Value = (ushort)(value >> 15);
	}

	/// <summary>
	/// Unknown chunk of data.
	/// </summary>
	public byte[] Unknown0x28
	{
		get => Data0x28_Unknown.Value;
		init => Data0x28_Unknown.Value = value;
	}

	[FileData]
	private UInt16Type Data0x00_NumVolumes { get; } = new UInt16Type(0x0000);

	[FileData]
	private UInt16Type Data0x02_HashBlockSize { get; } = new UInt16Type(0x0002);

	[FileData]
	private UInt16Type Data0x04_BlockSize { get; } = new UInt16Type(0x0004);

	[FileData]
	private UInt16Type Data0x06_SplitSize { get; } = new UInt16Type(0x0006);

	[FileData]
	private UInt32Type Data0x08_OffsetToPart1 { get; } = new UInt32Type(0x0008);

	[FileData]
	private UInt32Type Data0x0c_OffsetToPart6 { get; } = new UInt32Type(0x000c);

	[FileData]
	private UInt32Type Data0x10_OffsetToPart2 { get; } = new UInt32Type(0x0010);

	[FileData]
	private UInt32Type Data0x14_OffsetToPart7 { get; } = new UInt32Type(0x0014);

	[FileData]
	private UInt32Type Data0x18_OffsetToPart3 { get; } = new UInt32Type(0x0018);

	[FileData]
	private UInt32Type Data0x1c_OffsetToPart4 { get; } = new UInt32Type(0x001c);

	[FileData]
	private UInt32Type Data0x20_OffsetToPart5 { get; } = new UInt32Type(0x0020);

	[FileData]
	private UInt32Type Data0x24_OffsetToPart8 { get; } = new UInt32Type(0x0024);

	[FileData]
	private ByteArrayType Data0x28_Unknown { get; } = new ByteArrayType(0x0028, 0x58);

	/// <inheritdoc/>
	public uint ComputeNumChunks(uint extractedSize) =>
		(uint)Math.Ceiling(extractedSize / (double)BlockSize);
}
