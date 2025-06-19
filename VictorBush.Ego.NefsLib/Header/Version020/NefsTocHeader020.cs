// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version020;

public struct NefsTocHeader020 : INefsTocData<NefsTocHeader020>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocHeader020>();

	public uint Magic;
	public uint TocSize;
	public uint Version;
	public uint NumVolumes;
	public uint BlockSize;
	public uint EntryTableStart;
	public uint LinkTableStart;
	public uint NameTableStart;
	public uint BlockTableStart;
	public uint VolumeSizeTableStart;
	public AesKeyHexBuffer AesKey;

	public unsafe void ReverseEndianness()
	{
		var buffer = new Span<uint>(Unsafe.AsPointer(ref this), 10);
		for (var i = 0; i < buffer.Length; ++i)
		{
			buffer[i] = BinaryPrimitives.ReverseEndianness(buffer[i]);
		}
	}
}
