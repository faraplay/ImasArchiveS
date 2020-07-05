using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;

[assembly: InternalsVisibleTo("ImasArchiveLibTest")]

namespace ImasArchiveLib
{
    public class ArcFile : IDisposable
    {
        private readonly Stream _arcStream;
        private readonly Stream _binStream;
        private List<ArcEntry> _entries;
        private bool _disposed = false;


        public ReadOnlyCollection<ArcEntry> Entries { get { return new ReadOnlyCollection<ArcEntry>(_entries); } }
        private int totalProgress;
        private int countProgress;

        #region Constructors
        public ArcFile(string filename, string extraExtension = "")
        {
            string _arc_filename = filename + ".arc" + extraExtension;
            string _bin_filename = filename + ".bin" + extraExtension;
            if (!File.Exists(_arc_filename))
                throw new FileNotFoundException("Arc file not found.");
            if (!File.Exists(_bin_filename))
                throw new FileNotFoundException("Bin file not found. Make sure you have a .bin file with the same name in the same folder as the .arc file.");
            _arcStream = new FileStream(_arc_filename, FileMode.Open, FileAccess.Read);
            _binStream = new FileStream(_bin_filename, FileMode.Open, FileAccess.Read);
            BuildEntries();
        }

        private ArcFile() { }
        #endregion
        #region Initialisation
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        private void BuildEntries()
        {
            try
            {
                if (Utils.GetUInt(_binStream) != 0x50414100u)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }

                if (Utils.GetUInt(_binStream) != 0x00010000u)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                uint _entry_count = Utils.GetUInt(_binStream);
                if (Utils.GetUInt(_binStream) != 32)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                if (Utils.GetUInt(_binStream) != 16 * _entry_count + 32)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                Utils.GetUInt(_binStream);
                if (Utils.GetUInt(_binStream) != 0)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }
                if (Utils.GetUInt(_binStream) != 0)
                {
                    throw new InvalidDataException(Strings.InvalidData_BinHeader);
                }

                uint[] filePathOffsets = new uint[_entry_count];
                uint[] lengths = new uint[_entry_count];
                uint[] offsets = new uint[_entry_count];
                string[] filePaths = new string[_entry_count];

                for (int i = 0; i < _entry_count; i++)
                {
                    filePathOffsets[i] = Utils.GetUInt(_binStream);
                    lengths[i] = Utils.GetUInt(_binStream);
                    Utils.GetUInt(_binStream);
                    Utils.GetUInt(_binStream);
                }

                for (int i = 0; i < _entry_count; i++)
                {
                    offsets[i] = Utils.GetUInt(_binStream);
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
        #endregion
        #region Build Arc/Bin
        /// <summary>
        /// Asynchronously creates a new archive from the specified directory.
        /// </summary>
        /// <param name="dir">The directory to read from.</param>
        /// <param name="outputName">The name of the new archive.</param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async Task BuildFromDirectory(string dir, string outputName, IProgress<ProgressData> progress = null)
        {
            using ArcFile arcFile = new ArcFile
            {
                _entries = new List<ArcEntry>()
            };
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            Array.Sort(files, (a, b) => string.CompareOrdinal(a.FullName.ToUpper(), b.FullName.ToUpper()));

            using (FileStream newArcStream = new FileStream(outputName + ".arc", FileMode.Create, FileAccess.Write))
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
                    progress?.Report(new ProgressData { 
                        count = arcFile.countProgress, 
                        total = arcFile.totalProgress, 
                        filename = arcEntry.Filepath 
                    });
                }
            }
            using FileStream newBinStream = new FileStream(outputName + ".bin", FileMode.Create, FileAccess.Write);
            arcFile.BuildBin(newBinStream);
        }
        /// <summary>
        /// Asynchronously save a copy of the edited archive.
        /// </summary>
        /// <param name="filename">The name of the new file.</param>
        /// <param name="progress"></param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="IOException"/>
        public async Task SaveAs(string filename, IProgress<ProgressData> progress = null)
        {
            using (FileStream newArcStream = new FileStream(filename + ".arc", FileMode.Create, FileAccess.Write))
            {
                await BuildArc(newArcStream, progress);
            }
            using FileStream newBinStream = new FileStream(filename + ".bin", FileMode.Create, FileAccess.Write);
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
            _entries.Sort((a, b) => String.CompareOrdinal(a.Filepath.ToUpper(), b.Filepath.ToUpper()));

            newArcStream.Write(new byte[16]);
            totalProgress = _entries.Count;
            countProgress = 0;
            foreach (ArcEntry arcEntry in _entries)
            {
                await AppendEntry(newArcStream, arcEntry);
                countProgress++;
                progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = arcEntry.Filepath });
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
            arcEntry.SaveAsOffset = newArcStream.Position;
            stream.Position = 0;
            await stream.CopyToAsync(newArcStream);
            uint len = (uint)stream.Length;
            uint pad = (uint)((-len) & 15);
            await newArcStream.WriteAsync(new byte[pad]);
            if (!arcEntry.Edited)
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

