// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Header.Version140;

public struct NefsTocHeader140 : INefsTocData<NefsTocHeader140>
{
	/// <inheritdoc />
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocHeader140>();

	public uint Magic;
	public uint TocSize;
	public uint Version;
	public uint NumVolumes;
	public uint BlockSize;
	public uint SplitSize;
	public uint EntryTableStart;
	public uint LinkTableStart;
	public uint NameTableStart;
	public uint BlockTableStart;
	public uint VolumeSizeTableStart;
	public uint VolumeNameStartTableStart;
	public uint VolumeNameTableStart;
	public uint VolumeDataStartTableStart;
	public uint Unknown;
	public AesKeyHexBuffer AesKey;

	public unsafe void ReverseEndianness()
	{
		var buffer = new Span<uint>(Unsafe.AsPointer(ref this), 15);
		for (var i = 0; i < buffer.Length; ++i)
		{
			buffer[i] = BinaryPrimitives.ReverseEndianness(buffer[i]);
		}
	}
}
