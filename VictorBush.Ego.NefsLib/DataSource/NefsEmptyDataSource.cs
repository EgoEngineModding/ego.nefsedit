// See LICENSE.txt for license information.

using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.DataSource;

/// <summary>
/// Data source that has no data.
/// </summary>
public class NefsEmptyDataSource : INefsDataSource
{
	/// <inheritdoc/>
	public string FilePath => "";

	/// <inheritdoc/>
	public bool IsTransformed => true;

	/// <inheritdoc/>
	public long Offset => 0;

	/// <inheritdoc/>
	public NefsItemSize Size => new(0);

	/// <inheritdoc />
	public Stream OpenRead(IFileSystem fileSystem)
	{
		throw new NotSupportedException("Empty data source cannot be opened.");
	}

	/// <inheritdoc />
	public bool Exists(IFileSystem fileSystem)
	{
		return false;
	}
}
