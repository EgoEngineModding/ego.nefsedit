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

	/// <summary>
	/// Align the start of the object.
	/// </summary>
	public static int Align<T>(int offset)
		where T : unmanaged
	{
		var align = AlignOf<T>();
		return Align(offset, align);
	}

	/// <summary>
	/// Align the start of the object.
	/// </summary>
	public static long Align<T>(long offset)
		where T : unmanaged
	{
		var align = AlignOf<T>();
		return Align(offset, align);
	}

	/// <summary>
	/// Align the offset using the given alignment.
	/// </summary>
	public static int Align(int offset, int alignment)
	{
		var aligned = (offset + alignment - 1) & -alignment;
		return aligned;
	}

	/// <summary>
	/// Align the offset using the given alignment.
	/// </summary>
	public static long Align(long offset, int alignment)
	{
		var aligned = (offset + alignment - 1) & -alignment;
		return aligned;
	}

	/// <summary>
	/// Gets the alignment padding necessary to align the start of the object.
	/// </summary>
	public static int AlignPadding<T>(int offset)
		where T : unmanaged
	{
		var align = AlignOf<T>();
		var padding = -offset & (align - 1);
		return padding;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct AlignOfHelper<T>
	{
		private readonly byte _padding;
		private readonly T _value;
	}
}
