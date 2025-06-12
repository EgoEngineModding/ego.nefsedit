// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version200;

public struct NefsTocBlock200 : INefsTocData<NefsTocBlock200>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocBlock200>();

	public uint End;

	public void ReverseEndianness()
	{
		this.End = BinaryPrimitives.ReverseEndianness(this.End);
	}
}
