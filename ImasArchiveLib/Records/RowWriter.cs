using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imas.Records
{
    class RowWriter
    {
        readonly XlsxWriter xlsx;
        readonly Row row;
        int colIndex;

        public RowWriter(XlsxWriter xlsx, Row row)
        {
            this.xlsx = xlsx;
            this.row = row;
            colIndex = 1;
        }

        public void Write(bool value) => xlsx.AppendCell(row, ColumnName(colIndex++), value);
        public void Write(int value) => xlsx.AppendCell(row, ColumnName(colIndex++), value);
        public void Write(string value) => xlsx.AppendCell(row, ColumnName(colIndex++), value);
        public void Write(byte[] value) => xlsx.AppendCell(row, ColumnName(colIndex++), ImasEncoding.Custom.GetString(value));

        static string ColumnName(int colIndex)
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
