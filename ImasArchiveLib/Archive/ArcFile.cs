using Imas.Gtf;
using Imas.Records;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ImasArchiveLibTest")]

namespace Imas.Archive
{
    public class ArcFile : ContainerFile<ArcEntry>
    {
        private readonly Stream _binStream;

        private int totalProgress;
        private int countProgress;

        #region Constructors

        public ArcFile(string arcName)
        {
            (string body, string extension) = RemoveArcExtension(arcName);
            string _arc_filename = body + ".arc" + extension;
            string _bin_filename = body + ".bin" + extension;
            if (!File.Exists(_arc_filename))
                throw new FileNotFoundException("Arc file not found.");
            if (!File.Exists(_bin_filename))
                throw new FileNotFoundException("Bin file not found. Make sure you have a .bin file with the same name in the same folder as the .arc file.");
            _stream = new FileStream(_arc_filename, FileMode.Open, FileAccess.Read);
            _binStream = new FileStream(_bin_filename, FileMode.Open, FileAccess.Read);
            BuildEntries();
        }

        private ArcFile()
        {
        }

        #endregion Constructors

        #region Arc name parsing
        public static (string, string) RemoveArcExtension(string arcName)
        {
            if (arcName.EndsWith(".arc"))
            {
                return (arcName[0..^4], "");
            }
            else if (arcName.EndsWith(".arc.dat"))
            {
                return (arcName[0..^8], ".dat");
            }
            else
            {
                throw new ArgumentException("Filename does not have .arc or .arc.dat extension.");
            }
        }

        public static string GetBinName(string arcName)
        {
            (string body, string extension) = RemoveArcExtension(arcName);
            return body + ".bin" + extension;
        }
        #endregion Arc name parsing

        #region Initialisation

        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        private void BuildEntries()
        {
            try
            {
                Binary binary = new Binary(_binStream, true);
                if (binary.ReadUInt32() != 0x50414100u)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }

                if (binary.ReadUInt32() != 0x00010000u)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                uint _entry_count = binary.ReadUInt32();
                if (binary.ReadUInt32() != 32)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                if (binary.ReadUInt32() != 16 * _entry_count + 32)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                binary.ReadUInt32();
                if (binary.ReadUInt32() != 0)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                if (binary.ReadUInt32() != 0)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }

                uint[] filePathOffsets = new uint[_entry_count];
                uint[] lengths = new uint[_entry_count];
                uint[] offsets = new uint[_entry_count];
                string[] filePaths = new string[_entry_count];

                for (int i = 0; i < _entry_count; i++)
                {
                    filePathOffsets[i] = binary.ReadUInt32();
                    lengths[i] = binary.ReadUInt32();
                    binary.ReadUInt32();
                    binary.ReadUInt32();
                }

                for (int i = 0; i < _entry_count; i++)
                {
                    offsets[i] = binary.ReadUInt32();
                }

                for (int i = 0; i < _entry_count; i++)
                {
                    if (filePathOffsets[i] > _binStream.Length || filePathOffsets[i] < 0)
                        throw new InvalidDataException(Strings.InvalidData_BinHeader);
                    _binStream.Seek(filePathOffsets[i], SeekOrigin.Begin);
                    string filepath = "";
                    int b;
                    while ((b = _binStream.ReadByte()) > 0)
                    {
                        filepath += Convert.ToChar(b);
                    }
                    filePaths[i] = filepath;
                }

