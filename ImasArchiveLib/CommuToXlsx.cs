using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas
{
    class CommuToXlsx : IDisposable
    {
        readonly List<CommuLine> lines = new List<CommuLine>();
        readonly SpreadsheetDocument doc;
        readonly WorkbookPart workbookPart;
        readonly Sheets sheets;
        readonly Dictionary<string, WorksheetPart> worksheets = new Dictionary<string, WorksheetPart>();
        readonly SharedStringTablePart sharedStringTablePart;
        readonly Dictionary<string, int> strings = new Dictionary<string, int>();

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
            }
            disposed = true;
        }
        #endregion

        public CommuToXlsx() { }

        public CommuToXlsx(string fileName, FileMode fileMode)
        {
            switch (fileMode)
            {
                case FileMode.Create:
                    doc = SpreadsheetDocument.Create(fileName, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                    workbookPart = doc.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    sheets = doc.WorkbookPart.Workbook.AppendChild(new Sheets());
                    sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>();
                    sharedStringTablePart.SharedStringTable = new SharedStringTable();
                    break;
                case FileMode.Open:
                    doc = SpreadsheetDocument.Open(fileName, false);
                    break;
            }
        }

        public void AddCommuFromBin(Stream binStream, string fileName)
        {
            try
            {
                Binary binary = new Binary(binStream, true);
                if (binary.GetUInt() != 0x004D0053 ||
                    binary.GetUInt() != 0x00470000)
                {
                    throw new InvalidDataException();
                }

                int msgCount = (int)binary.GetUInt();

                if (binary.GetUInt() != 0)
                {
                    throw new InvalidDataException();
                }

                byte[] namebuf = new byte[32];
                byte[] msgbuf = new byte[128];
                for (int i = 0; i < msgCount; i++)
                {
                    int lineID = binary.GetInt32();
                    byte flag1 = binary.GetByte();
                    byte flag2 = binary.GetByte();
                    if (binary.GetUShort() != 0 ||
                        binary.GetUInt() != 0 ||
                        binary.GetUInt() != 0 ||
                        binary.GetUInt() != 0 ||
                        binary.GetUInt() != 0)
                    {
                        throw new InvalidDataException();
                    }
                    uint nameLen = binary.GetUInt();
                    uint msgLen = binary.GetUInt();

                    binStream.Read(namebuf);
                    binStream.Read(msgbuf);

                    string name = Encoding.BigEndianUnicode.GetString(namebuf);
                    name = name.Substring(0, name.IndexOf('\0'));
                    string msg = Encoding.BigEndianUnicode.GetString(msgbuf);
                    msg = msg.Substring(0, msg.IndexOf('\0'));

                    lines.Add(new CommuLine
                    {
                        file = fileName,
                        messageID = lineID,
                        flag1 = flag1,
                        flag2 = flag2,
                        name = name,
                        message = msg,
                    });
                }
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException();
            }

        }

        public void WriteXlsx()
        {
            CommuLine line = new CommuLine
            {
                file = "filename.txt",
                messageID = 1001,
                flag1 = 1,
                flag2 = 1,
                name = "Ami",
                message = "Hello there"
            };
            AppendRow("ami", line);
        }

        public void WriteCommuToXlsx()
        {
            foreach (CommuLine line in lines)
            {
                string sheetName;
                if (line.file[11] == '_')
                    sheetName = "other";
                else
                    sheetName = line.file[11..14];
                AppendRow(sheetName, line);
            }
            lines.Clear();
        }

        private WorksheetPart GetWorksheet(string sheetName)
        {
            if (worksheets.ContainsKey(sheetName))
                return worksheets[sheetName];
            else
                return AddWorksheet(sheetName);
        }

        private WorksheetPart AddWorksheet(string sheetName)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            Sheet sheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = (uint)(sheets.Elements<Sheet>().Count() + 1),
                Name = sheetName
            };
            sheets.Append(sheet);
            worksheets.Add(sheetName, worksheetPart);
            return worksheetPart;
        }

        private int GetStringID(string s)
        {
            if (strings.ContainsKey(s))
                return strings[s];
            else
                return AddString(s);
        }
        private int AddString(string s)
        {
            int index = sharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().Count();
            sharedStringTablePart.SharedStringTable.AppendChild(new SharedStringItem(new Text(s)));
            strings.Add(s, index);
            return index;
        }

        private void AppendCell(Row row, string colName, uint rowIndex, string text)
        {
            Cell cell = new Cell { CellReference = colName + rowIndex.ToString() };
            row.Append(cell);
            int stringIndex = GetStringID(text);
            cell.CellValue = new CellValue(stringIndex.ToString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.SharedString);
        }
        private void AppendCell(Row row, string colName, uint rowIndex, int value)
        {
            Cell cell = new Cell { CellReference = colName + rowIndex.ToString() };
            row.Append(cell);
            cell.CellValue = new CellValue(value.ToString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Number);
        }

        private void AppendRow(string sheetName, CommuLine line)
        {
            WorksheetPart worksheetPart = GetWorksheet(sheetName);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            uint rowIndex = (uint)(sheetData.Elements<Row>().Count() + 1);
            Row row = new Row { RowIndex = rowIndex };
            sheetData.Append(row);

            AppendCell(row, "A", rowIndex, line.file);
            AppendCell(row, "B", rowIndex, line.messageID);
            AppendCell(row, "C", rowIndex, line.flag1);
            AppendCell(row, "D", rowIndex, line.flag2);
            AppendCell(row, "E", rowIndex, line.name);
            AppendCell(row, "F", rowIndex, line.message);
        }
    }

    class CommuLine
    {
        public string file;
        public int messageID;
        public byte flag1;
        public byte flag2;
        public string name;
        public string message;
    }
}
