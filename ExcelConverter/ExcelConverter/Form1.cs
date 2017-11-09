using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void txtSource_Enter(object sender, EventArgs e)
        {
            if (txtSource.Text == "Source folder")
            {
                txtSource.Text = "";
                txtSource.ForeColor = Color.Black;
            }
        }

        private void txtDestination_Enter(object sender, EventArgs e)
        {
            if (txtDestination.Text == "Destination folder")
            {
                txtDestination.Text = "";
                txtDestination.ForeColor = Color.Black;
            }
        }

        private void txtSource_Leave(object sender, EventArgs e)
        {
            if (txtSource.Text == "")
            {
                txtSource.Text = "Source folder";
                txtSource.ForeColor = Color.LightGray;
            }
        }

        private void txtDestination_Leave(object sender, EventArgs e)
        {
            if (txtDestination.Text == "")
            {
                txtDestination.Text = "Destination folder";
                txtDestination.ForeColor = Color.LightGray;
            }
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtSource.Text = fbd.SelectedPath;
                    txtSource.ForeColor = Color.Black;
                }
            }
        }

        private void btnDestination_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtDestination.Text = fbd.SelectedPath;
                    txtDestination.ForeColor = Color.Black;
                }
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            var sourceFolder = new DirectoryInfo(txtSource.Text);
            var destinationFolder = new DirectoryInfo(txtDestination.Text);

            try
            {
                foreach (var file in sourceFolder.GetFiles())
                {
                    var stream = file.OpenRead();

                    ISheet sheet = null;
                    if (file.Extension.Equals(".xls"))
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(0);
                    }
                    else if (file.Extension.Equals(".xlsx"))
                    {
                        XSSFWorkbook xssfwb = new XSSFWorkbook(stream);
                        sheet = xssfwb.GetSheetAt(0);
                    }

                    if (sheet != null)
                    {
                        IEnumerator rows = sheet.GetRowEnumerator();
                        int? dataRow = null;
                        int dataCol = 0; //default sheet
                        var titleRow = sheet.GetRow(0);

                        bool isEOS = titleRow.Cells[0].ToString().Trim() == "No" ? true : false;
                        List<dynamic> records = new List<dynamic>();
                        for (int i = 1; i < sheet.PhysicalNumberOfRows; i++)
                        {
                            var row = sheet.GetRow(i);
                            string loginName = "";
                            double mark = -1;

                            if (isEOS)
                            {
                                loginName = row.Cells[1].ToString().Trim();
                                mark = row.Cells[7].NumericCellValue;
                            }
                            else
                            {
                                if (row.GetCell(4, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null && row.GetCell(9, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null)
                                {
                                    loginName = row.GetCell(4, MissingCellPolicy.RETURN_NULL_AND_BLANK).ToString().Trim().Split(new char[] { '@' })[0];
                                    mark = double.Parse(row.GetCell(9, MissingCellPolicy.RETURN_NULL_AND_BLANK).ToString());
                                }
                            }

                            if (loginName != "" && loginName != null && mark != -1)
                            {
                                Regex regex = new Regex("\\w{2}\\d+$");
                                var match = regex.Match(loginName.ToUpper()).Value;
                                loginName = match != "" ? match : $"MSSV({loginName})";
                                records.Add(new { LoginName = loginName, Mark = mark });
                            }
                        }

                        if (!(new DirectoryInfo(@destinationFolder.FullName/* + "\\Converted"*/)).Exists)
                        {
                            Directory.CreateDirectory(destinationFolder.FullName);
                            //DirectoryInfo temp2 = new DirectoryInfo(@destinationFolder.FullName);
                            //temp2.CreateSubdirectory("Converted");
                        }

                        using (FileStream destinationFileStream = new FileStream(@destinationFolder.FullName + /*"\\Converted" + */"\\" + file.Name, FileMode.Create, FileAccess.Write))
                        {
                            IWorkbook wb = new XSSFWorkbook();
                            ISheet sh = wb.CreateSheet("Sheet1");
                            ICreationHelper cH = wb.GetCreationHelper();

                            if (records.Count > 0)
                            {
                                IRow row = sh.CreateRow(0);
                                ICell cell = row.CreateCell(0);
                                cell.SetCellValue(cH.CreateRichTextString("Lop"));
                                cell = row.CreateCell(1);
                                cell.SetCellValue(cH.CreateRichTextString("MSSV"));
                                cell = row.CreateCell(2);
                                cell.SetCellValue(cH.CreateRichTextString("Mark"));
                                cell = row.CreateCell(3);
                                cell.SetCellValue(cH.CreateRichTextString("Note"));
                            }

                            for (int i = 0; i < records.Count; i++)
                            {
                                IRow row = sh.CreateRow(i + 1);
                                ICell cell = row.CreateCell(0);
                                cell.SetCellValue(cH.CreateRichTextString(""));
                                cell = row.CreateCell(1);
                                cell.SetCellValue(cH.CreateRichTextString(records[i].LoginName));
                                cell = row.CreateCell(2);
                                cell.SetCellValue(records[i].Mark);
                                cell = row.CreateCell(3);
                                cell.SetCellValue(cH.CreateRichTextString(""));
                            }

                            wb.Write(destinationFileStream);
                        }
                    }
                }

                lbMessage.Text = "Converted successfully";
                lbMessage.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lbMessage.Text = "Error! Please check your input";
                lbMessage.ForeColor = Color.Red;
            }
        }
    }
}
