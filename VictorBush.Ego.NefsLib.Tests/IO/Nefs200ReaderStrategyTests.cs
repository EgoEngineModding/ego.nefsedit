// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class Nefs200ReaderStrategyTests
{
	private readonly NefsProgress p = new(CancellationToken.None);

	[Fact]
	public async Task ReadHeaderPart4Async_ValidData_DataRead()
	{
		// Setup data
		byte[] bytes =
		{
			// Offset
			0xFF, 0xFF,

			// Item 1
			0x11, 0x12, 0x13, 0x14,
			0x15, 0x16, 0x17, 0x18,

			// Item 2
			0x21, 0x22, 0x23, 0x24,

			// Item 5
			0x31, 0x32, 0x33, 0x34,
			0x35, 0x36, 0x37, 0x38,
			0x39, 0x3A, 0x3B, 0x3C,

			// Last four bytes
			0x01, 0x02, 0x03, 0x04,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var size = 28;
		var offset = 2;

		// Test
		var part4 = await Nefs200ReaderStrategy.ReadHeaderPart4Version20Async(br, offset, size, this.p);

		// Verify
		Assert.Equal(7, part4.Entries.Count);

		// Item 1
		Assert.Equal((uint)0x14131211, part4.Entries[0].End);
		Assert.Equal((uint)0x18171615, part4.Entries[1].End);

		// Item 2
		Assert.Equal((uint)0x24232221, part4.Entries[2].End);

		// Item 3
		Assert.Equal((uint)0x34333231, part4.Entries[3].End);
		Assert.Equal((uint)0x38373635, part4.Entries[4].End);
		Assert.Equal((uint)0x3C3B3A39, part4.Entries[5].End);
	}
}
