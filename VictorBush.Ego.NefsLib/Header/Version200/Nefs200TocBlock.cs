// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version200;

public struct Nefs200TocBlock : INefsTocData<Nefs200TocBlock>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs200TocBlock>();

	public uint End;

	public void ReverseEndianness()
	{
		this.End = BinaryPrimitives.ReverseEndianness(this.End);
	}
}
