// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version151;

public struct Nefs150TocEntry : INefsTocData<Nefs150TocEntry>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs150TocEntry>();

	public ulong Start;
	public ushort Volume;
	public NefsTocEntryFlags Flags;
	public uint SharedInfo;
	public uint FirstBlock;
	public uint NextDuplicate;

	public void ReverseEndianness()
	{
		this.Start = BinaryPrimitives.ReverseEndianness(this.Start);
		this.Volume = BinaryPrimitives.ReverseEndianness(this.Volume);
		this.Flags = (NefsTocEntryFlags)BinaryPrimitives.ReverseEndianness((ushort)this.Flags);
		this.SharedInfo = BinaryPrimitives.ReverseEndianness(this.SharedInfo);
		this.FirstBlock = BinaryPrimitives.ReverseEndianness(this.FirstBlock);
		this.NextDuplicate = BinaryPrimitives.ReverseEndianness(this.NextDuplicate);
	}
}
