// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// A list of SHA-256 hashes of the archive's file data. The size of the hashed chunks are typically 0x800000. NeFS
/// version 1.6 has a Block Size property that can specify this size. There are some unknown oddities. The last chunk of
/// data does not seem to have a corresponding hash. There is additional data at the end of part 8 that is unknown.
/// </summary>
public sealed class Nefs160HeaderHashDigestTable : INefsTocTable<Nefs160TocHashDigest>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocHashDigest> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderHashDigestTable"/> class.
	/// </summary>
	internal Nefs160HeaderHashDigestTable(IReadOnlyList<Nefs160TocHashDigest> entries)
	{
		Entries = entries;
	}
}
