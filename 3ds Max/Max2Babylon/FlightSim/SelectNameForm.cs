using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autodesk.Max;
using Excel = Microsoft.Office.Interop.Excel;

namespace Max2Babylon.Forms
{
    public partial class SelectNameForm : Form
    {

        private AnimationGroup animationGroup;
        private TextBox nameTextBox;
        private MaxNodeTreeView maxNodeTreeView;

        public SelectNameForm(TextBox _nameBox, AnimationGroup _animationGroup , MaxNodeTreeView _maxNodeTreeView)
        {
            InitializeComponent();
            nameTextBox = _nameBox;
            animationGroup = _animationGroup;
            maxNodeTreeView = _maxNodeTreeView;
        }

        private void SelectNameForm_Load(object sender, EventArgs e)
        {
            List<TreeNode> result =  GetExcelFile();
            foreach (TreeNode treeNode in result)
            {
                preDefiniedNameList.Nodes.Add(treeNode);
            }
        }

        private List<TreeNode> GetExcelFile()
        {
            List<TreeNode> xmlElements = new List<TreeNode>();
            try
            {
                //Create COM Objects. Create a COM object for everything that is referenced
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(@"U:\Licences_Projets\FLIGHT_SIM\ART\Planes\NamingConventions.xlsx");
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;



                int targetColumn = 7;
                int subtargetColumn = 9;
                //excel is not zero based!!
                for (int i = 2; i <= rowCount; i++)
                {
                    if (xlRange.Cells[i, targetColumn] != null && xlRange.Cells[i, targetColumn].Value2 != null)
                    {
                        string rootElement = xlRange.Cells[i, targetColumn].Value2.ToString();
                        TreeNode element = new TreeNode(rootElement);

                        if (xlRange.Cells[i, subtargetColumn] != null && xlRange.Cells[i, subtargetColumn].Value2 != null)
                        {
                            //element has subelements
                            string subElementsValue = xlRange.Cells[i, subtargetColumn].Value2.ToString();
                            subElementsValue = subElementsValue.Replace(" ", ""); //remove empty spaces
                            var subElements = subElementsValue.Split(',');

                            foreach (string subElement in subElements)
                            {
                                TreeNode subTreeNode = new TreeNode(subElement);
                                element.Nodes.Add(subTreeNode);
                            }
                        }

                        xmlElements.Add(element);
                    }
                }

                //cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                //close and release
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                //quit and release
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
            catch (Exception e)
            {
                MessageBox.Show("Impossible to retrieve XLS file");
            }
            

            return xmlElements;
        }

        private void preDefName_DoubleClick(object sender, EventArgs e)
        {
            if (preDefiniedNameList.SelectedNode != null)
            {
                if (preDefiniedNameList.SelectedNode.Nodes.Count>0)
                {
                    MessageBox.Show("This Animation Group Template cannot be added because contains child");
                    return;
                }

                string element = preDefiniedNameList.SelectedNode.Text;
                nameTextBox.Text = element;
                if (maxNodeTreeView.Nodes.Count > 1)
                {
                    string message = "Multiple nodes detected in Animation Group. Cannot use XLS naming convention.";
                    MessageBox.Show(message);
                    return;
                }

                string regex = @"(?i)^x[0-9]_" + element;
                var nodesList = Tools.Nodes(Loader.Core.RootNode);
                List<IINode> capturedNodes = new List<IINode>();

                foreach (IINode iNode in nodesList)
                {
                    if (iNode.Name == element)
                    {
                        capturedNodes.Add(iNode);
                        break;
                    }

                    Match match = Regex.Match(iNode.Name, regex);
                    if (match.Success)
                    {
                        capturedNodes.Add(iNode);
                    }
                }

                if (capturedNodes.Count > 1)
                {
                    // case there are:
                    // x0_nodeName
                    // x1_nodeName
                    // X2_nodeName
                    MessageBox.Show("More then one node match the pattern " + regex);
                    return;
                }

                if (capturedNodes.Count == 1)
                {
                    //one node respect the pattern add it on the view
                    maxNodeTreeView.BeginUpdate();
                    maxNodeTreeView.QueueSetNodes(null);
                    maxNodeTreeView.QueueAddNode(capturedNodes[0]);
                    maxNodeTreeView.EndUpdate();
                    return;
                }

                if (capturedNodes.Count == 0 && maxNodeTreeView.Nodes.Count == 1)
                {
                    // no node respect the pattern
                    // rename the one in the tree view , with the same name of animgroup
                    string prefix = "";
                    string prefixRegex = @"(?i)^x[0-9]_";
                    Match match = Regex.Match(maxNodeTreeView.Nodes[0].Name, prefixRegex);
                    string previousNodeName = maxNodeTreeView.Nodes[0].Text;
                    IINode previousNode = Loader.Core.GetINodeByName(previousNodeName);
                    if (match.Success)
                    {
                        prefix = previousNode.Name.Substring(0, 2);
                    }
                    maxNodeTreeView.BeginUpdate();
                    maxNodeTreeView.QueueSetNodes(null);
                    previousNode.Name = prefix + element;
                    maxNodeTreeView.QueueAddNode(previousNode);
                    maxNodeTreeView.EndUpdate();
                }

                
            }
        }
    }
}
