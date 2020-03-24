// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.TestArchives
{
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    internal class TestArchiveNoItems
    {
        /// <summary>
        /// Creates a test archive. Does not write an archive to disk. Just creates a <see
        /// cref="NefsArchive"/> object.
        /// </summary>
        /// <param name="filePath">The file path to use for the archive.</param>
        /// <returns>A <see cref="NefsArchive"/>.</returns>
        public static NefsArchive Create(string filePath)
        {
            var items = new NefsItemList(filePath);

            Assert.Empty(items);

            var intro = new NefsHeaderIntro();
            intro.NumberOfItems.Value = (uint)items.Count;
            var toc = new NefsHeaderIntroToc();

            var header = new NefsHeader(intro, toc, items);

            return new NefsArchive(header, items);
        }
    }
}
