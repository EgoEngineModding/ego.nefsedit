// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header;

public struct Nefs160TocSharedEntryInfoWriteable : INefsTocData<Nefs160TocSharedEntryInfoWriteable>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs160TocSharedEntryInfoWriteable>();

	public uint NextSibling;
	public uint PatchedEntry;

	public void ReverseEndianness()
	{
		this.NextSibling = BinaryPrimitives.ReverseEndianness(this.NextSibling);
		this.PatchedEntry = BinaryPrimitives.ReverseEndianness(this.PatchedEntry);
	}
}
