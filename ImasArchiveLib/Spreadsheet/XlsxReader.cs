using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Imas.Spreadsheet
{
    internal class XlsxReader : IDisposable
    {
        private readonly SpreadsheetDocument doc;
        private readonly WorkbookPart workbookPart;
        private readonly Sheets sheets;
        private readonly SharedStringTablePart sharedStringTablePart;

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

        public XlsxReader(string xlsxName)
        {
            doc = SpreadsheetDocument.Open(xlsxName, false);
            workbookPart = doc.WorkbookPart;
            sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
            sheets = workbookPart.Workbook.Sheets;
        }

        public IEnumerable<T> GetRows<T>(Sheet sheet, IProgress<ProgressData> progress = null) where T : IRecordable, new()
        {
            WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(sheet.Id));
            var rows = worksheetPart.Worksheet.Descendants<Row>();
            int total = rows.Count();
            int count = 0;
            List<T> list = new List<T>();
            foreach (Row row in rows)
            {
                if (row.RowIndex == 1)
                    continue;
                T record = new T();
                record.ReadRow(this, row);
                list.Add(record);
                count++;
                progress?.Report(new ProgressData { count = count, total = total, filename = record.ToString() });
            }
            return list;
        }

        public IEnumerable<T> GetRows<T>(string sheetName) where T : IRecordable, new()
        {
            Sheet sheet = sheets.Descendants<Sheet>().FirstOrDefault(sheet => sheet.Name == sheetName);
            if (sheet != null)
                return GetRows<T>(sheet, null);
            else
                return Enumerable.Empty<T>();
        }

        public IEnumerable<Record> GetRows(string format, string sheetName)
        {
            Sheet sheet = sheets.Descendants<Sheet>().FirstOrDefault(sheet => sheet.Name == sheetName);
            if (sheet == null)
                return Enumerable.Empty<Record>();
            WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(sheet.Id));
            var rows = worksheetPart.Worksheet.Descendants<Row>();
            List<Record> list = new List<Record>();
            foreach (Row row in rows)
            {
                if (row.RowIndex == 1)
                    continue;
                Record record = new Record(format);
                record.ReadRow(this, row);
                list.Add(record);
            }
            return list;
        }

        #region Get Cell Value

        public string GetString(Row row, string colName)
        {
            string address = colName + row.RowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null)
            {
                if (cell.DataType == null || cell.DataType.Value == CellValues.Number)
                {
                    return cell.InnerText;
                }
                else
                {
                    return cell.DataType.Value switch
                    {
                        CellValues.SharedString => sharedStringTablePart.SharedStringTable.ElementAt(int.Parse(cell.InnerText))
                            .InnerText.Replace("_x000D_", ""),
                        CellValues.String => cell.CellValue.InnerText,
                        _ => throw new InvalidDataException("Expected a string in cell" + address),
                    };
                }
            }
            else
            {
                return "";
            }
        }

        public int GetInt(Row row, string colName)
        {
            string address = colName + row.RowIndex.ToString();
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

        public short GetShort(Row row, string colName)
        {
            string address = colName + row.RowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null && (cell.DataType == null || cell.DataType.Value == CellValues.Number))
            {
                if (short.TryParse(cell.InnerText, out short result))
                    return result;
                else
                    throw new InvalidDataException("Could not parse contents of " + address + " as integer");
            }
            else
                throw new InvalidDataException("Expected a number in cell " + address);
        }

        public byte GetByte(Row row, string colName)
        {
            string address = colName + row.RowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null && (cell.DataType == null || cell.DataType.Value == CellValues.Number))
            {
                if (byte.TryParse(cell.InnerText, out byte result))
                    return result;
                else
                    throw new InvalidDataException("Could not parse contents of " + address + " as integer");
            }
            else
                throw new InvalidDataException("Expected a number in cell " + address);
        }

        public bool GetBool(Row row, string colName)
        {
            string address = colName + row.RowIndex.ToString();
            Cell cell = row.Descendants<Cell>().FirstOrDefault(cell => cell.CellReference == address);
            if (cell != null && cell.DataType.Value == CellValues.Boolean)
            {
                return cell.InnerText != "0";
            }
            else
                throw new InvalidDataException("Expected a boolean in cell " + address);
        }

        #endregion Get Cell Value
    }
}