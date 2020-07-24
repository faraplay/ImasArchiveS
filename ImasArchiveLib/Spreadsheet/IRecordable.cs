using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.Spreadsheet
{
    interface IRecordable
    {
        public void Deserialise(Stream inStream);
        public void Serialise(Stream outStream);
        public void ReadRow(XlsxReader xlsx, Row row);
        public void WriteRow(XlsxWriter xlsx, Row row);
        public void WriteFirstRow(XlsxWriter xlsx, Row row);
    }
}
