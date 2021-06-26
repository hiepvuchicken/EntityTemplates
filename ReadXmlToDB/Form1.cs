using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace ReadXmlToDB
{
    public partial class Form1 : Form
    {
        private string xmlData = string.Empty;
        private string ActionTime = string.Empty;
        List<ItemInterchange> lsObjData;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog sourceFileOpenFileDialog = new OpenFileDialog();
                sourceFileOpenFileDialog.InitialDirectory = "C:\\";
                sourceFileOpenFileDialog.Filter = "XML Files (*.xml;)|*.xml;";
                sourceFileOpenFileDialog.FilterIndex = 2;
                sourceFileOpenFileDialog.RestoreDirectory = true;
                sourceFileOpenFileDialog.Multiselect = false;
                sourceFileOpenFileDialog.Title = "Chọn file import dữ liệu";
                if (sourceFileOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = Path.GetFullPath(sourceFileOpenFileDialog.FileName.ToString());
                    FileStream stream = File.Open(sourceFileOpenFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    if (System.IO.Path.GetExtension(sourceFileOpenFileDialog.FileName.ToString()).ToLower() == ".xml")
                    {
                        string _date = string.Empty;
                        string _intref = string.Empty;
                        string _mesref = string.Empty;
                        string _itemcode = string.Empty;

                        ActionTime = "Begin: " + DateTime.Now.ToString("HH:mm:ss.fff");

                        lsObjData = new List<ItemInterchange>();
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(stream);

                        XmlNodeList dateNodes = xmldoc.GetElementsByTagName("date");
                        foreach (XmlNode node in dateNodes)
                        {
                            _date = node.InnerText;
                        }

                        XmlNodeList intrefNodes = xmldoc.GetElementsByTagName("intref");
                        foreach (XmlNode node in intrefNodes)
                        {
                            _intref = node.InnerText;
                        }

                        XmlNodeList mesrefNodes = xmldoc.GetElementsByTagName("mesref");
                        foreach (XmlNode mesrefnode in mesrefNodes)
                        {
                            _mesref = mesrefnode.InnerText;
                        }

                        XmlNodeList valueNodes = xmldoc.GetElementsByTagName("value");
                        foreach (XmlNode valueNode in valueNodes)
                        {
                            _itemcode = valueNode.InnerText;
                            ItemInterchange objData = new ItemInterchange();
                            objData.Date = _date;
                            objData.Intref = _intref;
                            objData.mesref = _mesref;
                            objData.Itemcode = _itemcode;

                            lsObjData.Add(objData);
                        }

                        ActionTime += "- End: " + DateTime.Now.ToString("HH:mm:ss.fff");
                    }

                    lblNotification.Text = $"Có {lsObjData.Count} bản ghi."; 

                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSavePDF_Click(object sender, EventArgs e)
        {
            try
            {
                using (var ctx = new EntityTemplatesEntities())
                {
                    using (var dbtran = ctx.Database.BeginTransaction())
                    {
                        foreach (var item in lsObjData)
                        {
                            ctx.Entry(item).State = item.ID == 0 ? EntityState.Added : EntityState.Modified;
                        }
                        ctx.SaveChanges();
                        dbtran.Commit();
                    }
                }

                MessageBox.Show("Thêm dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }


}
