using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ImasArchiveLibTest")]

namespace ImasArchiveLib
{
    public class ArcFile : IDisposable
    {
        private Stream _arc_stream;
        private Stream _bin_stream;
        private string _arc_filename;
        private string _bin_filename;
        private List<ArcEntry> _entries;
        private bool _disposed = false;


        private int totalProgress;
        private int countProgress;

        public ArcFile(string filename, string extraExtension = "")
        {
            _arc_filename = filename + ".arc" + extraExtension;
            _bin_filename = filename + ".bin" + extraExtension;
            _arc_stream = new FileStream(_arc_filename, FileMode.Open, FileAccess.Read);
            _bin_stream = new FileStream(_bin_filename, FileMode.Open, FileAccess.Read);
            BuildEntries();
        }

        internal Stream ArcStream { get => _arc_stream; }

        public ReadOnlyCollection<ArcEntry> Entries { get { return new ReadOnlyCollection<ArcEntry>(_entries); } }
       
        private void BuildEntries()
        {
            if (Utils.GetUInt(_bin_stream) != 0x50414100u)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }

            if (Utils.GetUInt(_bin_stream) != 0x00010000u)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }
            uint _entry_count = Utils.GetUInt(_bin_stream);
            if (Utils.GetUInt(_bin_stream) != 32)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }
            if (Utils.GetUInt(_bin_stream) != 16 * _entry_count + 32)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }
            Utils.GetUInt(_bin_stream);
            if (Utils.GetUInt(_bin_stream) != 0)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }
            if (Utils.GetUInt(_bin_stream) != 0)
            {
                throw new InvalidDataException(Strings.InvalidData_BinHeader);
            }

            uint[] filePathOffsets = new uint[_entry_count];
            uint[] lengths = new uint[_entry_count];
            uint[] offsets = new uint[_entry_count];
            string[] filePaths = new string[_entry_count];

            for (int i = 0; i < _entry_count; i++)
            {
                filePathOffsets[i] = Utils.GetUInt(_bin_stream);
                lengths[i] = Utils.GetUInt(_bin_stream);
                Utils.GetUInt(_bin_stream);
                Utils.GetUInt(_bin_stream);
            }

            for (int i = 0; i < _entry_count; i++)
            {
                offsets[i] = Utils.GetUInt(_bin_stream);
            }

            for (int i = 0; i < _entry_count; i++)
            {
                _bin_stream.Seek(filePathOffsets[i], SeekOrigin.Begin);
                string filepath = "";
                int b;
                while ((b = _bin_stream.ReadByte()) > 0)
                {
                    filepath += Convert.ToChar(b);
                }
                filePaths[i] = filepath;
            }

            _entries = new List<ArcEntry>((int)_entry_count);
            for (int i = 0; i < _entry_count; i++)
            {
                _entries.Add(new ArcEntry(this, filePaths[i], offsets[i], lengths[i]));
            }
        }

        private static async Task AppendEntry(Stream newArcStream, ArcEntry arcEntry)
        {
            using Stream stream = arcEntry.OpenRaw();
            arcEntry.Base_offset = newArcStream.Position;
            stream.Position = 0;
            await stream.CopyToAsync(newArcStream);
            uint len = (uint)stream.Length;
            uint pad = (uint)((-len) & 15);
            await newArcStream.WriteAsync(new byte[pad]);

        }

        private async Task BuildArc(Stream newArcStream, IProgress<ProgressData> progress = null)
        {
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
        private void BuildBin(Stream newBinStream)
        {
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
                Utils.PutUInt(newBinStream, (uint)arcTrees[i].arcEntry.Base_offset);
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
                    filepath = _entries[i].Filepath
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

        public ArcEntry GetEntry(string filePath)
        {
            return _entries.Find(entry => entry.Filepath == filePath);
        }

        public ArcEntry NewEntry(string filePath)
        {
            return NewEntry(filePath, new MemoryStream()).Result;
        }

        public async Task<ArcEntry> NewEntry(string filePath, Stream stream)
        {
            string internalFilePath = filePath.Replace('\\', '/') + ".gz";
            ArcEntry arcEntry = new ArcEntry(this, internalFilePath, 0, 0);
            await arcEntry.Replace(stream);
            _entries.Add(arcEntry);
            return arcEntry;
        }

        internal bool RemoveEntry(ArcEntry arcEntry)
        {
            return _entries.Remove(arcEntry);
        }

        public void ExtractAll(string destDir)
        {
            if (destDir.EndsWith('/'))
                destDir = destDir.Substring(0, destDir.Length - 1);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir);
            foreach (ArcEntry arcEntry in _entries)
            {
                string dirs = arcEntry.Filepath.Substring(0, arcEntry.Filepath.LastIndexOf('/') + 1);
                if (dirs != "")
                    directoryInfo.CreateSubdirectory(dirs);
                string outFile = directoryInfo.FullName + '/' + arcEntry.Filepath;
                if (outFile.EndsWith(".gz"))
                    outFile = outFile.Substring(0, outFile.Length - 3);
                else
                    throw new InvalidDataException();
                using FileStream fileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
                using Stream stream = arcEntry.Open();
                stream.CopyTo(fileStream);
            }
        }

        public async Task ExtractAllAsync(string destDir, IProgress<ProgressData> progress = null)
        {
            if (destDir.EndsWith('/'))
                destDir = destDir.Substring(0, destDir.Length - 1);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir);
            //var tasks = new List<Task>();
            totalProgress = _entries.Count;
            countProgress = 0;
            foreach (ArcEntry arcEntry in _entries)
            {
                await ExtractEntryAsync(arcEntry, directoryInfo, progress);
            }
            //await Task.WhenAll(tasks);

        }

        private async Task ExtractEntryAsync(ArcEntry arcEntry, DirectoryInfo directoryInfo, IProgress<ProgressData> progress = null)
        {
            string dirs = arcEntry.Filepath.Substring(0, arcEntry.Filepath.LastIndexOf('/') + 1);
            if (dirs != "")
                directoryInfo.CreateSubdirectory(dirs);
            string outFile = directoryInfo.FullName + '/' + arcEntry.Filepath;
            if (outFile.EndsWith(".gz"))
                outFile = outFile.Substring(0, outFile.Length - 3);
            else
                throw new InvalidDataException();
            using FileStream fileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            using Stream stream = arcEntry.Open();
            await stream.CopyToAsync(fileStream);
            countProgress++;
            progress?.Report(new ProgressData { count = countProgress, total = totalProgress, filename = arcEntry.Filepath });
        }

        public async Task SaveAs(string filename, IProgress<ProgressData> progress = null)
        {
            using (FileStream newArcStream = new FileStream(filename + ".arc", FileMode.Create, FileAccess.Write))
            {
                await BuildArc(newArcStream, progress);
            }
            using (FileStream newBinStream = new FileStream(filename + ".bin", FileMode.Create, FileAccess.Write))
            {
                BuildBin(newBinStream);
            }
        }

        private ArcFile() { }

        private async Task<ArcEntry> NewEntry(string name, FileInfo fileInfo, IProgress<ProgressData> progress = null)
        {
            ArcEntry arcEntry;
            using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
            {
                arcEntry = await NewEntry(name, fileStream);
            }
            return arcEntry;
        }

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
                        fileInfo.FullName.Substring(directoryInfo.FullName.Length + 1), fileInfo, progress);
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
            using (FileStream newBinStream = new FileStream(outputName + ".bin", FileMode.Create, FileAccess.Write))
            {
                arcFile.BuildBin(newBinStream);
            }
        }

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
                _arc_stream?.Dispose();
                _bin_stream?.Dispose();
                foreach (ArcEntry arcEntry in _entries)
                {
                    arcEntry.Dispose();
                }
            }

            _disposed = true;
        }

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
