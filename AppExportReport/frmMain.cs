using Excel;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace AppExportReport
{
    public partial class frmMain : Form
    {
        DataSet dataSet;
        private int _iTotal;
        private List<Config> configs = new List<Config>();

        public frmMain()
        {
            InitializeComponent();
            
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblNotification.Text = "Vui lòng chọn file để kết xuất dữ liệu báo cáo!";

            //dpMonth.Format = DateTimePickerFormat.Custom;
            //dpMonth.CustomFormat = "MM/yyyy";
            //dpMonth.ShowUpDown = true;

            //prgBpercent.DisplayStyle = ProgressBarDisplayText.CustomText;
            prgBpercent.Visible = false;
            lblPercent.Visible = false;
            lblPercent.Text = "0";

            LoadConfig();

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog sourceFileOpenFileDialog = new OpenFileDialog();
                sourceFileOpenFileDialog.InitialDirectory = "C:\\";
                sourceFileOpenFileDialog.Filter = "Excel Files (*.xls;*.xlsx;)|*.xls;*.xlsx;";
                sourceFileOpenFileDialog.FilterIndex = 2;
                sourceFileOpenFileDialog.RestoreDirectory = true;
                sourceFileOpenFileDialog.Multiselect = false;
                sourceFileOpenFileDialog.Title = "Chọn file import dữ liệu";

                if (sourceFileOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileStream stream = File.Open(sourceFileOpenFileDialog.FileName, FileMode.Open, FileAccess.Read);
                        IExcelDataReader excelReader;

                        if (System.IO.Path.GetExtension(sourceFileOpenFileDialog.FileName.ToString()).ToLower() == ".xlsx")
                        {
                            //1. Reading from a OpenXml Excel file (2007 format; *.xlsx)
                            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else
                        {
                            //2. Reading from a binary Excel file ('97-2003 format; *.xls)
                            excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }

                        //3. DataSet - The result of each spreadsheet will be created in the result.Tables
                        //DataSet result = excelReader.AsDataSet();

                        //4. DataSet - Create column names from first row
                        excelReader.IsFirstRowAsColumnNames = true;
                        dataSet = excelReader.AsDataSet();

                        //5. Data Reader methods
                        try
                        {
                            while (excelReader.Read())
                            {

                            }
                        }
                        catch
                        { }

                        excelReader.Close();

                        txtFilePath.Text = Path.GetFullPath(sourceFileOpenFileDialog.FileName.ToString());



                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                dataSet.Tables[0].Rows[0].Delete();
                                dataSet.Tables[0].AcceptChanges();
                            }

                            lblNotification.Text = "Tổng số báo cáo sẽ kết xuất: " + dataSet.Tables[0].Rows.Count.ToString("#,###");
                            _iTotal = dataSet.Tables[0].Rows.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Có lỗi xảy ra khi Import thông tin ! \r\n " + ex.Message, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string filename = string.Empty;
            int i = 0;
            try
            {
                prgBpercent.Visible = true;
                //lblPercent.Visible = true;
                prgBpercent.Minimum = i;
                prgBpercent.Maximum = _iTotal;


                foreach (DataRow dr in dataSet.Tables[0].Rows)
                {
                    i++;
                    Dictionary<string, string> replaceDict = new Dictionary<string, string>();
                    foreach (var config in configs)
                    {
                        if (config.param.Equals("#requestcode#") || config.param.Equals("#requestdesc#")|| config.param.Equals("#filename#"))
                        {
                            if (config.param.Equals("#requestcode#") || config.param.Equals("#requestdesc#"))
                            {
                                if (dr[int.Parse(config.col)].ToString().Length > 7)
                                {
                                    replaceDict.Add("#requestcode#", dr[int.Parse(config.col)].ToString().Substring(0, 7));
                                    replaceDict.Add("#requestdesc#", dr[int.Parse(config.col)].ToString());
                                }
                                else
                                {
                                    replaceDict.Add("#requestcode#", dr[int.Parse(config.col)].ToString());
                                    replaceDict.Add("#requestdesc#", dr[int.Parse(config.col)].ToString());
                                }
                            }

                            if (config.param.Equals("#filename#"))
                            {
                                filename = dr[int.Parse(config.col)].ToString();
                            }
                        }
                        else {
                            if (config.format != null && config.format != "")
                            {
                                if (config.type == "datetime")
                                {
                                    DateTime d;
                                    if (DateTime.TryParse(dr[int.Parse(config.col)].ToString(), out d))
                                    {
                                        replaceDict.Add(config.param, d.ToString(config.format));
                                    }
                                }
                            }
                            else
                            {
                                if (int.Parse(config.col) == 16)
                                {
                                    replaceDict.Add(config.param, dr[int.Parse(config.col)].ToString().Replace('"', ' '));
                                }
                                else
                                {
                                    replaceDict.Add(config.param, dr[int.Parse(config.col)].ToString());
                                }

                            }
                        }
                    }

                    //if (dr[10].ToString().Length > 7)
                    //{
                    //    replaceDict.Add("#requestcode#", dr[10].ToString().Substring(0, 7));
                    //    replaceDict.Add("#requestdesc#", dr[10].ToString());
                    //}
                    //else
                    //{
                    //    replaceDict.Add("#requestcode#", dr[10].ToString());
                    //    replaceDict.Add("#requestdesc#", dr[10].ToString());
                    //}

                    replaceDict.Add("#receiver#", txtReceiver.Text.Trim());
                    replaceDict.Add("#month#", txtMonth.Text.Trim());
                    replaceDict.Add("#contractnumber#", txtContractNumber.Text.ToString());


                    //initialize word object  
                    Document document = new Document();
                    document.LoadFromFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\template_export.docx");

                    //get strings to replace  
                    Dictionary<string, string> dictReplace = replaceDict;
                    //Replace text  
                    foreach (KeyValuePair<string, string> kvp in dictReplace)
                    {
                        document.Replace(kvp.Key, kvp.Value, true, true);
                    }
                    //Save doc file.  
                    document.SaveToFile(txtPathSave.Text.Trim() + @"\" + filename + ".docx", FileFormat.Docx);
                    document.Close();

                    replaceDict.Clear();

                    prgBpercent.Value = i;
                    // lblPercent.Text = i.ToString("#,###");
                }


                //Convert to PDF  
                //document.SaveToFile(pdfPath, FileFormat.PDF);
                lblNotification.Text = "Tổng số báo cáo đã kết xuất: " + i.ToString("#,###");
                lblNotification.ForeColor = Color.Green;
                MessageBox.Show($"Đã kết xuất thành công {i}", "Export doc processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
                prgBpercent.Visible = false;
                lblPercent.Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + $"Có lỗi xảy ra.STT: {i}");
                Cursor.Current = Cursors.WaitCursor;
            }
        }

        private void btnPathSave_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                //string folderName = folderBrowserDialog1.SelectedPath;
                txtPathSave.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnSavePDF_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string filename = string.Empty;
            int i = 0;
            try
            {
                prgBpercent.Visible = true;
                //lblPercent.Visible = true;
                prgBpercent.Minimum = i;
                prgBpercent.Maximum = _iTotal;

                foreach (DataRow dr in dataSet.Tables[0].Rows)
                {
                    i++;
                    Dictionary<string, string> replaceDict = new Dictionary<string, string>();
                    foreach (var config in configs)
                    {
                        if (config.param.Equals("#requestcode#") || config.param.Equals("#requestdesc#") || config.param.Equals("#filename#"))
                        {
                            if (config.param.Equals("#requestcode#") || config.param.Equals("#requestdesc#"))
                            {
                                if (dr[int.Parse(config.col)].ToString().Length > 7)
                                {
                                    replaceDict.Add("#requestcode#", dr[int.Parse(config.col)].ToString().Substring(0, 7));
                                    replaceDict.Add("#requestdesc#", dr[int.Parse(config.col)].ToString());
                                }
                                else
                                {
                                    replaceDict.Add("#requestcode#", dr[int.Parse(config.col)].ToString());
                                    replaceDict.Add("#requestdesc#", dr[int.Parse(config.col)].ToString());
                                }
                            }

                            if (config.param.Equals("#filename#"))
                            {
                                filename = dr[int.Parse(config.col)].ToString();
                            }
                        }
                        else
                        {
                            if (config.format != null && config.format != "")
                            {
                                if (config.type == "datetime")
                                {
                                    DateTime d;
                                    if (DateTime.TryParse(dr[int.Parse(config.col)].ToString(), out d))
                                    {
                                        replaceDict.Add(config.param, d.ToString(config.format));
                                    }
                                }
                            }
                            else
                            {
                                if (int.Parse(config.col) == 16)
                                {
                                    replaceDict.Add(config.param, dr[int.Parse(config.col)].ToString().Replace('"', ' '));
                                }
                                else
                                {
                                    replaceDict.Add(config.param, dr[int.Parse(config.col)].ToString());
                                }

                            }
                        }
                    }

                    replaceDict.Add("#receiver#", txtReceiver.Text.Trim());
                    replaceDict.Add("#month#", txtMonth.Text.Trim());
                    replaceDict.Add("#contractnumber#", txtContractNumber.Text.ToString());

                    //initialize word object  
                    Document document = new Document();
                    document.LoadFromFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\template_export.docx");

                    //get strings to replace  
                    Dictionary<string, string> dictReplace = replaceDict;
                    //Replace text  
                    foreach (KeyValuePair<string, string> kvp in dictReplace)
                    {
                        document.Replace(kvp.Key, kvp.Value, true, true);
                    }
                    //Save doc file.  
                    document.SaveToFile(txtPathSave.Text.Trim() + @"\" + filename + ".pdf", FileFormat.PDF);
                    document.Close();

                    replaceDict.Clear();

                    prgBpercent.Value = i;
                    // lblPercent.Text = i.ToString("#,###");
                }


                //Convert to PDF  
                //document.SaveToFile(pdfPath, FileFormat.PDF);
                lblNotification.Text = "Tổng số báo cáo đã kết xuất: " + i.ToString("#,###");
                lblNotification.ForeColor = Color.Green;
                MessageBox.Show($"Đã kết xuất thành công {i}", "Export PDF processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
                prgBpercent.Visible = false;
                lblPercent.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + $"Có lỗi xảy ra.STT: {i}");
                Cursor.Current = Cursors.WaitCursor;
            }
        }

        #region old export static
        //private void btnSavePDF_Click(object sender, EventArgs e)
        //{
        //    Cursor.Current = Cursors.WaitCursor;
        //    int i = 0;
        //    try
        //    {
        //        prgBpercent.Visible = true;
        //        //lblPercent.Visible = true;
        //        prgBpercent.Minimum = i;
        //        prgBpercent.Maximum = _iTotal;

        //        foreach (DataRow dr in dataSet.Tables[0].Rows)
        //        {
        //            i++;
        //            Dictionary<string, string> replaceDict = new Dictionary<string, string>();
        //            replaceDict.Add("#id#", dr[6].ToString());
        //            replaceDict.Add("#type#", dr[9].ToString());
        //            replaceDict.Add("#requestname#", dr[1].ToString());
        //            if (dr[10].ToString().Length > 7)
        //            {
        //                replaceDict.Add("#requestcode#", dr[10].ToString().Substring(0, 7));
        //                replaceDict.Add("#requestdesc#", dr[10].ToString());
        //            }
        //            else
        //            {
        //                replaceDict.Add("#requestcode#", dr[10].ToString());
        //                replaceDict.Add("#requestdesc#", dr[10].ToString());
        //            }
        //            DateTime dTimeDeadline;
        //            if (DateTime.TryParse(dr[12].ToString(), out dTimeDeadline))
        //            {
        //                replaceDict.Add("#timedeadline#", dTimeDeadline.ToString("dd/MM/yyyy"));
        //            }
        //            else
        //            {
        //                replaceDict.Add("#timedeadline#", dr[12].ToString());
        //            }
        //            replaceDict.Add("#receiver#", txtReceiver.Text.Trim());
        //            replaceDict.Add("#processer#", dr[21].ToString());
        //            replaceDict.Add("#timereceive#", dr[12].ToString());
        //            replaceDict.Add("#timefinish#", dr[18].ToString());
        //            replaceDict.Add("#content#", dr[16].ToString().Replace('"', ' '));
        //            replaceDict.Add("#month#", txtMonth.Text.Trim());
        //            replaceDict.Add("#contractnumber#", txtContractNumber.Text.ToString());

        //            //initialize word object  
        //            Document document = new Document();
        //            document.LoadFromFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\template_export.docx");

        //            //get strings to replace  
        //            Dictionary<string, string> dictReplace = replaceDict;
        //            //Replace text  
        //            foreach (KeyValuePair<string, string> kvp in dictReplace)
        //            {
        //                document.Replace(kvp.Key, kvp.Value, true, true);
        //            }
        //            //Save doc file.  
        //            //document.SaveToFile(txtPathSave.Text.Trim() + @"\" + dr[6].ToString() + ".docx", FileFormat.Docx);
        //            document.SaveToFile(txtPathSave.Text.Trim() + @"\" + dr[6].ToString() + ".pdf", FileFormat.PDF);
        //            document.Close();

        //            replaceDict.Clear();

        //            prgBpercent.Value = i;
        //            //lblPercent.Text = i.ToString("#,###");
        //            //float percent = (prgBpercent.Value / prgBpercent.Maximum) * 100;
        //            //lblPercent.Text = percent.ToString("#,###.##") + " %";
        //        }


        //        //Convert to PDF  
        //        //document.SaveToFile(pdfPath, FileFormat.PDF);
        //        lblNotification.Text = "Tổng số báo cáo đã kết xuất: " + i.ToString("#,###");
        //        lblNotification.ForeColor = Color.Green;
        //        MessageBox.Show($"Đã kết xuất thành công {i}", "Export PDF processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        Cursor.Current = Cursors.Default;
        //        prgBpercent.Visible = false;
        //        lblPercent.Visible = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message + "\r\n" + $"Có lỗi xảy ra.STT: {i}");
        //        Cursor.Current = Cursors.WaitCursor;
        //    }
        //}
        #endregion


        private DataSet ConvertXmlDocToDataSet(XmlDocument pv_xmlDoc)
        {
            try
            {
                var v_ds = new DataSet("Object");
                DataRow v_dr;
                DataColumn v_dc;
                int v_intCountRow, v_intCountCol;
                System.Xml.XmlNode v_XmlNode;
                v_ds.Tables.Add("RptData");
                v_intCountRow = pv_xmlDoc.FirstChild.ChildNodes.Count;
                if (v_intCountRow > 0)
                {
                    v_intCountCol = pv_xmlDoc.FirstChild.FirstChild.ChildNodes.Count;
                    for (int i = 0, loopTo = v_intCountCol - 1; i <= loopTo; i++)
                    {
                        v_dc = new DataColumn(pv_xmlDoc.FirstChild.FirstChild.ChildNodes[i].Attributes["fldname"].InnerText);
                        v_dc.ColumnName = pv_xmlDoc.FirstChild.FirstChild.ChildNodes[i].Attributes["fldname"].InnerText;
                        switch (pv_xmlDoc.FirstChild.FirstChild.ChildNodes[i].Attributes["fldtype"].InnerText ?? "")
                        {
                            case "System.Decimal":
                                {
                                    v_dc.DataType = typeof(decimal);
                                    break;
                                }

                            case "System.String":
                                {
                                    v_dc.DataType = typeof(string);
                                    break;
                                }

                            case "System.Double":
                                {
                                    v_dc.DataType = typeof(double);
                                    break;
                                }

                            case "System.DateTime":
                                {
                                    v_dc.DataType = typeof(DateTime);
                                    break;
                                }

                            default:
                                {
                                    v_dc.DataType = typeof(string);
                                    break;
                                }
                        }

                        v_ds.Tables[0].Columns.Add(v_dc);
                    }

                    v_XmlNode = pv_xmlDoc.FirstChild;
                    for (int j = 0, loopTo1 = v_intCountRow - 1; j <= loopTo1; j++)
                    {
                        v_dr = v_ds.Tables[0].NewRow();
                        for (int i = 0, loopTo2 = v_intCountCol - 1; i <= loopTo2; i++)
                            v_dr[i] = (v_XmlNode.ChildNodes[j].ChildNodes[i].InnerText).Trim();
                        v_ds.Tables[0].Rows.Add(v_dr);
                    }
                }

                return v_ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnPrintReport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        dataSet.Tables[0].Columns.Add("Month");
                        dataSet.Tables[0].Columns.Add("Contractnumber");
                        dataSet.Tables[0].Columns.Add("Receiver");
                        dataSet.Tables[0].Columns.Add("TimeDeadline");
                        dataSet.Tables[0].Columns.Add("RequestCode");
                        dataSet.Tables[0].Columns.Add("RequestDesc");
                    }

                    dataSet.Tables[0].AcceptChanges();

                    string _Month = string.Empty;
                    string _Contractnumber = string.Empty;
                    string _Receiver = string.Empty;

                    _Month = txtMonth.Text.Trim();
                    _Contractnumber = txtContractNumber.Text.Trim();
                    _Receiver = txtReceiver.Text.Trim();

                    var colMonth = dataSet.Tables[0].Columns["Month"];
                    var colContractnumber = dataSet.Tables[0].Columns["Contractnumber"];
                    var colReceiver = dataSet.Tables[0].Columns["Receiver"];
                    var colTimeDeadlineData = dataSet.Tables[0].Columns["Column12"];
                    var colTimeDeadline = dataSet.Tables[0].Columns["TimeDeadline"];
                    var colRequestData = dataSet.Tables[0].Columns["Column10"];
                    var colRequestCode = dataSet.Tables[0].Columns["RequestCode"];
                    var colRequestDesc = dataSet.Tables[0].Columns["RequestDesc"];

                    foreach (DataRow dr in dataSet.Tables[0].Rows)
                    {
                        dr[colMonth] = _Month;
                        dr[colContractnumber] = _Contractnumber;
                        dr[colReceiver] = _Receiver;

                        DateTime dTimeDeadline;
                        if (DateTime.TryParse(dr[colTimeDeadlineData].ToString(), out dTimeDeadline))
                        {
                            //replaceDict.Add("#timedeadline#", dTimeDeadline.ToString("dd/MM/yyyy"));
                            dr[colTimeDeadline] = dTimeDeadline.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            dr[colTimeDeadline] = dr[colTimeDeadline].ToString();
                        }

                        if (dr[colRequestData].ToString().Length > 7)
                        {
                            dr[colRequestCode] = dr[colRequestData].ToString().Substring(0, 7);
                            dr[colRequestDesc] = dr[colRequestData].ToString();
                        }
                        else
                        {
                            dr[colRequestCode] = dr[colRequestData].ToString();
                            dr[colRequestDesc] = dr[colRequestData].ToString();
                        }
                    }

                    //dataSet.WriteXml("d:\\Data.xml");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadConfig()
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files = di.GetFiles("*.json");
            if (files.Length == 0)
            {
                MessageBox.Show("Lỗi", "Không tìm thấy file config.json", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string text = File.ReadAllText(Directory.GetCurrentDirectory() + "\\Config.json"); // relative path
                configs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Config>>(text);
            }
        }
    }

    public class Config
    {
        public string col { get; set; }
        public string param { get; set; }
        public string format { get; set; }
        public string type { get; set; }
    }
}
