using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System;

namespace Imas.Records
{
    internal class RowReader
    {
        private readonly XlsxReader xlsx;
        private readonly Row row;
        private int colIndex;

        public RowReader(XlsxReader xlsx, Row row)
        {
            this.xlsx = xlsx;
            this.row = row;
            colIndex = 1;
        }

        public bool ReadBool() => xlsx.GetBool(row, ColumnName(colIndex++));

        public byte ReadByte() => xlsx.GetByte(row, ColumnName(colIndex++));

        public short ReadShort() => xlsx.GetShort(row, ColumnName(colIndex++));

        public int ReadInt() => xlsx.GetInt(row, ColumnName(colIndex++));

        public string ReadString() => xlsx.GetString(row, ColumnName(colIndex++));

        private static string ColumnName(int colIndex)
        {
            if (colIndex == 0)
                return "";
            else if (colIndex > 0)
                return ColumnName((colIndex - 1) / 26) + (char)('A' + ((colIndex - 1) % 26));
            else
                throw new ArgumentOutOfRangeException("Column index must be nonnegative");
        }
    }
}