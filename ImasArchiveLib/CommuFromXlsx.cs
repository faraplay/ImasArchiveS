using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Imas
{
    public class CommuFromXlsx : IDisposable
    {
        readonly List<CommuLine> lines = new List<CommuLine>();
        readonly SpreadsheetDocument doc;
        readonly WorkbookPart workbookPart;
        readonly Sheets sheets;
        readonly SharedStringTablePart sharedStringTablePart;
        readonly List<WorksheetPart> worksheetParts = new List<WorksheetPart>();
        readonly List<string> commuFilenames = new List<string>();

        readonly ZipArchive zipArchive;

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                doc?.Dispose();
                zipArchive?.Dispose();
            }
            disposed = true;
        }
        #endregion

        public CommuFromXlsx(string xlsxName, string zipName)
        {
            doc = SpreadsheetDocument.Open(xlsxName, false);
            workbookPart = doc.WorkbookPart;
            sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
            sheets = workbookPart.Workbook.Sheets;

            zipArchive = new ZipArchive(new FileStream(zipName, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Create);
        }

        #region Get Commus
        public void GetAllCommus()
        {
            foreach (Sheet sheet in sheets.Descendants<Sheet>())
            {
                GetCommuLines(sheet);
            }
        }
        private void GetCommuLines(Sheet sheet)
        {
            WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(sheet.Id));
            var rows = worksheetPart.Worksheet.Descendants<Row>();
            foreach (Row row in rows)
            {
                GetCommuLine(row);
            }
        }

        private void GetCommuLine(Row row)
        {
            try
            {
                uint index = row.RowIndex;
                string file = CellValueString(row, "A", index);
                int messageID = CellValueInt(row, "B", index);
                byte flag1 = CellValueBool(row, "C", index) ? (byte)1 : (byte)0;
                byte flag2 = CellValueBool(row, "D", index) ? (byte)1 : (byte)0;
                string name_raw = CellValueString(row, "E", index);
                string message_raw = CellValueString(row, "F", index);
                string name = CellValueString(row, "G", index);
                string message1 = CellValueString(row, "H", index);
                string message2 = CellValueString(row, "J", index);
                string message = message1;
                if (!string.IsNullOrWhiteSpace(message2))
                    message += '\n' + message2;
                lines.Add(new CommuLine
                {
                    file = file,
                    messageID = messageID,
                    flag1 = flag1,
                    flag2 = flag2,
                    name_raw = name_raw,
                    message_raw = message_raw,
                    name = name,
                    message = message
                });
                if (!commuFilenames.Contains(file))
                    commuFilenames.Add(file);
            }
            catch (InvalidDataException ex)
            {
                ex.ToString();
            }
        }

        private string CellValueString(Row row, string colName, uint rowIndex)
        {
            string address = colName + rowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null)
            {
                if (cell.DataType == null || cell.DataType.Value == CellValues.Number)
                {
                    return cell.InnerText;
                }
                else
                {
                    switch (cell.DataType.Value)
                    {
                        case CellValues.SharedString:
                            return sharedStringTablePart.SharedStringTable.ElementAt(int.Parse(cell.InnerText)).InnerText;
                        default:
                            throw new InvalidDataException("Expected a string in cell" + address);
                    }
                }
            }
            else
            {
                return "";
            }
        }

        private int CellValueInt(Row row, string colName, uint rowIndex)
        {
            string address = colName + rowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null && (cell.DataType == null || cell.DataType.Value == CellValues.Number))
            {
                if (int.TryParse(cell.InnerText, out int result))
                    return result;
                else
                    throw new InvalidDataException("Could not parse contents of " + address + " as integer");
            }
            else
                throw new InvalidDataException("Expected a number in cell " + address);
        }
        private bool CellValueBool(Row row, string colName, uint rowIndex)
        {
            string address = colName + rowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null && cell.DataType.Value == CellValues.Boolean)
            {
                return cell.InnerText != "0";
            }
            else
                throw new InvalidDataException("Expected a boolean in cell " + address);
        }
        #endregion
        #region Write Commus
        public void GetAndWriteAllCommus()
        {
            foreach (Sheet sheet in sheets.Descendants<Sheet>())
            {
                GetCommuLines(sheet);
                foreach (string filename in commuFilenames)
                {
                    WriteBin(filename);
                }
                lines.Clear();
                commuFilenames.Clear();
            }
        }
        private void WriteBin(string commuName)
        {
            if (lines.Any(line =>
                line.file == commuName &&
                (!string.IsNullOrWhiteSpace(line.name) || !string.IsNullOrWhiteSpace(line.message))
                ))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry(commuName);
                using Stream stream = entry.Open();
                WriteBin(stream, lines.Where(line => line.file == commuName));
            }

        }

        private void WriteBin(Stream stream, IEnumerable<CommuLine> lines)
        {
            Binary binary = new Binary(stream, true);
            binary.PutUInt(0x004D0053);
            binary.PutUInt(0x00470000);
            binary.PutInt32(lines.Count());
            binary.PutUInt(0);
            foreach (CommuLine line in lines)
            {
                binary.PutInt32(line.messageID);
                binary.PutByte(line.flag1);
                binary.PutByte(line.flag2);
                binary.PutUShort(0);
                binary.PutUInt(0);
                binary.PutUInt(0);
                binary.PutUInt(0);
                binary.PutUInt(0);

                string name = string.IsNullOrWhiteSpace(line.name) ? line.name_raw : line.name;
                string message = string.IsNullOrWhiteSpace(line.message) ? line.message_raw : line.message;

                byte[] namebytes = ToCustomEncoding(name);
                byte[] msgbytes = ToCustomEncoding(message);

                int nameLen = Math.Min(namebytes.Length, 30);
                int msgLen = Math.Min(msgbytes.Length, 126);

                binary.PutInt32(nameLen / 2);
                binary.PutInt32(msgLen / 2);

                stream.Write(namebytes, 0, nameLen);
                stream.Write(new byte[32 - nameLen]);
                stream.Write(msgbytes, 0, msgLen);
                stream.Write(new byte[128 - msgLen]);
            }
        }

        private static byte[] ToCustomEncoding(string s)
        {
            List<byte> list = new List<byte>();
            NextByteOptions next = NextByteOptions.None;
            foreach (char c in s)
            {
                switch (next)
                {
                    case NextByteOptions.AllASCII:
                        if (c > 0x20 && c < 0x7F)
                        {
                            list.Add((byte)(c + 0x80));
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            list.Add(0);
                        break;
                    case NextByteOptions.Lowercase:
                        if (c == 0x20 || c > 0x60 && c <= 0x7A)
                        {
                            list.Add((byte)(c + 0x80));
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            list.Add(0);
                        break;
                    case NextByteOptions.SpaceOnly:
                        if (c == 0x20)
                        {
                            list.Add((byte)(c + 0x80));
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            list.Add(0);
                        break;
                    case NextByteOptions.None:
                        break;
                }
                if (c == 0x20)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.AllASCII;
                }
                else if (c > 0x60 && c <= 0x7A)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.Lowercase;
                }
                else if (c >= 0x20 && c < 0x7F)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.SpaceOnly;
                }
                else
                {
                    list.Add((byte)((c & 0xFF00) >> 8));
                    list.Add((byte)(c & 0xFF));
                    next = NextByteOptions.None;
                }
            }
            switch (next)
            {
                case NextByteOptions.AllASCII:
                case NextByteOptions.Lowercase:
                case NextByteOptions.SpaceOnly:
                    list.Add(0);
                    break;
                case NextByteOptions.None:
                    break;
            }
            return list.ToArray();
        }
        private enum NextByteOptions
        {
            None,
            SpaceOnly,
            Lowercase,
            AllASCII
        }
        #endregion
    }
}
