using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    public class ParFile : IDisposable
    {
        #region Fields
        readonly Stream _stream;
        int fileCount;
        List<ParEntry> _entries;

        #endregion
        #region Properties
        public ReadOnlyCollection<ParEntry> Entries
        {
            get => new ReadOnlyCollection<ParEntry>(_entries);
        }
        #endregion
        #region Constructors
        public ParFile(Stream stream)
        {
            _stream = stream;
            ReadHeader();
        }
        #endregion
        #region IDisposable
        private bool disposed = false;
        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                _stream.Dispose();
            }
            disposed = true;
        }
        ~ParFile() => Dispose(false);
        #endregion
        #region Reading Header
        private void ReadHeader()
        {
            if (_stream.ReadByte() != 0x50 ||
                _stream.ReadByte() != 0x41 ||
                _stream.ReadByte() != 0x52)
                throw new InvalidDataException(Strings.InvalidData_ParHeader);
            switch (_stream.ReadByte())
            {
                case 0:
                    ReadHeaderLittleEndian();
                    break;
                case 1:
                case 2:
                    ReadHeaderBigEndian();
                    break;
                default:
                    throw new InvalidDataException(Strings.InvalidData_ParHeader);
            }
        }

        private void ReadHeaderLittleEndian()
        {
            BinaryReader reader = new BinaryReader(_stream);
            int nameLength = reader.ReadInt32() switch
            {
                2 => 0x20,
                3 => 0x80,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };

            fileCount = reader.ReadInt32();

            bool lengthsKnown = _stream.ReadByte() switch
            {
                1 => true,
                0 => false,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };
            _stream.Position += 3;

            int[] offsets = new int[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                offsets[i] = reader.ReadInt32();
            }

            long pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            string[] filenames = new string[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                byte[] namebuf = new byte[nameLength];
                _stream.Read(namebuf);
                string namezero = Encoding.ASCII.GetString(namebuf);
                filenames[i] = namezero.Remove(namezero.IndexOf('\0'));
            }

            int[] props = new int[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                props[i] = reader.ReadInt32();
            }
            pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            int[] lengths = new int[fileCount];
            if (lengthsKnown)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    lengths[i] = reader.ReadInt32();
                }
                pad = (-_stream.Position) & 15;
                _stream.Position += pad;
            }
            else
            {
                for (int i = 0; i < fileCount - 1; i++)
                {
                    lengths[i] = offsets[i + 1] - offsets[i];
                }
                lengths[fileCount - 1] = (int)_stream.Length - offsets[fileCount - 1];
            }
            pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            _entries = new List<ParEntry>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                _entries.Add(new ParEntry(this, filenames[i], offsets[i], lengths[i], props[i]));
            }
        }


        private void ReadHeaderBigEndian()
        {
            int nameLength = Utils.GetInt32(_stream) switch
            {
                2 => 0x20,
                3 => 0x80,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };

            fileCount = Utils.GetInt32(_stream);
            
            bool lengthsKnown = _stream.ReadByte() switch
            {
                3 => true,
                2 => false,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };
            _stream.Position += 3;

            int[] offsets = new int[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                offsets[i] = Utils.GetInt32(_stream);
            }

            long pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            string[] filenames = new string[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                byte[] namebuf = new byte[nameLength];
                _stream.Read(namebuf);
                string namezero = Encoding.ASCII.GetString(namebuf);
                filenames[i] = namezero.Remove(namezero.IndexOf('\0'));
            }

            int[] props = new int[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                props[i] = Utils.GetInt32(_stream);
            }
            pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            int[] lengths = new int[fileCount];
            if (lengthsKnown) {
                for (int i = 0; i < fileCount; i++)
                {
                    lengths[i] = Utils.GetInt32(_stream);
                }
                pad = (-_stream.Position) & 15;
                _stream.Position += pad;
            } 
            else
            {
                for (int i = 0; i < fileCount - 1; i++)
                {
                    lengths[i] = offsets[i + 1] - offsets[i];
                }
                lengths[fileCount - 1] = (int)_stream.Length - offsets[fileCount - 1];
            }

            _entries = new List<ParEntry>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                _entries.Add(new ParEntry(this, filenames[i], offsets[i], lengths[i], props[i]));
            }
        }
        #endregion
        internal Substream GetSubstream(int offset, int length)
        {
            return new Substream(_stream, offset, length);
        }
        #region Extract
        public async Task ExtractAll(string destDir)
        {
            Directory.CreateDirectory(destDir);
            WriteDotParInfo(destDir);
            foreach (ParEntry entry in _entries)
            {
                if (entry._name.EndsWith(".par"))
                {
                    using Stream stream = entry.Open();
                    ParFile parFile = new ParFile(stream);
                    await parFile.ExtractAll(destDir + "\\" + entry._name[0..^4] + "_par");
                }
                else if (entry._name.EndsWith(".pta"))
                {
                    using Stream stream = entry.Open();
                    ParFile parFile = new ParFile(stream);
                    await parFile.ExtractAll(destDir + "\\" + entry._name[0..^4] + "_pta");
                }
                else
                {
                    using Stream stream = entry.Open();
                    using FileStream fileStream = new FileStream(destDir + "\\" + entry._name, FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        private void WriteDotParInfo(string destDir)
        {
            using StreamWriter writer = new StreamWriter(destDir + "\\.parinfo");
            writer.WriteLine(fileCount);
            writer.WriteLine();
            foreach (ParEntry entry in _entries)
            {
                writer.WriteLine(entry._property);
                writer.WriteLine(entry._name);
            }
        }
        #endregion

        public ParEntry GetEntry(string fileName)
        {
            return _entries.Find(e => e._name == fileName);
        }
    }
}


// shop/item
// shop/shopIcons
// ui/commu/arcWidget
// ui/game/unitMiniGame
