// // See LICENSE.txt for license information.
//
// using VictorBush.Ego.NefsLib.Item;
//
// namespace VictorBush.Ego.NefsLib.Header;
//
// /// <summary>
// /// Header part 6.
// /// </summary>
// public sealed class Nefs20HeaderPart6
// {
// 	/// <summary>
// 	/// Initializes a new instance of the <see cref="Nefs20HeaderPart6"/> class from a list of items.
// 	/// </summary>
// 	/// <param name="items">The list of items in the archive.</param>
// 	internal Nefs20HeaderPart6(NefsItemList items)
// 	{
// 		this.entriesByIndex = new List<Nefs20HeaderPart6Entry>();
// 		this.entriesByGuid = new Dictionary<Guid, Nefs20HeaderPart6Entry>();
//
// 		// Sort part 6 by item id. Part 1 and part 6 order must match.
// 		foreach (var item in items.EnumerateById())
// 		{
// 			var flags = Nefs200TocEntryFlags.None;
// 			flags |= item.Attributes.V20IsZlib ? Nefs200TocEntryFlags.IsZlib : 0;
// 			flags |= item.Attributes.V20IsAes ? Nefs200TocEntryFlags.IsAes : 0;
// 			flags |= item.Attributes.IsDirectory ? Nefs200TocEntryFlags.IsDirectory : 0;
// 			flags |= item.Attributes.IsDuplicated ? Nefs200TocEntryFlags.IsDuplicated : 0;
// 			flags |= item.Attributes.V20Unknown0x10 ? Nefs200TocEntryFlags.Unknown0x10 : 0;
// 			flags |= item.Attributes.V20Unknown0x20 ? Nefs200TocEntryFlags.Unknown0x20 : 0;
// 			flags |= item.Attributes.V20Unknown0x40 ? Nefs200TocEntryFlags.Unknown0x40 : 0;
// 			flags |= item.Attributes.V20Unknown0x80 ? Nefs200TocEntryFlags.Unknown0x80 : 0;
//
// 			var entry = new Nefs20HeaderPart6Entry(item.Guid)
// 			{
// 				Flags = flags,
// 				Volume = item.Attributes.Part6Volume,
// 			};
//
// 			this.entriesByGuid.Add(item.Guid, entry);
// 			this.entriesByIndex.Add(entry);
// 		}
// 	}
// }
