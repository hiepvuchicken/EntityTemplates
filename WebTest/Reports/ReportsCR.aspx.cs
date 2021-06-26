using BarcodeLib.Barcode;
using BarcodeLib.Barcode.CrystalReports;
using CommonLib;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Entity;
using System;
using System.Collections;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Web.UI.WebControls;

namespace WebTest.Reports
{
    public partial class ReportsCR : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportDocument report = new ReportDocument();
            ParameterField paramField = new ParameterField();

            report.Load(Server.MapPath("/Reports/CR_BD14.rpt"));

            var dt = GetdataFromDb();
            DataSet1 ds = new DataSet1();
            ds.Tables[0].Merge(dt);

            report.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
            var TotalRecord = ds.Tables[0].Rows.Count;

            //foreach (DataRow dr in ds.Tables[0].Rows)
            //{
            //    dr["Barcode"] = Barcode.ImageItemBarcode(dr["ItemCode"].ToString());
            //}

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                dr["BarcodeFont"] = "*" + dr["ItemCode"].ToString() + "*";
            }

            //report.SetParameterValue("TotalRecord", TotalRecord);
            //report.SetParameterValue("Postman", "12340-NGUYỄN TRỌNG MINH");

            report.SetDataSource(ds);

            report.Refresh();
            // Set Paper Orientation.
            report.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
            // Set Paper Size.
            report.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA4;

            report.PrintOptions.PrinterName = GetDefaultPrinter();
            //report.PrintToPrinter(1, true, 0, 1);

            crViewer.ReportSource = report;
            crViewer.PrintMode = CrystalDecisions.Web.PrintMode.ActiveX;
        }

        public DataTable GetdataFromDb()
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                using (var ctx = new DBBAOPHATEntities())
                {
                    var lsReturn = ctx.service_process_LoadItemList_printf_dev("12340", "756010", "20180810", "").ToList();

                    if (lsReturn.Count > 0)
                    {
                        dt = CommonLib.Common.ConvertToDataTable<service_process_LoadItemList_printf_dev_Result>(lsReturn);
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetDefaultPrinter()
        {
            PrinterSettings settings = new PrinterSettings();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                settings.PrinterName = printer;
                if (settings.IsDefaultPrinter)
                {
                    return printer;
                }
            }
            return string.Empty;
        }
    }
}