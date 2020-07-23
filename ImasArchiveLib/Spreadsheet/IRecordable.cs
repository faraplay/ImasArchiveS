using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.Spreadsheet
{
    interface IRecordable
    {
        public void Serialise(Stream outStream);
        public void ReadRow(Row row, XlsxReader xlsx);
    }
}
