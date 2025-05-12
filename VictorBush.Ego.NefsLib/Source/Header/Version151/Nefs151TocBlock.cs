// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version151;

public struct Nefs151TocBlock : INefsTocData<Nefs151TocBlock>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs151TocBlock>();

	public uint End;
	public ushort Transformation;
	public ushort Checksum;

	public void ReverseEndianness()
	{
		this.End = BinaryPrimitives.ReverseEndianness(this.End);
		this.Transformation = BinaryPrimitives.ReverseEndianness(this.Transformation);
		this.Checksum = BinaryPrimitives.ReverseEndianness(this.Checksum);
	}
}
