using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        #region Header
        private byte parVersion;
        private int nameLength;
        private bool lengthsKnown;
        private void ReadHeader()
        {
            if (_stream.ReadByte() != 0x50 ||
                _stream.ReadByte() != 0x41 ||
                _stream.ReadByte() != 0x52)
                throw new InvalidDataException(Strings.InvalidData_ParHeader);
            parVersion = (byte)_stream.ReadByte();
            bool isBigEndian = parVersion switch
            {
                0 => false,
                1 => true,
                2 => true,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };
            Binary binary = new Binary(_stream, isBigEndian);
            nameLength = binary.GetInt32() switch
            {
                2 => 0x20,
                3 => 0x80,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };

            fileCount = binary.GetInt32();

            lengthsKnown = binary.GetByte() switch
            {
                // v1/2
                3 => true,
                2 => false,
                // v0
                1 => true,
                0 => false,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };
            _stream.Position += 3;

            int[] offsets = new int[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                offsets[i] = binary.GetInt32();
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
                props[i] = binary.GetInt32();
            }
            pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            int[] lengths = new int[fileCount];
            if (lengthsKnown)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    lengths[i] = binary.GetInt32();
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

        private void WriteHeader(Stream stream)
        {
            stream.WriteByte(0x50);
            stream.WriteByte(0x41);
            stream.WriteByte(0x52);
            stream.WriteByte(parVersion);
            bool isBigEndian = parVersion switch
            {
                0 => false,
                1 => true,
                2 => true,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };
            Binary binary = new Binary(stream, isBigEndian);
            binary.PutInt32(nameLength switch
            {
                0x20 => 2,
                0x80 => 3,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            });
            binary.PutInt32(fileCount);

            byte lengthsByte = (byte)(
                (isBigEndian ? 2 : 0) | (lengthsKnown ? 1 : 0));
            binary.PutByte(lengthsByte);
            binary.PutByte(0);
            binary.PutByte(0);
            binary.PutByte(0);

            for (int i = 0; i < fileCount; i++)
            {
                binary.PutInt32(Entries[i].Offset);
            }
            long pad = (-stream.Position) & 15;
            stream.Write(new byte[pad]);

            byte[] namebuf = new byte[nameLength];
            for (int i = 0; i < fileCount; i++)
            {
                Array.Clear(namebuf, 0, nameLength);
                Encoding.ASCII.GetBytes(Entries[i].Name, namebuf);
                stream.Write(namebuf);
            }

            for (int i = 0; i < fileCount; i++)
            {
                binary.PutInt32(Entries[i].Property);
            }
            pad = (-stream.Position) & 15;
            stream.Write(new byte[pad]);

            if (lengthsKnown)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    binary.PutInt32(Entries[i].Length);
                }
                pad = (-stream.Position) & 15;
                stream.Write(new byte[pad]);
            }

            Debug.Assert(stream.Position == 16 + nameLength * fileCount +
                (lengthsKnown ? 12 : 8) * ((fileCount + 3) & -4));
        }

        private async Task WriteEntries(Stream stream)
        {
            long pad = (-stream.Position) & 0x7F;
            stream.Write(new byte[pad]);
            foreach (ParEntry parEntry in Entries)
            {
                parEntry.Offset = (int)stream.Position;
                using Stream entryStream = await parEntry.GetData().ConfigureAwait(false);
                await entryStream.CopyToAsync(stream).ConfigureAwait(false); pad = (-stream.Position) & 0x7F;
                stream.Write(new byte[pad]);
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
                if (entry.Name.EndsWith(".par"))
                {
                    using Stream stream = await entry.GetData();
                    ParFile parFile = new ParFile(stream);
                    await parFile.ExtractAll(destDir + "\\" + entry.Name[0..^4] + "_par");
                }
                else if (entry.Name.EndsWith(".pta"))
                {
                    using Stream stream = await entry.GetData();
                    ParFile parFile = new ParFile(stream);
                    await parFile.ExtractAll(destDir + "\\" + entry.Name[0..^4] + "_pta");
                }
                else
                {
                    using Stream stream = await entry.GetData();
                    using FileStream fileStream = new FileStream(destDir + "\\" + entry.Name, FileMode.Create, FileAccess.Write);
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
                writer.WriteLine(entry.Property);
                writer.WriteLine(entry.Name);
            }
        }
        #endregion
        #region Save to Stream
        /// <summary>
        /// Writes par file to a stream.
        /// Make sure the stream is at position 0 before using.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task SaveTo(Stream stream)
        {
            int headerLength = 16 + nameLength * fileCount +
                (lengthsKnown ? 12 : 8) * ((fileCount + 3) & -4);
            stream.Write(new byte[headerLength]);
            await WriteEntries(stream).ConfigureAwait(false);
            stream.Position = 0;
            WriteHeader(stream);
        }
        #endregion

        public ParEntry GetEntry(string fileName)
        {
            return _entries.Find(e => e.Name == fileName);
        }
    }
}
