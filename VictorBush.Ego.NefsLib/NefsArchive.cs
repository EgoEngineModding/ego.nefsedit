using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib
{
    public class NefsArchive
    {
        private static readonly ILog log = LogHelper.GetLogger();

        string _filePath;
        string _filePathHash;
        NefsHeader _header;
        List<NefsItem> _items = new List<NefsItem>();
        bool _modified = false;

        public NefsArchive(string filePath, NefsProgressInfo p)
        {
            float taskWeightHeader = 0.55f;
            float taskWeightItems = 0.45f;

            _filePath = Path.GetFullPath(filePath);

            using (var file = new FileStream(_filePath, FileMode.Open))
            {
                /* Create a hash of this archive's file path */
                _filePathHash = FilePathHelper.HashStringMD5(_filePath);

                /******************************************************************
                 * READ HEADER
                 ******************************************************************/
                p.BeginTask(taskWeightHeader, "Reading NeFS header...");
                _header = new NefsHeader(file, this, p);

                if( !checkHash(file) )
                {
                    log.Error("Header hash does not match expected value.");
                }
                p.EndTask();

                /******************************************************************
                 * READ ITEMS
                 ******************************************************************/
                p.BeginTask(taskWeightItems, "Loading NeFS items...");
                var numEntries = _header.Part1.Entries.Count;

                for (int i = 0; i < numEntries; i++)
                {
                    p.BeginTask(1 / (float)numEntries, String.Format("Loading item {0}/{1}", i + 1, numEntries));
                    var entry = _header.Part1.Entries[i];

                    try
                    {
                        var item = new NefsItem(file, this, entry.Id);
                        _items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Error loading item with id " + entry.Id, ex);
                    }
                    p.EndTask();
                }
                p.EndTask();
            }
        }

        /// <summary>
        /// The path to the .nefs file.
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// Hash string of the archive's file path.
        /// </summary>
        public string FilePathHash
        {
            get { return _filePathHash; }
        }

        /// <summary>
        /// The NeFS file header.
        /// </summary>
        public NefsHeader Header
        {
            get { return _header; }
        }

        /// <summary>
        /// List of items in this archive.
        /// </summary>
        public List<NefsItem> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Indicates if this archive has been modified and not yet saved.
        /// </summary>
        public bool Modified
        {
            get { return _modified; }
            internal set { _modified = value; }
        }

        /// <summary>
        /// Gets the item in the archive by id.
        /// </summary>
        /// <param name="id">The id of the item to get.</param>
        public NefsItem GetItem(UInt32 id)
        {
            var item = from i in Items
                       where i.Id == id
                       select i;

            if (item.Count() > 1)
            {
                throw new Exception("Archive has two items with id " + id.ToHexString());
            }

            return item.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously loads a NeFS archive from disk.
        /// </summary>
        /// <param name="filepath">The path to the .nefs file to load.</param>
        /// <param name="p">Progress info for reporting progress.</param>
        /// <returns></returns>
        public static async Task<NefsArchive> LoadAsync(String filePath, NefsProgressInfo p)
        {
            // TODO : Move the async stuff to UI project like replace/save??

            var archive = await Task.Run(() => {
                try
                {
                    log.Info("----------------------------");
                    log.Info(String.Format("Opening archive {0}...", filePath));
                    var ret = new NefsArchive(filePath, p);
                    log.Info("Archive opened successfully.");
                    return ret;
                }
                catch (OperationCanceledException ex)
                {
                    log.Info("Open archive operation cancelled.");
                    return null;
                }
                catch (IOException ex)
                {
                    log.Error(String.Format("An error occurred while reading the file {0}.", filePath), ex);
                    return null;
                }
            });

            return archive;
        }

        /// <summary>
        /// Saves the archive to disk.
        /// </summary>
        /// <param name="destFilePath">The file path to save the archive to.</param>
        /// <param name="p">Progress info for reporting progress.</param>
        public void Save(string destFilePath, NefsProgressInfo p)
        {
            float taskWeightPrep = 0.05f;
            float taskWeightSave = 0.9f;
            float taskWeightCleanup = 0.05f;

            /******************************************************************
             * PREPARE
             ******************************************************************/
            p.BeginTask(taskWeightPrep, "Preparing...");
            
            /*
             * Setup the temp working directory
             */

            /* Temp working directory */
            var workDir = Path.Combine(FilePathHelper.TempDirectory, FilePathHash);

            /* Create the working directory if it doesn't exist */
            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            p.EndTask();

            /******************************************************************
             * SAVE
             ******************************************************************/
            p.BeginTask(taskWeightSave, String.Format("Saving {0}...", destFilePath));

            /*
             * Open the temporary file stream
             */
            var tempFilePath = Path.Combine(workDir, "tempArchive.nefs");

            using (var file = new FileStream(tempFilePath, FileMode.Create))
            {
                /* Write the data - MUST BE DONE FIRST */
                foreach (var item in _items)
                {
                    p.BeginTask(1.0f / (float)_items.Count);
                    item.Write(file, p);
                    p.EndTask();

                    /* Update the header data for this item */
                    item.Part1Entry.Update(item);

                    /* Since some items share Part 2 entries, only update that entry once */
                    if (item.Part1Entry.Id == item.Part2Entry.Id)
                    {
                        item.Part2Entry.Update(item);
                    }
                }

                /* Update the archive file size entry */
                // TODO : Is this even right????
                Header.Part5.ArchiveSize = Items.Last().DataOffset + Items.Last().CompressedSize;

                /* Write the header */
                _header.Write(file, p);
                
                /* Recompute the hash and update the expected hash */
                var dataToHash = new byte[Header.Intro.HeaderSize - 0x20];
                file.Seek(0x0, SeekOrigin.Begin);
                file.Read(dataToHash, 0, 4);

                file.Seek(0x24, SeekOrigin.Begin);
                file.Read(dataToHash, 4, (int)Header.Intro.HeaderSize - 0x24);
                
                SHA256 hash = SHA256.Create();
                byte[] hashOut = hash.ComputeHash(dataToHash);
                
                file.Seek(0x04, SeekOrigin.Begin);
                file.Write(hashOut, 0, 0x20);
            }

            /*
             * Copy the temp file to the destination (overwrite if needed)
             */
            File.Copy(tempFilePath, destFilePath, true);

            p.EndTask();

            /******************************************************************
             * CLEANUP
             ******************************************************************/
            p.BeginTask(taskWeightCleanup, String.Format("Cleaning up..."));

            /* Delete the working directory for this archive */
            Directory.Delete(workDir, true);

            /* Mark all items as not pending injections */
            foreach (var item in _items)
            {
                p.BeginTask(1.0f / (float)_items.Count);
                item.ClearPendingInjection();
                p.EndTask();
            }

            /* Update archive's filepath */
            _filePath = destFilePath;
            _filePathHash = FilePathHelper.HashStringMD5(_filePath);

            /* Mark archive as not modified anymore */
            _modified = false;

            p.EndTask();
        }

        private bool checkHash(FileStream file)
        {
            byte[] dataToHash = new byte[Header.Intro.IsEncrypted ? Header.Intro.HeaderSize - 0x22 : Header.Intro.HeaderSize - 0x20];
            Stream stream = Header.Intro.IsEncrypted ? Header.Intro.DecryptedHeader : file;


            stream.Seek(0x0, SeekOrigin.Begin);
            stream.Read(dataToHash, 0, 4);

            stream.Seek(0x24, SeekOrigin.Begin);
            if (!Header.Intro.IsEncrypted) {
                stream.Read(dataToHash, 4, (int)Header.Intro.HeaderSize - 0x24);
            }
            else
            {
                stream.Read(dataToHash, 4, 0x5A);
                stream.Seek(0x80, SeekOrigin.Begin);
                stream.Read(dataToHash, 0x5E, (int)Header.Intro.HeaderSize - 0x80);
            }

            
            SHA256 hash = SHA256.Create();
            byte[] hashOut = hash.ComputeHash(dataToHash);

            bool isHashEqual = hashOut.SequenceEqual(Header.Intro.ExpectedHash);

            return isHashEqual;
        }
    }
}
