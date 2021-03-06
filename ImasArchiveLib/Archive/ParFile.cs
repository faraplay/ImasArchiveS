﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public class ParFile : ContainerFile<ParEntry>
    {
        #region Fields

        private int fileCount;

        #endregion Fields

        private int HeaderLength => 16 + nameLength * fileCount +
                (lengthsKnown ? 12 : 8) * ((fileCount + 3) & -4);

        #region Constructors

        public ParFile(Stream stream)
        {
            _stream = stream;
            ReadHeader();
        }

        #endregion Constructors

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
            nameLength = binary.ReadInt32() switch
            {
                2 => 0x20,
                3 => 0x80,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            };

            fileCount = binary.ReadInt32();

            lengthsKnown = binary.ReadByte() switch
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
                offsets[i] = binary.ReadInt32();
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
                props[i] = binary.ReadInt32();
            }
            pad = (-_stream.Position) & 15;
            _stream.Position += pad;

            int[] lengths = new int[fileCount];
            if (lengthsKnown)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    lengths[i] = binary.ReadInt32();
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
            binary.WriteInt32(nameLength switch
            {
                0x20 => 2,
                0x80 => 3,
                _ => throw new InvalidDataException(Strings.InvalidData_ParHeader)
            });
            binary.WriteInt32(Entries.Count);

            byte lengthsByte = (byte)(
                (isBigEndian ? 2 : 0) | (lengthsKnown ? 1 : 0));
            binary.WriteByte(lengthsByte);
            binary.WriteByte(0);
            binary.WriteByte(0);
            binary.WriteByte(0);

            foreach (ParEntry entry in Entries)
            {
                binary.WriteUInt32((uint)entry.Offset);
            }
            long pad = (baseOffset - stream.Position) & 15;
            stream.Write(new byte[pad]);

            byte[] namebuf = new byte[nameLength];
            foreach (ParEntry entry in Entries)
            {
                Array.Clear(namebuf, 0, nameLength);
                Encoding.ASCII.GetBytes(entry.FileName, namebuf);
                stream.Write(namebuf);
            }

            foreach (ParEntry entry in Entries)
            {
                binary.WriteInt32(entry.Property);
            }
            pad = (baseOffset - stream.Position) & 15;
            stream.Write(new byte[pad]);

            if (lengthsKnown)
            {
                foreach (ParEntry entry in Entries)
                {
                    binary.WriteUInt32((uint)entry.Length);
                }
                pad = (baseOffset - stream.Position) & 15;
                stream.Write(new byte[pad]);
            }

            Debug.Assert(stream.Position - baseOffset == HeaderLength);
        }

        #endregion Header

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

        #endregion Extract

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

        #endregion Save to Stream

        /// <summary>
        /// Replaces contents of the par file with files in the directory
        /// with matching name.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public async Task ReplaceEntries(IFileSource fileSource)
        {
            foreach (ParEntry entry in Entries)
            {
                if (fileSource.FileExists(entry.FileName))
                {
                    using Stream fileStream = fileSource.OpenFile(entry.FileName);
                    await entry.SetData(fileStream).ConfigureAwait(false);
                }
                else if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                {
                    string childDir = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                    if (fileSource.DirectoryExists(childDir))
                    {
                        using Stream entryStream1 = await entry.GetData().ConfigureAwait(false);
                        using ParFile childPar = new ParFile(entryStream1);
                        await childPar.ReplaceEntries(fileSource.OpenDirectory(childDir)).ConfigureAwait(false);
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
        public async Task ReplaceEntriesAndSaveTo(Stream outStream, IFileSource fileSource)
        {
            long baseOffset = outStream.Position;
            outStream.Write(new byte[HeaderLength]);
            long pad = (-HeaderLength) & 0x7F;
            outStream.Write(new byte[pad]);

            foreach (ParEntry entry in Entries)
            {
                entry.Offset = (int)(outStream.Position - baseOffset);

                if (fileSource.FileExists(entry.FileName))
                {
                    using Stream fileStream = fileSource.OpenFile(entry.FileName);
                    await entry.SetData(fileStream).ConfigureAwait(false);
                }
                else
                {
                    if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                    {
                        string childDir = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                        if (fileSource.DirectoryExists(childDir))
                        {
                            using Stream entryStream1 = await entry.GetData().ConfigureAwait(false);
                            using ParFile childPar = new ParFile(entryStream1);
                            entry.NewData.SetLength(0);
                            await childPar.ReplaceEntriesAndSaveTo(entry.NewData, fileSource.OpenDirectory(childDir)).ConfigureAwait(false);
                        }
                    }
                }
                using Stream entryStream = await entry.GetData().ConfigureAwait(false);
                await entryStream.CopyToAsync(outStream).ConfigureAwait(false);
                pad = (baseOffset - outStream.Position) & 0x7F;
                outStream.Write(new byte[pad]);
            }

            outStream.Position = baseOffset;
            WriteHeader(outStream, baseOffset);
        }
    }
}