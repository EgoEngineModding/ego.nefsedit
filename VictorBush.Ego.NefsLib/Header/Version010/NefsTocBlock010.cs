// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version010;

public struct NefsTocBlock010 : INefsTocData<NefsTocBlock010>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocBlock010>();

	public uint End;
	public uint Transformation;

	public void ReverseEndianness()
	{
		this.End = BinaryPrimitives.ReverseEndianness(this.End);
		this.Transformation = BinaryPrimitives.ReverseEndianness(this.Transformation);
	}
}
