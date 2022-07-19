// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.DataSource;

public sealed class NefsDataChunkTests
{
	[Fact]
	public void CreateChunkList_EmptyCumulativeSizes_EmptyListReturned()
	{
		var result = NefsDataChunk.CreateChunkList(Array.Empty<uint>(), new NefsDataTransform(0x10));
		Assert.Empty(result);
	}

	[Fact]
	public void CreateChunkList_MultipleCumulativeSize_MultipleChunksReturned()
	{
		var transform = new NefsDataTransform(0x50);
		var result = NefsDataChunk.CreateChunkList(new[] { 0x10u, 0x20u, 0x2Bu }, transform);
		Assert.Equal(3, result.Count);

		Assert.Equal(0x10u, result[0].Size);
		Assert.Equal(0x10u, result[0].CumulativeSize);
		Assert.Equal(transform, result[0].Transform);

		Assert.Equal(0x10u, result[1].Size);
		Assert.Equal(0x20u, result[1].CumulativeSize);
		Assert.Equal(transform, result[1].Transform);

		Assert.Equal(0xBu, result[2].Size);
		Assert.Equal(0x2Bu, result[2].CumulativeSize);
		Assert.Equal(transform, result[2].Transform);
	}

	[Fact]
	public void CreateChunkList_SingleCumulativeSize_SingleChunkReturned()
	{
		var transform = new NefsDataTransform(0x10);
		var result = NefsDataChunk.CreateChunkList(new[] { 0x10u }, transform);
		Assert.Single(result);
		Assert.Equal(0x10u, result[0].Size);
		Assert.Equal(0x10u, result[0].CumulativeSize);
		Assert.Equal(transform, result[0].Transform);
	}
}
