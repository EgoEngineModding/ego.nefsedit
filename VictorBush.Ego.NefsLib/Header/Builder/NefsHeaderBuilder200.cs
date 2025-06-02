// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsHeaderBuilder200 : NefsHeaderBuilder160Base<Nefs200Header>
{
	internal override uint ComputeDataOffset(Nefs200Header sourceHeader, NefsItemList items)
	{
		var nonDuplicateCount = items.EnumerateById().Count(x => !x.IsDuplicate);
		var entryTableSize = items.Count * Nefs160TocEntry.ByteCount;
		var sharedEntryInfoTableSize = nonDuplicateCount * Nefs160TocSharedEntryInfo.ByteCount;
		var nameTableSize = items.EnumerateById().Where(x => !x.IsDuplicate).Distinct().Sum(x => x.FileName.Length);
		var blockTableSize = items.EnumerateById().Where(x => !x.IsDuplicate).Sum(x => x.DataSource.Size.Chunks.Count) * Nefs200TocBlock.ByteCount;
		var volumeInfoTableSize = sourceHeader.Volumes.Count * Nefs150TocVolumeInfo.ByteCount;
		var writeableEntryTableSize = items.Count * Nefs160TocEntryWriteable.ByteCount;
		var writeableSharedEntryInfoTableSize = nonDuplicateCount * Nefs160TocSharedEntryInfoWriteable.ByteCount;

		var hashBlockSize = sourceHeader.TableOfContents.HashBlockSize;
		var dataSize = items.EnumerateById().Sum(x => x.CompressedSize);
		var numHashDigests = hashBlockSize == 0 ? 0 : (dataSize + hashBlockSize - 1) / hashBlockSize;
		var hashDigestTableSize = numHashDigests * Nefs160TocHashDigest.ByteCount;

		var sizeSum = Nefs160TocHeaderA.ByteCount + Nefs200TocHeaderB.ByteCount + entryTableSize +
		              sharedEntryInfoTableSize + nameTableSize + blockTableSize + volumeInfoTableSize +
		              writeableEntryTableSize + writeableSharedEntryInfoTableSize + hashDigestTableSize;
		var tocSize = StructEx.Align(sizeSum, NefsConstants.IntroSize);
		var tocFinalEnd = Convert.ToUInt32(StructEx.Align(tocSize, Convert.ToInt32(sourceHeader.BlockSize)));
		return tocFinalEnd;
	}

	internal override Nefs200Header Build(Nefs200Header sourceHeader, NefsItemList items, NefsProgress p)
	{
		p.CancellationToken.ThrowIfCancellationRequested();
		var entryTable = BuildEntryTable160(items);
		var nameTable = new NefsHeaderPart3(items);
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
		var toc = new Nefs200TocHeaderB
		{
			NumVolumes = sourceHeader.TableOfContents.NumVolumes,
			HashBlockSize = hashBlockSize,
			EntryTableStart = Convert.ToUInt32(Nefs160TocHeaderA.ByteCount + Nefs200TocHeaderB.ByteCount),
			RandomPadding = sourceHeader.TableOfContents.RandomPadding
		};
		toc.SharedEntryInfoTableStart = Convert.ToUInt32(toc.EntryTableStart + entryTable.ByteCount());
		toc.NameTableStart = Convert.ToUInt32(toc.SharedEntryInfoTableStart + sharedEntryInfoTable.ByteCount());
		toc.BlockTableStart =
			Convert.ToUInt32(StructEx.Align<Nefs200TocBlock>(toc.NameTableStart + nameTable.Size));
		toc.VolumeInfoTableStart =
			Convert.ToUInt32(StructEx.Align<Nefs150TocVolumeInfo>(toc.BlockTableStart + blockTable.ByteCount()));
		toc.WritableEntryTableStart =
			Convert.ToUInt32(toc.VolumeInfoTableStart + toc.NumVolumes * Nefs150TocVolumeInfo.ByteCount);
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
		var tocSize = StructEx.Align(tocEnd, NefsConstants.IntroSize);
		var tocFinalEnd = Convert.ToUInt32(StructEx.Align(tocSize, Convert.ToInt32(sourceHeader.BlockSize)));
		var p5 = new NefsHeaderPart5
		{
			DataFileNameStringOffset = nameTable.OffsetsByFileName[items.DataFileName],
			// Single-file archives this is the size of the whole file
			DataSize = tocFinalEnd + dataSize,
			FirstDataOffset = tocFinalEnd,
		};

		var intro = new Nefs160TocHeaderA
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

		return new Nefs200Header(sourceHeader.WriterSettings, intro, toc, entryTable, sharedEntryInfoTable, nameTable,
			blockTable, p5, writeableEntryTable, writeableSharedEntryInfoTable, hashDigestTable);
	}

	private static Nefs200HeaderBlockTable Build200BlockTable(NefsItemList items)
	{
		var entries = new List<Nefs200TocBlock>(items.Count);
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
				var entry = new Nefs200TocBlock
				{
					End = chunk.CumulativeSize
				};

				entries.Add(entry);
			}
		}

		return new Nefs200HeaderBlockTable(entries);
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
		var flags = Nefs200TocEntryFlags.None;
		flags |= item.Attributes.V20IsZlib ? Nefs200TocEntryFlags.IsZlib : 0;
		flags |= item.Attributes.V20IsAes ? Nefs200TocEntryFlags.IsAes : 0;
		flags |= item.Attributes.IsDirectory ? Nefs200TocEntryFlags.IsDirectory : 0;
		flags |= item.Attributes.IsDuplicated ? Nefs200TocEntryFlags.IsDuplicated : 0;
		flags |= item.Attributes.IsLastSibling ? Nefs200TocEntryFlags.LastSibling : 0;
		return (ushort)flags;
	}
}
