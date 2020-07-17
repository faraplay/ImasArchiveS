using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public class ParFile : ContainerFile<ParEntry>
    {
        #region Fields
        int fileCount;

        #endregion

        private int HeaderLength => 16 + nameLength * fileCount +
                (lengthsKnown ? 12 : 8) * ((fileCount + 3) & -4);

        #region Constructors
        public ParFile(Stream stream)
        {
            _stream = stream;
            ReadHeader();
        }
        #endregion

        #region Header
        private byte parVersion;
        private int nameLength;
        private bool lengthsKnown;
        private void ReadHeader()
        {
            _stream.Position = 0;
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

        private void WriteHeader(Stream stream, long baseOffset)
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
                binary.PutUInt((uint)Entries[i].Offset);
            }
            long pad = (baseOffset - stream.Position) & 15;
            stream.Write(new byte[pad]);

            byte[] namebuf = new byte[nameLength];
            for (int i = 0; i < fileCount; i++)
            {
                Array.Clear(namebuf, 0, nameLength);
                Encoding.ASCII.GetBytes(Entries[i].FileName, namebuf);
                stream.Write(namebuf);
            }

            for (int i = 0; i < fileCount; i++)
            {
                binary.PutInt32(Entries[i].Property);
            }
            pad = (baseOffset - stream.Position) & 15;
            stream.Write(new byte[pad]);

            if (lengthsKnown)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    binary.PutUInt((uint)Entries[i].Length);
                }
                pad = (baseOffset - stream.Position) & 15;
                stream.Write(new byte[pad]);
            }

            Debug.Assert(stream.Position - baseOffset == HeaderLength);
        }
        #endregion
        #region Extract
        public async Task ExtractAll(string destDir)
        {
            Directory.CreateDirectory(destDir);
            WriteDotParInfo(destDir);
            foreach (ParEntry entry in _entries)
            {
                if (entry.FileName.EndsWith(".par"))
                {
                    using Stream stream = await entry.GetData();
                    ParFile parFile = new ParFile(stream);
                    await parFile.ExtractAll(destDir + "\\" + entry.FileName[0..^4] + "_par");
                }
                else if (entry.FileName.EndsWith(".pta"))
                {
                    using Stream stream = await entry.GetData();
                    ParFile parFile = new ParFile(stream);
                    await parFile.ExtractAll(destDir + "\\" + entry.FileName[0..^4] + "_pta");
                }
                else
                {
                    using Stream stream = await entry.GetData();
                    using FileStream fileStream = new FileStream(destDir + "\\" + entry.FileName, FileMode.Create, FileAccess.Write);
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
                writer.WriteLine(entry.FileName);
            }
        }
        #endregion
        #region Save to Stream
        /// <summary>
        /// Writes par file to a stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task SaveTo(Stream stream)
        {
            long baseOffset = stream.Position;
            stream.Write(new byte[HeaderLength]);
            await WriteEntries(stream, baseOffset).ConfigureAwait(false);
            stream.Position = baseOffset;
            WriteHeader(stream, baseOffset);
        }

        private async Task WriteEntries(Stream stream, long baseOffset)
        {
            long pad = (baseOffset - stream.Position) & 0x7F;
            stream.Write(new byte[pad]);
            foreach (ParEntry parEntry in Entries)
            {
                parEntry.Offset = (int)(stream.Position - baseOffset);
                using Stream entryStream = await parEntry.GetData().ConfigureAwait(false);
                await entryStream.CopyToAsync(stream).ConfigureAwait(false); 
                pad = (baseOffset - stream.Position) & 0x7F;
                stream.Write(new byte[pad]);
            }
        }
        #endregion

        /// <summary>
        /// Replaces contents of the par file with files in the directory 
        /// with matching name.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public async Task ReplaceEntries(string dirName)
        {
            foreach (ParEntry entry in Entries)
            {
                string path = dirName + "\\" + entry.FileName;

                if (File.Exists(path))
                {
                    using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    await entry.SetData(fileStream).ConfigureAwait(false);
                }
                else if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                {
                    string childDir = path[0..^4] + '_' + path[^3..];
                    if (Directory.Exists(childDir))
                    {
                        using Stream entryStream1 = await entry.GetData().ConfigureAwait(false);
                        using ParFile childPar = new ParFile(entryStream1);
                        await childPar.ReplaceEntries(childDir).ConfigureAwait(false);
                        entry.NewData.SetLength(0);
                        await childPar.SaveTo(entry.NewData).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces contents of the par file with files in the directory 
        /// with matching name, and saves to a stream.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public async Task ReplaceEntriesAndSaveTo(Stream outStream, string dirName)
        {
            long baseOffset = outStream.Position;
            outStream.Write(new byte[HeaderLength]);
            long pad = (-HeaderLength) & 0x7F;
            outStream.Write(new byte[pad]);

            foreach (ParEntry parEntry in Entries)
            {
                parEntry.Offset = (int)(outStream.Position - baseOffset);
                string path = dirName + "\\" + parEntry.FileName;

                if (File.Exists(path))
                {
                    using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    await parEntry.SetData(fileStream).ConfigureAwait(false);
                }
                else
                {
                    if (parEntry.FileName.EndsWith(".par") || parEntry.FileName.EndsWith(".pta"))
                    {
                        string childDir = path[0..^4] + '_' + path[^3..];
                        if (Directory.Exists(childDir))
                        {
                            using Stream entryStream1 = await parEntry.GetData().ConfigureAwait(false);
                            using ParFile childPar = new ParFile(entryStream1);
                            parEntry.NewData.SetLength(0);
                            await childPar.ReplaceEntriesAndSaveTo(parEntry.NewData, childDir).ConfigureAwait(false);
                        }
                    }
                }
                using Stream entryStream = await parEntry.GetData().ConfigureAwait(false);
                await entryStream.CopyToAsync(outStream).ConfigureAwait(false);
                pad = (baseOffset - outStream.Position) & 0x7F;
                outStream.Write(new byte[pad]);
            }

            outStream.Position = baseOffset;
            WriteHeader(outStream, baseOffset);
        }

        public ParEntry GetEntry(string fileName)
        {
            return _entries.Find(e => e.FileName == fileName);
        }
    }
}
