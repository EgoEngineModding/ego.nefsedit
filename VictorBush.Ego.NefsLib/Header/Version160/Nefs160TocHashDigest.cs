// See LICENSE.txt for license information.

using System.Runtime.CompilerServices;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header;

public struct Nefs160TocHashDigest : INefsTocData<Nefs160TocHashDigest>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<Nefs160TocHashDigest>();

	public Sha256Hash Data;

	public void ReverseEndianness()
	{
	}
}
