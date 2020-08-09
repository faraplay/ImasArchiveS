using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    static class Pastbl
    {
        public static readonly string[] fileNames =
        {
            "alf/auditionText_par/auditionNorText.pastbl",
            "alf/auditionText_par/auditionDanText.pastbl",
            "alf/auditionText_par/auditionVisText.pastbl",
            "alf/auditionText_par/auditionVocText.pastbl",
            "alf/liveText_par/liveText.pastbl"
        };
        public static IEnumerable<Record> ReadFile(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            if (binary.ReadUInt64() != 0x706173747274626C) //"pastrtbl"
            {
                return Enumerable.Empty<Record>();
            }
            if (binary.ReadInt32() != 1)
            {
                return Enumerable.Empty<Record>();
            }
            int recCount = binary.ReadInt32();
            int offOffset = binary.ReadInt32();
            int lenOffset = binary.ReadInt32();

            Record[] records = new Record[recCount];
            for (int i = 0; i < recCount; i++)
            {
                records[i] = new Record("ISX");
            }
            stream.Position = offOffset;
            foreach (Record record in records)
            {
                record[0] = binary.ReadInt32();
            }
            stream.Position = lenOffset;
            foreach (Record record in records)
            {
                record[1] = binary.ReadInt16();
            }

            byte[] strBytes = new byte[256];
            foreach (Record record in records)
            {
                stream.Position = (int)record[0];
                int byteCount = 2 * (short)record[1];
                stream.Read(strBytes, 0, byteCount);
                record[2] = Encoding.BigEndianUnicode.GetString(strBytes, 0, byteCount);
            }

            return records;
        }

        public static void WriteFile(Stream stream, List<string> strings)
        {
            Binary binary = new Binary(stream, true);
            int recCount = strings.Count();
            int[] offsets = new int[recCount];
            int[] lengths = new int[recCount];
            byte[][] arrays = new byte[recCount][];
            int currentOffset = 0x20 + ((4 * recCount + 15) & -16) + ((2 * recCount + 15) & -16);
            for (int i = 0; i < recCount; i++)
            {
                int[] tempArray = ImasEncoding.Custom.GetValues(strings[i]);
                offsets[i] = currentOffset;
                lengths[i] = tempArray.Length;
                int arrayLen = (2 * lengths[i] + 16) & -16;
                arrays[i] = new byte[arrayLen];
                for (int j = 0; j < tempArray.Length; j++)
                {
                    arrays[i][2 * j] = (byte)((tempArray[j] >> 8) & 0xFF);
                    arrays[i][2 * j + 1] = (byte)(tempArray[j] & 0xFF);
                }
                currentOffset += arrayLen;
            }

            binary.WriteInt64(0x706173747274626C);
            binary.WriteInt32(1);
            binary.WriteInt32(recCount);
            binary.WriteInt32(0x20);
            binary.WriteInt32(0x20 + ((4 * recCount + 15) & -16));
            binary.WriteInt32(0);
            binary.WriteInt32(0);
            for (int i = 0; i < recCount; i++)
            {
                binary.WriteInt32(offsets[i]);
            }
            for (int k = recCount; k % 4 != 0; k++)
                binary.WriteInt32(0);
            for (int i = 0; i < recCount; i++)
            {
                binary.WriteInt16((short)lengths[i]);
            }
            for (int k = recCount; k % 8 != 0; k++)
                binary.WriteInt16(0);
            for (int i = 0; i < recCount; i++)
            {
                stream.Write(arrays[i]);
            }
        }
    }
}
