using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System.IO;

namespace Imas.Records
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