                _entries = new List<ArcEntry>((int)_entry_count);
                for (int i = 0; i < _entry_count; i++)
                {
                    _entries.Add(new ArcEntry(this, filePaths[i][0..^3], offsets[i], lengths[i]));
                }
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }
        }

        #endregion Initialisation

        #region Build Arc/Bin

        /// <summary>
        /// Asynchronously creates a new archive from the specified directory.
        /// </summary>
        /// <param name="dir">The directory to read from.</param>
        /// <param name="outputArcName">The name of the new arc file.</param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async Task BuildFromDirectory(string dir, string outputArcName, IProgress<ProgressData> progress = null)
        {
            _ = RemoveArcExtension(outputArcName);
            using ArcFile arcFile = new ArcFile
            {
                _entries = new List<ArcEntry>()
            };
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            Array.Sort(files, (a, b) => string.CompareOrdinal(a.FullName.ToUpper(), b.FullName.ToUpper()));

            using (FileStream newArcStream = new FileStream(outputArcName, FileMode.Create, FileAccess.Write))
            {
                await newArcStream.WriteAsync(new byte[16]);
                arcFile.totalProgress = files.Length;
                arcFile.countProgress = 0;
                foreach (FileInfo fileInfo in files)
                {
                    ArcEntry arcEntry = await arcFile.NewEntry(
                        fileInfo.FullName.Substring(directoryInfo.FullName.Length + 1), fileInfo);
                    await AppendEntry(newArcStream, arcEntry);
                    arcEntry.ClearMemoryStream();
                    arcFile.countProgress++;
                    progress?.Report(new ProgressData
                    {
                        count = arcFile.countProgress,
                        total = arcFile.totalProgress,
                        filename = arcEntry.FileName
                    });
                }
            }
            using FileStream newBinStream = new FileStream(GetBinName(outputArcName), FileMode.Create, FileAccess.Write);
            arcFile.BuildBin(newBinStream);
        }

        /// <summary>
        /// Asynchronously save a copy of the edited archive.
        /// </summary>
        /// <param name="arcName">The name of the new arc file.</param>
        /// <param name="progress"></param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="IOException"/>
        public async Task SaveAs(string arcName, IProgress<ProgressData> progress = null)
        {
            _ = RemoveArcExtension(arcName);
            using (FileStream newArcStream = new FileStream(arcName, FileMode.Create, FileAccess.Write))
            {
                await BuildArc(newArcStream, progress);
            }
            using FileStream newBinStream = new FileStream(GetBinName(arcName), FileMode.Create, FileAccess.Write);
            BuildBin(newBinStream);
        }

        /// <exception cref="IOException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        private async Task BuildArc(Stream newArcStream, IProgress<ProgressData> progress = null)
        {
            if (newArcStream == null)
                throw new ArgumentNullException(nameof(newArcStream));
            _entries.Sort((a, b) => string.CompareOrdinal(a.FileName.ToUpper(), b.FileName.ToUpper()));

            newArcStream.Write(new byte[16]);
            totalProgress = _entries.Count;
            countProgress = 0;
            foreach (ArcEntry arcEntry in _entries)
            {
                await AppendEntry(newArcStream, arcEntry);
                countProgress++;
                progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = arcEntry.FileName });
            }
        }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="NotSupportedException"/>
        private static async Task AppendEntry(Stream newArcStream, ArcEntry arcEntry)
        {
            if (newArcStream == null || arcEntry == null)
                throw new ArgumentNullException();
            Stream stream = arcEntry.OpenRaw();
            arcEntry.Offset = newArcStream.Position;
            stream.Position = 0;
            await stream.CopyToAsync(newArcStream);
            uint len = (uint)stream.Length;
            uint pad = (uint)((-len) & 15);
            await newArcStream.WriteAsync(new byte[pad]);
            if (!arcEntry.UsesMemoryStream)
                stream.Dispose();
        }

        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        private void BuildBin(Stream newBinStream)
        {
            if (newBinStream == null)
                throw new ArgumentNullException();
            List<BinNode> arcTrees;
            int root;
            (arcTrees, root) = BuildBinTree();

            using MemoryStream filepaths = new MemoryStream();
            int filepathOffset = 0;
            foreach (BinNode arcTree in arcTrees)
            {
                filepaths.Write(Encoding.ASCII.GetBytes(arcTree.filepath), 0, arcTree.filepath.Length);
                filepaths.WriteByte(0);
                int len = arcTree.filepath.Length + 1;
                int pad = (-len) & 15;
                for (int j = 0; j < pad; j++)
                    filepaths.WriteByte(0);
                arcTree.stringOffset = filepathOffset;
                filepathOffset += len + pad;
            }
            int stringsStart = 20 * _entries.Count + 32;
            int offsetsPad = (-stringsStart) & 15;
            stringsStart += offsetsPad;

            Binary binary = new Binary(newBinStream, true);

            binary.WriteUInt32(0x50414100u);
            binary.WriteUInt32(0x00010000u);
            binary.WriteInt32(Entries.Count);
            binary.WriteUInt32(32);

            binary.WriteInt32(16 * Entries.Count + 32);
            binary.WriteInt32(root);
            binary.WriteUInt32(0);
            binary.WriteUInt32(0);

            for (int i = 0; i < arcTrees.Count; i++)
            {
                binary.WriteInt32(stringsStart + arcTrees[i].stringOffset);
                binary.WriteUInt32((uint)arcTrees[i].arcEntry.PastLength);
                binary.WriteInt32(arcTrees[i].left);
                binary.WriteInt32(arcTrees[i].right);
            }

            for (int i = 0; i < arcTrees.Count; i++)
            {
                binary.WriteUInt32((uint)arcTrees[i].arcEntry.Offset);
            }
            newBinStream.Write(new byte[offsetsPad]);
            filepaths.Position = 0;
            filepaths.CopyTo(newBinStream);
        }

        private (List<BinNode>, int) BuildBinTree()
        {
            List<BinNode> arcTrees = new List<BinNode>();
            for (int i = 0; i < _entries.Count; i++)
            {
                arcTrees.Add(new BinNode
                {
                    arcEntry = _entries[i],
                    index = i,
                    filepath = _entries[i].FileName + ".gz"
                });
            }
            arcTrees.Sort((a, b) => String.CompareOrdinal(a.filepath, b.filepath));
            int root = arcTrees[BuildBinSubTree(arcTrees, 0, arcTrees.Count)].index;
            arcTrees.Sort((a, b) => a.index.CompareTo(b.index));
            return (arcTrees, root);
        }

        private int BuildBinSubTree(List<BinNode> arcTrees, int start, int length)
        {
            if (length == 0)
                return -1;

            int halfLength = length / 2;
            int midpoint = start + halfLength;
            int lsize = halfLength;
            int rsize = length - halfLength - 1;
            arcTrees[midpoint].left = (lsize == 0) ? -1 : arcTrees[BuildBinSubTree(arcTrees, start, lsize)].index;
            arcTrees[midpoint].right = (rsize == 0) ? -1 : arcTrees[BuildBinSubTree(arcTrees, midpoint + 1, rsize)].index;
            return midpoint;
        }

        #endregion Build Arc/Bin

        #region Entry Operations

        /// <summary>
        /// Creates a new entry in the ArcFile with no data.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="IOException"/>
        public ArcEntry NewEntry(string filePath)
        {
            return NewEntry(filePath, new MemoryStream()).Result;
        }

        private async Task<ArcEntry> NewEntry(string name, FileInfo fileInfo)
        {
            ArcEntry arcEntry;
            using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
            {
                arcEntry = await NewEntry(name, fileStream);
            }
            return arcEntry;
        }

        /// <summary>
        /// Asynchronously creates a new entry in the ArcFile and copies over the specified stream's data.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="stream"></param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="IOException"/>
        public async Task<ArcEntry> NewEntry(string filePath, Stream stream)
        {
            string internalFilePath = filePath.Replace('\\', '/');
            ArcEntry arcEntry = await ArcEntry.NewEntry(this, internalFilePath, stream);
            _entries.Add(arcEntry);
            return arcEntry;
        }

        internal bool RemoveEntry(ArcEntry arcEntry)
        {
            return _entries.Remove(arcEntry);
        }

        #endregion Entry Operations

        #region Export entries

        public async Task ExtractAllAsync(string destDir, bool extractPar, IProgress<ProgressData> progress = null)
        {
            if (destDir.EndsWith('/'))
                destDir = destDir[0..^1];
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir);
            totalProgress = _entries.Count;
            countProgress = 0;
            foreach (ArcEntry arcEntry in _entries)
            {
                countProgress++;
                progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = arcEntry.FileName });
                await ExtractEntryAsync(arcEntry, directoryInfo, extractPar);
            }
        }

        private async Task ExtractEntryAsync(ArcEntry arcEntry, DirectoryInfo directoryInfo, bool extractPar)
        {
            string dirs = arcEntry.FileName.Substring(0, arcEntry.FileName.LastIndexOf('/') + 1);
            if (dirs != "")
                directoryInfo.CreateSubdirectory(dirs);
            string outFile = directoryInfo.FullName + '/' + arcEntry.FileName;
            if (extractPar && (outFile.EndsWith(".par") || outFile.EndsWith(".pta")))
            {
                string outDir = outFile[0..^4] + "_" + outFile[^3..];
                using ParFile parFile = new ParFile(await arcEntry.GetData().ConfigureAwait(false));
                await parFile.ExtractAll(outDir).ConfigureAwait(false);
            }
            else
            {
                using FileStream fileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
                using Stream stream = await arcEntry.GetData();
                await stream.CopyToAsync(fileStream);
            }
        }

        #endregion Export entries

        #region Replace

        public async Task ReplaceEntries(IFileSource fileSource, IProgress<ProgressData> progress = null)
        {
            totalProgress = Entries.Count;
            countProgress = 0;
            foreach (ArcEntry entry in Entries)
            {
                countProgress++;

                if (fileSource.FileExists(entry.FileName))
                {
                    progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = entry.FileName });
                    using Stream fileStream = fileSource.OpenFile(entry.FileName);
                    await entry.SetData(fileStream);
                }
                else if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                {
                    string childDir = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                    if (fileSource.DirectoryExists(childDir))
                    {
                        progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = entry.FileName });
                        using Stream entryStream1 = await entry.GetData();
                        using ParFile childPar = new ParFile(entryStream1);
                        await childPar.ReplaceEntries(fileSource.OpenDirectory(childDir));
                        using MemoryStream memStream = new MemoryStream();
                        await childPar.SaveTo(memStream);
                        memStream.Position = 0;
                        await entry.SetData(memStream);
                    }
                }
            }
        }

        public static async Task PatchArcFromZip(string inArcName, string patchName, string outArcName, IProgress<ProgressData> progress = null)
        {
            using ZipSourceParent zipSourceParent = new ZipSourceParent(patchName);
            await PatchArc(inArcName, zipSourceParent.GetZipSource(), outArcName, progress);
        }

        public static async Task PatchArcFromFolder(string inArcName, string folderName, string outArcName, IProgress<ProgressData> progress = null)
        {
            await PatchArc(inArcName, new FileSource(folderName), outArcName, progress);
        }

        private static async Task PatchArc(string inArcName, IFileSource fileSource, string outArcName, IProgress<ProgressData> progress)
        {
            using ArcFile arcFile = new ArcFile(inArcName);
            await arcFile.ReplaceAndSaveTo(fileSource, outArcName, progress);
        }

        private async Task ReplaceAndSaveTo(IFileSource fileSource, string outArcName, IProgress<ProgressData> progress)
        {
            (string outName, string outExt) = RemoveArcExtension(outArcName);
            using (FileStream newArcStream = new FileStream(outArcName, FileMode.Create, FileAccess.Write))
            {
                totalProgress = Entries.Count;
                countProgress = 0;
                foreach (ArcEntry entry in Entries)
                {
                    countProgress++;
                    if (fileSource.FileExists(entry.FileName))
                    {
                        progress?.Report(new ProgressData
                        {
                            count = countProgress,
                            total = totalProgress,
                            filename = entry.FileName
                        });
                        using Stream fileStream = fileSource.OpenFile(entry.FileName);
                        await entry.SetData(fileStream);
                    }
                    else if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                    {
                        string childDir = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                        if (fileSource.DirectoryExists(childDir))
                        {
                            progress?.Report(new ProgressData
                            {
                                count = countProgress,
                                total = totalProgress,
                                filename = entry.FileName
                            });
                            using Stream entryStream1 = await entry.GetData();
                            using ParFile childPar = new ParFile(entryStream1);
                            await childPar.ReplaceEntries(fileSource.OpenDirectory(childDir));
                            using MemoryStream memStream = new MemoryStream();
                            await childPar.SaveTo(memStream);
                            memStream.Position = 0;
                            await entry.SetData(memStream);
                        }
                    }

                    await AppendEntry(newArcStream, entry);
                    entry.ClearMemoryStream();
                }
            }
            using FileStream newBinStream = new FileStream(outName + ".bin" + outExt, FileMode.Create, FileAccess.Write);
            BuildBin(newBinStream);
        }

        #endregion Replace

        #region Extracting

        public async Task ExtractCommusToXlsx(string xlsxName, bool overwrite, IProgress<ProgressData> progress = null)
        {
            using CommuToXlsx commuToXlsx = new CommuToXlsx(xlsxName, overwrite);
            IEnumerable<ContainerEntry> commuEntries = Entries.Where(entry => entry.FileName.StartsWith("commu2/par/"));
            totalProgress = commuEntries.Count();
            countProgress = 0;
            foreach (ContainerEntry arcEntry in commuEntries)
            {
                countProgress++;
                progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = arcEntry.FileName });

                using Stream parStream = await arcEntry.GetData();
                ParFile parFile = new ParFile(parStream);
                ContainerEntry binEntry = parFile.Entries.FirstOrDefault(entry => entry.FileName.EndsWith("_m.bin"));
                if (binEntry != null)
                {
                    using Stream stream = await binEntry.GetData();
                    commuToXlsx.GetAndWriteCommu(stream, arcEntry.FileName[0..^4] + "_" + arcEntry.FileName[^3..] + "/" + binEntry.FileName);
                }
            }
        }

        public async Task ExtractParameterToXlsx(string xlsxName, bool overwrite, IProgress<ProgressData> progress = null)
        {
            using XlsxWriter xlsxWriter = new XlsxWriter(xlsxName, overwrite);
            foreach (RecordFormat format in RecordFormat.formats)
            {
                using EntryStack entryStack = await GetEntryRecursive(format.fileName);
                if (!xlsxWriter.HasWorksheet(format.sheetName) && entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = format.fileName });
                    List<Record> records = new List<Record>();
                    using (Stream stream = await entryStack.Entry.GetData())
                    {
                        records = Record.GetRecords(stream, format.format, format.headers);
                    }
                    xlsxWriter.AppendRows(format.sheetName, records);
                }
            }
            using (EntryStack entryStack = await GetEntryRecursive("parameter/stage_info.par"))
            {
                if (!xlsxWriter.HasWorksheet(StageInfo.sheetName) && entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = "parameter/stage_info.par" });
                    using Stream parStream = await entryStack.Entry.GetData();
                    using ParFile parFile = new ParFile(parStream);
                    foreach (ContainerEntry entry in parFile.Entries)
                    {
                        using Stream stream = await entry.GetData();
                        var records = StageInfo.ReadFile(stream, entry.FileName);
                        xlsxWriter.AppendRows(StageInfo.sheetName, records);
                    }
                }
            }
            using (EntryStack entryStack = await GetEntryRecursive("parameter/fanLetterInfo.bin"))
            {
                if (entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = "parameter/fanLetterInfo.bin" });
                    using Stream stream = await entryStack.Entry.GetData();
                    FanLetterInfo.ReadFile(stream, xlsxWriter);
                }
            }
            foreach (string fileName in Pastbl.fileNames)
            {
                using EntryStack entryStack = await GetEntryRecursive(fileName);
                if (entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = fileName });
                    using Stream stream = await entryStack.Entry.GetData();
                    IEnumerable<Record> newRecords = Pastbl.ReadFile(stream).Select(
                        record =>
                        {
                            Record newRec = new Record("XX", Pastbl.headers);
                            newRec[0] = fileName;
                            newRec[1] = record[2];
                            return newRec;
                        });
                    xlsxWriter.AppendRows("pastbl", newRecords);
                }
            }
            foreach (string fileName in Catalog.fileNames)
            {
                using EntryStack entryStack = await GetEntryRecursive(fileName);
                if (entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = fileName });
                    using Stream stream = await entryStack.Entry.GetData();
                    var newRecords = Catalog.ReadFile(stream, fileName);
                    xlsxWriter.AppendRows("catalog", newRecords);
                }
            }
            using (EntryStack entryStack = await GetEntryRecursive("songinfo/songResource.bin"))
            {
                if (!xlsxWriter.HasWorksheet("songInfo") && entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = "songinfo/songResource.bin" });
                    using Stream stream = await entryStack.Entry.GetData();
                    IEnumerable<Record> records = SongInfo.ReadFile(stream);
                    xlsxWriter.AppendRows("songInfo", records);
                }
            }
            using (EntryStack entryStack = await GetEntryRecursive("ui/menu/skillBoard/skillBoard.info"))
            {
                if (!xlsxWriter.HasWorksheet("skillBoard") && !xlsxWriter.HasWorksheet("skillBoardStrings") && entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = "ui/menu/skillBoard/skillBoard.info" });
                    using Stream stream = await entryStack.Entry.GetData();
                    SkillBoard.ReadFile(stream, xlsxWriter);
                }
            }
            using (EntryStack entryStack = await GetEntryRecursive("text/im2nx_text.ja_jp"))
            {
                if (entryStack != null)
                {
                    progress?.Report(new ProgressData { filename = "text/im2nx_text.ja_jp" });
                    using Stream stream = await entryStack.Entry.GetData();
                    JaJpText.ReadFile(stream, xlsxWriter);
                }
            }
            using (EntryStack mailEntryStack = await GetEntryRecursive("parameter/mail_idol_par/_dlc01_mail_idol.bin"))
            {
                using EntryStack infoEntryStack = await GetEntryRecursive("parameter/mail_idol_par/_ps3_info_idol.bin");
                if (mailEntryStack != null && infoEntryStack != null)
                {
                    progress?.Report(new ProgressData { filename = "parameter/mail_idol_par/_dlc01_mail_idol.bin" });
                    using Stream mailStream = await mailEntryStack.Entry.GetData();
                    using Stream infoStream = await infoEntryStack.Entry.GetData();
                    IdolMail.ReadFile(mailStream, infoStream, xlsxWriter);
                }
            }
        }

        public async Task ExtractLyrics(string outDir, bool overwrite, IProgress<ProgressData> progress = null)
        {
            Directory.CreateDirectory(outDir);
            IEnumerable<ContainerEntry> commuEntries = Entries.Where(entry => entry.FileName.StartsWith("songinfo/") && entry.FileName.EndsWith(".xmb"));
            totalProgress = commuEntries.Count();
            countProgress = 0;
            List<Record> records = new List<Record>();
            foreach (ContainerEntry arcEntry in commuEntries)
            {
                using Stream stream = await arcEntry.GetData();
                Xmb xmb = new Xmb();
                xmb.ReadXmb(stream);
                string filename = arcEntry.FileName;
                string newName = filename.Substring(filename.LastIndexOf('/') + 1)[0..^4] + ".xml";
                using FileStream fileStream = new FileStream(outDir + "/" + newName, FileMode.Create, FileAccess.Write);
                xmb.WriteXml(fileStream);
                Record record = new Record("XX", Xmb.filenameXlsxHeaders);
                record[0] = filename;
                record[1] = newName;
                records.Add(record);
            }
            using XlsxWriter xlsxWriter = new XlsxWriter(outDir + "/filenames.xlsx", overwrite);
            xlsxWriter.AppendRows("filenames", records);
        }

        public async Task ExtractAllImages(string outDir, IProgress<ProgressData> progress = null)
        {
            Dictionary<string, string> hashFileName = new Dictionary<string, string>();
            Dictionary<string, int> fileNameCount = new Dictionary<string, int>();
            List<Record> records = new List<Record>();

            Directory.CreateDirectory(outDir);
            await ForAllTask((entry, filename) => ExtractImage(entry, filename, hashFileName, fileNameCount, outDir, records), progress);
            using XlsxWriter xlsxWriter = new XlsxWriter(outDir + "/filenames.xlsx", true);
            xlsxWriter.AppendRows("filenames", records);
        }

        private static readonly string[] ImageFilenameXlsxHeaders = { "Original Filename", "New Filename", "GTF Type" };
        private async Task ExtractImage(ContainerEntry entry, string filename,
            Dictionary<string, string> hashFileName, Dictionary<string, int> fileNameCount, string outDir, List<Record> records)
        {
            try
            {
                if (filename.EndsWith(".gtf") || filename.EndsWith(".dds") || filename.EndsWith(".tex"))
                {
                    using Stream inStream = await entry.GetData();
                    using GTF gtf = GTF.CreateFromGtfStream(inStream);
                    string name = filename.Substring(filename.LastIndexOf('/') + 1);
                    string outNameNoExtend = name[0..^4];
                    using MemoryStream memStream = new MemoryStream();
                    gtf.SavePngTo(memStream);
                    memStream.Position = 0;

                    string hashString;
                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] hash = sha.ComputeHash(memStream);
                        hashString = "";
                        foreach (byte b in hash)
                        {
                            hashString += b.ToString("X2");
                        }
                    }

                    string outName;
                    if (hashFileName.ContainsKey(hashString))
                    {
                        outName = hashFileName[hashString] + ".png";
                    }
                    else
                    {
                        if (fileNameCount.ContainsKey(outNameNoExtend))
                        {
                            fileNameCount[outNameNoExtend]++;
                            outNameNoExtend += "(" + fileNameCount[outNameNoExtend].ToString() + ")";
                        }
                        else
                        {
                            fileNameCount.Add(outNameNoExtend, 1);
                        }
                        hashFileName.Add(hashString, outNameNoExtend);
                        outName = outNameNoExtend + ".png";

                        string outPath = outDir + "/" + outNameNoExtend + ".png";
                        using FileStream outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write);
                        memStream.Position = 0;
                        await memStream.CopyToAsync(outStream).ConfigureAwait(false);
                    }

                    Record record = new Record("XXI", ImageFilenameXlsxHeaders);
                    record[0] = filename;
                    record[1] = outName;
                    record[2] = gtf.Type;
                    records.Add(record);
                }
            }
            catch (NotSupportedException)
            { }
        }

        #endregion Extracting

        #region IDisposable

        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _binStream?.Dispose();
                foreach (ArcEntry arcEntry in _entries)
                {
                    arcEntry.Dispose();
                }
            }
            _disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable

        private class BinNode
        {
            public int index;
            public int left;
            public int right;
            public string filepath;
            public ArcEntry arcEntry;
            public int stringOffset;
        }
    }
}