// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

public interface INefsTocData<T> where T : unmanaged, INefsTocData<T>
{
	/// <summary>
	/// The size of the data in bytes.
	/// </summary>
	static abstract int ByteCount { get; }

	void ReverseEndianness();
}
