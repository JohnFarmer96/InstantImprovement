// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XLSExporter.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// XLSExporter extracts relevant data and stores is into self-created Excel-sheet
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;

namespace InstantImprovement.DataControl
{
    /// <summary>
    /// XLSExporter extracts relevant data and stores is into self-created Excel-sheet
    /// </summary>
    public static class XLSExporter
    {
        /// <summary>
        /// Name of used Workbook
        /// </summary>
        private static readonly string _workbookName = string.Empty;

        /// <summary>
        /// Reference to Excel Application
        /// </summary>
        private static Excel.Application _app;

        /// <summary>
        /// Main Chart to display summary
        /// </summary>
        private static Excel.Chart _mainChart;

        /// <summary>
        /// Series Collection for main-chart
        /// </summary>
        private static Excel.SeriesCollection _mainChartSeriesCollection;

        /// <summary>
        /// Reference to Excel Workbook
        /// </summary>
        private static Excel.Workbook _workbook;

        /// <summary>
        /// Add Worksheet to current Excel-Workbook
        /// </summary>
        /// <param name="name">Desired Sheetname</param>
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

        /// <summary>
        /// Initialize Excel Application
        /// </summary>
        /// <param name="sheetname">Desired Sheetname</param>
        /// <param name="visible">Determine whether Excel-Sheet is visible or not</param>
        public static void InitializeExcel(string sheetname, bool visible = true)
        {
            // Create a new Excel Application
            _app = new Excel.Application();
            _app.Visible = visible;

            // Create a new, empty workbook and add it to the collection returned
            _workbook = _app.Workbooks.Add(Missing.Value);
            _workbook.Worksheets.Item[1].Name = sheetname;

            // Initialize _mainChart.
            var worksheet = _workbook.Worksheets.Item[1] as
                Microsoft.Office.Interop.Excel.Worksheet;
            var charts = worksheet.ChartObjects() as
                Microsoft.Office.Interop.Excel.ChartObjects;
            var chartObject = charts.Add(10, 10, 800, 450) as
                Microsoft.Office.Interop.Excel.ChartObject;
            _mainChart = chartObject.Chart;
            _mainChart.ChartType = Microsoft.Office.Interop.Excel.XlChartType.xlXYScatterLinesNoMarkers;
            _mainChart.ChartWizard(Title: sheetname);
            _mainChartSeriesCollection = _mainChart.SeriesCollection();
        }

        /// <summary>
        /// Check if Excel Application is installed on computer
        /// </summary>
        /// <returns>Indicator about Excel being available</returns>
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

        /// <summary>
        /// Make Excel Application visible
        /// </summary>
        public static void ShowResults()
        {
            _app.Visible = true;
        }

        /// <summary>
        /// Write RingBuffer to Excel
        /// </summary>
        /// <param name="sheetname"></param>
        /// <param name="ringBuffer"></param>
        public static void WriteMetaToExcel(string sheetname, RingBuffer ringBuffer)
        {
            int row = 1;
            int column = 1;
            XLSExporter.WriteValue(sheetname, "Instant Improvement Evaluation Results", row, column);

            row = 3;

            //Headlines
            XLSExporter.WriteValue(sheetname, "Delta T [s]", row, column);
            XLSExporter.WriteValue(sheetname, "Values", row, column + 1);

            row++;

            for (int i = 0; i < ringBuffer.RawData.Count; i++)
            {
                XLSExporter.WriteValue(sheetname, ringBuffer.RawData[i].Timestamp, row + i, column, false);
                XLSExporter.WriteValue(sheetname, ringBuffer.RawData[i].Value, row + i, column + 1, false);
            }

            GenerateChart(sheetname, rowStart: row, rowEnd: row + ringBuffer.RawData.Count - 1);
        }

