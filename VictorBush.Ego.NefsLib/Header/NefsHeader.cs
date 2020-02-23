using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.Header;

namespace VictorBush.Ego.NefsLib.Header
{
    public class NefsHeader
    {
        NefsArchive _archive;
        NefsHeaderIntro _intro;
        NefsHeaderPt1 _part1;
        NefsHeaderPt2 _part2;
        NefsHeaderPt3 _part3;
        NefsHeaderPt4 _part4;
        NefsHeaderPt5 _part5;
        NefsHeaderPt6 _part6;
        NefsHeaderPt7 _part7;

        internal NefsHeader(FileStream file, NefsArchive archive, NefsProgressInfo p)
        {
            _archive = archive;

            p.BeginTask(0.15f, "Reading header intro...");
            _intro = new NefsHeaderIntro(file, p);
            p.EndTask();

            Stream header;
            if (_intro.IsEncrypted)
                header = _intro.DecryptedHeader;
            else
                header = file;

            p.BeginTask(0.15f, "Reading header part 1...");
            _part1 = new NefsHeaderPt1(header, _intro.Part1Offset, _intro.Part1Size, p);
            p.EndTask();

            p.BeginTask(0.15f, "Reading header part 2...");
            _part2 = new NefsHeaderPt2(header, _intro.Part2Offset, _intro.Part2Size, p);
            p.EndTask();

            p.BeginTask(0.10f, "Reading header part 3...");
            _part3 = new NefsHeaderPt3(header, _intro.Part3Offset, _intro.Part3Size, p);
            p.EndTask();

            p.BeginTask(0.15f, "Reading header part 4...");
            _part4 = new NefsHeaderPt4(header, _intro.Part4Offset, _intro.Part4Size, p);
            p.EndTask();

            p.BeginTask(0.10f, "Reading header part 5...");
            _part5 = new NefsHeaderPt5(header, _intro.Part5Offset, _intro.Part5Size, p);
            p.EndTask();

            p.BeginTask(0.10f, "Reading header part 6...");
            _part6 = new NefsHeaderPt6(header, _intro.Part6Offset, _intro.Part6Size, p);
            p.EndTask();

            p.BeginTask(0.10f, "Reading header part 7...");
            // Theres a section of data after header part 6 and the first section of compressed data.
            // I'm not sure what it is. Just copying for now.
            var firstItemOffset = _part1.FirstItemDataOffset;
            if( firstItemOffset > 0 && firstItemOffset > _intro.DataOffset)
            {
                _part7 = new NefsHeaderPt7(header, _intro.DataOffset, (uint)firstItemOffset - _intro.DataOffset);
            }
            p.EndTask();
        }

        /// <summary>
        /// The header intro section.
        /// </summary>
        public NefsHeaderIntro Intro
        {
            get { return _intro; }
        }

        /// <summary>
        /// Header part 1.
        /// </summary>
        public NefsHeaderPt1 Part1
        {
            get { return _part1; }
        }

        /// <summary>
        /// Header part 2.
        /// </summary>
        public NefsHeaderPt2 Part2
        {
            get { return _part2; }
        }

        /// <summary>
        /// Header part 3.
        /// </summary>
        public NefsHeaderPt3 Part3
        {
            get { return _part3; }
        }

        /// <summary>
        /// Header part 4.
        /// </summary>
        public NefsHeaderPt4 Part4
        {
            get { return _part4; }
        }

        /// <summary>
        /// Header part 5.
        /// </summary>
        public NefsHeaderPt5 Part5
        {
            get { return _part5; }
        }

        /// <summary>
        /// Header part 6.
        /// </summary>
        public NefsHeaderPt6 Part6
        {
            get { return _part6; }
        }

        /// <summary>
        /// Header part 7.
        /// </summary>
        public NefsHeaderPt7 Part7
        {
            get { return _part7; }
        }

        /// <summary>
        /// Writes the header to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="p">Progress info.</param>
        public void Write(FileStream file, NefsProgressInfo p)
        {
            /* PART 1 */
            p.BeginTask(0.15f, "Writing header part 1...");
            {
                _part1.Write(file, p);

                Intro.Part1Size = Part1.Size;
                Part2.Offset = Part1.Offset + Part1.Size;
                Intro.Part2Offset = Part2.Offset;
            }
            p.EndTask();

            /* PART 2 */
            p.BeginTask(0.15f, "Writing header part 2...");
            {
                _part2.Write(file, p);

                Intro.Part2Size = Part2.Size;
                Part3.Offset = Part2.Offset + Part2.Size;
                Intro.Part3Offset = Part3.Offset;
            }
            p.EndTask();

            /* PART 3 */
            p.BeginTask(0.10f, "Writing header part 3...");
            {
                _part3.Write(file, p);

                Intro.Part3Size = Part3.Size;
                Part4.Offset = Part3.Offset + Part3.Size;
                Intro.Part4Offset = Part4.Offset;
            }
            p.EndTask();

            /* PART 4 */
            p.BeginTask(0.15f, "Writing header part 4...");
            {
                _part4.Write(file, _archive.Items, p);

                Intro.Part4Size = Part4.Size;
                Part5.Offset = Part4.Offset + Part4.Size;
                Intro.Part5Offset = Part5.Offset;
                Intro.OffsetIntoPart5 = Part5.Offset + 0x10; // Not sure what this offset is for yet
            }
            p.EndTask();

            /* PART 5 */
            p.BeginTask(0.10f, "Writing header part 5...");
            {
                _part5.Write(file, p);

                Intro.Part5Size = Part5.Size;
                Part6.Offset = Part5.Offset + Part5.Size;
                Intro.Part6Offset = Part6.Offset;
            }
            p.EndTask();

            /* PART 6 */
            p.BeginTask(0.10f, "Writing header part 6...");
            {
                _part6.Write(file, p);

                Intro.Part6Size = Part6.Size;
                Intro.DataOffset = Part6.Offset + Part6.Size;
                //         Part7.Offset = Part6.Offset + Part6.Size;
                //Intro.Part7Offset = Part7.Offset;
            }
            p.EndTask();

            /* PART 7 */
            p.BeginTask(0.10f, "Writing header part 7...");
            {
  //              _part7.Write(file);

                //Intro.Part7Size = Part7.Size;
            }
            p.EndTask();
                        
            /* INTRO - Header intro must be written last */
            p.BeginTask(0.15f, "Writing header intro...");
            {
                _intro.Write(file);
            }
            p.EndTask();
        }
    }
}
