// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Source.Tests.Utility
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Utility;
    using Xunit;

    public sealed class StreamExtensionsTests
    {
        [Fact]
        public async Task CopyPartialAsync_LengthGreaterThanCopyBuffer()
        {
            var inputLength = StreamExtensions.CopyBufferSize + 8;
            var input = new byte[inputLength];
            var copyLength = StreamExtensions.CopyBufferSize + 4;

            using (var inputStream = new MemoryStream(input))
            using (var outputStream = new MemoryStream())
            {
                await inputStream.CopyPartialAsync(outputStream, copyLength, CancellationToken.None);

                var output = new byte[outputStream.Length];
                await outputStream.ReadAsync(output, 0, (int)outputStream.Length);

                Assert.Equal(outputStream.Length, copyLength);

                for (var i = 0; i < outputStream.Length; ++i)
                    Assert.Equal(input[i], output[i]);
            }
        }
    }
}
