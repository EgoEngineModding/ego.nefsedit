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
