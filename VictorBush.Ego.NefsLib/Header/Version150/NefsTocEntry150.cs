// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct NefsTocEntry150 : INefsTocData<NefsTocEntry150>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocEntry150>();

	public ulong Start;
	public ushort Volume;
	public ushort Flags;
	public uint SharedInfo;
	public uint FirstBlock;
	public uint NextDuplicate;

	public void ReverseEndianness()
	{
		this.Start = BinaryPrimitives.ReverseEndianness(this.Start);
		this.Volume = BinaryPrimitives.ReverseEndianness(this.Volume);
		this.Flags = BinaryPrimitives.ReverseEndianness(this.Flags);
		this.SharedInfo = BinaryPrimitives.ReverseEndianness(this.SharedInfo);
		this.FirstBlock = BinaryPrimitives.ReverseEndianness(this.FirstBlock);
		this.NextDuplicate = BinaryPrimitives.ReverseEndianness(this.NextDuplicate);
	}
}
