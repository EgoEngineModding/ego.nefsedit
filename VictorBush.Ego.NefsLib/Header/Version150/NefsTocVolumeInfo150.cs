// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct NefsTocVolumeInfo150 : INefsTocData<NefsTocVolumeInfo150>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocVolumeInfo150>();

	public ulong Size;
	public uint NameOffset;
	public uint DataOffset;

	public void ReverseEndianness()
	{
		this.Size = BinaryPrimitives.ReverseEndianness(this.Size);
		this.NameOffset = BinaryPrimitives.ReverseEndianness(this.NameOffset);
		this.DataOffset = BinaryPrimitives.ReverseEndianness(this.DataOffset);
	}
}
