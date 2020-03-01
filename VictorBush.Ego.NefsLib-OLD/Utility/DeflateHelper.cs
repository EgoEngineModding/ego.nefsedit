using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.Utility
{
    class DeflateHelper
    {
        /// <summary>
        /// Takes data from an input file stream, compresses it, and writes it to the specified
        /// output file. File streams should already seek to the proper location before calling
        /// this function.
        /// </summary>
        /// <param name="infs">The input file stream to read from.</param>
        /// <param name="numBytes">Number of bytes to read in.</param>
        /// <param name="outfs">The output file stream to write compressed chunk to.</param>
        /// <param name="chunkSize">The compressed size of the chunk that was written.</param>
        /// <returns>Number of bytes actually read from input file.</returns>
        public static int DeflateToFile(FileStream infs, int numBytes, FileStream outfs, out int chunkSize)
        {
            /* Read in the input data to compress */
            var inData = new byte[numBytes];
            var bytesRead = infs.Read(inData, 0, numBytes);
            chunkSize = 0;

            /* Deflate stream doesn't write properly directly to a FileStream when
             * doing this chunk business. So have to do this multi-stream setup. */

            using (var inStream = new MemoryStream())
            using (var outStream = new MemoryStream())
            using (var deflateStream = new DeflateStream(outStream, CompressionMode.Compress))
            {
                /* Read input chunk into memory stream */
                inStream.Write(inData, 0, bytesRead);
                inStream.Seek(0, SeekOrigin.Begin);

                /* Compress the chunk with deflate stream */
                inStream.CopyTo(deflateStream, bytesRead);

                /* Close deflate stream to finalize compression - breaking 
                 * the "using()" convention, but this is needed */
                deflateStream.Close();

                /* Write the compressed chunk to the output file */
                var compressedData = outStream.ToArray();
                chunkSize = (int)compressedData.Length;
                outfs.Write(compressedData, 0, compressedData.Length);
            }
            
            return bytesRead;
        }
    }
}
