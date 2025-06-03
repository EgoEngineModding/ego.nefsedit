// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header.Version160;

public struct NefsTocHeaderA160 : INefsTocData<NefsTocHeaderA160>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocHeaderA160>();

	public uint Magic = NefsConstants.FourCc;
	public Sha256Hash Hash;
	public AesKeyHexBuffer AesKey;
	public uint TocSize;
	public uint Version;
	public uint NumEntries;
	public uint UserValue;
	public uint RandomPadding1;
	public uint RandomPadding2;
	public ushort RandomPadding3;
	public ushort Unused;

	public unsafe byte[] RandomPadding
	{
		get
		{
			var buffer = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref this), ByteCount);
			return buffer[116..ByteCount].ToArray();
		}
		set
		{
			var buffer = new Span<byte>(Unsafe.AsPointer(ref this), ByteCount);
			value.CopyTo(buffer[116..ByteCount]);
		}
	}

	public NefsTocHeaderA160()
	{
	}

	public void ReverseEndianness()
	{
		this.Magic = BinaryPrimitives.ReverseEndianness(this.Magic);
		this.TocSize = BinaryPrimitives.ReverseEndianness(this.TocSize);
		this.Version = BinaryPrimitives.ReverseEndianness(this.Version);
		this.NumEntries = BinaryPrimitives.ReverseEndianness(this.NumEntries);
		this.UserValue = BinaryPrimitives.ReverseEndianness(this.UserValue);
	}
}
