using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelPrint
{
    public class PrintToExcel
    {
        public Excel.Application Application;
        public Excel.Workbook Workbook;
        public Excel.Worksheet Worksheet;
        public Excel.Range CurrentCells;
        public PrintToExcel()
        {
            Application = new Excel.Application();
        }


        public void Open(string path)
        {
            Workbook = Application.Workbooks.Open(path);
        }

        public void CreateWorkBook()
        {
            Workbook = Application.Workbooks.Add(Type.Missing);
        }

        public void SetCurrentSheet(int index)
        {
            Worksheet = (Excel.Worksheet)Workbook.Worksheets.get_Item(index);
        }

        public void SetCurrentCell(string start, string end)
        {
            CurrentCells = Worksheet.get_Range(start, end);
        }

        public void SetCurrentCell(int startRowIndex, int startColumnIndex, int endRowIndex, int endColumnIndex)
        {
            CurrentCells = Worksheet.get_Range(((Excel.Range)Worksheet.Cells[startRowIndex, startColumnIndex]).Address, ((Excel.Range)Worksheet.Cells[endRowIndex, endColumnIndex]).Address);
        }

        public void SetCurrentCell(int RowIndex, int ColumnIndex)
        {
            CurrentCells = Worksheet.get_Range(((Excel.Range)Worksheet.Cells[RowIndex, ColumnIndex]).Address, ((Excel.Range)Worksheet.Cells[RowIndex, ColumnIndex]).Address);
        }

        public void SetCurrentCellByName(string nameRange)
        {
            for (int i = 1; i < Worksheet.Names.Count; i++)
            {
                if (Worksheet.Names.Item(i, Type.Missing, Type.Missing).Name == Worksheet.Name + "!" + nameRange)
                {
                    CurrentCells = Worksheet.Names.Item(i, Type.Missing, Type.Missing).RefersToRange;
                    return;
                }
            }
            CurrentCells = null;
        }

        public void CopyRowToNew(int destRow)
        {
            Excel.Range range = ((Excel.Range)Worksheet.Cells[destRow, 1]).EntireRow;
            range.Insert(Excel.XlInsertShiftDirection.xlShiftDown, false);
            range = ((Excel.Range)Worksheet.Cells[destRow + 1, 1]).EntireRow;
            range.Copy(((Excel.Range)Worksheet.Cells[destRow, 1]).EntireRow);
        }

        public void CreateStandartHeader(string text)
        {
            CurrentCells.Merge(Type.Missing);
            CurrentCells.Font.Name = "Times News Roman";
            CurrentCells.Font.Bold = true;
            CurrentCells.Font.Size = 12;
            CurrentCells.Borders.Weight = 3;
            CurrentCells.Borders.Value = 1;
            CurrentCells.HorizontalAlignment = Excel.Constants.xlCenter;
            CurrentCells.VerticalAlignment = Excel.Constants.xlCenter;
            CurrentCells.Value2 = text;
            CurrentCells.WrapText = true;
        }

        public void SelectedCellsToBuffer(DataGridView gridView)
        {
            if (gridView.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    Clipboard.SetDataObject(
                        gridView.GetClipboardContent());
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    MessageBox.Show("The Clipboard could not be accessed. Please try again.");
                }
            }
        }



        public void PasteFromBuffer(string headerText, int rowCount, int columnCount, bool wrapText)
        {
            SetCurrentCell(1, 1, 1, columnCount);
            CreateStandartHeader(headerText);
            SetCurrentCell(2, 1, rowCount + 2, columnCount);
            Worksheet.Paste(CurrentCells, false);
            CurrentCells.Font.Name = "Times News Roman";
            CurrentCells.Font.Size = 10;
            CurrentCells.Borders.Value = 1;
            CurrentCells.Borders.Weight = 2;



            ((Excel.Range)Worksheet.Cells[2, 1]).Value2 = "№ п/п";
            ((Excel.Range)Worksheet.Cells[2, 1]).Font.Bold = true;
            for (int i = 0; i < rowCount; i++)
            {
                ((Excel.Range)Worksheet.Cells[i + 3, 1]).Value2 = (i + 1);
            }

            SetCurrentCell(3, 1, rowCount + 2, columnCount);
            CurrentCells.WrapText = wrapText;
            CurrentCells.EntireColumn.AutoFit();
            CurrentCells.EntireRow.AutoFit();

            //Application.Visible = true;
            Finally();
        }

        public void PasteFromBuffer(string headerText, int rowCount, int rowHeight, int columnCount, int columnWidth, bool wrapText, bool NeedFinally)
        {
            Worksheet.Columns.AutoFit();
            SetCurrentCell(1, 1, 1, columnCount);
            CreateStandartHeader(headerText);
            SetCurrentCell(2, 1, rowCount + 2, columnCount);

            
            Worksheet.Paste(CurrentCells, false);
            CurrentCells.Font.Name = "Times News Roman";
            CurrentCells.Font.Size = 10;
            CurrentCells.Borders.Value = 1;
            CurrentCells.Borders.Weight = 2;

            

            ((Excel.Range)Worksheet.Cells[2, 1]).Value2 = "№";
            ((Excel.Range)Worksheet.Cells[2, 1]).Font.Bold = true;
            for (int i = 0; i < rowCount; i++)
            {
                ((Excel.Range)Worksheet.Cells[i + 3, 1]).Value2 = (i + 1);
            }
            SetCurrentCell(3, 1, rowCount + 2, columnCount);
            CurrentCells.WrapText = wrapText;
            CurrentCells.EntireColumn.ColumnWidth = columnWidth;
            CurrentCells.EntireRow.RowHeight = rowHeight;
            CurrentCells.EntireColumn.AutoFit();
            //CurrentCells.EntireRow.AutoFit();
            //Application.Visible = true;
            if (NeedFinally)
                Finally();
        }

        public void Finally()
        {
            Application.Visible = true;

            Application.Interactive = true;
            Application.ScreenUpdating = true;
            Application.UserControl = true;
            releaseObject(CurrentCells);
            releaseObject(Worksheet);
            releaseObject(Application);
        }


        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                GC.Collect();
            }
        }

    }
}
