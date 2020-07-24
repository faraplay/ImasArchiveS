using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Spreadsheet
{
    class XlsxReader : IDisposable
    {
        readonly SpreadsheetDocument doc;
        readonly WorkbookPart workbookPart;
        readonly Sheets sheets;
        readonly SharedStringTablePart sharedStringTablePart;

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
        #endregion

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
                count++;
                progress?.Report(new ProgressData { count = count, total = total });
                if (row.RowIndex == 1)
                    continue;
                T record = new T();
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
                        CellValues.SharedString => sharedStringTablePart.SharedStringTable.ElementAt(int.Parse(cell.InnerText)).InnerText,
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
        #endregion
    }
}
