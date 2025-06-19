// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version010;

public struct NefsTocEntry010 : INefsTocData<NefsTocEntry010>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocEntry010>();

	public ulong Start;
	public ulong LinkOffset;
	public uint Size;
	public uint FirstBlock;
	public uint Volume;
	public uint Flags;

	public void ReverseEndianness()
	{
		this.Start = BinaryPrimitives.ReverseEndianness(this.Start);
		this.LinkOffset = BinaryPrimitives.ReverseEndianness(this.LinkOffset);
		this.Size = BinaryPrimitives.ReverseEndianness(this.Size);
		this.FirstBlock = BinaryPrimitives.ReverseEndianness(this.FirstBlock);
		this.Volume = BinaryPrimitives.ReverseEndianness(this.Volume);
		this.Flags = BinaryPrimitives.ReverseEndianness(this.Flags);
	}
}
