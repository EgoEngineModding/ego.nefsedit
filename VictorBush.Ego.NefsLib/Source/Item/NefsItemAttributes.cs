// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;

    /// <summary>
    /// Additional attributes that describe an item.
    /// </summary>
    public struct NefsItemAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemAttributes"/> struct.
        /// </summary>
        /// <param name="isCacheable">Indicates if item is cacheable.</param>
        /// <param name="isDirectory">Indicates if item is directory.</param>
        /// <param name="isDuplicated">Indicates if item has duplicates.</param>
        /// <param name="isPatched">Indicates if item is patched.</param>
        /// <param name="isTransformed">Indicates if item's data has transformations applied.</param>
        /// <param name="v16Unknown0x10">Unknown flag 0x10 (v1.6).</param>
        /// <param name="v16Unknown0x40">Unknown flag 0x40 (v1.6).</param>
        /// <param name="v16Unknown0x80">Unknown flag 0x80 (v1.6).</param>
        /// <param name="v20Unknown0x02">Unknown flag 0x02.</param>
        /// <param name="v20Unknown0x10">Unknown flag 0x10.</param>
        /// <param name="v20Unknown0x20">Unknown flag 0x20.</param>
        /// <param name="v20Unknown0x40">Unknown flag 0x40.</param>
        /// <param name="v20Unknown0x80">Unknown flag 0x80.</param>
        /// <param name="part6Volume">Item volume.</param>
        /// <param name="part6Unknown0x3">Unknown value.</param>
        public NefsItemAttributes(
            bool isCacheable = false,
            bool isDirectory = false,
            bool isDuplicated = false,
            bool isPatched = false,
            bool isTransformed = false,
            bool v16Unknown0x10 = false,
            bool v16Unknown0x40 = false,
            bool v16Unknown0x80 = false,
            bool v20Unknown0x02 = false,
            bool v20Unknown0x10 = false,
            bool v20Unknown0x20 = false,
            bool v20Unknown0x40 = false,
            bool v20Unknown0x80 = false,
            UInt16 part6Volume = 0,
            byte part6Unknown0x3 = 0)
        {
            this.IsCacheable = isCacheable;
            this.IsDirectory = isDirectory;
            this.IsDuplicated = isDuplicated;
            this.IsPatched = isPatched;
            this.IsTransformed = isTransformed;
            this.V16Unknown0x10 = v16Unknown0x10;
            this.V16Unknown0x40 = v16Unknown0x40;
            this.V16Unknown0x80 = v16Unknown0x80;
            this.V20Unknown0x02 = v20Unknown0x02;
            this.V20Unknown0x10 = v20Unknown0x10;
            this.V20Unknown0x20 = v20Unknown0x20;
            this.V20Unknown0x40 = v20Unknown0x40;
            this.V20Unknown0x80 = v20Unknown0x80;
            this.Part6Volume = part6Volume;
            this.Part6Unknown0x3 = part6Unknown0x3;
        }

        /// <summary>
        /// A flag indicating whether this item is cacheable (presumably by the game engine).
        /// </summary>
        public bool IsCacheable { get; }

        /// <summary>
        /// A flag indicating whether this item is a directory.
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// A flag indicating whether this item is duplicated.
        /// </summary>
        public bool IsDuplicated { get; }

        /// <summary>
        /// A flag indicating whether this item is patched (meaning unknown).
        /// </summary>
        public bool IsPatched { get; }

        /// <summary>
        /// A flag indicating whether the item's data has transforms applied (compressed, encrypted, etc).
        /// </summary>
        public bool IsTransformed { get; }

        /// <summary>
        /// Unknown data (from part 6).
        /// </summary>
        public byte Part6Unknown0x3 { get; }

        /// <summary>
        /// Meaning unknown (from part 6).
        /// </summary>
        public UInt16 Part6Volume { get; }

        /// <summary>
        /// Version 1.6 unknown flag.
        /// </summary>
        public bool V16Unknown0x10 { get; }

        /// <summary>
        /// Version 1.6 unknown flag.
        /// </summary>
        public bool V16Unknown0x40 { get; }

        /// <summary>
        /// Version 1.6 unknown flag.
        /// </summary>
        public bool V16Unknown0x80 { get; }

        /// <summary>
        /// Version 2.0 unknown flag.
        /// </summary>
        public bool V20Unknown0x02 { get; }

        /// <summary>
        /// Version 2.0 unknown flag.
        /// </summary>
        public bool V20Unknown0x10 { get; }

        /// <summary>
        /// Version 2.0 unknown flag.
        /// </summary>
        public bool V20Unknown0x20 { get; }

        /// <summary>
        /// Version 2.0 unknown flag.
        /// </summary>
        public bool V20Unknown0x40 { get; }

        /// <summary>
        /// Version 2.0 unknown flag.
        /// </summary>
        public bool V20Unknown0x80 { get; }
    }
}
