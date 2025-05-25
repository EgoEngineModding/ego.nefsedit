// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version160;

public struct Nefs160TocEntry : INefsTocData<Nefs160TocEntry>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs160TocEntry>();

	// likely split for proper contiguous struct alignment
	public uint StartA;
	public uint StartB;
	public uint SharedInfo;
	public uint FirstBlock;
	public uint NextDuplicate;

	public ulong Start
	{
		get => this.StartA | ((ulong)this.StartB << 32);
		set
		{
			this.StartA = (uint)value;
			this.StartB = (uint)(value >> 32);
		}
	}

	public void ReverseEndianness()
	{
		this.StartA = BinaryPrimitives.ReverseEndianness(this.StartA);
		this.StartB = BinaryPrimitives.ReverseEndianness(this.StartB);
		(this.StartA, this.StartB) = (this.StartB, this.StartA);
		this.SharedInfo = BinaryPrimitives.ReverseEndianness(this.SharedInfo);
		this.FirstBlock = BinaryPrimitives.ReverseEndianness(this.FirstBlock);
		this.NextDuplicate = BinaryPrimitives.ReverseEndianness(this.NextDuplicate);
	}
}
