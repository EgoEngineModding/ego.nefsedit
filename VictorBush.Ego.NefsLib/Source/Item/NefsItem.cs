// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;
    using VictorBush.Ego.NefsLib.DataSource;

    /// <summary>
    /// An item in a NeFS archive (file or directory).
    /// </summary>
    public class NefsItem : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItem"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier for this item.</param>
        /// <param name="id">The item id.</param>
        /// <param name="fileName">The file name within the archive.</param>
        /// <param name="directoryId">The directory id the item is in.</param>
        /// <param name="type">The type of item.</param>
        /// <param name="dataSource">The data source for the item's data.</param>
        /// <param name="transform">
        /// The transform that is applied to this item's data. Can be null if no transform.
        /// </param>
        /// <param name="unknownData">Unknown metadata.</param>
        /// <param name="state">The item state.</param>
        public NefsItem(
            Guid guid,
            NefsItemId id,
            string fileName,
            NefsItemId directoryId,
            NefsItemType type,
            INefsDataSource dataSource,
            NefsDataTransform transform,
            NefsItemUnknownData unknownData,
            NefsItemState state = NefsItemState.None)
        {
            this.Guid = guid;
            this.Id = id;
            this.DirectoryId = directoryId;
            this.Type = type;
            this.DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.State = state;
            this.Transform = transform;

            // Unknown data
            this.Part6Unknown0x00 = unknownData.Part6Unknown0x00;
            this.Part6Unknown0x01 = unknownData.Part6Unknown0x01;
            this.Part6Unknown0x02 = unknownData.Part6Unknown0x02;
            this.Part6Unknown0x03 = unknownData.Part6Unknown0x03;

            // Save file name
            this.FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        /// <summary>
        /// The size of the item's data in the archive.
        /// </summary>
        public UInt32 CompressedSize => this.DataSource?.Size.TransformedSize ?? 0;

        /// <summary>
        /// The current data source for this item.
        /// </summary>
        public INefsDataSource DataSource { get; private set; }

        /// <summary>
        /// The id of the directory item this item is in. When the item is in the root directory,
        /// the parent id is the same as the item's id.
        /// </summary>
        public NefsItemId DirectoryId { get; }

        /// <summary>
        /// The size of the item's data when extracted from the archive.
        /// </summary>
        public UInt32 ExtractedSize => this.DataSource?.Size.ExtractedSize ?? 0;

        /// <summary>
        /// The item's file or directory name, depending on type.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// A unique identifier for this item.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// The id of this item. This is not guaranteed to be unique in an archive, there can be
        /// items with duplicate id values. For a unique identifer, use <see cref="Guid"/>.
        /// </summary>
        public NefsItemId Id { get; }

        /// <summary>
        /// Unknown data in the part 6 entry.
        /// </summary>
        public byte Part6Unknown0x00 { get; }

        /// <summary>
        /// Unknown data in the part 6 entry.
        /// </summary>
        public byte Part6Unknown0x01 { get; }

        /// <summary>
        /// Unknown data in the part 6 entry.
        /// </summary>
        public byte Part6Unknown0x02 { get; }

        /// <summary>
        /// Unknown data in the part 6 entry.
        /// </summary>
        public byte Part6Unknown0x03 { get; }

        /// <summary>
        /// The modification state of the item. Represents any pending changes to this item. Pending
        /// changes are applied when the archive is saved.
        /// </summary>
        public NefsItemState State { get; private set; }

        /// <summary>
        /// The transform that is applied to this item's data. Is null if no transform.
        /// </summary>
        public NefsDataTransform Transform { get; }

        /// <summary>
        /// The type of item this is.
        /// </summary>
        public NefsItemType Type { get; }

        /// <summary>
        /// Clones this item metadata.
        /// </summary>
        /// <returns>A new <see cref="NefsItem"/>.</returns>
        public object Clone()
        {
            var unknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = this.Part6Unknown0x00,
                Part6Unknown0x01 = this.Part6Unknown0x01,
                Part6Unknown0x02 = this.Part6Unknown0x02,
                Part6Unknown0x03 = this.Part6Unknown0x03,
            };

            return new NefsItem(
                this.Guid,
                this.Id,
                this.FileName,
                this.DirectoryId,
                this.Type,
                this.DataSource,
                this.Transform,
                unknownData,
                state: this.State);
        }

        /// <summary>
        /// Updates the data source for the item and the item's state. If the data source has
        /// changed, the new item data will be written to the archive when it is saved. If
        /// compression is required, the file data will be compressed when the archive is written.
        /// </summary>
        /// <param name="dataSource">The new data source to use.</param>
        /// <param name="state">The new item state.</param>
        public void UpdateDataSource(INefsDataSource dataSource, NefsItemState state)
        {
            if (this.Type == NefsItemType.Directory)
            {
                throw new InvalidOperationException($"Cannot perform {nameof(this.UpdateDataSource)} on a directory.");
            }

            this.DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.State = state;
        }

        /// <summary>
        /// Updates the item state.
        /// </summary>
        /// <param name="state">The new state.</param>
        public void UpdateState(NefsItemState state)
        {
            this.State = state;
        }
    }
}