            Utils.PutUInt(newBinStream, 0x50414100u);
            Utils.PutUInt(newBinStream, 0x00010000u);
            Utils.PutUInt(newBinStream, (uint)Entries.Count);
            Utils.PutUInt(newBinStream, 32);

            Utils.PutUInt(newBinStream, 16 * (uint)Entries.Count + 32);
            Utils.PutUInt(newBinStream, (uint)root);
            Utils.PutUInt(newBinStream, 0);
            Utils.PutUInt(newBinStream, 0);

            for (int i = 0; i < arcTrees.Count; i++)
            {
                Utils.PutUInt(newBinStream, (uint)(stringsStart + arcTrees[i].stringOffset));
                Utils.PutUInt(newBinStream, (uint)arcTrees[i].arcEntry.Length);
                Utils.PutUInt(newBinStream, (uint)arcTrees[i].left);
                Utils.PutUInt(newBinStream, (uint)arcTrees[i].right);
            }

            for (int i = 0; i < arcTrees.Count; i++)
            {
                Utils.PutUInt(newBinStream, (uint)arcTrees[i].arcEntry.SaveAsOffset);
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
                    filepath = _entries[i].Filepath + ".gz"
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
        #endregion
        #region Entry Operations
        public ArcEntry GetEntry(string filePath)
        {
            return _entries.Find(entry => entry.Filepath == filePath);
        }
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
        #endregion
        #region Export entries

        public async Task ExtractAllAsync(string destDir, IProgress<ProgressData> progress = null)
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
                await ExtractEntryAsync(arcEntry, directoryInfo, progress);
            }

        }

        private async Task ExtractEntryAsync(ArcEntry arcEntry, DirectoryInfo directoryInfo, IProgress<ProgressData> progress = null)
        {
            string dirs = arcEntry.Filepath.Substring(0, arcEntry.Filepath.LastIndexOf('/') + 1);
            if (dirs != "")
                directoryInfo.CreateSubdirectory(dirs);
            string outFile = directoryInfo.FullName + '/' + arcEntry.Filepath;
            using FileStream fileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            using Stream stream = arcEntry.Open();
            await stream.CopyToAsync(fileStream);
            countProgress++;
            progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = arcEntry.Filepath });
        }
        #endregion

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal Substream GetSubstream(long offset, long length) => new Substream(_arcStream, offset, length);

        #region Commu
        public async Task ExtractCommusDir(string outDirName, IProgress<ProgressData> progress = null)
        {
            Directory.CreateDirectory(outDirName);
            totalProgress = Entries.Count;
            countProgress = 0;
            foreach (ArcEntry entry in Entries)
            {
                countProgress++;
                progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = entry.Filepath });
                await Task.Run(() => entry.TryGetCommuText(outDirName));
            }
        }
        public async Task ReplaceCommusDir(string commuDirName, IProgress<ProgressData> progress = null)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(commuDirName);
            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException();
            }
            var files = directoryInfo.GetFiles("*.txt", SearchOption.AllDirectories);
            totalProgress = files.Length;
            countProgress = 0;
            foreach (FileInfo file in files)
            {
                await ReplaceCommu(file.FullName);
                countProgress++;
                progress.Report(new ProgressData { count = countProgress, filename = file.FullName, total = totalProgress });
            }
        }
        public async Task ReplaceCommu(string commuFileName)
        {
            string parPath;
            if (!File.Exists(commuFileName))
            {
                throw new FileNotFoundException("Could not find file" ,commuFileName);
            }
            using (StreamReader reader = new StreamReader(commuFileName))
            {
                parPath = reader.ReadLine();
            }
            ArcEntry arcEntry = GetEntry(parPath + ".gz");
            if (arcEntry == null)
                return;
            await arcEntry.TryReplaceCommuText(commuFileName);
        }
        #endregion
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ArcFile() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _arcStream?.Dispose();
                _binStream?.Dispose();
                foreach (ArcEntry arcEntry in _entries)
                {
                    arcEntry.Dispose();
                }
            }

            _disposed = true;
        }
        #endregion

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
