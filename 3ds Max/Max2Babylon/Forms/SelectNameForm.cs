using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace Max2Babylon.Forms
{
    public partial class SelectNameForm : Form
    {

        private TextBox nameTextBox;

        public SelectNameForm(TextBox _nameBox)
        {
            InitializeComponent();
            nameTextBox = _nameBox;
        }

        private void SelectNameForm_Load(object sender, EventArgs e)
        {
            preDefiniedNameList.DataSource = GetExcelFile();
        }

        private List<string> GetExcelFile()
        {
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(@"U:\Licences_Projets\FLIGHT_SIM\ART\Planes\NamingConventions.xlsx");
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            List<string> result = new List<string>();

            int targetColumn = 7;
            //excel is not zero based!!
            for (int i = 2; i <= rowCount; i++)
            {
                if (xlRange.Cells[i, targetColumn] != null && xlRange.Cells[i, targetColumn].Value2 != null)
                {
                    string value = xlRange.Cells[i, targetColumn].Value2.ToString();
                    result.Add(value);
                }
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return result;
        }

        private void preDefName_DoubleClick(object sender, MouseEventArgs e)
        {
            if (preDefiniedNameList.SelectedItem != null)
            {
                nameTextBox.Text = preDefiniedNameList.SelectedItem.ToString();
            }
        }
    }
}
