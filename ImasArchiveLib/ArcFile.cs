using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

namespace ImasArchiveLib
{
    public class ArcFile : IDisposable
    {
        private Stream _arc_stream;
        private Stream _bin_stream;
        private string _arc_filename;
        private string _bin_filename;
        private List<ArcEntry> _entries;
        private uint _entry_count;
        private bool _disposed = false;

        public ArcFile(string filename, string extraExtension = "")
        {
            _arc_filename = filename + ".arc" + extraExtension;
            _bin_filename = filename + ".bin" + extraExtension;
            _arc_stream = new FileStream(_arc_filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            _bin_stream = new FileStream(_bin_filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            BuildEntries();
        }

        internal Stream ArcStream { get => _arc_stream; }
        internal string ArcFilename { get => _arc_filename; }

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
            _entry_count = Utils.GetUInt(_bin_stream);
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

        public ArcEntry GetEntry(string filePath)
        {
            return _entries.Find(entry => entry.Filepath == filePath);
        }

        public void Flush()
        {

        }

        internal bool RemoveEntry(ArcEntry arcEntry)
        {
            return _entries.Remove(arcEntry);
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
    }
}
