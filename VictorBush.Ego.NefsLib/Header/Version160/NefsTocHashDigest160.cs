// See LICENSE.txt for license information.

using System.Runtime.CompilerServices;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header;

public struct NefsTocHashDigest160 : INefsTocData<NefsTocHashDigest160>
{
	public static int ByteCount { get; } = Unsafe.SizeOf<NefsTocHashDigest160>();

	public Sha256Hash Data;

	public void ReverseEndianness()
	{
	}
}
