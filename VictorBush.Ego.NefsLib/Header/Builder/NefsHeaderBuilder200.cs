// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsHeaderBuilder200 : NefsHeaderBuilder160Base<NefsHeader200>
{
	internal override uint ComputeDataOffset(NefsHeader200 sourceHeader, NefsItemList items)
	{
		var nonDuplicateCount = items.EnumerateById().Count(x => !x.IsDuplicate);
		var entryTableSize = items.Count * NefsTocEntry160.ByteCount;
		var sharedEntryInfoTableSize = nonDuplicateCount * NefsTocSharedEntryInfo160.ByteCount;
		var nameTableSize = items.EnumerateById().Where(x => !x.IsDuplicate).Distinct().Sum(x => x.FileName.Length);
		var blockTableSize = items.EnumerateById().Where(x => !x.IsDuplicate).Sum(x => x.DataSource.Size.Chunks.Count) * NefsTocBlock200.ByteCount;
		var volumeInfoTableSize = sourceHeader.Volumes.Count * NefsTocVolumeInfo150.ByteCount;
		var writeableEntryTableSize = items.Count * NefsTocEntryWriteable160.ByteCount;
		var writeableSharedEntryInfoTableSize = nonDuplicateCount * NefsTocSharedEntryInfoWriteable160.ByteCount;

		var hashBlockSize = sourceHeader.TableOfContents.HashBlockSize;
		var dataSize = items.EnumerateById().Sum(x => x.CompressedSize);
		var numHashDigests = hashBlockSize == 0 ? 0 : (dataSize + hashBlockSize - 1) / hashBlockSize;
		var hashDigestTableSize = numHashDigests * NefsTocHashDigest160.ByteCount;

		var sizeSum = NefsTocHeaderA160.ByteCount + NefsTocHeaderB200.ByteCount + entryTableSize +
		              sharedEntryInfoTableSize + nameTableSize + blockTableSize + volumeInfoTableSize +
		              writeableEntryTableSize + writeableSharedEntryInfoTableSize + hashDigestTableSize;
		var tocSize = StructEx.Align(sizeSum, NefsConstants.AesBlockSize);
		var tocFinalEnd = Convert.ToUInt32(StructEx.Align(tocSize, Convert.ToInt32(sourceHeader.BlockSize)));
		return tocFinalEnd;
	}

	internal override NefsHeader200 Build(NefsHeader200 sourceHeader, NefsItemList items, NefsProgress p)
	{
		p.CancellationToken.ThrowIfCancellationRequested();
		var entryTable = BuildEntryTable160(items);
		var nameTable = new NefsHeaderNameTable(items);
		p.CancellationToken.ThrowIfCancellationRequested();
		var sharedEntryInfoTable = BuildSharedEntryInfoTable160(items, nameTable);
		var blockTable = Build200BlockTable(items);
		p.CancellationToken.ThrowIfCancellationRequested();
		var writeableEntryTable = BuildWriteableEntryTable160(items);
		var writeableSharedEntryInfoTable = BuildWriteableSharedEntryInfoTable160(items);
		p.CancellationToken.ThrowIfCancellationRequested();

		var hashBlockSize = sourceHeader.TableOfContents.HashBlockSize;
		var dataSize = (ulong)items.EnumerateById().Sum(x => x.CompressedSize);
		var hashDigestTable = BuildHashDigestTable160(dataSize, hashBlockSize);

		// Get sizes, offsets and account for alignment where needed
		var toc = new NefsTocHeaderB200
		{
			NumVolumes = sourceHeader.TableOfContents.NumVolumes,
			HashBlockSize = hashBlockSize,
			EntryTableStart = Convert.ToUInt32(NefsTocHeaderA160.ByteCount + NefsTocHeaderB200.ByteCount),
			RandomPadding = sourceHeader.TableOfContents.RandomPadding
		};
		toc.SharedEntryInfoTableStart = Convert.ToUInt32(toc.EntryTableStart + entryTable.ByteCount());
		toc.NameTableStart = Convert.ToUInt32(toc.SharedEntryInfoTableStart + sharedEntryInfoTable.ByteCount());
		toc.BlockTableStart =
			Convert.ToUInt32(StructEx.Align<NefsTocBlock200>(toc.NameTableStart + nameTable.Size));
		toc.VolumeInfoTableStart =
			Convert.ToUInt32(StructEx.Align<NefsTocVolumeInfo150>(toc.BlockTableStart + blockTable.ByteCount()));
		toc.WritableEntryTableStart =
			Convert.ToUInt32(toc.VolumeInfoTableStart + toc.NumVolumes * NefsTocVolumeInfo150.ByteCount);
		toc.WritableSharedEntryInfoTableStart =
			Convert.ToUInt32(toc.WritableEntryTableStart + writeableEntryTable.ByteCount());

		var writeableSharedEntryInfoTableEnd =
			toc.WritableSharedEntryInfoTableStart + writeableSharedEntryInfoTable.ByteCount();
		if (hashBlockSize > 0)
		{
			// TODO: needs alignment padding?
			toc.HashDigestTableStart = Convert.ToUInt32(writeableSharedEntryInfoTableEnd);
		}

		// Align by AES key size, for toc size, and then align it to block size for data offset
		var tocEnd = hashBlockSize > 0
			? toc.HashDigestTableStart + hashDigestTable.ByteCount()
			: writeableSharedEntryInfoTableEnd;
		var tocSize = StructEx.Align(tocEnd, NefsConstants.AesBlockSize);
		var tocFinalEnd = Convert.ToUInt32(StructEx.Align(tocSize, Convert.ToInt32(sourceHeader.BlockSize)));
		var volume0 = new NefsTocVolumeInfo150
		{
			// Single-file archives this is the size of the whole file
			Size = tocFinalEnd + dataSize,
			NameOffset = nameTable.OffsetsByFileName[Path.GetFileName(items.Volumes[0].FilePath)],
			DataOffset = tocFinalEnd
		};
		var volumeInfoTable = new NefsHeaderVolumeInfoTable150([volume0]);

		var intro = new NefsTocHeaderA160
		{
			Magic = NefsConstants.FourCc,
			Hash = new Sha256Hash(), // will be updated later
			AesKey = sourceHeader.Intro.AesKey,
			TocSize = Convert.ToUInt32(tocSize),
			Version = (uint)NefsVersion.Version200,
			NumEntries = (uint)items.Count,
			UserValue = sourceHeader.Intro.UserValue,
			RandomPadding = sourceHeader.Intro.RandomPadding,
			Unused = sourceHeader.Intro.Unused
		};

		return new NefsHeader200(sourceHeader.WriterSettings, intro, toc, entryTable, sharedEntryInfoTable, nameTable,
			blockTable, volumeInfoTable, writeableEntryTable, writeableSharedEntryInfoTable, hashDigestTable);
	}

	private static NefsHeaderBlockTable200 Build200BlockTable(NefsItemList items)
	{
		var entries = new List<NefsTocBlock200>(items.Count);
		foreach (var item in items.EnumerateById())
		{
			if (item.Type == NefsItemType.Directory || item.DataSource.Size.Chunks.Count == 0 || item.IsDuplicate)
			{
				// Item does not have blocks
				continue;
			}

			// Create entry for each data block
			foreach (var chunk in item.DataSource.Size.Chunks)
			{
				var entry = new NefsTocBlock200
				{
					End = chunk.CumulativeSize
				};

				entries.Add(entry);
			}
		}

		return new NefsHeaderBlockTable200(entries);
	}

	/// <inheritdoc />
	protected override uint GetFirstBlock(NefsItem item, ref uint firstBlock)
	{
		if (item.Type == NefsItemType.Directory)
		{
			return 0;
		}

		if (item.DataSource.Size.Chunks.Count == 0)
		{
			return NefsConstants.NoBlocksIndex;
		}

		// Item is compressed; get current block, and increment for the next item
		var current = firstBlock;
		firstBlock += (uint)item.DataSource.Size.Chunks.Count;
		return current;
	}

	/// <inheritdoc />
	protected override ushort GetItemFlags(NefsItem item)
	{
		var flags = NefsTocEntryFlags200.None;
		flags |= item.Attributes.V20IsZlib ? NefsTocEntryFlags200.IsZlib : 0;
		flags |= item.Attributes.V20IsAes ? NefsTocEntryFlags200.IsAes : 0;
		flags |= item.Attributes.IsDirectory ? NefsTocEntryFlags200.IsDirectory : 0;
		flags |= item.Attributes.IsDuplicated ? NefsTocEntryFlags200.IsDuplicated : 0;
		flags |= item.Attributes.IsLastSibling ? NefsTocEntryFlags200.LastSibling : 0;
		return (ushort)flags;
	}
}
