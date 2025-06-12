// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct NefsTocSharedEntryInfo150 : INefsTocData<NefsTocSharedEntryInfo150>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocSharedEntryInfo150>();

	public uint Parent;
	public uint NextSibling;
	public uint FirstChild;
	public uint NameOffset;
	public uint Size;
	public uint FirstDuplicate;
	public uint PatchedEntry;

	public void ReverseEndianness()
	{
		this.Parent = BinaryPrimitives.ReverseEndianness(this.Parent);
		this.NextSibling = BinaryPrimitives.ReverseEndianness(this.NextSibling);
		this.FirstChild = BinaryPrimitives.ReverseEndianness(this.FirstChild);
		this.NameOffset = BinaryPrimitives.ReverseEndianness(this.NameOffset);
		this.Size = BinaryPrimitives.ReverseEndianness(this.Size);
		this.FirstDuplicate = BinaryPrimitives.ReverseEndianness(this.FirstDuplicate);
		this.PatchedEntry = BinaryPrimitives.ReverseEndianness(this.PatchedEntry);
	}
}
