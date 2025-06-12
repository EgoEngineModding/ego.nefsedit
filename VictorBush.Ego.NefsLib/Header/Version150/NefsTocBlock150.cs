// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct NefsTocBlock150 : INefsTocData<NefsTocBlock150>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocBlock150>();

	public uint End;
	public uint Transformation;

	public void ReverseEndianness()
	{
		this.End = BinaryPrimitives.ReverseEndianness(this.End);
		this.Transformation = BinaryPrimitives.ReverseEndianness(this.Transformation);
	}
}
