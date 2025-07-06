// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource;

public class NefsVolumeSource
{
	private const int PrimaryFileNumber = -1;
	private readonly string baseFilePath;

	/// <summary>
	/// The primary volume file path.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// The offset to the data in the volume.
	/// </summary>
	public uint DataOffset { get; }

	/// <summary>
	/// The volume data split size.
	/// </summary>
	public uint SplitSize { get; }

	/// <summary>
	/// Whether the volume data is split.
	/// </summary>
	public bool IsSplit => SplitSize != 0;

	/// <summary>
	/// Initializes a new instance of <see cref="NefsVolumeSource"/>.
	/// </summary>
	/// <param name="filePath">The primary volume file path.</param>
	/// <param name="dataOffset">The offset to the data in the volume.</param>
	/// <param name="splitSize">The volume data split size.</param>
	internal NefsVolumeSource(string filePath, uint dataOffset, uint splitSize)
	{
		FilePath = filePath;
		DataOffset = dataOffset;
		SplitSize = splitSize;

		this.baseFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty,
			Path.GetFileNameWithoutExtension(filePath)) + ".";
	}

	/// <summary>
	/// Gets the path at the given volume byte position.
	/// </summary>
	/// <param name="position">The byte position in the volume.</param>
	/// <returns>The split file path, or the primary path at the given position.</returns>
	internal string GetPathAtPosition(long position)
	{
		return GetPathAtFileNumber(GetFileNumberAtPosition(position));
	}

	/// <summary>
	/// Gets the path for the given split file number. If the volume is not split, the primary path is returned.
	/// </summary>
	/// <param name="number">The split file number.</param>
	/// <returns>The split file number path, or the primary path if number is -1 or not split.</returns>
	internal string GetPathAtFileNumber(int number)
	{
		if (!IsSplit || number == PrimaryFileNumber)
		{
			return FilePath;
		}

		return this.baseFilePath + number.ToString("D3");
	}

	/// <summary>
	/// Gets the split file number at the given volume byte position.
	/// </summary>
	/// <param name="position">The byte position in the volume.</param>
	/// <returns>The split file number, or -1 to represent the primary path.</returns>
	internal int GetFileNumberAtPosition(long position)
	{
		return !IsSplit || position < DataOffset
			? PrimaryFileNumber
			: Convert.ToInt32((position - DataOffset) / SplitSize);
	}
}
