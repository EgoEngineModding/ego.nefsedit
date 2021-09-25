//using System;
//using System.Collections.Generic;
//using System.IO.Abstractions;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using VictorBush.Ego.NefsLib.Utility;

//namespace VictorBush.Ego.NefsLib.Source.IO
//{
//    public class NefsExeSearcher
//    {
//        public NefsExeSearcher(IFileSystem fileSystem)
//        {
//            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
//        }

//        public IFileSystem FileSystem { get; }
//        private static readonly ILogger Log = NefsLog.GetLogger();

//        public async Task<List<NefsArchiveSource>> FindHeadersAsync(string exePath, string dataFileDir, NefsProgress p)
//        {

//        }

//        public async Task SDFDSF(byte[] exeBytes)
//        {
//            var offset = 0U;
//            var peRdataOffset = GetAbsoluteOffsetToRdataSection(exeBytes);
//            var peDataOffset = GetAbsoluteOffsetToDataSection(exeBytes);

//            var nextNefsHeaderOffset = 0U;

//            // do until no more nefs headers found
//            {
//                // Compute relative offset
//                // Search for references to that offset (if more than one, fail)
//                // Try to disassemble the code at that offset?
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        private ulong GetAbsoluteOffsetToRdataSection(byte[] exeBytes)
//        {
//            if (!PeHelper.GetRawOffsetToSection(exeBytes, ".rdata", out var sectionOffset))
//            {
//                throw new ArgumentException("Failed to find .rdata section in the executable.");
//            }

//            return sectionOffset;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        private ulong GetAbsoluteOffsetToDataSection(byte[] exeBytes)
//        {
//            if (!PeHelper.GetRawOffsetToSection(exeBytes, ".data", out var sectionOffset))
//            {
//                throw new ArgumentException("Failed to find .data section in the executable.");
//            }

//            return sectionOffset;
//        }

//        private async Task<uint> FindHeaderOffsetAsync(byte[] exeBytes, uint nextOffset)
//        {
//            var sources = new List<NefsArchiveSource>();
//            var nextPart6Offset = 0U;

//            // Load exe into memory
//            var exeBytes = this.FileSystem.File.ReadAllBytes(exePath);

//            // Search for the part 6 base offset. For NeFS version 1.6 and 2.0 (maybe others?)
//            // header parts 6 and 7 are stored separate from the other header parts. So far all the
//            // part 6/7 data has been in the ".data" section of the exe. So we can get that offset
//            // from the PE header. Some games (e.g. Grid 2) have other data that comes before the
//            // part 6/7 data in the ".data" section. So we have to look for a pattern that looks
//            // like the data we are looking for.
//            try
//            {
//                if (!PeHelper.GetRawOffsetToSection(exeBytes, ".data", out var dataSectionOffset))
//                {
//                    Log.LogError("Failed to find part 6 offset; using 0 as offset.");
//                }

//                nextPart6Offset = (uint)dataSectionOffset;
//            }
//            catch (Exception ex)
//            {
//                Log.LogError(ex, "Failed to find part 6 offset; using 0 as offset.");
//            }

//            // Search for headers
//            var i = 0;
//            while (i + 4 <= exeBytes.Length)
//            {
//                var offset = i;
//                i += 4;

//                // Check for cancel
//                p.CancellationToken.ThrowIfCancellationRequested();

//                // Searching for a NeFS header: Look for 4E 65 46 53 (NeFS). This is the NeFS header
//                // magic number.
//                if (exeBytes[offset] != 0x4E
//                    || exeBytes[offset + 1] != 0x65
//                    || exeBytes[offset + 2] != 0x46
//                    || exeBytes[offset + 3] != 0x53)
//                {
//                    continue;
//                }

//                // Check for a known version number
//                var version = BitConverter.ToUInt32(exeBytes, offset + 0x68);
//                if (version != 0x20000 && version != 0x10600)
//                {
//                    continue;
//                }

//                // Try to read header intro
//                try
//                {
//                    using (var byteStream = new MemoryStream(exeBytes))
//                    {
//                        var (intro, headerStream) = await this.ReadHeaderIntroAsync(byteStream, (ulong)offset, p);
//                        using (headerStream)
//                        {
//                            INefsHeaderIntroToc toc;
//                            uint p6Size;
//                            uint p7Size;

//                            // Find next part 6 offset - there may be padding or other data before
//                            // the part 6/7 data
//                            nextPart6Offset = this.FindNextPart6Offset(nextPart6Offset, exeBytes);

//                            // Read table of contents
//                            if (version == (int)NefsVersion.Version200)
//                            {
//                                toc = await this.Read20HeaderIntroTocAsync(headerStream, Nefs20HeaderIntroToc.Offset, p);

//                                var numPart1Entries = toc.Part1Size / NefsHeaderPart1Entry.Size;
//                                var numPart2Entries = toc.Part2Size / NefsHeaderPart2Entry.Size;
//                                p6Size = numPart1Entries * Nefs20HeaderPart6Entry.Size;
//                                p7Size = numPart2Entries * NefsHeaderPart7Entry.Size;
//                            }
//                            else
//                            {
//                                toc = await this.Read16HeaderIntroTocAsync(headerStream, Nefs16HeaderIntroToc.Offset, p);

//                                var numPart1Entries = toc.Part1Size / NefsHeaderPart1Entry.Size;
//                                var numPart2Entries = toc.Part2Size / NefsHeaderPart2Entry.Size;
//                                p6Size = numPart1Entries * Nefs16HeaderPart6Entry.Size;
//                                p7Size = numPart2Entries * NefsHeaderPart7Entry.Size;
//                            }

//                            // Read part 5
//                            var p5 = await this.ReadHeaderPart5Async(headerStream, toc.OffsetToPart5, NefsHeaderPart5.Size, p);

//                            // Find file name
//                            headerStream.Seek(toc.OffsetToPart3, SeekOrigin.Begin);
//                            headerStream.Seek(p5.ArchiveNameStringOffset, SeekOrigin.Current);

//                            // Read 256 bytes - this is overkill, probably won't have a filename
//                            // that big
//                            var nameBytes = new byte[256];
//                            await headerStream.ReadAsync(nameBytes, 0, 256, p.CancellationToken);

//                            var name = StringHelper.TryReadNullTerminatedAscii(nameBytes, 0, nameBytes.Length);
//                            if (string.IsNullOrWhiteSpace(name))
//                            {
//                                // Failed to get name
//                                Log.LogError($"Thought we found a header at {offset}, but could not read data file name.");
//                                continue;
//                            }

//                            // Create archive source for this header
//                            var dataFilePath = Path.Combine(dataFileDir, name);
//                            var source = new NefsArchiveSource(exePath, (ulong)offset, nextPart6Offset, dataFilePath);
//                            sources.Add(source);

//                            // Keep looking
//                            offset += (int)intro.HeaderSize;

//                            // Update part 6 search offset to skip the one we just used
//                            nextPart6Offset += p6Size + p7Size;
//                        }
//                    }
//                }
//                catch (Exception)
//                {
//                    // Failed to read header, so assume not a header
//                    continue;
//                }
//            }

//            return sources;
//        }
//    }
//}
