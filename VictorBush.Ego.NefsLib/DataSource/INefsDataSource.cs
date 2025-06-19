// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.DataSource;

/// <summary>
/// Defines the source of file data for an item in an archive.
/// </summary>
public interface INefsDataSource
{
	/// <summary>
	/// The path of the file that contains the item's data.
	/// </summary>
	string FilePath { get; }

	/// <summary>
	/// A value indicating whether the data in this data source has had any applicable transformations applied. When
	/// replacing a file in an archive, the replacement data source will most likely set this to false. When the archive
	/// is saved, the transformation is applied.
	/// </summary>
	bool IsTransformed { get; }

	/// <summary>
	/// The offset in the source file where the data begins.
	/// </summary>
	long Offset { get; }

	/// <summary>
	/// The size information about the source data.
	/// </summary>
	NefsItemSize Size { get; }
}