        /// <summary>
        /// Add Labels to Chart Axis
        /// </summary>
        /// <param name="chart">Desired Chart</param>
        private static void AddAxisLabels(Excel.Chart chart)
        {
            var yAxis = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary);
            yAxis.HasTitle = true;
            yAxis.AxisTitle.Text = "Erkennungsrate [%]";
            yAxis.MaximumScale = 100;
            yAxis.AxisTitle.Orientation = Excel.XlOrientation.xlVertical;

            var xAxis = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary);
            xAxis.HasTitle = true;
            xAxis.AxisTitle.Text = "Zeit [s]";
            xAxis.AxisTitle.Orientation = Excel.XlOrientation.xlHorizontal;
        }

        /// <summary>
        /// Get address of certain cell by row- and column-index
        /// </summary>
        /// <param name="sht">Desired Worksheet</param>
        /// <param name="row">Row Index</param>
        /// <param name="col">Column Index</param>
        /// <returns></returns>
        private static string CellAddress(Excel._Worksheet sht, int row, int col)
        {
            return RangeAddress(sht.Cells[row, col]);
        }

        /// <summary>
        /// Auto Generate Chart according to stored RingBuffer-Values 
        /// </summary>
        /// <param name="sheetname">Desired Workscheet</param>
        /// <param name="rowStart">Starting Row Index</param>
        /// <param name="rowEnd">Ending Row Index</param>
        private static void GenerateChart(string sheetname, int rowStart, int rowEnd)
        {
            // Add chart.
            var worksheet = _workbook.Sheets[sheetname] as
                Microsoft.Office.Interop.Excel.Worksheet;
            var charts = worksheet.ChartObjects() as
                Microsoft.Office.Interop.Excel.ChartObjects;
            var chartObject = charts.Add(120, 10, 600, 300) as
                Microsoft.Office.Interop.Excel.ChartObject;
            var chart = chartObject.Chart;

            // Set chart range.
            var range = worksheet.get_Range(CellAddress(worksheet, rowStart, 1), CellAddress(worksheet, rowEnd, 2));
            chart.SetSourceData(range);

            // Set chart properties.
            chart.ChartType = Microsoft.Office.Interop.Excel.XlChartType.xlXYScatterLinesNoMarkers;
            chart.ChartWizard(Source: range,
                Title: sheetname);

            AddAxisLabels(chart);

            // Add to main Chart
            var line = _mainChartSeriesCollection.NewSeries();
            line.XValues = worksheet.get_Range(CellAddress(worksheet, rowStart, 1), CellAddress(worksheet, rowEnd, 1));
            line.Values = worksheet.get_Range(CellAddress(worksheet, rowStart, 2), CellAddress(worksheet, rowEnd, 2));
            line.Name = sheetname;

            AddAxisLabels(_mainChart);
        }

        /// <summary>
        /// Get Address of corresponding range
        /// </summary>
        /// <param name="rng">Desired Range</param>
        /// <returns>Address-String</returns>
        private static string RangeAddress(Excel.Range rng)
        {
            return rng.get_AddressLocal(false, false, Excel.XlReferenceStyle.xlA1,
                   Missing.Value, Missing.Value);
        }

        /// <summary>
        /// Native Implementation of Value-Writing into certain Excel-Worksheet
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="InputWorksheet">Desired Worksheet</param>
        /// <param name="value">Value to write</param>
        /// <param name="row">Row-Index</param>
        /// <param name="column">Column-Index</param>
        /// <param name="toString">Convert Value to String?</param>
        private static void WriteValue<T>(string InputWorksheet, T value, int row, int column, bool toString = true)
        {
            // Get Current Worksheet
            var sheet = (Excel.Worksheet)_workbook.Sheets[InputWorksheet];

            // Write Values to Cells
            string cellName;

            cellName = CellAddress(sheet, row, column);
            if (toString == true)
                _app.Range[cellName].Value = value.ToString().Replace('.', ',');
            else
                _app.Range[cellName].Value = value;
        }
    }
}