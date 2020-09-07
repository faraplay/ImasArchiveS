using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imas.Spreadsheet
{
    internal class XlsxWriter : IDisposable
    {
        private readonly SpreadsheetDocument doc;
        private readonly WorkbookPart workbookPart;
        private readonly Sheets sheets;
        private readonly SharedStringTablePart sharedStringTablePart;

        private readonly Dictionary<string, WorksheetPart> worksheets = new Dictionary<string, WorksheetPart>();
        private readonly Dictionary<string, int> strings = new Dictionary<string, int>();

        public Sheets Sheets => sheets;

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

        #endregion IDisposable

        public XlsxWriter(string fileName)
        {
            doc = SpreadsheetDocument.Create(fileName, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            workbookPart = doc.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            sheets = doc.WorkbookPart.Workbook.AppendChild(new Sheets());
            sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>();
            sharedStringTablePart.SharedStringTable = new SharedStringTable();
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

        #region SharedString

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

        #endregion SharedString

        #region AppendCell

        public void AppendCell(Row row, string colName, string value)
        {
            Cell cell = new Cell { CellReference = colName + row.RowIndex.ToString() };
            row.Append(cell);
            int stringIndex = GetStringID(value);
            cell.CellValue = new CellValue(stringIndex.ToString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.SharedString);
        }

        public void AppendCell(Row row, string colName, int value)
        {
            Cell cell = new Cell { CellReference = colName + row.RowIndex.ToString() };
            row.Append(cell);
            cell.CellValue = new CellValue(value.ToString());
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Number);
        }

        public void AppendCell(Row row, string colName, bool value)
        {
            Cell cell = new Cell { CellReference = colName + row.RowIndex.ToString() };
            row.Append(cell);
            cell.CellValue = new CellValue(value ? "1" : "0");
            cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Boolean);
        }

        #endregion AppendCell

        public void AppendRow<T>(string sheetName, T record) where T : IRecordable
        {
            WorksheetPart worksheetPart = GetWorksheet(sheetName);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            AppendRow<T>(sheetData, record);
        }

        private void AppendRow<T>(SheetData sheetData, T record) where T : IRecordable
        {
            uint rowIndex = (uint)(sheetData.Elements<Row>().Count() + 1);
            if (rowIndex == 1)
            {
                Row firstRow = new Row { RowIndex = rowIndex };
                sheetData.Append(firstRow);
                record.WriteFirstRow(this, firstRow);
                rowIndex++;
            }

            Row row = new Row { RowIndex = rowIndex };
            sheetData.Append(row);
            record.WriteRow(this, row);
        }

        public void AppendRows<T>(string sheetName, IEnumerable<T> records) where T : IRecordable
        {
            WorksheetPart worksheetPart = GetWorksheet(sheetName);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            foreach (T record in records)
            {
                AppendRow(sheetData, record);
            }
        }
    }
}