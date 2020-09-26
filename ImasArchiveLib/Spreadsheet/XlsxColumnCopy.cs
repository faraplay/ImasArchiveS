using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Imas.Spreadsheet
{
    public class XlsxColumnCopy : IDisposable
    {
        private readonly SpreadsheetDocument refDoc;
        private readonly WorkbookPart refWorkbookPart;
        private readonly Sheets refSheets;
        private readonly SharedStringTablePart refSharedStringTablePart;


        private readonly SpreadsheetDocument editDoc;
        private readonly WorkbookPart editWorkbookPart;
        private readonly Sheets editSheets;
        private readonly SharedStringTablePart editSharedStringTablePart;
        private readonly Dictionary<string, int> strings;

        private static readonly string[] sheetsToCopy =
        {
            "accessory",
            "album",
            "albumCommu",
            "costume",
            "dlcName",
            "item",
            "lesson_menu",
            "mail_system",
            "money",
            "nonUnitFanUp",
            "producerRank",
            "profile",
            "reporter",
            "seasonText",
            "fanLetterStrings",
            "jaJpStrings",
            "workInfo",
            "rivalInfo",
            "pastbl",
            "songInfo",
            "skillBoardStrings",
        };

        public XlsxColumnCopy(string editXlsxName, string refXlsxName)
        {
            refDoc = SpreadsheetDocument.Open(refXlsxName, false);
            refWorkbookPart = refDoc.WorkbookPart;
            refSharedStringTablePart = refWorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            refSheets = refWorkbookPart.Workbook.Sheets;

            editDoc = SpreadsheetDocument.Open(editXlsxName, true);
            editWorkbookPart = editDoc.WorkbookPart;
            editSheets = editDoc.WorkbookPart.Workbook.Sheets;
            editSharedStringTablePart = editWorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            strings = new Dictionary<string, int>();
            foreach (var item in editSharedStringTablePart.SharedStringTable.Elements<SharedStringItem>())
            {
                strings.Add(item.InnerText, strings.Count);
            }
        }


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
                refDoc?.Dispose();
                editDoc?.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable

        public void CopyColumns(IProgress<ProgressData> progress = null)
        {
            int total = sheetsToCopy.Length;
            int count = 0;
            foreach (string sheetName in sheetsToCopy)
            {
                count++;
                progress?.Report(new ProgressData { count = count, total = total, filename = sheetName });
                CopySheetColumns(sheetName, sheetName);
            }
        }

        private void CopySheetColumns(string refSheetName, string editSheetName)
        {
            Worksheet refWorksheet = GetWorksheet(refSheetName, refSheets, refWorkbookPart);
            Worksheet editWorksheet = GetWorksheet(editSheetName, editSheets, editWorkbookPart);
            if (refWorksheet == null || editWorksheet == null)
            {
                return;
            }
            var refHeaderCells = GetHeaderCells(refWorksheet).Where(cell => !string.IsNullOrEmpty(GetRefString(cell)));
            var editHeaderCells = GetHeaderCells(editWorksheet).Where(cell => !string.IsNullOrEmpty(GetEditString(cell)));
            foreach (Cell refCell in refHeaderCells)
            {
                string headerText = GetRefString(refCell);
                Cell editCell = editHeaderCells.FirstOrDefault(cell => GetEditString(cell) == headerText);
                if (editCell != default)
                {
                    string refColumnAddress = GetColumnName(refCell.CellReference);
                    string editColumnAddress = GetColumnName(editCell.CellReference);
                    CopyColumn(refWorksheet, refColumnAddress, editWorksheet, editColumnAddress);
                }
            }
        }

        private void CopyColumn(Worksheet refWorksheet, string refColumnAddress, Worksheet editWorksheet, string editColumnAddress)
        {
            Cell[] refColumn = refWorksheet.Descendants<Cell>()
                .Where(cell => GetColumnName(cell.CellReference) == refColumnAddress)
                .OrderBy(cell => GetRowIndex(cell.CellReference))
                .ToArray();
            Cell[] editColumn = editWorksheet.Descendants<Cell>()
                .Where(cell => GetColumnName(cell.CellReference) == editColumnAddress)
                .OrderBy(cell => GetRowIndex(cell.CellReference))
                .ToArray();
            int top = Math.Min(refColumn.Length, editColumn.Length);
            for (int i = 1; i < top; i++) // start from 1 to skip header cell
            {
                string refValue = GetRefString(refColumn[i]);
                if (!string.IsNullOrEmpty(refValue))
                {
                    editColumn[i].DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.SharedString);
                    editColumn[i].CellValue = new CellValue(GetEditSharedStringID(refValue).ToString());
                }
            }
        }

        private Worksheet GetWorksheet(string sheetName, Sheets sheets, WorkbookPart workbookPart)
        {
            Sheet sheet = sheets.Descendants<Sheet>().FirstOrDefault(sheet => sheet.Name == sheetName);
            if (sheet == null)
                return null;
            WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(sheet.Id));
            return worksheetPart.Worksheet;
        }

        private IEnumerable<Cell> GetHeaderCells(Worksheet worksheet)
        {
            return worksheet.Descendants<Cell>().Where(cell => GetRowIndex(cell.CellReference) == 1);
        }

        private string GetRefString(Cell cell)
        {
            if (cell.DataType != null)
            {
                return cell.DataType.Value switch
                {
                    CellValues.SharedString => refSharedStringTablePart.SharedStringTable.ElementAt(int.Parse(cell.InnerText))
                        .InnerText.Replace("_x000D_", ""),
                    _ => cell.CellValue.Text,
                };
            }
            else
            {
                return "";
            }
        }

        private string GetEditString(Cell cell)
        {
            if (cell.DataType != null)
            {
                return cell.DataType.Value switch
                {
                    CellValues.SharedString => editSharedStringTablePart.SharedStringTable.ElementAt(int.Parse(cell.InnerText))
                        .InnerText.Replace("_x000D_", ""),
                    _ => cell.CellValue.Text,
                };
            }
            else
            {
                return "";
            }
        }

        // Given a cell name, parses the specified cell to get the column name.
        private static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        // Given a cell name, parses the specified cell to get the row index.
        private static uint GetRowIndex(string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellName);

            return uint.Parse(match.Value);
        }


        #region SharedString

        private int GetEditSharedStringID(string s)
        {
            if (strings.ContainsKey(s))
                return strings[s];
            else
                return AddEditSharedString(s);
        }

        private int AddEditSharedString(string s)
        {
            int index = editSharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().Count();
            editSharedStringTablePart.SharedStringTable.AppendChild(new SharedStringItem(new Text(s)));
            strings.Add(s, index);
            return index;
        }

        #endregion SharedString
    }
}
