// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version160;

public record struct Nefs160TocEntryWriteable : INefsTocData<Nefs160TocEntryWriteable>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs160TocEntryWriteable>();

	public ushort Volume;
	public ushort Flags;

	public void ReverseEndianness()
	{
		this.Volume = BinaryPrimitives.ReverseEndianness(this.Volume);
		this.Flags = BinaryPrimitives.ReverseEndianness(this.Flags);
	}
}
