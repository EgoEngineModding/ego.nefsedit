// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.DataTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class ListTypeTests
    {
        [Fact]
        public async Task ListType_ListHasData_ItemsLoaded()
        {
            var itemSize = 0x2;
            var testBytes = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };

            using (var stream = new MemoryStream(testBytes))
            {
                var data = new ListType<byte[]>(0, itemSize, 3, bytes => bytes.ToArray(), bytes => bytes);
                await data.ReadAsync(stream, 0, new NefsProgress());

                Assert.Equal(3, data.ItemCount);
                Assert.True(data.Items[0].SequenceEqual(new byte[] { 0x1, 0x2 }));
                Assert.True(data.Items[1].SequenceEqual(new byte[] { 0x3, 0x4 }));
                Assert.True(data.Items[2].SequenceEqual(new byte[] { 0x5, 0x6 }));
                Assert.True(testBytes.SequenceEqual(data.GetBytes()));
            }
        }

        [Fact]
        public void SetItems_ItemsAreValid_ItemsLoaded()
        {
            var itemSize = 0x2;
            var testBytes = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
            var testItems = new List<byte[]>
            {
                new byte[] { 0x1, 0x2 },
                new byte[] { 0x3, 0x4 },
                new byte[] { 0x5, 0x6 },
            };

            var data = new ListType<byte[]>(0, itemSize, 3, bytes => bytes.ToArray(), bytes => bytes);
            data.SetItems(testItems);

            Assert.Equal(itemSize, data.ItemSize);
            Assert.Equal(3, data.ItemCount);
            Assert.Equal(3, data.Items.Count);
            Assert.True(data.Items[0].SequenceEqual(new byte[] { 0x1, 0x2 }));
            Assert.True(data.Items[1].SequenceEqual(new byte[] { 0x3, 0x4 }));
            Assert.True(data.Items[2].SequenceEqual(new byte[] { 0x5, 0x6 }));
            Assert.True(testBytes.SequenceEqual(data.GetBytes()));
        }
    }
}
