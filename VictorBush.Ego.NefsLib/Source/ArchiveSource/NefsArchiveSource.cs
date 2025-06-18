// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource;

/// <summary>
/// Defines location of a NeFS archive.
/// </summary>
public abstract class NefsArchiveSource
{
	protected NefsArchiveSource(string filePath)
	{
		FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
	}

	/// <summary>
	/// The name of the archive file.
	/// </summary>
	public string FileName => Path.GetFileName(FilePath);

	/// <summary>
	/// Path to the archive file. containing the archive's file data.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Header and file data are stored in separate files. Usually the header is split into two sections, a primary
	/// section (header parts 1-5, 8) and a secondary section (parts 6-7).
	/// </summary>
	/// <param name="dataFilePath">
	/// Path of the data file. This contains the file data for the archive (e.g., "game.dat", "game.bin", "dr.nic").
	/// </param>
	/// <param name="headerFilePath">Path of the file that contains the archive's header data (i.e., the game executable).</param>
	/// <param name="primaryOffset">Offset into the header file to the primary header section.</param>
	/// <param name="primarySize">Size of the primary header section.</param>
	/// <param name="secondaryOffset">Offset into the header file to the secondary header section.</param>
	/// <param name="secondarySize">Size of the secondary header section.</param>
	public static HeadlessSource Headless(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize)
	{
		return new HeadlessSource(dataFilePath, headerFilePath, primaryOffset, primarySize, secondaryOffset, secondarySize);
	}

	/// <summary>
	/// Header and file data are both stored in the same file.
	/// </summary>
	/// <param name="filePath">Path of the archive file.</param>
	public static StandardSource Standard(string filePath)
	{
		return new StandardSource(filePath);
	}
}
