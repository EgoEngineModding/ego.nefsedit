// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version140;

public struct NefsTocVolumeDataStart140 : INefsTocData<NefsTocVolumeDataStart140>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocVolumeDataStart140>();

	public uint Start;

	public void ReverseEndianness()
	{
		this.Start = BinaryPrimitives.ReverseEndianness(this.Start);
	}
}
