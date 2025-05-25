// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version150;

public struct Nefs150TocHeader : INefsTocData<Nefs150TocHeader>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs150TocHeader>();

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
	public AesKeyBuffer AesKeyBuffer;

	public unsafe void ReverseEndianness()
	{
		var buffer = new Span<uint>(Unsafe.AsPointer(ref this), 12);
		for (var i = 0; i < buffer.Length; ++i)
		{
			buffer[i] = BinaryPrimitives.ReverseEndianness(buffer[i]);
		}
	}
}
