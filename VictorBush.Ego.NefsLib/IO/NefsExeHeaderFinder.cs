// See LICENSE.txt for license information.

using System.Buffers.Binary;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

/// <inheritdoc />
public class NefsExeHeaderFinder : INefsExeHeaderFinder
{
	private static readonly ILogger Log = NefsLog.GetLogger();
	private static ReadOnlySpan<uint> FourCcValues => new[]
		{ NefsConstants.FourCc, BinaryPrimitives.ReverseEndianness(NefsConstants.FourCc) };
	private readonly IFileSystem fileSystem;

	public NefsExeHeaderFinder(IFileSystem fileSystem)
	{
		this.fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public async Task<List<HeadlessSource>> FindHeadersAsync(string exePath, string dataFileDir, NefsProgress p)
	{
		const float findDataSectionWeight = 0.25f;
		const float findFourCcWeight = 0.05f;
		const float findWriteableOffsetWeight = 0.7f;
		var sources = new List<HeadlessSource>();

		// Load exe into memory
		var exeBytes = await this.fileSystem.File.ReadAllBytesAsync(exePath).ConfigureAwait(false);
		await using var exeStream = new MemoryStream(exeBytes);
		long? dataSectionStart = null;
		long? dataSectionEnd = null;
		try
		{
			using var _ = p.BeginTask(findDataSectionWeight, "Finding '.data' section in exe");
			if (PeHelper.Identify(exeStream))
			{
				var dataSection = PeHelper.GetSectionDataInfo(exeStream, ".data");
				dataSectionStart = Convert.ToInt64(dataSection?.Position);
				dataSectionEnd = dataSectionStart + Convert.ToInt64(dataSection?.Size);
			}
			else if (ElfHelper.Identify(exeStream))
			{
				var dataSection = ElfHelper.GetSectionDataInfo(exeStream, ".data");
				dataSectionStart = Convert.ToInt64(dataSection?.Position);
				dataSectionEnd = dataSectionStart + Convert.ToInt64(dataSection?.Size);
			}
			else if (MachOHelper.Identify(exeStream))
			{
				var dataSection = MachOHelper.GetSectionDataInfo(exeStream, "__data");
				dataSectionStart = Convert.ToInt64(dataSection?.Position);
				dataSectionEnd = dataSectionStart + Convert.ToInt64(dataSection?.Size);
			}
		}
		catch
		{
			Log.LogError("Failed to find data section offset; using 0 as offset.");
		}

		// Search for headers and create strategies for each
		var strategies = new List<NefsExeHeaderFinderStrategy>();
		using var fourCcTask = p.BeginTask(findFourCcWeight, "Finding headers in exe");
		foreach (var fourCcOffset in GetFourCcOffsets(exeBytes, p))
		{
			// Check for cancel
			p.CancellationToken.ThrowIfCancellationRequested();

			// Try to read header intro
			try
			{
				var isLittleEndian = NefsConstants.FourCc ==
				                     BinaryPrimitives.ReadUInt32LittleEndian(exeBytes.AsSpan()[fourCcOffset..]);
				using var br = new EndianBinaryReader(exeStream, isLittleEndian);
				br.BaseStream.Seek(fourCcOffset, SeekOrigin.Begin);

				var intro = await br.ReadTocDataAsync<NefsTocHeaderA160>(p.CancellationToken).ConfigureAwait(false);
				var version = (NefsVersion)intro.Version;
				NefsExeHeaderFinderStrategy? strategy = version switch
				{
					NefsVersion.Version160 => new NefsExeHeaderFinderStrategy160(intro, fourCcOffset, isLittleEndian),
					NefsVersion.Version200 => new NefsExeHeaderFinderStrategy200(intro, fourCcOffset, isLittleEndian),
					_ => null
				};

				if (strategy is null)
				{
					continue;
				}

				strategies.Add(strategy);
			}
			catch (Exception)
			{
				// Failed to read header, so assume not a header
			}
		}
		fourCcTask.Dispose();

		// Find the writeable data offset
		// Ordered by the largest volume so that smaller writeable data isn't found within a larger one
		using var writeableDataTask = p.BeginTask(findWriteableOffsetWeight, "Finding writeable data offsets in exe");
		var foundWriteableRanges = new List<DataRange>();
		foreach (var strategy in strategies.OrderByDescending(x => x.TocSize))
		{
			using var _ = p.BeginSubTask(1f / strategies.Count);
			// Check for cancel
			p.CancellationToken.ThrowIfCancellationRequested();

			// Check for a known version number
			using var br = new EndianBinaryReader(exeStream, strategy.IsLittleEndian);

			// Try to read header intro
			try
			{
				// Read preliminary data
				var name = await strategy.ReadAsync(br, p).ConfigureAwait(false);
				if (string.IsNullOrWhiteSpace(name))
				{
					// Failed to get name
					Log.LogError($"Found a header at {strategy.FourCcOffset}, but could not read data file name.");
					continue;
				}

				// Find writeable data offset
				var dataSectionRange = new DataRange(
					dataSectionStart ?? strategy.FourCcOffset,
					dataSectionEnd ?? br.BaseStream.Length);
				var writeableRange = await strategy
					.FindWriteableDataOffsetAsync(br, dataSectionRange, foundWriteableRanges, p)
					.ConfigureAwait(false);
				if (writeableRange.Start == -1)
				{
					Log.LogError($"Found a header at {strategy.FourCcOffset}, but could not find writeable data.");
					continue;
				}

				// Create an archive source for this header
				var dataFilePath = Path.Combine(dataFileDir, name);
				var source = new HeadlessSource(dataFilePath, exePath,
					strategy.FourCcOffset,
					null,
					writeableRange.Start,
					null);
				sources.Add(source);
				foundWriteableRanges.Add(writeableRange);
			}
			catch (Exception)
			{
				// Failed to read header, so assume not a header
			}
		}

		return sources;
	}

	private static List<int> GetFourCcOffsets(byte[] buffer, NefsProgress p)
    {
        var uintSpan = MemoryMarshal.Cast<byte, uint>(buffer);
        var positions = new List<int>();

        int pos;
        var offset = 0;
        while ((pos = uintSpan.IndexOfAny(FourCcValues)) != -1)
        {
	        p.CancellationToken.ThrowIfCancellationRequested();
	        positions.Add((offset + pos) * sizeof(uint));
	        var nextStart = pos + 1;
	        offset += nextStart;
	        uintSpan = uintSpan[nextStart..];
        }

        return positions;
    }

    private readonly record struct DataRange(long Start, long End) : IComparable<DataRange>
    {
	    public int CompareTo(DataRange other)
	    {
		    return Start.CompareTo(other.Start);
	    }

	    public bool Overlaps(DataRange other)
	    {
		    return Start < other.End && End > other.Start;
	    }
    }

    private abstract class NefsExeHeaderFinderStrategy
    {
	    public int FourCcOffset { get; }

	    public bool IsLittleEndian { get; }

	    public long TocSize { get; }

	    public long WriteableDataSize { get; protected set; }

	    public NefsExeHeaderFinderStrategy(NefsTocHeaderA160 intro, int fourCcOffset, bool isLittleEndian)
	    {
		    IsLittleEndian = isLittleEndian;
		    FourCcOffset = fourCcOffset;
		    TocSize = intro.TocSize;
	    }

	    public abstract ValueTask<string> ReadAsync(EndianBinaryReader reader, NefsProgress p);

	    /// <summary>
	    /// Find the writeable data in an exe.
	    /// </summary>
	    /// <returns>The writeable data range, or -1 if not found.</returns>
	    public async ValueTask<DataRange> FindWriteableDataOffsetAsync(
		    EndianBinaryReader reader,
		    DataRange sectionRange,
		    IReadOnlyList<DataRange> foundWriteableRanges,
		    NefsProgress p)
	    {
		    reader.BaseStream.Seek(sectionRange.Start, SeekOrigin.Begin);

		    while (reader.BaseStream.Position + WriteableDataSize <= sectionRange.End)
		    {
			    if (OverlapAdjust())
			    {
				    // position may have moved so continue check the while condition
				    continue;
			    }

			    var valid = await ValidateWriteableDataAsync(reader, p).ConfigureAwait(false);
			    if (!valid)
			    {
				    continue;
			    }

			    var end = reader.BaseStream.Position;
			    return new DataRange(end - WriteableDataSize, end);
		    }

		    return new DataRange(-1, -1);

		    bool OverlapAdjust()
		    {
			    var needAdjust = false;
			    var start = reader.BaseStream.Position;
			    var range = new DataRange(start, start + WriteableDataSize);
			    foreach (var foundWriteableRange in foundWriteableRanges.Order())
			    {
				    if (!foundWriteableRange.Overlaps(range))
				    {
					    continue;
				    }

				    range = new DataRange(foundWriteableRange.End, foundWriteableRange.End + WriteableDataSize);
				    needAdjust = true;
			    }

			    if (needAdjust)
			    {
				    reader.BaseStream.Seek(range.Start, SeekOrigin.Begin);
			    }

			    return needAdjust;
		    }
	    }

	    protected abstract ValueTask<bool> ValidateWriteableDataAsync(EndianBinaryReader reader, NefsProgress p);

	    protected async ValueTask<string> ReadFileName(
		    EndianBinaryReader reader,
		    uint nameTableStart,
		    uint volumeTableStart,
		    NefsProgress p)
	    {
		    // Find file name of volume info
		    reader.BaseStream.Seek(FourCcOffset + volumeTableStart, SeekOrigin.Begin);
		    var volInfo = await reader.ReadTocDataAsync<NefsTocVolumeInfo150>(p.CancellationToken)
			    .ConfigureAwait(false);
		    reader.BaseStream.Seek(FourCcOffset + nameTableStart + volInfo.NameOffset, SeekOrigin.Begin);

		    // Read 256 bytes - this is overkill, probably won't have a filename that big
		    var nameBytes = new byte[256];
		    await reader.BaseStream.ReadExactlyAsync(nameBytes, p.CancellationToken).ConfigureAwait(false);

		    return StringHelper.TryReadNullTerminatedAscii(nameBytes);
	    }

	    protected static async ValueTask<T[]> ReadTocEntriesAsync<T>(
		    EndianBinaryReader reader,
		    long offset,
		    int size,
		    NefsProgress p)
		    where T : unmanaged, INefsTocData<T>
	    {
		    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		    var numEntries = size / T.ByteCount;
		    var entries = new T[numEntries];
		    for (var i = 0; i < numEntries; ++i)
		    {
			    entries[i] = await reader.ReadTocDataAsync<T>(p.CancellationToken).ConfigureAwait(false);
		    }

		    return entries;
	    }
    }

    private class NefsExeHeaderFinderStrategy160 : NefsExeHeaderFinderStrategy
    {
	    private NefsTocHeaderB160 toc;
	    private IReadOnlyList<NefsTocEntry160> entries;
	    private IReadOnlyList<NefsTocSharedEntryInfo160> entryInfos;

	    public NefsExeHeaderFinderStrategy160(NefsTocHeaderA160 intro, int fourCcOffset, bool isLittleEndian)
		    : base(intro, fourCcOffset, isLittleEndian)
	    {
		    this.entries = [];
		    this.entryInfos = [];
	    }

	    public override async ValueTask<string> ReadAsync(EndianBinaryReader reader, NefsProgress p)
	    {
		    reader.BaseStream.Seek(FourCcOffset + NefsTocHeaderA160.ByteCount, SeekOrigin.Begin);
		    var toc = await reader.ReadTocDataAsync<NefsTocHeaderB160>(p.CancellationToken)
			    .ConfigureAwait(false);
		    this.toc = toc;
		    var name = await ReadFileName(reader, toc.NameTableStart, toc.VolumeInfoTableStart, p).ConfigureAwait(false);

		    this.entries = await ReadTocEntriesAsync<NefsTocEntry160>(reader, FourCcOffset + toc.EntryTableStart,
			    Convert.ToInt32(toc.SharedEntryInfoTableStart - toc.EntryTableStart), p).ConfigureAwait(false);
		    this.entryInfos = await ReadTocEntriesAsync<NefsTocSharedEntryInfo160>(reader, FourCcOffset + toc.SharedEntryInfoTableStart,
			    Convert.ToInt32(toc.NameTableStart - toc.SharedEntryInfoTableStart), p).ConfigureAwait(false);
		    WriteableDataSize = this.entries.Count * NefsTocEntryWriteable160.ByteCount +
		                        this.entryInfos.Count * NefsTocSharedEntryInfoWriteable160.ByteCount;
		    return name;
	    }

	    protected override async ValueTask<bool> ValidateWriteableDataAsync(EndianBinaryReader reader, NefsProgress p)
	    {
		    var position = reader.BaseStream.Position;
		    var flagMask = Enum.GetValuesAsUnderlyingType<NefsTocEntryFlags150>().Cast<ushort>().Sum(x => x);
		    var writeableEntries = new NefsTocEntryWriteable160[this.entries.Count];
		    for (var i = 0; i < writeableEntries.Length; ++i)
		    {
			    p.CancellationToken.ThrowIfCancellationRequested();
			    var writeableEntry = await reader.ReadTocDataAsync<NefsTocEntryWriteable160>(p.CancellationToken)
				    .ConfigureAwait(false);
			    if (writeableEntry.Volume >= this.toc.NumVolumes ||
			        (writeableEntry.Flags & flagMask) != writeableEntry.Flags)
			    {
				    return false;
			    }

			    writeableEntries[i] = writeableEntry;
		    }

		    var writeableEntryInfos = await ReadTocEntriesAsync<NefsTocSharedEntryInfoWriteable160>(reader,
				    reader.BaseStream.Position,
				    Convert.ToInt32(this.entryInfos.Count * NefsTocSharedEntryInfoWriteable160.ByteCount), p)
			    .ConfigureAwait(false);
		    var valid = true;
		    for (var i = 0; i < writeableEntries.Length; ++i)
		    {
			    p.CancellationToken.ThrowIfCancellationRequested();
			    var entry = this.entries[i];
			    var writeableEntry = writeableEntries[i];
			    var entryInfo = this.entryInfos[(int)entry.SharedInfo];
			    var writeableEntryInfo = writeableEntryInfos[entry.SharedInfo];

			    // Validate
			    var firstDup = entryInfo.FirstDuplicate;
			    var flags = (NefsTocEntryFlags150)writeableEntry.Flags;
			    var fDir = flags.HasFlag(NefsTocEntryFlags150.Directory);
			    if (((fDir && entry.Start == 0) || (!fDir && entryInfo.FirstChild == firstDup)) &&
			        (flags.HasFlag(NefsTocEntryFlags150.Patched) || writeableEntryInfo.PatchedEntry == firstDup) &&
			        (flags.HasFlag(NefsTocEntryFlags150.Duplicated) || firstDup == i))
			    {
				    continue;
			    }

			    valid = false;
			    // setup position for the next read to be just past the first entry
			    reader.BaseStream.Seek(position + NefsTocEntryWriteable160.ByteCount, SeekOrigin.Begin);
			    break;
		    }

		    return valid;
	    }
    }

    private class NefsExeHeaderFinderStrategy200 : NefsExeHeaderFinderStrategy
    {
	    private NefsTocHeaderB200 toc;
	    private IReadOnlyList<NefsTocEntry160> entries;
	    private IReadOnlyList<NefsTocSharedEntryInfo160> entryInfos;

	    public NefsExeHeaderFinderStrategy200(NefsTocHeaderA160 intro, int fourCcOffset, bool isLittleEndian)
		    : base(intro, fourCcOffset, isLittleEndian)
	    {
		    this.entries = [];
		    this.entryInfos = [];
	    }

	    public override async ValueTask<string> ReadAsync(EndianBinaryReader reader, NefsProgress p)
	    {
		    reader.BaseStream.Seek(FourCcOffset + NefsTocHeaderA160.ByteCount, SeekOrigin.Begin);
		    var toc = await reader.ReadTocDataAsync<NefsTocHeaderB200>(p.CancellationToken)
			    .ConfigureAwait(false);
		    this.toc = toc;
		    var name = await ReadFileName(reader, toc.NameTableStart, toc.VolumeInfoTableStart, p).ConfigureAwait(false);

		    this.entries = await ReadTocEntriesAsync<NefsTocEntry160>(reader, FourCcOffset + toc.EntryTableStart,
			    Convert.ToInt32(toc.SharedEntryInfoTableStart - toc.EntryTableStart), p).ConfigureAwait(false);
		    this.entryInfos = await ReadTocEntriesAsync<NefsTocSharedEntryInfo160>(reader, FourCcOffset + toc.SharedEntryInfoTableStart,
			    Convert.ToInt32(toc.NameTableStart - toc.SharedEntryInfoTableStart), p).ConfigureAwait(false);
		    WriteableDataSize = this.entries.Count * NefsTocEntryWriteable160.ByteCount +
		                        this.entryInfos.Count * NefsTocSharedEntryInfoWriteable160.ByteCount;
		    return name;
	    }

	    protected override async ValueTask<bool> ValidateWriteableDataAsync(EndianBinaryReader reader, NefsProgress p)
	    {
		    var position = reader.BaseStream.Position;
		    var flagMask = Enum.GetValuesAsUnderlyingType<NefsTocEntryFlags200>().Cast<ushort>().Sum(x => x);
		    var writeableEntries = new NefsTocEntryWriteable160[this.entries.Count];
		    for (var i = 0; i < writeableEntries.Length; ++i)
		    {
			    p.CancellationToken.ThrowIfCancellationRequested();
			    var writeableEntry = await reader.ReadTocDataAsync<NefsTocEntryWriteable160>(p.CancellationToken)
				    .ConfigureAwait(false);
			    if (writeableEntry.Volume >= this.toc.NumVolumes ||
			        (writeableEntry.Flags & flagMask) != writeableEntry.Flags)
			    {
				    return false;
			    }

			    writeableEntries[i] = writeableEntry;
		    }

		    var writeableEntryInfos = await ReadTocEntriesAsync<NefsTocSharedEntryInfoWriteable160>(reader,
				    reader.BaseStream.Position,
				    Convert.ToInt32(this.entryInfos.Count * NefsTocSharedEntryInfoWriteable160.ByteCount), p)
			    .ConfigureAwait(false);
		    var valid = true;
		    for (var i = 0; i < writeableEntries.Length; ++i)
		    {
			    p.CancellationToken.ThrowIfCancellationRequested();
			    var entry = this.entries[i];
			    var writeableEntry = writeableEntries[i];
			    var entryInfo = this.entryInfos[(int)entry.SharedInfo];
			    var writeableEntryInfo = writeableEntryInfos[entry.SharedInfo];

			    // Validate
			    var firstDup = entryInfo.FirstDuplicate;
			    var flags = (NefsTocEntryFlags200)writeableEntry.Flags;
			    var fDir = flags.HasFlag(NefsTocEntryFlags200.IsDirectory);
			    if (((fDir && entry.Start == 0) || (!fDir && entryInfo.FirstChild == firstDup)) &&
			        writeableEntryInfo.PatchedEntry == firstDup &&
			        (flags.HasFlag(NefsTocEntryFlags200.IsDuplicated) || firstDup == i))
			    {
				    continue;
			    }

			    valid = false;
			    // setup position for the next read to be just past the first entry
			    reader.BaseStream.Seek(position + NefsTocEntryWriteable160.ByteCount, SeekOrigin.Begin);
			    break;
		    }

		    return valid;
	    }
    }
}
