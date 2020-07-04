using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Linq;

namespace InstantImprovement.DataControl
{
    public static class XLSExporter
    {
        private static readonly string _workbookName = string.Empty;
        private static Excel.Application _app;
        private static Excel.Workbook _workbook;

        public static void InitializeExcel(string sheetname, bool visible = true)
        {
            // Create a new Excel Application
            _app = new Excel.Application();
            _app.Visible = visible;

            // Create a new, empty workbook and add it to the collection returned
            _workbook = _app.Workbooks.Add(Missing.Value);
            _workbook.Worksheets.Item[1].Name = sheetname;
        }

        public static void AddWorksheet(string name)
        {
            var sheets = _workbook.Sheets;
            // Check if name length is sufficient
            if (name.Length > 30)
                name = name.Substring(0, 30);

            // Add new Worksheet to Collection
            var sheet = sheets.Add(Missing.Value, sheets[sheets.Count], Missing.Value, Excel.XlSheetType.xlWorksheet);
            sheet.Name = name;
        }

        public static void WriteValue<T>(string InputWorksheet, T value, int row, int column, bool dotReplace = true)
        {
            // Get Current Worksheet
            var sheet = (Excel.Worksheet)_workbook.Sheets[InputWorksheet];

            // Write Values to Cells
            string cellName;

            cellName = CellAddress(sheet, row, column);
            if(dotReplace == true)
                _app.Range[cellName].Value = value.ToString().Replace('.', ',');
            else
                _app.Range[cellName].Value = value.ToString();
            row++;
    }

        public static int GetWorksheetIndex(string name)
        {
            int index = 0;
            foreach (Excel._Worksheet worksheet in _app.Sheets)
            {
                if (string.Equals(worksheet.Name, name) == true)
                    return index;

                index++;
            }
            return -1;
        }

        private static string CellAddress(Excel._Worksheet sht, int row, int col)
        {
            return RangeAddress(sht.Cells[row, col]);
        }

        private static string RangeAddress(Excel.Range rng)
        {
            return rng.get_AddressLocal(false, false, Excel.XlReferenceStyle.xlA1,
                   Missing.Value, Missing.Value);
        }

        public static void ShowResults()
        {
            _app.Visible = true;
        }

        public static bool IsExcelInstalled()
        {
            Type officeType = Type.GetTypeFromProgID("Excel.Application");
            if (officeType == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void WriteMetaToExcel(string sheetname, Dictionary<string, RingBuffer> RingBufferDictionary)
        {
            int row = 1;
            int column = 1;
            XLSExporter.WriteValue(sheetname, "Instant Improvement Evaluation Results", row, column);

                        
            foreach (KeyValuePair<string, RingBuffer> RingBufferPair in RingBufferDictionary)
            {
                row = 3;
                XLSExporter.WriteValue(sheetname, RingBufferPair.Key, row++, column);

                for (int i = 0; i < RingBufferPair.Value.RawData.Count; i++)
                {
                    XLSExporter.WriteValue(sheetname, RingBufferPair.Value.RawData[i], row++, column);
                }

                column++;
            }
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
