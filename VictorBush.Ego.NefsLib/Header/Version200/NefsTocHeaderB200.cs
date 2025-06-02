// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version200;

public struct NefsTocHeaderB200 : INefsTocData<NefsTocHeaderB200>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocHeaderB200>();

	public ushort NumVolumes;
	public ushort HashBlockSizeLo;
	public uint EntryTableStart;
	public uint WritableEntryTableStart;
	public uint SharedEntryInfoTableStart;
	public uint WritableSharedEntryInfoTableStart;
	public uint NameTableStart;
	public uint BlockTableStart;
	public uint VolumeInfoTableStart;
	public uint HashDigestTableStart;
	public uint RandomPadding1;
	public uint RandomPadding2;
	public uint RandomPadding3;
	public uint RandomPadding4;
	public uint RandomPadding5;
	public uint RandomPadding6;
	public uint RandomPadding7;
	public uint RandomPadding8;
	public uint RandomPadding9;
	public uint RandomPadding10;
	public uint RandomPadding11;
	public uint RandomPadding12;
	public uint RandomPadding13;
	public uint RandomPadding14;
	public uint RandomPadding15;
	public uint RandomPadding16;
	public uint RandomPadding17;
	public uint RandomPadding18;
	public uint RandomPadding19;
	public uint RandomPadding20;
	public uint RandomPadding21;
	public uint RandomPadding22;
	public uint RandomPadding23;

	public uint HashBlockSize
	{
		get => (uint)this.HashBlockSizeLo << 15;
		set => this.HashBlockSizeLo = Convert.ToUInt16(value >>> 15);
	}

	public unsafe byte[] RandomPadding
	{
		get
		{
			var buffer = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref this), ByteCount);
			return buffer[36..ByteCount].ToArray();
		}
		set
		{
			var buffer = new Span<byte>(Unsafe.AsPointer(ref this), ByteCount);
			value.CopyTo(buffer[36..ByteCount]);
		}
	}

	public void ReverseEndianness()
	{
		this.NumVolumes = BinaryPrimitives.ReverseEndianness(this.NumVolumes);
		this.HashBlockSizeLo = BinaryPrimitives.ReverseEndianness(this.HashBlockSizeLo);
		this.EntryTableStart = BinaryPrimitives.ReverseEndianness(this.EntryTableStart);
		this.WritableEntryTableStart = BinaryPrimitives.ReverseEndianness(this.WritableEntryTableStart);
		this.SharedEntryInfoTableStart = BinaryPrimitives.ReverseEndianness(this.SharedEntryInfoTableStart);
		this.WritableSharedEntryInfoTableStart = BinaryPrimitives.ReverseEndianness(this.WritableSharedEntryInfoTableStart);
		this.NameTableStart = BinaryPrimitives.ReverseEndianness(this.NameTableStart);
		this.BlockTableStart = BinaryPrimitives.ReverseEndianness(this.BlockTableStart);
		this.VolumeInfoTableStart = BinaryPrimitives.ReverseEndianness(this.VolumeInfoTableStart);
		this.HashDigestTableStart = BinaryPrimitives.ReverseEndianness(this.HashDigestTableStart);
	}
}
