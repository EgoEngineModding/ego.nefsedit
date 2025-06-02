// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct NefsTocBlock151 : INefsTocData<NefsTocBlock151>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocBlock151>();

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
