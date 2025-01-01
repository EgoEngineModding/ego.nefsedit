// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VictorBush.Ego.NefsLib.Header.Version151;

public struct Nefs151TocHeader : INefsTocData<Nefs151TocHeader>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs151TocHeader>();

	public uint Magic;
	public uint TocSize;
	public uint Version;
	public uint NumVolumes;
	public uint NumEntries;
	public uint BlockSize;
	public uint SplitSize;
	public uint EntryTableStart;
	public uint SharedEntryInfoTableStart;
	public uint NameTableStart;
	public uint BlockTableStart;
	public uint VolumeInfoTableStart;
	public uint Unknown1;
	public uint Unknown2;
	public uint Unknown3;
	public AesKeyBuffer AesKeyBuffer;
	public uint Unknown4;

	public unsafe void ReverseEndianness()
	{
		var buffer = new Span<uint>(Unsafe.AsPointer(ref this), 15);
		for (var i = 0; i < buffer.Length; ++i)
		{
			buffer[i] = BinaryPrimitives.ReverseEndianness(buffer[i]);
		}

		// Leave Unknown4 alone for now. Not sure if actually part of header
	}
}
