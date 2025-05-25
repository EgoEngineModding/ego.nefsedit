// See LICENSE.txt for license information.

using System.Runtime.InteropServices;

namespace VictorBush.Ego.NefsLib.IO;

public static class StructEx
{
	/// <summary>
	/// Gets the alignment of the object.
	/// </summary>
	public static unsafe int AlignOf<T>()
		where T : unmanaged
	{
		return sizeof(AlignOfHelper<T>) - sizeof(T);
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct AlignOfHelper<T>
	{
		private readonly byte _padding;
		private readonly T _value;
	}
}
