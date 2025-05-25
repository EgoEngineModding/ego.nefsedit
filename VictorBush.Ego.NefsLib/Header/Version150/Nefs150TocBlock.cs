// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct Nefs150TocBlock : INefsTocData<Nefs150TocBlock>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs150TocBlock>();

	public uint End;
	public uint Transformation;

	public void ReverseEndianness()
	{
		this.End = BinaryPrimitives.ReverseEndianness(this.End);
		this.Transformation = BinaryPrimitives.ReverseEndianness(this.Transformation);
	}
}
