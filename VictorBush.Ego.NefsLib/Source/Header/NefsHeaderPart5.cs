// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version151;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header part 5.
/// </summary>
public sealed class NefsHeaderPart5
{
	private readonly Nefs150TocVolumeInfo data;

	/// <summary>
	/// The size of header part 5.
	/// </summary>
	public const int Size = 0x10;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart5"/> class.
	/// </summary>
	internal NefsHeaderPart5(Nefs150TocVolumeInfo? data = null)
	{
		this.data = data ?? new Nefs150TocVolumeInfo();
		if (data is null)
		{
			FirstDataOffset = Nefs200Header.DataOffsetDefault;
		}
	}

	/// <summary>
	/// The underlying data.
	/// </summary>
	public Nefs150TocVolumeInfo Data => this.data;

	/// <summary>
	/// Offset into header part 3 for the name of the file containing the item data. For headless archives, it would be
	/// something like game.dat, game.bin, etc. For standard archives, it would be the name of the archive.
	/// </summary>
	public uint DataFileNameStringOffset
	{
		get => this.data.NameOffset;
		init => this.data.NameOffset = value;
	}

	/// <summary>
	/// The size of all item data. For headless archives, this is the size of the data file.
	/// </summary>
	public ulong DataSize
	{
		get => this.data.Size;
		init => this.data.Size = value;
	}

	/// <summary>
	/// Offset to first item data.
	/// </summary>
	public uint FirstDataOffset
	{
		get => this.data.DataOffset;
		init => this.data.DataOffset = value;
	}
}
