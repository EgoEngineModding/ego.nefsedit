// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version130;

public struct NefsTocVolumeNameStart130 : INefsTocData<NefsTocVolumeNameStart130>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocVolumeNameStart130>();

	public uint Start;

	public void ReverseEndianness()
	{
		this.Start = BinaryPrimitives.ReverseEndianness(this.Start);
	}
}
