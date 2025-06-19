// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version010;

public struct NefsTocLink010 : INefsTocData<NefsTocLink010>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocLink010>();

	public uint ParentOffset;
	public uint NextSiblingOffset;
	public uint FirstChildOffset;
	public uint NameOffset;

	public void ReverseEndianness()
	{
		this.ParentOffset = BinaryPrimitives.ReverseEndianness(this.ParentOffset);
		this.NextSiblingOffset = BinaryPrimitives.ReverseEndianness(this.NextSiblingOffset);
		this.FirstChildOffset = BinaryPrimitives.ReverseEndianness(this.FirstChildOffset);
		this.NameOffset = BinaryPrimitives.ReverseEndianness(this.NameOffset);
	}
}
