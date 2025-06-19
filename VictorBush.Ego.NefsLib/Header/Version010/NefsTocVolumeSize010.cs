// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version010;

public struct NefsTocVolumeSize010 : INefsTocData<NefsTocVolumeSize010>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocVolumeSize010>();

	public ulong Size;

	public void ReverseEndianness()
	{
		this.Size = BinaryPrimitives.ReverseEndianness(this.Size);
	}
}
