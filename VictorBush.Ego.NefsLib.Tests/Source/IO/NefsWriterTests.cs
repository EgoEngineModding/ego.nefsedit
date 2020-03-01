// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.IO
{
    using System.IO.Abstractions;
    using System.Threading;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class NefsWriterTests
    {
        private readonly NefsProgress p = new NefsProgress(CancellationToken.None);

        //[Fact]
        //public async void CompressFileAsync_1Chunk_FileCompressed()
        //{
        //    var input = @"C:\Users\Victor\Desktop\nefswork\1 chunk\int_fr5.xml";
        //    var output = @"C:\Users\Victor\Desktop\nefswork\1 chunk\lol_lol.dat";
        //    var temp = @"C:\Users\Victor\Desktop\nefswork\1 chunk\temp\";

        //    var writer = new NefsWriter(temp, new FileSystem());
        //    await writer.CompressFileAsync(input, output, NefsArchive.ChunkSize, this.p);
        //}

        //[Fact]
        //public async void IDK_LOL()
        //{
        //    var input = @"E:\Applications\Steam\steamapps\common\DiRT Rally 2.0\cars\fr2.nefs";
        //    var output = @"E:\Libraries\Desktop\Temp\dirt\lol.nefs";
        //    var temp = @"E:\Libraries\Desktop\Temp\dirt\temp\";

        //    var reader = new NefsReader(new FileSystem());
        //    var nefs = await reader.ReadArchiveAsync(input, new NefsProgress(new CancellationTokenSource().Token));

        //    var writer = new NefsWriter(temp, new FileSystem());
        //    await writer.WriteArchiveAsync(output, nefs, new NefsProgress(new CancellationTokenSource().Token));
        //}
    }
}
