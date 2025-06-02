// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header;

public struct NefsTocSharedEntryInfoWriteable160 : INefsTocData<NefsTocSharedEntryInfoWriteable160>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocSharedEntryInfoWriteable160>();

	public uint NextSibling;
	public uint PatchedEntry;

	public void ReverseEndianness()
	{
		this.NextSibling = BinaryPrimitives.ReverseEndianness(this.NextSibling);
		this.PatchedEntry = BinaryPrimitives.ReverseEndianness(this.PatchedEntry);
	}
}
