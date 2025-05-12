// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

[Flags]
public enum NefsTocEntryFlags : ushort
{
	None = 0,
	Transformed = 1 << 0,
	Directory = 1 << 1,
	Duplicated = 1 << 2,
	Cacheable = 1 << 3,
	LastSibling = 1 << 4,
	Patched = 1 << 5
}
