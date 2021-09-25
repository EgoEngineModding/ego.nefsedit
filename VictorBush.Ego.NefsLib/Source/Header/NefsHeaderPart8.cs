// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Source.Utility;

    /// <summary>
    /// A list of SHA-256 hashes of the archive's file data. The size of the hashed chunks are
    /// typically 0x800000. NeFS version 1.6 has a Block Size property that can specify this size.
    /// There are some unknown oddities. The last chunk of data does not seem to have a
    /// corresponding hash. There is additional data at the end of part 8 that is unknown.
    /// </summary>
    public class NefsHeaderPart8
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart8"/> class.
        /// </summary>
        internal NefsHeaderPart8(IEnumerable<Sha256Hash> hashes)
        {
            var numHashes = hashes.Count();
            this.Data0x00_FileDataHashes = new ListType<Sha256Hash>(0, Sha256Hash.Size, numHashes, bytes => new Sha256Hash(bytes), hash => hash.Value);
            this.Data0x00_FileDataHashes.SetItems(hashes);
        }

        internal NefsHeaderPart8(int numberOfHashes)
        {
            this.Data0x00_FileDataHashes = new ListType<Sha256Hash>(0, Sha256Hash.Size, numberOfHashes, bytes => new Sha256Hash(bytes), hash => hash.Value);
        }

        public int Size => this.Data0x00_FileDataHashes.Size;

        public IReadOnlyList<Sha256Hash> FileDataHashes => this.Data0x00_FileDataHashes.Items;

        [FileData]
        internal ListType<Sha256Hash> Data0x00_FileDataHashes { get; }
    }
}
