// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version160;

public struct NefsTocSharedEntryInfo160 : INefsTocData<NefsTocSharedEntryInfo160>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocSharedEntryInfo160>();

	public uint Parent;
	public uint FirstChild;
	public uint NameOffset;
	public uint Size;
	public uint FirstDuplicate;

	public void ReverseEndianness()
	{
		this.Parent = BinaryPrimitives.ReverseEndianness(this.Parent);
		this.FirstChild = BinaryPrimitives.ReverseEndianness(this.FirstChild);
		this.NameOffset = BinaryPrimitives.ReverseEndianness(this.NameOffset);
		this.Size = BinaryPrimitives.ReverseEndianness(this.Size);
		this.FirstDuplicate = BinaryPrimitives.ReverseEndianness(this.FirstDuplicate);
	}
}
