// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version160;

public struct Nefs160TocSharedEntryInfo : INefsTocData<Nefs160TocSharedEntryInfo>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs160TocSharedEntryInfo>();

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
