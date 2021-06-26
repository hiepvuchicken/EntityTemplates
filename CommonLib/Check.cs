using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Globalization;
using Ctin.Css.DAO;
using Ctin.Css.Mod.Acceptance.DomesticSingleItem.Forms;
using Ctin.Css.Core;
using Ctin.Css.Core.UI.WinForm;
using Ctin.Css.Core.Ultilities;
using Ctin.Css.Core.Constance;
using Ctin.Css.Core.Business;
using Ctin.Css.Core.Enums;
using Ctin.Css.Configuration;
using System.Threading;
using System.IO;
using Excel;
using Ctin.Css.CRMServiceReference.CRMServiceReference;
using System.Net;
using Newtonsoft.Json;
using Fitech.DAL;

namespace Ctin.Css.Mod.Acceptance.HCC.Forms
{

    public partial class frmAcceptanceFromDieuTin : Ctin.Css.Core.UI.WinForm.PForm
    {
        private string ErrorSource = "Ctin.Css.Mod.Acceptance.HCC.Forms.frmAcceptanceFromDieuTin.";
        private bool alwaysAsk = true;

        private List<string> provinceListCode = new List<string>();

        private DateTime dtOldDateTime;

        private bool inputReplace = false;

        DataSet dsDiscountFeedbackStep = new DataSet();

        Billing oBilling = new Billing();

        BillingInput eBillingInput = new BillingInput();

        private FreightCalculator _freightCalculator = new FreightCalculator();

        List<ItemEntity> entityItemListGlobal;

        List<ValueAddedServiceItemEntity> entityValueAddedServiceItemListGlobal;

        List<ItemVASPropertyValueEntity> entityItemVASPropertyValueListGlobal;

        List<DetailItemEntity> entityDetailItemListGlobal;

        List<ItemCommodityTypeEntity> entityItemCommodityTypeListGlobal;

        List<ShiftHandoverItemEntity> entityShiftHandoverItemListGlobal;

        List<TraceItemEntity> entityTraceItemListGlobal;

        List<TransactionsCollectionEntity> entityTransactionsCollectionListGlobal;

        List<TransactionsCollectionDetailEntity> entityTransactionsCollectionDetailListGlobal;

        List<ItemAdviceOfReceiptEntity> entityItemAdviceOfReceiptListGlobal;

        List<KT1ExpectedTimeEntity> entityKT1ExpectedTimeEntityListGlobal;

        List<SortingItemEntity> entitySortingItemListGlobal;

        List<AttachDocumentsItemEntity> entityAttachDocumentsItemListGlobal;

        private List<string> itemListTransferWait = new List<string>();
        List<ValueAddedServiceFreight> VASFreightList = new List<ValueAddedServiceFreight>();
        List<ValueAddedServiceFreight> VASFreightListOriginal = new List<ValueAddedServiceFreight>();
        private double c_VATPercentage = 0; //Biến dùng để lưu %VAT

        DataSet dtData = new DataSet();

        private string NumberFormat(double dNumber)
        {
            if (dNumber == 0)
                return "0";
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            string result = "";
            result = String.Format(currentCulture, "{0:N}", dNumber);
            if (result.Substring(result.Length - 3).Equals(currentCulture.NumberFormat.CurrencyDecimalSeparator + "00"))
                result = result.Substring(0, result.Length - 3);
            return result;
        }

        public frmAcceptanceFromDieuTin()
        {
            this.InitializeComponent();

            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("vi-VN");
            Application.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        private void frmAcceptanceFromDieuTin_Load(object sender, EventArgs e)
        {
            dtpFromDate.Value = DateTimeServer.Now;
            dtpToDate.Value = DateTimeServer.Now;

            //dtOldDateTime = new DateTime(dtpFromDate.Value.Year, dtpFromDate.Value.Month, dtpFromDate.Value.Day);

            if (this.ParameterList != null && this.ParameterList.Count > 0)
            {
                foreach (Parameter parameter in this.ParameterList)
                {
                    if (parameter.ParameterName.Equals("NhapThayThe"))
                    {
                        if (parameter.ParameterValue.Equals("NhapThayThe"))
                        {
                            inputReplace = true;
                        }
                    }
                }
            }



            //SelectCustomerGroup();

            SelectProvinceBy(this.POSCode);

            SelectServiceByPOSCode(this.POSCode);

            if (CheckShifted())
            {
                ShowMessageBoxWarning("Ca làm việc hiện tại đã được chốt. Không cho phép điều chỉnh dữ liệu trong ca");
                alwaysAsk = false;
                this.Close();
            }

            CommunicationConfigDAO obj = new CommunicationConfigDAO();
            var objEntity = obj.SelectOne("DIEUTIN");
            string value = objEntity.ConfigValue;
            //if (value == null) value = "https://tiepnhanhoso.vnpost.vn";
            Base_ulr_get = value;
            ////Base_ulr_get1 = value + "/serviceApi/v1/getItemsByItemCode/";
            Base_ulr_update = value;
            GetListCustomer();
        }
        public static string Base_ulr_get = string.Empty;
        public static string Base_ulr_update = string.Empty;

        public void GetListCustomer()
        {
            try
            {
                string poscode = this.POSCode;
                string url;
                url = string.Format(Base_ulr_get + "/serviceApi/v2/GetCustomerforBCCP?posCode={0}", this.POSCode);
                //url = string.Format(Base_ulr_get + "/serviceApi/v2/GetCustomerforBCCP?posCode={0}", "333546");
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json, text/javascript, */*";
                request.Method = "GET";
                using (HttpWebResponse responses = request.GetResponse() as HttpWebResponse)
                {
                    if (responses.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        StreamReader reader = new StreamReader(responses.GetResponseStream());
                        string rawresp = reader.ReadToEnd();
                        var dt = JsonConvert.DeserializeObject<DataTable>(rawresp);

                        cboCustomerCode.DataSource = dt;
                        cboCustomerCode.DisplayMember = "FullName";
                        cboCustomerCode.ValueMember = "CustomerCode";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessageBoxWarning(ex.Message);
            }

        }

        public void Get123()
        {
            try
            {
                string poscode = this.POSCode;
                //ConfigurationManager.AppSettings["Base_POSCode"];
                string ulr;
                //if (t == 0)
                //{ ulr = Base_ulr_get1 + richTextBox1.Text.Replace("\n", ","); }
                //else
                //{
                //    ulr = Base_ulr_get + this.POSCode;
                //    // ulr = "http://uat-tiepnhanhoso.vnpost.vn/serviceApi/v1/GetItemsForBCCP/100900";
                //    //ConfigurationManager.AppSettings["Poscode"];
                //}

                // ulr = "http://uat-tiepnhanhoso.vnpost.vn/serviceApi/v1/GetItemsForBCCP/901745";


                //ulr = string.Format(Base_ulr_get + "/serviceApi/v2/GetListDataToBccpV4?POSCode={0}&ServiceCode={1}&fromDate={2}&toDate={3}&customerCode={4}", "333546", "E", "20190401", "20190402", "70001S96001570000");
                ulr = string.Format(Base_ulr_get + "/serviceApi/v2/GetListDataToBccpV4?POSCode={0}&ServiceCode={1}&fromDate={2}&toDate={3}&customerCode={4}", this.POSCode, cboService.SelectedValue.ToString(), dtpFromDate.Value.ToString("yyyyMMdd"), dtpToDate.Value.AddDays(1).ToString("yyyyMMdd"), cboCustomerCode.SelectedValue.ToString());
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ulr);
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json, text/javascript, */*";
                request.Method = "GET";
                using (HttpWebResponse responses = request.GetResponse() as HttpWebResponse)
                {
                    if (responses.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        StreamReader reader = new StreamReader(responses.GetResponseStream());
                        string rawresp = reader.ReadToEnd();
                        dtData = new DataSet();
                        var dt = JsonConvert.DeserializeObject<DataTable>(rawresp);
                        dtData.Tables.Add(dt);
                    }
                }

            }
            catch (Exception ex)
            {
                ShowMessageBoxWarning(ex.Message);
            }

        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (CheckShifted())
            {
                return;
            }
            if (cboCustomerCode.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn mã khách hàng để lấy dữ liệu");
                return;
            }
            if (cboService.SelectedValue != null)
            {
                if (cboService.SelectedValue.Equals(ServiceConstance.KT1))
                {
                    dgvListItems.Columns["colDeliveryTime"].Visible = false;
                }
                else
                {
                    dgvListItems.Columns["colDeliveryTime"].Visible = true;
                }

                Get123();
                if (dtData.Tables.Count > 0 && dtData.Tables[0].Rows.Count > 0)
                {
                    if (dtData.Tables[0].Rows.Count > 0)
                    {
                        if (dgvListItems.Rows.Count > 0)
                        {
                            dgvListItems.EndEdit();
                            dgvListItems.Rows.Clear();
                        }

                        foreach (DataRow dataRow in dtData.Tables[0].Rows)
                        {
                            #region bindding Grid
                            if (dtData.Tables[0].Columns.Contains("SoHieuBuuGui"))
                            {
                                if (!string.IsNullOrEmpty(dataRow["SoHieuBuuGui"].ToString()))
                                {
                                    if (dtData.Tables[0].Columns.Contains("HoTenNguoiNhan"))
                                    {
                                        if (!string.IsNullOrEmpty(dataRow["HoTenNguoiNhan"].ToString()))
                                        {
                                            int index = dgvListItems.Rows.Add();

                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIndex"].Value = (index + 1).ToString();

                                            if (dtData.Tables[0].Columns.Contains("SoHieuBuuGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoHieuBuuGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBarcode"].Value = dataRow["SoHieuBuuGui"].ToString().ToUpper();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoCongVan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoCongVan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDataCode"].Value = dataRow["SoCongVan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BuuGuiSuVu"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BuuGuiSuVu"].ToString()))
                                                {
                                                    bool bIsAffair;

                                                    if (bool.TryParse(dataRow["BuuGuiSuVu"].ToString(), out bIsAffair))
                                                    {
                                                        if (bIsAffair)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAffair"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (cboService.SelectedValue.ToString().Equals(ServiceConstance.DHL))
                                            {
                                                if (dtData.Tables[0].Columns.Contains("KHChiDinhDHL"))
                                                {
                                                    if (!string.IsNullOrEmpty(dataRow["KHChiDinhDHL"].ToString()))
                                                    {
                                                        bool bIsCollection;

                                                        if (bool.TryParse(dataRow["KHChiDinhDHL"].ToString(), out bIsCollection))
                                                        {
                                                            if (bIsCollection)
                                                            {
                                                                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIsCollection"].Value = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (dtData.Tables[0].Columns.Contains("SoTaiKhoanKH"))
                                                {
                                                    if (!string.IsNullOrEmpty(dataRow["SoTaiKhoanKH"].ToString()))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerAccountNo"].Value = dataRow["SoTaiKhoanKH"].ToString();
                                                    }
                                                }
                                            }

                                            bool TraTienMat = false;
                                            string LuuYKhiPhat = "";

                                            if (dtData.Tables[0].Columns.Contains("MaKH"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MaKH"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerCode"].Value = dataRow["MaKH"].ToString();

                                                    CustomerDAO daoCustomer = new CustomerDAO();
                                                    CustomerEntity enCustomer = daoCustomer.SelectOne(dataRow["MaKH"].ToString());
                                                    if (enCustomer != null)
                                                    {
                                                        if (!enCustomer.IsNullPaymentType)
                                                        {
                                                            if (enCustomer.PaymentType == CustomerPaymentTypeConstance.TIEN_MAT)
                                                            {
                                                                TraTienMat = true;
                                                            }
                                                        }

                                                        if (!enCustomer.IsNullServiceInstruction)
                                                        {
                                                            if (!string.IsNullOrEmpty(enCustomer.ServiceInstruction))
                                                            {
                                                                LuuYKhiPhat = enCustomer.ServiceInstruction;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("NhomKH"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["NhomKH"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerGroup"].Value = dataRow["NhomKH"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HoTenNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HoTenNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderFullName"].Value = dataRow["HoTenNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("DiaChiNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["DiaChiNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderAddress"].Value = dataRow["DiaChiNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("DienThoaiNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["DienThoaiNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderTel"].Value = dataRow["DienThoaiNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("EmailNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["EmailNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderEmail"].Value = dataRow["EmailNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("MaBuuChinhNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MaBuuChinhNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderPOSCode"].Value = dataRow["MaBuuChinhNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("MaSoThueNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MaSoThueNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderTaxCode"].Value = dataRow["MaSoThueNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoCMTNDNguoiGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoCMTNDNguoiGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderID"].Value = dataRow["SoCMTNDNguoiGui"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("MaKhachHangNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MaKhachHangNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverCustomerCode"].Value = dataRow["MaKhachHangNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HoTenNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HoTenNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverFullName"].Value = dataRow["HoTenNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("DiaChiNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["DiaChiNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverAddress"].Value = dataRow["DiaChiNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("NuocNhan") && !string.IsNullOrEmpty(dataRow["NuocNhan"].ToString()) && !dataRow["NuocNhan"].ToString().ToUpper().Equals("VN"))
                                            {
                                                CountryDAO daoCountry = new CountryDAO();

                                                CountryEntity enCountry = daoCountry.SelectOne(dataRow["NuocNhan"].ToString());

                                                if (enCountry != null && !enCountry.IsNullCountryCode)
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCountryCode"].Value = enCountry.CountryCode;

                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCountryName"].Value = enCountry.CountryName;
                                                }
                                            }
                                            else
                                            {
                                                if (dtData.Tables[0].Columns.Contains("TinhNhan"))
                                                {
                                                    if (!string.IsNullOrEmpty(dataRow["TinhNhan"].ToString()))
                                                    {
                                                        ProvinceDAO daoProvince = new ProvinceDAO();

                                                        ProvinceEntity enProvince = daoProvince.SelectOne(dataRow["TinhNhan"].ToString());

                                                        if (enProvince != null && !enProvince.IsNullProvinceCode)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colProvinceCode"].Value = enProvince.ProvinceCode;

                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colProvinceName"].Value = enProvince.ProvinceName;

                                                            if (dtData.Tables[0].Columns.Contains("HuyenNhan"))
                                                            {
                                                                if (!string.IsNullOrEmpty(dataRow["HuyenNhan"].ToString()))
                                                                {
                                                                    DistrictDAO daoDistrict = new DistrictDAO();

                                                                    List<DistrictEntity> enDistrictList = daoDistrict.SelectAllByDistrictCodeProvinceCode(dataRow["HuyenNhan"].ToString(), enProvince.ProvinceCode);

                                                                    if (enDistrictList != null && enDistrictList.Count > 0)
                                                                    {
                                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDistrictCode"].Value = enDistrictList[0].DistrictCode;

                                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDistrictName"].Value = enDistrictList[0].DistrictName;

                                                                        if (dtData.Tables[0].Columns.Contains("XaNhan"))
                                                                        {
                                                                            if (!string.IsNullOrEmpty(dataRow["XaNhan"].ToString()))
                                                                            {
                                                                                CommuneDAO daoCommune = new CommuneDAO();

                                                                                List<CommuneEntity> enCommuneList = daoCommune.SelectAllByCommuneCodeDistrictCode(dataRow["XaNhan"].ToString(), enDistrictList[0].DistrictCode);

                                                                                if (enCommuneList != null && enCommuneList.Count > 0)
                                                                                {
                                                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCommuneCode"].Value = enCommuneList[0].CommuneCode;

                                                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCommuneName"].Value = enCommuneList[0].CommuneName;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("DienThoaiNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["DienThoaiNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverTel"].Value = dataRow["DienThoaiNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("EmailNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["EmailNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverEmail"].Value = dataRow["EmailNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HoTenNguoiLienHe"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HoTenNguoiLienHe"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverContact"].Value = dataRow["HoTenNguoiLienHe"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("MaBuuChinhNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MaBuuChinhNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverPOSCode"].Value = dataRow["MaBuuChinhNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("MaSoThueNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MaSoThueNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverTaxCode"].Value = dataRow["MaSoThueNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoCMTNDNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoCMTNDNguoiNhan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverID"].Value = dataRow["SoCMTNDNguoiNhan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("VungSauVungXa"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["VungSauVungXa"].ToString()))
                                                {
                                                    bool bIsFarRegion;

                                                    if (bool.TryParse(dataRow["VungSauVungXa"].ToString(), out bIsFarRegion))
                                                    {
                                                        if (bIsFarRegion)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFarRegion"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("VanChuyenDuongBay"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["VanChuyenDuongBay"].ToString()))
                                                {
                                                    bool bIsAir;

                                                    if (bool.TryParse(dataRow["VanChuyenDuongBay"].ToString(), out bIsAir))
                                                    {
                                                        if (bIsAir)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colisAir"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoLenhDieuHanh"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoLenhDieuHanh"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colExecuteOrder"].Value = dataRow["SoLenhDieuHanh"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HoaDonGuiKem"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HoaDonGuiKem"].ToString()))
                                                {
                                                    bool bIsInvoice;

                                                    if (bool.TryParse(dataRow["HoaDonGuiKem"].ToString(), out bIsInvoice))
                                                    {
                                                        if (bIsInvoice)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colInvoice"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("GiayToKhacGuiKem"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["GiayToKhacGuiKem"].ToString()))
                                                {
                                                    bool bIsOther;

                                                    if (bool.TryParse(dataRow["GiayToKhacGuiKem"].ToString(), out bIsOther))
                                                    {
                                                        if (bIsOther)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOther"].Value = true;

                                                            if (dtData.Tables[0].Columns.Contains("LoaiGiayToKhac"))
                                                            {
                                                                if (!string.IsNullOrEmpty(dataRow["LoaiGiayToKhac"].ToString()))
                                                                {
                                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOtherInfo"].Value = dataRow["LoaiGiayToKhac"].ToString();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("NoiDungHangGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["NoiDungHangGui"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDetailItem"].Value = dataRow["NoiDungHangGui"].ToString();

                                                    DetailItemEntity enDetailItemTemp = new DetailItemEntity();
                                                    enDetailItemTemp.ItemIndex = 1;
                                                    //enDetailItemTemp.ItemCode = dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBarcode"].Value.ToString();
                                                    enDetailItemTemp.Quantity = 0;
                                                    enDetailItemTemp.Amount = 0;
                                                    enDetailItemTemp.Unit = "";
                                                    enDetailItemTemp.Price = 0;
                                                    enDetailItemTemp.TaxCode = "";
                                                    enDetailItemTemp.Weight = 0;
                                                    enDetailItemTemp.DetailItemName = dataRow["NoiDungHangGui"].ToString();
                                                    enDetailItemTemp.Width = 0;
                                                    enDetailItemTemp.Height = 0;
                                                    enDetailItemTemp.Length = 0;
                                                    enDetailItemTemp.LightItem = 0;

                                                    if (dataRow["NoiDungHangGui"].ToString().Trim().Length > 50)
                                                    {
                                                        enDetailItemTemp.DetailItemName = dataRow["NoiDungHangGui"].ToString().Trim().Substring(0, 50);
                                                    }

                                                    List<DetailItemEntity> enDetailItemListTemp = new List<DetailItemEntity>();

                                                    enDetailItemListTemp.Add(enDetailItemTemp);

                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDetailItem"].Tag = enDetailItemListTemp;
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("LoaiBuuGui"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["LoaiBuuGui"].ToString()))
                                                {
                                                    ItemTypeDAO daoItemType = new ItemTypeDAO();

                                                    ItemTypeEntity enItemType = daoItemType.SelectOne(dataRow["LoaiBuuGui"].ToString());

                                                    if (enItemType != null && !enItemType.IsNullItemTypeCode)
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemType"].Value = enItemType.ItemTypeCode;

                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemTypeName"].Value = enItemType.ItemTypeName;
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("ChiDanKhongPhatDuoc"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ChiDanKhongPhatDuoc"].ToString()))
                                                {
                                                    byte bUndiliveryResult;

                                                    if (byte.TryParse(dataRow["ChiDanKhongPhatDuoc"].ToString(), out bUndiliveryResult))
                                                    {
                                                        UndeliveryGuideDAO daoUndeliveryGuide = new UndeliveryGuideDAO();

                                                        UndeliveryGuideEntity enUndeliveryGuide = daoUndeliveryGuide.SelectOne(bUndiliveryResult);

                                                        if (enUndeliveryGuide != null && !enUndeliveryGuide.IsNullUndeliveryGuideCode)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colUndeliveryIndicator"].Value = enUndeliveryGuide.UndeliveryGuideCode;

                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colUndeliveryIndicatorName"].Value = enUndeliveryGuide.UndeliveryGuideName;
                                                        }
                                                    }
                                                }

                                            }

                                            if (dtData.Tables[0].Columns.Contains("LuuYKhiPhat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["LuuYKhiPhat"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDeliveryNote"].Value = dataRow["LuuYKhiPhat"].ToString();
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(LuuYKhiPhat))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDeliveryNote"].Value = LuuYKhiPhat;
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("KhoiLuong"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["KhoiLuong"].ToString()))
                                                {
                                                    double dWeightResult;

                                                    if (double.TryParse(dataRow["KhoiLuong"].ToString(), out dWeightResult))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colWeight"].Value = NumberFormat(dWeightResult);
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("ChieuDai"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ChieuDai"].ToString()))
                                                {
                                                    double dLengthResult;

                                                    if (double.TryParse(dataRow["ChieuDai"].ToString(), out dLengthResult))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colLength"].Value = NumberFormat(dLengthResult);
                                                    }
                                                }
                                            }
                                            if (dtData.Tables[0].Columns.Contains("ChieuRong"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ChieuRong"].ToString()))
                                                {
                                                    double dWidthResult;

                                                    if (double.TryParse(dataRow["ChieuRong"].ToString(), out dWidthResult))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colWidth"].Value = NumberFormat(dWidthResult);
                                                    }
                                                }
                                            }
                                            if (dtData.Tables[0].Columns.Contains("ChieuCao"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ChieuCao"].ToString()))
                                                {
                                                    double dHeightResult;

                                                    if (double.TryParse(dataRow["ChieuCao"].ToString(), out dHeightResult))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHeight"].Value = NumberFormat(dHeightResult);
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("MienCuoc"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["MienCuoc"].ToString()))
                                                {
                                                    bool bIsMienCuoc;

                                                    if (bool.TryParse(dataRow["MienCuoc"].ToString(), out bIsMienCuoc))
                                                    {
                                                        if (bIsMienCuoc)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFreePost"].Value = true;
                                                        }
                                                    }
                                                }
                                            }
                                            if (dtData.Tables[0].Columns.Contains("GhiNo"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["GhiNo"].ToString()))
                                                {
                                                    bool bIsGhiNo;

                                                    if (bool.TryParse(dataRow["GhiNo"].ToString(), out bIsGhiNo))
                                                    {
                                                        if (bIsGhiNo)
                                                        {
                                                            if (dtData.Tables[0].Columns.Contains("MaKH"))
                                                            {
                                                                if (!string.IsNullOrEmpty(dataRow["MaKH"].ToString()))
                                                                {
                                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDebt"].Value = true;

                                                                    if (TraTienMat)
                                                                    {
                                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDebt"].Value = false;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("XuatHoaDon"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["XuatHoaDon"].ToString()))
                                                {
                                                    bool bIsXuatHoaDon;

                                                    if (bool.TryParse(dataRow["XuatHoaDon"].ToString(), out bIsXuatHoaDon))
                                                    {
                                                        if (bIsXuatHoaDon)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colInvoiceExport"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BuuCucNhanChuyenThu"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BuuCucNhanChuyenThu"].ToString()))
                                                {
                                                    if (cboService.SelectedValue != null)
                                                    {
                                                        ExchangeRelationshipDAO daoEx = new ExchangeRelationshipDAO();

                                                        List<ExchangeRelationshipEntity> enExchangeRelationship = daoEx.SelectAllFilter("OnMailRoutePOSCode =N'" + this.POSCode + "' AND ExchangePOSCode = N'" + dataRow["BuuCucNhanChuyenThu"].ToString() + "' AND ServiceCode =N'" + cboService.SelectedValue.ToString() + "'");

                                                        if (enExchangeRelationship != null && enExchangeRelationship.Count > 0)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDestinationPOSCode"].Value = dataRow["BuuCucNhanChuyenThu"].ToString();
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("COD"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["COD"].ToString()))
                                                {
                                                    bool bCODResult;

                                                    if (bool.TryParse(dataRow["COD"].ToString(), out bCODResult))
                                                    {
                                                        if (bCODResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCOD"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoTienCOD"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoTienCOD"].ToString()))
                                                {
                                                    double dSoTienCOD;

                                                    if (double.TryParse(dataRow["SoTienCOD"].ToString(), out dSoTienCOD))
                                                    {
                                                        if (dSoTienCOD > 0)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAmount"].Value = NumberFormat(dSoTienCOD);
                                                        }
                                                    }
                                                }
                                            }

                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderPostage"].Value = true;

                                            if (dtData.Tables[0].Columns.Contains("NguoiGuiTraCuocChuyenPhat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["NguoiGuiTraCuocChuyenPhat"].ToString()))
                                                {
                                                    bool bNguoiGuiTraCuocChuyenPhat;

                                                    if (bool.TryParse(dataRow["NguoiGuiTraCuocChuyenPhat"].ToString(), out bNguoiGuiTraCuocChuyenPhat))
                                                    {
                                                        if (bNguoiGuiTraCuocChuyenPhat)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderPostage"].Value = true;
                                                        }
                                                        else
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderPostage"].Value = false;
                                                        }
                                                    }
                                                }
                                            }

                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderCODPostage"].Value = true;

                                            if (dtData.Tables[0].Columns.Contains("NguoiGuiTraCuocThuHo"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["NguoiGuiTraCuocThuHo"].ToString()))
                                                {
                                                    bool bNguoiGuiTraCuocThuHo;

                                                    if (bool.TryParse(dataRow["NguoiGuiTraCuocThuHo"].ToString(), out bNguoiGuiTraCuocThuHo))
                                                    {
                                                        if (bNguoiGuiTraCuocThuHo)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderCODPostage"].Value = true;
                                                        }
                                                        else
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderCODPostage"].Value = false;
                                                        }
                                                    }
                                                }
                                            }

                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCash"].Value = true;

                                            if (dtData.Tables[0].Columns.Contains("HinhThucThanhToanTienMat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HinhThucThanhToanTienMat"].ToString()))
                                                {
                                                    bool bHinhThucThanhToanTienMat;

                                                    if (bool.TryParse(dataRow["HinhThucThanhToanTienMat"].ToString(), out bHinhThucThanhToanTienMat))
                                                    {
                                                        if (bHinhThucThanhToanTienMat)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCash"].Value = true;
                                                        }
                                                        else
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCash"].Value = false;
                                                        }
                                                    }
                                                }
                                            }

                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPayPOS"].Value = true;

                                            if (dtData.Tables[0].Columns.Contains("TraTienTaiBuuCuc"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["TraTienTaiBuuCuc"].ToString()))
                                                {
                                                    bool bTraTienTaiBuuCuc;

                                                    if (bool.TryParse(dataRow["TraTienTaiBuuCuc"].ToString(), out bTraTienTaiBuuCuc))
                                                    {
                                                        if (bTraTienTaiBuuCuc)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPayPOS"].Value = true;
                                                        }
                                                        else
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPayPOS"].Value = false;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoTaiKhoan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoTaiKhoan"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAccount"].Value = dataRow["SoTaiKhoan"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("NganHang"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["NganHang"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBank"].Value = dataRow["NganHang"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("ChiNhanh"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ChiNhanh"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBranch"].Value = dataRow["ChiNhanh"].ToString();
                                                }
                                            }

                                            //if (dtData.Tables[0].Columns.Contains("PhiChuyenKhoan"))
                                            //{
                                            //    if (!string.IsNullOrEmpty(dataRow["PhiChuyenKhoan"].ToString()))
                                            //    {
                                            //        double dPhiChuyenKhoan;
                                            //        if (double.TryParse(dataRow["PhiChuyenKhoan"].ToString(), out dPhiChuyenKhoan))
                                            //        {
                                            //            if (dPhiChuyenKhoan > 0)
                                            //            {
                                            //                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colChargeTransfer"].Value = dPhiChuyenKhoan.ToString();
                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colChargeTransfer"].Value = "0";
                                            //            }
                                            //        }
                                            //    }
                                            //}

                                            if (dtData.Tables[0].Columns.Contains("PhatDongKiem"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["PhatDongKiem"].ToString()))
                                                {
                                                    bool bPDKResult;

                                                    if (bool.TryParse(dataRow["PhatDongKiem"].ToString(), out bPDKResult))
                                                    {
                                                        if (bPDKResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPDK"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BaoPhat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BaoPhat"].ToString()))
                                                {
                                                    bool bARResult;

                                                    if (bool.TryParse(dataRow["BaoPhat"].ToString(), out bARResult))
                                                    {
                                                        if (bARResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAR"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BaoPhatEmail"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BaoPhatEmail"].ToString()))
                                                {
                                                    bool bAREmailResult;

                                                    if (bool.TryParse(dataRow["BaoPhatEmail"].ToString(), out bAREmailResult))
                                                    {
                                                        if (bAREmailResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAREmail"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BaoPhatSMS"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BaoPhatSMS"].ToString()))
                                                {
                                                    bool bARSMSResult;

                                                    if (bool.TryParse(dataRow["BaoPhatSMS"].ToString(), out bARSMSResult))
                                                    {
                                                        if (bARSMSResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colARSMS"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("PhatTanTay"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["PhatTanTay"].ToString()))
                                                {
                                                    bool bPTTResult;

                                                    if (bool.TryParse(dataRow["PhatTanTay"].ToString(), out bPTTResult))
                                                    {
                                                        if (bPTTResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPTT"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("VUN"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["VUN"].ToString()))
                                                {
                                                    bool bVUNResult;

                                                    if (bool.TryParse(dataRow["VUN"].ToString(), out bVUNResult))
                                                    {
                                                        if (bVUNResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVUN"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            /* Added by Quangnd */
                                            if (dtData.Tables[0].Columns.Contains("TuyetMat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["TuyetMat"].ToString()))
                                                {
                                                    bool bKAResult;

                                                    if (bool.TryParse(dataRow["TuyetMat"].ToString(), out bKAResult))
                                                    {
                                                        if (bKAResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colKA"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("ToiMat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ToiMat"].ToString()))
                                                {
                                                    bool bKBResult;

                                                    if (bool.TryParse(dataRow["ToiMat"].ToString(), out bKBResult))
                                                    {
                                                        if (bKBResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colKB"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("Mat"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["Mat"].ToString()))
                                                {
                                                    bool bKCResult;

                                                    if (bool.TryParse(dataRow["Mat"].ToString(), out bKCResult))
                                                    {
                                                        if (bKCResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colKC"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HenGioTrungTamTinhTP"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HenGioTrungTamTinhTP"].ToString()))
                                                {
                                                    bool bHGNResult;

                                                    if (bool.TryParse(dataRow["HenGioTrungTamTinhTP"].ToString(), out bHGNResult))
                                                    {
                                                        if (bHGNResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHGN"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HenGioKhac"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HenGioKhac"].ToString()))
                                                {
                                                    bool bHGLResult;

                                                    if (bool.TryParse(dataRow["HenGioKhac"].ToString(), out bHGLResult))
                                                    {
                                                        if (bHGLResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHGL"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HoaTocTrungTamTinhTP"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HoaTocTrungTamTinhTP"].ToString()))
                                                {
                                                    bool bHTNResult;

                                                    if (bool.TryParse(dataRow["HoaTocTrungTamTinhTP"].ToString(), out bHTNResult))
                                                    {
                                                        if (bHTNResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHTN"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HoaTocKhac"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HoaTocKhac"].ToString()))
                                                {
                                                    bool bHTLResult;

                                                    if (bool.TryParse(dataRow["HoaTocKhac"].ToString(), out bHTLResult))
                                                    {
                                                        if (bHTLResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHTL"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("KhaiGia"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["KhaiGia"].ToString()))
                                                {
                                                    bool bVResult;

                                                    if (bool.TryParse(dataRow["KhaiGia"].ToString(), out bVResult))
                                                    {
                                                        if (bVResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colV"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("GiaTriKhaiGia"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["GiaTriKhaiGia"].ToString()))
                                                {
                                                    double dDeclaredValue;

                                                    if (double.TryParse(dataRow["GiaTriKhaiGia"].ToString(), out dDeclaredValue))
                                                    {
                                                        if (dDeclaredValue > 0)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colGiaTriKhaiGia"].Value = NumberFormat(dDeclaredValue);
                                                        }
                                                    }
                                                }
                                            }

                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAuthorReceiver"].Value = false;

                                            if (dtData.Tables[0].Columns.Contains("UyQuyenChoNguoiNhan"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["UyQuyenChoNguoiNhan"].ToString()))
                                                {
                                                    bool bUyQuyenChoNguoiNhan;

                                                    if (bool.TryParse(dataRow["UyQuyenChoNguoiNhan"].ToString(), out bUyQuyenChoNguoiNhan))
                                                    {
                                                        if (bUyQuyenChoNguoiNhan)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAuthorReceiver"].Value = true;
                                                        }
                                                        else
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAuthorReceiver"].Value = false;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("PPA"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["PPA"].ToString()))
                                                {
                                                    bool bPPAResult;

                                                    if (bool.TryParse(dataRow["PPA"].ToString(), out bPPAResult))
                                                    {
                                                        if (bPPAResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPPA"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoHopDongPPA"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoHopDongPPA"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colContractNumberPPA"].Value = dataRow["SoHopDongPPA"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HanHopDongPPA"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HanHopDongPPA"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colContractDatePPA"].Value = dataRow["HanHopDongPPA"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("C"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["C"].ToString()))
                                                {
                                                    bool bCResult;

                                                    if (bool.TryParse(dataRow["C"].ToString(), out bCResult))
                                                    {
                                                        if (bCResult)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colC"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoHopDongC"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoHopDongC"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colContractNumberC"].Value = dataRow["SoHopDongC"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HanHopDongC"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HanHopDongC"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colContractDateC"].Value = dataRow["HanHopDongC"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BenThu3TraCuoc"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BenThu3TraCuoc"].ToString()))
                                                {
                                                    bool bT3Result;

                                                    if (bool.TryParse(dataRow["BenThu3TraCuoc"].ToString(), out bT3Result))
                                                    {
                                                        if (bT3Result)
                                                        {
                                                            dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBenThu3"].Value = true;
                                                        }
                                                    }
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoHopDongBenThu3"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoHopDongBenThu3"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colContractNumberT3"].Value = dataRow["SoHopDongBenThu3"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("HanHopDongBenThu3"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["HanHopDongBenThu3"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colContractDateT3"].Value = dataRow["HanHopDongBenThu3"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("TenBenThu3"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["TenBenThu3"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colThirdPartyName"].Value = dataRow["TenBenThu3"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("SoHieuBuuGuiGoc"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["SoHieuBuuGuiGoc"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemOriginal"].Value = dataRow["SoHieuBuuGuiGoc"].ToString();
                                                }
                                            }
                                            //Dungnt-Them dich vu cong them
                                            if (dtData.Tables[0].Columns.Contains("DanhSachDichVuGTGT"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["DanhSachDichVuGTGT"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVASService"].Value = dataRow["DanhSachDichVuGTGT"].ToString();
                                                }
                                            }
                                            if (dtData.Tables[0].Columns.Contains("LoaiHang"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["LoaiHang"].ToString()))
                                                {
                                                    if (dataRow["LoaiHang"].ToString().Contains(CommodityTypeConstance.HANG_NHE))
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colComodityType"].Value = CommodityTypeConstance.HANG_NHE;
                                                    }
                                                    else
                                                    {
                                                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colComodityType"].Value = dataRow["LoaiHang"].ToString();
                                                    }

                                                }
                                            }
                                            if (dtData.Tables[0].Columns.Contains("TGPhatDuKien"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["TGPhatDuKien"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDeliveryTime"].Value = dataRow["TGPhatDuKien"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("ExtendData"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["ExtendData"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colExtendData"].Value = dataRow["ExtendData"].ToString();
                                                }
                                            }

                                            if (dtData.Tables[0].Columns.Contains("BusinessId"))
                                            {
                                                if (!string.IsNullOrEmpty(dataRow["BusinessId"].ToString()))
                                                {
                                                    dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBusinessId"].Value = dataRow["BusinessId"].ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        ShowMessageBoxWarning("Không có bưu gửi");
                        return;
                    }


                    ConvertWeight();

                    displayFreight();

                    CalculatorTotalItem();

                    CalculatorTotalWeight();

                    CalculatorTotalFreight();
                }
                else
                {
                    ShowMessageBoxWarning("Không có bưu gửi");
                    return;
                }
            }
            else
            {
                ShowMessageBoxWarning("Yêu cầu chọn dịch vụ");
            }
        }

        private bool CheckFirstItemCode(string FirstItemCode)
        {
            bool bResult = true;

            if (string.IsNullOrEmpty(FirstItemCode))
            {
                ShowMessageBoxWarning("Yêu cầu nhập số hiệu bưu gửi bắt đầu");
                return false;
            }
            else
            {
                if (FirstItemCode.Length != 13)
                {
                    ShowMessageBoxWarning("Yêu cầu số hiệu bưu gửi bắt đầu 13 ký tự");
                    return false;
                }
                else
                {
                    if (!provinceListCode.Contains(FirstItemCode.Substring(2, 2)))
                    {
                        ShowMessageBoxWarning("Số hiệu bưu gửi bắt đầu không đúng mã tỉnh/thành phố nhận gửi");
                        return false;
                    }
                    else if (!FirstItemCode.Substring(11, 2).Equals(SendingCountriesConstance.VIET_NAM))
                    {
                        ShowMessageBoxWarning("Số hiệu bưu gửi bắt đầu không đúng định dạng theo quốc gia nhận gửi");
                        return false;
                    }
                    else
                    {

                    }

                    if (!CheckSumItemCode(FirstItemCode))
                    {
                        ShowMessageBoxWarning("Số hiệu bưu gửi bắt đầu không đúng số checksum");
                        return false;
                    }

                }
            }

            return bResult;
        }

        private string AutoGenItemCode(string LastItemCode)
        {
            if (LastItemCode.Length != 13)
                return "";
            //if (LastCode.Substring(0, 1) != "R")
            //    return "";
            string strTemp = LastItemCode.Substring(2, 8);
            int s11Number = 0;
            strTemp = (Convert.ToInt32(strTemp) + 1).ToString();
            while (strTemp.Length < 8)
            {
                strTemp = "0" + strTemp;
            }

            s11Number = Convert.ToInt32(strTemp.Substring(0, 1)) * 8 + Convert.ToInt32(strTemp.Substring(1, 1)) * 6 + Convert.ToInt32(strTemp.Substring(2, 1)) * 4 + Convert.ToInt32(strTemp.Substring(3, 1)) * 2 + Convert.ToInt32(strTemp.Substring(4, 1)) * 3 + Convert.ToInt32(strTemp.Substring(5, 1)) * 5 + Convert.ToInt32(strTemp.Substring(6, 1)) * 9 + Convert.ToInt32(strTemp.Substring(7, 1)) * 7;
            s11Number = s11Number % 11;
            s11Number = 11 - s11Number;
            if (s11Number == 10)
                s11Number = 0;
            if (s11Number == 11)
                s11Number = 5;
            return LastItemCode.Substring(0, 2) + strTemp + s11Number + "VN";
        }

        private bool CheckItemExists(string itemCode)
        {
            bool result = false;
            ItemDAO daoItem = new ItemDAO();
            ItemEntity enItem = daoItem.SelectOne(itemCode);
            if (enItem != null && !enItem.IsNullItemCode)
            {
                return true;
            }
            else
            {
                ShiftHandoverItemDAO daoShiftHandoverItem = new ShiftHandoverItemDAO();
                List<ShiftHandoverItemEntity> enShiftHandoverItemList = daoShiftHandoverItem.SelectAllFilter("ItemCode =N'" + itemCode + "'");
                if (enShiftHandoverItemList != null && enShiftHandoverItemList.Count > 0)
                {
                    return true;
                }
            }

            return result;
        }

        private bool CheckItemFormat(string itemCode)
        {
            bool result = true;

            if (itemCode.Replace(" ", "").Length != 13)
            {
                //Độ dài không đúng.
                return false;
            }
            else
            {
                if (cboService.SelectedValue != null)
                {
                    if (!itemCode.Substring(0, 1).ToUpper().Equals(cboService.SelectedValue.ToString().ToUpper()))
                    {
                        //Số hiệu bưu gửi không đúng định dạng theo dịch vụ
                        return false;
                    }
                    //else if (!itemCode.Substring(2, 2).Equals(provinceCode))
                    else if (!provinceListCode.Contains(itemCode.Substring(2, 2)))
                    {
                        //Số hiệu bưu gửi không đúng mã tỉnh/thành phố nhận gửi"
                        return false;
                    }
                    else if (!itemCode.Substring(11, 2).ToUpper().Equals("VN"))
                    {
                        //"Số hiệu bưu gửi không đúng định dạng theo quốc gia nhận gửi"
                        return false;
                    }
                    else
                    {

                    }
                }

            }

            return result;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            //ItemEntity enItem = new ItemEntity();
            //enItem.SendingTime = dtpSendingTime.Value;
            //enItem.SenderFullname = txtSenderFullName.Text.Trim();

            //frmPrintOption frmOption = new frmPrintOption();
            //frmOption.POSCode = this.POSCode; 
            //frmOption.ServiceCode = cboService.SelectedValue.ToString();
            //frmOption.PhaseCode = PhaseConstance.NHAN_GUI_SLL;
            //frmOption.AcceptanceType = AcceptanceTypeConstance.BUU_GUI_SLL;
            //frmOption.EntityItem = enItem;
            //frmOption.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmAcceptanceFromDieuTin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.F10))
            {
                Accept();
            }
            else if (e.KeyCode.Equals(Keys.Escape))
            {
                this.Close();
            }
            else
            { }
        }

        private void frmAcceptanceFromDieuTin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (alwaysAsk)
            {
                if (ShowMesageBoxConfirm("Bạn có muốn thoát màn hình import bưu gửi số lượng lớn từ file không?").Equals(DialogResult.Yes))
                    e.Cancel = false;
                else
                    e.Cancel = true;
            }
            else
            {
                if (itemListTransferWait.Count > 0)
                {
                    if (Ctin.Css.Configuration.ConfigManager.IsOffline)
                    {
                        MessageBox.Show("Bưu cục đang hoạt động ở chế độ Offline. Không cho phép truyền/nhận dữ liệu.");
                    }
                    else
                    {
                        List<string> errors;
                        List<string> infos;

                        var message = Ctin.Communication.Core.CommunicationHelpers.GetMessageFromDatabase(out errors, MessageTypes.DU_LIEU_BUU_GUI_THEO_LIST, "Item", Ctin.Communication.Core.CommunicationHelpers.GetSelectSqlFromList(itemListTransferWait, "Item", "ItemCode"));

                        if (message == null)
                        {
                            ShowMessageBoxWarning("Không lấy được dữ liệu");
                        }
                        else
                        {
                            var reservationId = Ctin.Communication.ExternalServices.CommunicationServiceHelpers.GetNewReservationId(out errors, out infos);

                            message.ReservationId = reservationId;

                            var result = Ctin.Communication.ExternalServices.CommunicationServiceHelpers.SendMessage(message, true, out errors, out infos);

                            if (result == false)
                            {
                                Error("Chưa truyền được dữ liệu lên tổng công ty.");
                                if (ConfigManager.IsDebugMode)
                                {
                                    Error(errors);
                                }
                            }
                            else
                            {
                                //ItemDAO daoItem = new ItemDAO();

                                //foreach (string value in itemListTransferWait)
                                //{
                                //    daoItem.UpdateTransferStatus(true, value);
                                //}

                                ShowMessageBoxInformation("Truyền thông tin bưu gửi thành công : " + itemListTransferWait.Count.ToString());
                            }
                        }
                    }
                }

                e.Cancel = false;
            }
        }

        #region Key pressed event

        private void cboService_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else
            { }
        }

        private void dtpSendingTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else
            { }
        }

        private void cboService_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboService.SelectedValue != null)
            {
                ConfigBackColor(cboService.SelectedValue.ToString());

                if (!cboService.SelectedValue.ToString().Equals(ServiceConstance.DHL))
                {
                    foreach (DataGridViewRow rows in dgvListItems.Rows)
                    {
                        rows.Cells["colIsCollection"].Value = false;
                        rows.Cells["colCustomerAccountNo"].Value = "";
                    }
                }
            }

            displayFreight();
        }

        //private void timerStop_Tick(object sender, EventArgs e)
        //{
        //    dtpFromDate.Value = DateTimeServer.Now;
        //}

        private void chkTimeStop_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else
            { }
        }

        #endregion

        #region Load data on combobox, datagridview

        private void SelectProvinceBy(string strPOSCode)
        {
            provinceListCode = new List<string>();

            ProvinceDAO daoProvince = new ProvinceDAO();

            ProvinceEntity enProvince = daoProvince.SelectProvinceBy(strPOSCode);

            if (enProvince != null)
            {
                if (!string.IsNullOrEmpty(enProvince.ProvinceListCode))
                {
                    provinceListCode = new List<string>(enProvince.ProvinceListCode.Split(';'));
                }
                else
                {
                    provinceListCode.Add(enProvince.ProvinceCode);
                }
            }
        }

        private void SelectServiceByPOSCode(string strPOSCode)
        {
            ServiceDAO daoService = new ServiceDAO();
            List<ServiceEntity> entityServiceList = daoService.SelectServiceByPOSCode(this.POSCode);
            if (entityServiceList != null && entityServiceList.Count > 0)
            {
                cboService.DataSource = entityServiceList;
                cboService.DisplayMember = "ServiceName";
                cboService.ValueMember = "ServiceCode";

                if (cboService.SelectedValue != null)
                {

                    if (!string.IsNullOrEmpty(this.ServiceCode))
                        cboService.SelectedValue = this.ServiceCode;

                    ConfigBackColor(cboService.SelectedValue.ToString());
                }


            }
            else
            {
                cboService.DataSource = null;
            }
        }

        #endregion

        private void ConfigBackColor(string strServiceCode)
        {
            if (strServiceCode.Equals(ServiceConstance.BPBD))
            {
                this.BackColor = ColorServiceConstance.BPBD;

                pGroupBox4.BackColor = ColorServiceConstance.BPBD;
                //pGroupBox1.BackColor = ColorServiceConstance.BPBD;
                pGroupBox2.BackColor = ColorServiceConstance.BPBD;
            }
            else if (strServiceCode.Equals(ServiceConstance.KT1))
            {
                this.BackColor = ColorServiceConstance.KT1;

                pGroupBox4.BackColor = ColorServiceConstance.KT1;
                //pGroupBox1.BackColor = ColorServiceConstance.KT1;
                pGroupBox2.BackColor = ColorServiceConstance.KT1;
            }
            else if (strServiceCode.Equals(ServiceConstance.BCUT))
            {
                this.BackColor = ColorServiceConstance.BCUT;

                pGroupBox4.BackColor = ColorServiceConstance.BCUT;
                //pGroupBox1.BackColor = ColorServiceConstance.BCUT;
                pGroupBox2.BackColor = ColorServiceConstance.BCUT;
            }
            else if (strServiceCode.Equals(ServiceConstance.BK))
            {
                this.BackColor = ColorServiceConstance.BK;

                pGroupBox4.BackColor = ColorServiceConstance.BK;
                //pGroupBox1.BackColor = ColorServiceConstance.BK;
                pGroupBox2.BackColor = ColorServiceConstance.BK;

            }
            else if (strServiceCode.Equals(ServiceConstance.DHL))
            {
                this.BackColor = ColorServiceConstance.DHL;
                pGroupBox4.BackColor = ColorServiceConstance.DHL;
                //pGroupBox1.BackColor = ColorServiceConstance.DHL;
                pGroupBox2.BackColor = ColorServiceConstance.DHL;
            }
            else if (strServiceCode.Equals(ServiceConstance.VNQuickpost))
            {
                this.BackColor = ColorServiceConstance.VNQuickpost;

                pGroupBox4.BackColor = ColorServiceConstance.VNQuickpost;
                //pGroupBox1.BackColor = ColorServiceConstance.VNQuickpost;
                pGroupBox2.BackColor = ColorServiceConstance.VNQuickpost;
            }
            else if (strServiceCode.Equals(ServiceConstance.UPS))
            {
                this.BackColor = ColorServiceConstance.UPS;
                pGroupBox4.BackColor = ColorServiceConstance.UPS;
                //pGroupBox1.BackColor = ColorServiceConstance.UPS;
                pGroupBox2.BackColor = ColorServiceConstance.UPS;
            }
            else if (strServiceCode.Equals(ServiceConstance.EMS))
            {
                this.BackColor = ColorServiceConstance.EMS;
                pGroupBox4.BackColor = ColorServiceConstance.EMS;
                //pGroupBox1.BackColor = ColorServiceConstance.EMS;
                pGroupBox2.BackColor = ColorServiceConstance.EMS;

            }
            else
            {
                this.BackColor = ColorServiceConstance.NORMAL;
                pGroupBox4.BackColor = ColorServiceConstance.NORMAL;
                //pGroupBox1.BackColor = ColorServiceConstance.NORMAL;
                pGroupBox2.BackColor = ColorServiceConstance.NORMAL;
            }
        }

        private bool CheckSendingTime()
        {
            bool resullt = true;

            return resullt;
        }

        private bool CheckItemCode()
        {
            bool resullt = true;

            string messageEmptyError = "";
            string messageExistError = "";
            string messageFormatError = "";
            string messageSymbolError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colBarcode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colBarcode"].Value.ToString()))
                {
                    //Check bưu gửi đã tồn tại chưa?
                    if (CheckItemExists(rows.Cells["colBarcode"].Value.ToString()))
                    {
                        rows.Cells["colBarcode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageExistError))
                            messageExistError = messageExistError + "\r\n" + rows.Cells["colBarcode"].Value.ToString();
                        else
                            messageExistError = rows.Cells["colBarcode"].Value.ToString();
                    }

                    //Check định dạng số hiệu bưu gửi
                    if (!CheckItemFormat(rows.Cells["colBarcode"].Value.ToString()))
                    {
                        rows.Cells["colBarcode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageFormatError))
                            messageFormatError = messageFormatError + "\r\n" + rows.Cells["colBarcode"].Value.ToString();
                        else
                            messageFormatError = rows.Cells["colBarcode"].Value.ToString();
                    }

                    //Check ký tự đặc biệt | ; @ # $ ^ & *
                    if (rows.Cells["colBarcode"].Value.ToString().Contains("|") || rows.Cells["colBarcode"].Value.ToString().Contains(";") || rows.Cells["colBarcode"].Value.ToString().Contains("@") || rows.Cells["colBarcode"].Value.ToString().Contains("#") ||
                        rows.Cells["colBarcode"].Value.ToString().Contains("$") || rows.Cells["colBarcode"].Value.ToString().Contains("^") || rows.Cells["colBarcode"].Value.ToString().Contains("&") || rows.Cells["colBarcode"].Value.ToString().Contains("*"))
                    {
                        rows.Cells["colBarcode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageFormatError))
                            messageSymbolError = messageFormatError + "\r\n" + rows.Cells["colBarcode"].Value.ToString();
                        else
                            messageSymbolError = rows.Cells["colBarcode"].Value.ToString();
                    }
                }
                else
                {
                    rows.Cells["colBarcode"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageEmptyError))
                        messageEmptyError = messageEmptyError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageEmptyError = rows.Cells["colIndex"].Value.ToString();
                }
            }

            if (!string.IsNullOrEmpty(messageEmptyError))
            {
                messageEmptyError = "Yêu cầu nhập số hiệu bưu gửi số \r\n" + messageEmptyError;
            }

            if (!string.IsNullOrEmpty(messageExistError))
            {
                messageExistError = "Danh sách các bưu gửi đã tồn tại trong hệ thống \r\n" + messageExistError;
            }

            if (!string.IsNullOrEmpty(messageFormatError))
            {
                messageFormatError = "Số hiệu bưu gửi không đúng định dạng \r\n" + messageFormatError;
            }

            if (!string.IsNullOrEmpty(messageSymbolError))
            {
                messageSymbolError = "Số hiệu bưu gửi không được sử dụng các ký tự đặc biệt | ; @ # $ ^ & * \r\n" + messageSymbolError;
            }

            if (!string.IsNullOrEmpty(messageEmptyError) || !string.IsNullOrEmpty(messageExistError) || !string.IsNullOrEmpty(messageFormatError) || !string.IsNullOrEmpty(messageSymbolError))
            {
                ShowMessageBoxWarning(messageEmptyError + "\r\n" + messageExistError + "\r\n" + messageFormatError + "\r\n" + messageSymbolError);
                return false;
            }

            return resullt;
        }

        private bool CheckSumItem()
        {
            bool resullt = true;

            if (cboService.SelectedValue != null && cboService.SelectedValue.ToString().Equals(ServiceConstance.EMS))
            {
                string messageFormatError = "";

                foreach (DataGridViewRow rows in dgvListItems.Rows)
                {
                    if (rows.Cells["colBarcode"].Value != null)
                    {
                        if (!CheckSumItemCode(rows.Cells["colBarcode"].Value.ToString()))
                        {
                            rows.Cells["colBarcode"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageFormatError))
                                messageFormatError = messageFormatError + "\r\n" + rows.Cells["colBarcode"].Value.ToString();
                            else
                                messageFormatError = rows.Cells["colBarcode"].Value.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(messageFormatError))
                {
                    ShowMessageBoxWarning("Số hiệu bưu gửi không đúng số checksum: " + messageFormatError);
                    return false;
                }
            }

            return resullt;
        }

        private bool CheckSumItemCode(string CurrentItemCode)
        {
            if (CurrentItemCode.Length != 13)
                return false;

            string strTemp = CurrentItemCode.Substring(2, 8);

            int iResult;
            if (int.TryParse(strTemp, out iResult))
            {
                int s11Number = 0;

                s11Number = Convert.ToInt32(strTemp.Substring(0, 1)) * 8 + Convert.ToInt32(strTemp.Substring(1, 1)) * 6 + Convert.ToInt32(strTemp.Substring(2, 1)) * 4 + Convert.ToInt32(strTemp.Substring(3, 1)) * 2 + Convert.ToInt32(strTemp.Substring(4, 1)) * 3 + Convert.ToInt32(strTemp.Substring(5, 1)) * 5 + Convert.ToInt32(strTemp.Substring(6, 1)) * 9 + Convert.ToInt32(strTemp.Substring(7, 1)) * 7;
                s11Number = s11Number % 11;
                s11Number = 11 - s11Number;
                if (s11Number == 10)
                    s11Number = 0;
                if (s11Number == 11)
                    s11Number = 5;

                string strCheckSumCurrent = CurrentItemCode.Substring(10, 1);

                if (strCheckSumCurrent.Equals(s11Number.ToString()))
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }
        }

        private bool CheckSymbolItem()
        {
            bool resullt = true;

            string messageFormatError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colBarcode"].Value != null)
                {
                    if (!CheckSymbolItemCode(rows.Cells["colBarcode"].Value.ToString()))
                    {
                        rows.Cells["colBarcode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageFormatError))
                            messageFormatError = messageFormatError + "\r\n" + rows.Cells["colBarcode"].Value.ToString();
                        else
                            messageFormatError = rows.Cells["colBarcode"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageFormatError))
            {
                ShowMessageBoxWarning("Số hiệu bưu gửi không đúng định dạng " + messageFormatError);
                return false;
            }

            return resullt;
        }

        private bool CheckSymbolItemCode(string CurrentItemCode)
        {
            if (CurrentItemCode.Length != 13)
                return false;

            string strTemp = CurrentItemCode.Substring(2, 9);

            int iResult;

            if (int.TryParse(strTemp, out iResult))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckDataCode()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colDataCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colDataCode"].Value.ToString()))
                {

                }
                else
                {
                    if (rows.Cells["colItemType"].Value != null && !string.IsNullOrEmpty(rows.Cells["colItemType"].Value.ToString()))
                    {
                        if (rows.Cells["colItemType"].Value.ToString().Equals(ItemTypeConstance.HO_SO_XET_TUYEN) || rows.Cells["colItemType"].Value.ToString().Equals(ItemTypeConstance.HSXTM) || rows.Cells["colItemType"].Value.ToString().Equals(ItemTypeConstance.HSXT_XNNV))
                        {
                            rows.Cells["colDataCode"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập số CV/ số hiệu KH. Bưu gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckCustomerCode()
        {
            bool resullt = true;

            string messageError = "";

            CustomerDAO daoCustomer = new CustomerDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colCustomerCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colCustomerCode"].Value.ToString()))
                {
                    List<CustomerEntity> enCustomerList = daoCustomer.SelectAllFilter("CustomerCode = N'" + rows.Cells["colCustomerCode"].Value.ToString() + "'");

                    if (enCustomerList != null && enCustomerList.Count > 0)
                    {

                    }
                    else
                    {
                        rows.Cells["colCustomerCode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Mã khách hàng gửi không tồn tại trong danh mục khách hàng. Bưu gửi :" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckSenderFullName()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colSenderFullName"].Value != null && !string.IsNullOrEmpty(rows.Cells["colSenderFullName"].Value.ToString()))
                {

                }
                else
                {
                    rows.Cells["colSenderFullName"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập họ tên người gửi bưu gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckSenderFullNameSymbol()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colSenderFullName"].Value != null && !string.IsNullOrEmpty(rows.Cells["colSenderFullName"].Value.ToString()))
                {
                    if (rows.Cells["colSenderFullName"].Value.ToString().Contains("|") || rows.Cells["colSenderFullName"].Value.ToString().Contains(";") || rows.Cells["colSenderFullName"].Value.ToString().Contains("@") || rows.Cells["colSenderFullName"].Value.ToString().Contains("#") ||
                        rows.Cells["colSenderFullName"].Value.ToString().Contains("$") || rows.Cells["colSenderFullName"].Value.ToString().Contains("^") || rows.Cells["colSenderFullName"].Value.ToString().Contains("&") || rows.Cells["colSenderFullName"].Value.ToString().Contains("*"))
                    {
                        rows.Cells["colSenderFullName"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
                else
                {

                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Họ tên người gửi không được sử dụng các ký tự đặc biệt | ; @ # $ ^ & * " + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckSenderAddress()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colSenderAddress"].Value != null && !string.IsNullOrEmpty(rows.Cells["colSenderAddress"].Value.ToString()))
                {

                }
                else
                {
                    rows.Cells["colSenderAddress"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập địa chỉ người gửi bưu gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckSenderAddressSymbol()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colSenderAddress"].Value != null && !string.IsNullOrEmpty(rows.Cells["colSenderAddress"].Value.ToString()))
                {
                    if (rows.Cells["colSenderAddress"].Value.ToString().Contains("|") || rows.Cells["colSenderAddress"].Value.ToString().Contains(";") || rows.Cells["colSenderAddress"].Value.ToString().Contains("@") || rows.Cells["colSenderAddress"].Value.ToString().Contains("#") ||
                        rows.Cells["colSenderAddress"].Value.ToString().Contains("$") || rows.Cells["colSenderAddress"].Value.ToString().Contains("^") || rows.Cells["colSenderAddress"].Value.ToString().Contains("&") || rows.Cells["colSenderAddress"].Value.ToString().Contains("*"))
                    {
                        rows.Cells["colSenderAddress"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
                else
                {

                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Địa chỉ người gửi không được sử dụng các ký tự đặc biệt | ; @ # $ ^ & * " + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverCustomerCode()
        {
            bool resullt = true;

            string messageError = "";

            CustomerDAO daoCustomer = new CustomerDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colReceiverCustomerCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colReceiverCustomerCode"].Value.ToString()))
                {
                    List<CustomerEntity> enCustomerList = daoCustomer.SelectAllFilter("CustomerCode = N'" + rows.Cells["colReceiverCustomerCode"].Value.ToString() + "'");

                    if (enCustomerList != null && enCustomerList.Count > 0)
                    {

                    }
                    else
                    {
                        rows.Cells["colReceiverCustomerCode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Mã khách hàng nhận không tồn tại trong danh mục khách hàng. Bưu gửi :" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverCustomerCodeByItemType()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colReceiverCustomerCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colReceiverCustomerCode"].Value.ToString()))
                {
                }
                else
                {
                    if (rows.Cells["colItemType"].Value != null && !string.IsNullOrEmpty(rows.Cells["colItemType"].Value.ToString()))
                    {
                        if (rows.Cells["colItemType"].Value.ToString().Equals(ItemTypeConstance.HO_SO_XET_TUYEN) || rows.Cells["colItemType"].Value.ToString().Equals(ItemTypeConstance.HSXTM) || rows.Cells["colItemType"].Value.ToString().Equals(ItemTypeConstance.HSXT_XNNV))
                        {
                            rows.Cells["colReceiverCustomerCode"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập mã khách hàng nhận. Bưu gửi :" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverFullName()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colReceiverFullName"].Value != null && !string.IsNullOrEmpty(rows.Cells["colReceiverFullName"].Value.ToString()))
                {

                }
                else
                {
                    rows.Cells["colReceiverFullName"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập họ tên người nhận bưu gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverFullNameSymbol()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colReceiverFullName"].Value != null && !string.IsNullOrEmpty(rows.Cells["colReceiverFullName"].Value.ToString()))
                {
                    if (rows.Cells["colReceiverFullName"].Value.ToString().Contains("|") || rows.Cells["colReceiverFullName"].Value.ToString().Contains(";") || rows.Cells["colReceiverFullName"].Value.ToString().Contains("@") || rows.Cells["colReceiverFullName"].Value.ToString().Contains("#") ||
                        rows.Cells["colReceiverFullName"].Value.ToString().Contains("$") || rows.Cells["colReceiverFullName"].Value.ToString().Contains("^") || rows.Cells["colReceiverFullName"].Value.ToString().Contains("&") || rows.Cells["colReceiverFullName"].Value.ToString().Contains("*"))
                    {
                        rows.Cells["colReceiverFullName"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
                else
                {

                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Họ tên người nhận không được sử dụng các ký tự đặc biệt | ; @ # $ ^ & * " + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverAddress()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colReceiverAddress"].Value != null && !string.IsNullOrEmpty(rows.Cells["colReceiverAddress"].Value.ToString()))
                {

                }
                else
                {
                    rows.Cells["colReceiverAddress"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập địa chỉ người nhận bưu gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverAddressSymbol()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colReceiverAddress"].Value != null && !string.IsNullOrEmpty(rows.Cells["colReceiverAddress"].Value.ToString()))
                {
                    if (rows.Cells["colReceiverAddress"].Value.ToString().Contains("|") || rows.Cells["colReceiverAddress"].Value.ToString().Contains(";") || rows.Cells["colReceiverAddress"].Value.ToString().Contains("@") || rows.Cells["colReceiverAddress"].Value.ToString().Contains("#") ||
                        rows.Cells["colReceiverAddress"].Value.ToString().Contains("$") || rows.Cells["colReceiverAddress"].Value.ToString().Contains("^") || rows.Cells["colReceiverAddress"].Value.ToString().Contains("&") || rows.Cells["colReceiverAddress"].Value.ToString().Contains("*"))
                    {
                        rows.Cells["colReceiverAddress"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
                else
                {

                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Địa chỉ người nhận không được sử dụng các ký tự đặc biệt | ; @ # $ ^ & * " + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckCountryProvince()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colCountryCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colCountryCode"].Value.ToString()))
                {
                }
                else
                {
                    if (rows.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                    {

                    }
                    else
                    {
                        rows.Cells["colProvinceCode"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập quốc gia, tỉnh nhận bưu gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckItemType()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colItemType"].Value != null && !string.IsNullOrEmpty(rows.Cells["colItemType"].Value.ToString()))
                {
                    ServiceItemTypeDAO daoServiceItemType = new ServiceItemTypeDAO();
                    ServiceItemTypeEntity enServiceItemType = daoServiceItemType.SelectOne(cboService.SelectedValue.ToString(), rows.Cells["colItemType"].Value.ToString());
                    if (enServiceItemType != null && !enServiceItemType.IsNullServiceCode)
                    {
                        #region DungNT-20180227-KT1 check thoi gian du kien
                        if (enServiceItemType.ServiceCode.Equals(ServiceConstance.KT1))
                        {
                            ItemTypeDAO daoItemTypeDAO = new ItemTypeDAO();
                            ItemTypeEntity enItemTypeEntity = daoItemTypeDAO.SelectOne(enServiceItemType.ItemTypeCode);
                            if (enItemTypeEntity.ItemTypeName.IndexOf("Hẹn giờ") >= 0)
                            {
                                DateTime c_dtpDeliveryTime;
                                try
                                {
                                    c_dtpDeliveryTime = DateTime.Parse(rows.Cells["colDeliveryTime"].Value.ToString());
                                }
                                catch (Exception)
                                {
                                    c_dtpDeliveryTime = new DateTime(DateTimeServer.Now.Year, DateTimeServer.Now.Month, DateTimeServer.Now.Day, DateTimeServer.Now.Hour, DateTimeServer.Now.Minute, DateTimeServer.Now.Second);
                                }

                                DateTime currentTime = new DateTime(DateTimeServer.Now.Year, DateTimeServer.Now.Month, DateTimeServer.Now.Day, DateTimeServer.Now.Hour, DateTimeServer.Now.Minute, DateTimeServer.Now.Second);

                                if (c_dtpDeliveryTime <= currentTime)
                                {
                                    rows.Cells["colItemType"].Style.BackColor = Color.Red;
                                    if (!string.IsNullOrEmpty(messageError))
                                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString() + ": Thời gian dự kiến phát phải lớn hơn thời gian hiện tại";
                                    else
                                        messageError = rows.Cells["colIndex"].Value.ToString();
                                };
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        rows.Cells["colItemType"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
                else
                {
                    rows.Cells["colItemType"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Loại bưu gửi không đúng với dịch vụ:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckItemContent()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colDetailItem"].Value != null && !string.IsNullOrEmpty(rows.Cells["colDetailItem"].Value.ToString()))
                {

                }
                else
                {
                    if (cboService.SelectedValue.ToString().Equals(ServiceConstance.BK) || cboService.SelectedValue.ToString().Equals(ServiceConstance.BCUT))
                    {
                        rows.Cells["colDetailItem"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                    else
                    {
                        if (cboService.SelectedValue.ToString().Equals(ServiceConstance.BPBD))
                        {

                        }
                    }
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập nội dung hàng gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckUndeliveryGuide()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colUndeliveryIndicator"].Value != null && !string.IsNullOrEmpty(rows.Cells["colUndeliveryIndicator"].Value.ToString()))
                {
                    int iResult;
                    if (int.TryParse(rows.Cells["colUndeliveryIndicator"].Value.ToString(), out iResult))
                    {
                        UndeliveryGuideDAO daoUndeliveryGuide = new UndeliveryGuideDAO();
                        UndeliveryGuideEntity enUndeliveryGuide = daoUndeliveryGuide.SelectOne((byte)iResult);

                        if (enUndeliveryGuide != null && !enUndeliveryGuide.IsNullUndeliveryGuideCode)
                        {
                        }
                        else
                        {
                            rows.Cells["colUndeliveryIndicator"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                    else
                    {
                        rows.Cells["colUndeliveryIndicator"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
                else
                {
                    rows.Cells["colUndeliveryIndicator"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Chỉ dẫn khi không phát được không đúng:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckWeight()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colWeight"].Value != null && !string.IsNullOrEmpty(rows.Cells["colWeight"].Value.ToString()))
                {
                    double dResult;
                    if (!double.TryParse(rows.Cells["colWeight"].Value.ToString(), out dResult))
                    {
                        rows.Cells["colWeight"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                    else
                    {
                        if (dResult <= 0)
                        {
                            rows.Cells["colWeight"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                }
                else
                {
                    rows.Cells["colWeight"].Style.BackColor = Color.Red;

                    if (!string.IsNullOrEmpty(messageError))
                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                    else
                        messageError = rows.Cells["colIndex"].Value.ToString();
                }

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Khối lượng bưu gửi không đúng." + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckLength()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colLength"].Value != null && !string.IsNullOrEmpty(rows.Cells["colLength"].Value.ToString()))
                {
                    double dResult;
                    if (!double.TryParse(rows.Cells["colLength"].Value.ToString(), out dResult))
                    {
                        rows.Cells["colLength"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                    else
                    {
                        if (dResult < 0)
                        {
                            rows.Cells["colLength"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                }
                //else
                //{
                //    rows.Cells["colWeight"].Style.BackColor = Color.Red;

                //    if (!string.IsNullOrEmpty(messageError))
                //        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                //    else
                //        messageError = rows.Cells["colIndex"].Value.ToString();
                //}

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Chiều dài bưu gửi không đúng." + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckWidth()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colWidth"].Value != null && !string.IsNullOrEmpty(rows.Cells["colWidth"].Value.ToString()))
                {
                    double dResult;
                    if (!double.TryParse(rows.Cells["colWidth"].Value.ToString(), out dResult))
                    {
                        rows.Cells["colWidth"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                    else
                    {
                        if (dResult < 0)
                        {
                            rows.Cells["colWidth"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                }
                //else
                //{
                //    rows.Cells["colWeight"].Style.BackColor = Color.Red;

                //    if (!string.IsNullOrEmpty(messageError))
                //        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                //    else
                //        messageError = rows.Cells["colIndex"].Value.ToString();
                //}

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Chiều rộng bưu gửi không đúng." + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckHeight()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colHeight"].Value != null && !string.IsNullOrEmpty(rows.Cells["colHeight"].Value.ToString()))
                {
                    double dResult;
                    if (!double.TryParse(rows.Cells["colHeight"].Value.ToString(), out dResult))
                    {
                        rows.Cells["colHeight"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                    else
                    {
                        if (dResult < 0)
                        {
                            rows.Cells["colHeight"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                }
                //else
                //{
                //    rows.Cells["colWeight"].Style.BackColor = Color.Red;

                //    if (!string.IsNullOrEmpty(messageError))
                //        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                //    else
                //        messageError = rows.Cells["colIndex"].Value.ToString();
                //}

            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Chiều cao bưu gửi không đúng." + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckCOD()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bCOD = false;
                double dTienThuHo = 0;

                if (rows.Cells["colCOD"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colCOD"].Value))
                    {
                        bCOD = true;
                    }
                }

                if (rows.Cells["colAmount"].Value != null && !string.IsNullOrEmpty(rows.Cells["colAmount"].Value.ToString()))
                {
                    double dSoTienCODResult;

                    if (double.TryParse(rows.Cells["colAmount"].Value.ToString(), out dSoTienCODResult))
                    {
                        if (dSoTienCODResult > 0)
                        {
                            dTienThuHo = dSoTienCODResult;
                        }
                    }
                }

                if (bCOD)
                {
                    if (dTienThuHo > 0)
                    {
                    }
                    else
                    {
                        rows.Cells["colAmount"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Yêu cầu nhập số tiền thu hộ khi sử dụng dịch vụ COD:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckContractNumberPPA()
        {
            bool resullt = true;

            string messageError = "";

            CustomerContractDAO daoCustomerContract = new CustomerContractDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bPPA = false;

                string SoHopDongPPA = "";

                if (rows.Cells["colPPA"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colPPA"].Value))
                    {
                        bPPA = true;
                    }
                }

                if (rows.Cells["colContractNumberPPA"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractNumberPPA"].Value.ToString()))
                {
                    SoHopDongPPA = rows.Cells["colContractNumberPPA"].Value.ToString();
                }

                if (bPPA)
                {
                    if (!string.IsNullOrEmpty(SoHopDongPPA))
                    {
                        List<CustomerContractEntity> enCustomerContractList = daoCustomerContract.SelectAllFilter("ContractNumber = N'" + SoHopDongPPA.Replace("'", "''") + "'");

                        if (enCustomerContractList != null && enCustomerContractList.Count > 0)
                        {
                        }
                        else
                        {
                            rows.Cells["colContractNumberPPA"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                    else
                    {
                        rows.Cells["colContractNumberPPA"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Số hợp đồng dịch vụ PPA không đúng:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckContractNumberC()
        {
            bool resullt = true;

            string messageError = "";

            CustomerContractDAO daoCustomerContract = new CustomerContractDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bC = false;

                string SoHopDongC = "";

                if (rows.Cells["colC"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colC"].Value))
                    {
                        bC = true;
                    }
                }

                if (rows.Cells["colContractNumberC"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractNumberC"].Value.ToString()))
                {
                    SoHopDongC = rows.Cells["colContractNumberC"].Value.ToString();
                }

                if (bC)
                {
                    if (!string.IsNullOrEmpty(SoHopDongC))
                    {
                        List<CustomerContractEntity> enCustomerContractList = daoCustomerContract.SelectAllFilter("ContractNumber = N'" + SoHopDongC.Replace("'", "''") + "'");

                        if (enCustomerContractList != null && enCustomerContractList.Count > 0)
                        {
                        }
                        else
                        {
                            rows.Cells["colContractNumberC"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                    else
                    {
                        rows.Cells["colContractNumberC"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Số hợp đồng dịch vụ C không đúng:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckContractNumberT3()
        {
            bool resullt = true;

            string messageError = "";

            CustomerContractDAO daoCustomerContract = new CustomerContractDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bT3 = false;

                string SoHopDongT3 = "";

                if (rows.Cells["colBenThu3"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colBenThu3"].Value))
                    {
                        bT3 = true;
                    }
                }

                if (rows.Cells["colContractNumberT3"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractNumberT3"].Value.ToString()))
                {
                    SoHopDongT3 = rows.Cells["colContractNumberT3"].Value.ToString();
                }

                if (bT3)
                {
                    if (!string.IsNullOrEmpty(SoHopDongT3))
                    {
                        List<CustomerContractEntity> enCustomerContractList = daoCustomerContract.SelectAllFilter("ContractNumber = N'" + SoHopDongT3.Replace("'", "''") + "'");

                        if (enCustomerContractList != null && enCustomerContractList.Count > 0)
                        {
                        }
                        else
                        {
                            rows.Cells["colContractNumberT3"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageError))
                                messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                            else
                                messageError = rows.Cells["colIndex"].Value.ToString();
                        }
                    }
                    else
                    {
                        rows.Cells["colContractNumberT3"].Style.BackColor = Color.Red;

                        if (!string.IsNullOrEmpty(messageError))
                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                        else
                            messageError = rows.Cells["colIndex"].Value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Số hợp đồng dịch vụ thu cước bên thứ 3 không đúng:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckContractDatePPA()
        {
            bool resullt = true;

            string messageError = "";

            CustomerContractDAO daoCustomerContract = new CustomerContractDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bPPA = false;

                string SoHopDongPPA = "";

                string HanHopDongPPA = "";

                if (rows.Cells["colPPA"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colPPA"].Value))
                    {
                        bPPA = true;
                    }
                }

                if (rows.Cells["colContractNumberPPA"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractNumberPPA"].Value.ToString()))
                {
                    SoHopDongPPA = rows.Cells["colContractNumberPPA"].Value.ToString();
                }

                if (rows.Cells["colContractDatePPA"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractDatePPA"].Value.ToString()))
                {
                    HanHopDongPPA = rows.Cells["colContractDatePPA"].Value.ToString();
                }

                if (bPPA)
                {
                    if (!string.IsNullOrEmpty(SoHopDongPPA))
                    {
                        List<CustomerContractEntity> enCustomerContractList = daoCustomerContract.SelectAllFilter("ContractNumber = N'" + SoHopDongPPA.Replace("'", "''") + "'");

                        if (enCustomerContractList != null && enCustomerContractList.Count > 0)
                        {
                            DateTime HanHDPPA;

                            if (DateTime.TryParseExact(HanHopDongPPA, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDPPA))
                            {
                                DateTime dtSendingTime = new DateTime(dtpFromDate.Value.Year, dtpFromDate.Value.Month, dtpFromDate.Value.Day);

                                DateTime dtSignDate = new DateTime(enCustomerContractList[0].SignDate.Year, enCustomerContractList[0].SignDate.Month, enCustomerContractList[0].SignDate.Day);

                                DateTime dtEndDate = new DateTime(HanHDPPA.Year, HanHDPPA.Month, HanHDPPA.Day);

                                if (dtSendingTime > dtEndDate)
                                {
                                    rows.Cells["colContractDatePPA"].Style.BackColor = Color.Red;

                                    if (!string.IsNullOrEmpty(messageError))
                                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                    else
                                        messageError = rows.Cells["colIndex"].Value.ToString();
                                }
                                else
                                {
                                    //Ngày hiệu lực hợp đồng lớn hơn ngày nhận gửi
                                    //if (dtSendingTime < dtSignDate)
                                    //{
                                    //    rows.Cells["colContractDatePPA"].Style.BackColor = Color.Red;

                                    //    if (!string.IsNullOrEmpty(messageError))
                                    //        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                    //    else
                                    //        messageError = rows.Cells["colIndex"].Value.ToString();       
                                    //}
                                    //else
                                    //{
                                    //}
                                }
                            }
                            else
                            {
                                rows.Cells["colContractDatePPA"].Style.BackColor = Color.Red;

                                if (!string.IsNullOrEmpty(messageError))
                                    messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                else
                                    messageError = rows.Cells["colIndex"].Value.ToString();
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Hạn hợp đồng dịch vụ PPA không đúng hoặc đã hết hạn so với ngày nhận gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckContractDateC()
        {
            bool resullt = true;

            string messageError = "";

            CustomerContractDAO daoCustomerContract = new CustomerContractDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bC = false;

                string SoHopDongC = "";

                string HanHopDongC = "";

                if (rows.Cells["colC"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colC"].Value))
                    {
                        bC = true;
                    }
                }

                if (rows.Cells["colContractNumberC"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractNumberC"].Value.ToString()))
                {
                    SoHopDongC = rows.Cells["colContractNumberC"].Value.ToString();
                }

                if (rows.Cells["colContractDateC"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractDateC"].Value.ToString()))
                {
                    HanHopDongC = rows.Cells["colContractDateC"].Value.ToString();
                }

                if (bC)
                {
                    if (!string.IsNullOrEmpty(SoHopDongC))
                    {
                        List<CustomerContractEntity> enCustomerContractList = daoCustomerContract.SelectAllFilter("ContractNumber = N'" + SoHopDongC.Replace("'", "''") + "'");

                        if (enCustomerContractList != null && enCustomerContractList.Count > 0)
                        {
                            DateTime HanHDC;

                            if (DateTime.TryParseExact(HanHopDongC, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDC))
                            {
                                DateTime dtSendingTime = new DateTime(dtpFromDate.Value.Year, dtpFromDate.Value.Month, dtpFromDate.Value.Day);

                                DateTime dtSignDate = new DateTime(enCustomerContractList[0].SignDate.Year, enCustomerContractList[0].SignDate.Month, enCustomerContractList[0].SignDate.Day);

                                DateTime dtEndDate = new DateTime(HanHDC.Year, HanHDC.Month, HanHDC.Day);

                                if (dtSendingTime > dtEndDate)
                                {
                                    rows.Cells["colContractDateC"].Style.BackColor = Color.Red;

                                    if (!string.IsNullOrEmpty(messageError))
                                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                    else
                                        messageError = rows.Cells["colIndex"].Value.ToString();
                                }
                                else
                                {
                                    //Ngày hiệu lực hợp đồng lớn hơn ngày nhận gửi
                                    //if (dtSendingTime < dtSignDate)
                                    //{
                                    //    rows.Cells["colContractDateC"].Style.BackColor = Color.Red;

                                    //    if (!string.IsNullOrEmpty(messageError))
                                    //        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                    //    else
                                    //        messageError = rows.Cells["colIndex"].Value.ToString();       
                                    //}
                                    //else
                                    //{
                                    //}
                                }
                            }
                            else
                            {
                                rows.Cells["colContractDateC"].Style.BackColor = Color.Red;

                                if (!string.IsNullOrEmpty(messageError))
                                    messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                else
                                    messageError = rows.Cells["colIndex"].Value.ToString();
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Hạn hợp đồng dịch vụ C không đúng hoặc đã hết hạn so với ngày nhận gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckContractDateT3()
        {
            bool resullt = true;

            string messageError = "";

            CustomerContractDAO daoCustomerContract = new CustomerContractDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                bool bT3 = false;

                string SoHopDongT3 = "";

                string HanHopDongT3 = "";

                if (rows.Cells["colBenThu3"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colBenThu3"].Value))
                    {
                        bT3 = true;
                    }
                }

                if (rows.Cells["colContractNumberT3"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractNumberT3"].Value.ToString()))
                {
                    SoHopDongT3 = rows.Cells["colContractNumberT3"].Value.ToString();
                }

                if (rows.Cells["colContractDateT3"].Value != null && !string.IsNullOrEmpty(rows.Cells["colContractDateT3"].Value.ToString()))
                {
                    HanHopDongT3 = rows.Cells["colContractDateT3"].Value.ToString();
                }

                if (bT3)
                {
                    if (!string.IsNullOrEmpty(SoHopDongT3))
                    {
                        List<CustomerContractEntity> enCustomerContractList = daoCustomerContract.SelectAllFilter("ContractNumber = N'" + SoHopDongT3.Replace("'", "''") + "'");

                        if (enCustomerContractList != null && enCustomerContractList.Count > 0)
                        {
                            DateTime HanHDT3;

                            if (DateTime.TryParseExact(HanHopDongT3, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDT3))
                            {
                                DateTime dtSendingTime = new DateTime(dtpFromDate.Value.Year, dtpFromDate.Value.Month, dtpFromDate.Value.Day);

                                DateTime dtSignDate = new DateTime(enCustomerContractList[0].SignDate.Year, enCustomerContractList[0].SignDate.Month, enCustomerContractList[0].SignDate.Day);

                                DateTime dtEndDate = new DateTime(HanHDT3.Year, HanHDT3.Month, HanHDT3.Day);

                                if (dtSendingTime > dtEndDate)
                                {
                                    rows.Cells["colContractDateT3"].Style.BackColor = Color.Red;

                                    if (!string.IsNullOrEmpty(messageError))
                                        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                    else
                                        messageError = rows.Cells["colIndex"].Value.ToString();
                                }
                                else
                                {
                                    //Ngày hiệu lực hợp đồng lớn hơn ngày nhận gửi
                                    //if (dtSendingTime < dtSignDate)
                                    //{
                                    //    rows.Cells["colContractDateT3"].Style.BackColor = Color.Red;

                                    //    if (!string.IsNullOrEmpty(messageError))
                                    //        messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                    //    else
                                    //        messageError = rows.Cells["colIndex"].Value.ToString();       
                                    //}
                                    //else
                                    //{
                                    //}
                                }
                            }
                            else
                            {
                                rows.Cells["colContractDateT3"].Style.BackColor = Color.Red;

                                if (!string.IsNullOrEmpty(messageError))
                                    messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                else
                                    messageError = rows.Cells["colIndex"].Value.ToString();
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Hạn hợp đồng dịch vụ thu cước bên thứ 3 không đúng hoặc đã hết hạn so với ngày nhận gửi:" + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckDetailItemNameSymbol()
        {
            bool resullt = true;

            string messageError = "";

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colDetailItem"].Tag != null)
                {
                    List<DetailItemEntity> entityDetailItemListTemp = (List<DetailItemEntity>)rows.Cells["colDetailItem"].Tag;

                    foreach (DetailItemEntity detailItemValue in entityDetailItemListTemp)
                    {
                        if (!string.IsNullOrEmpty(detailItemValue.DetailItemName))
                        {
                            if (detailItemValue.DetailItemName.Contains("|") || detailItemValue.DetailItemName.Contains(";") || detailItemValue.DetailItemName.Contains("@") || detailItemValue.DetailItemName.Contains("#") ||
                                detailItemValue.DetailItemName.Contains("$") || detailItemValue.DetailItemName.Contains("^") || detailItemValue.DetailItemName.Contains("&") || detailItemValue.DetailItemName.Contains("*"))
                            {
                                rows.Cells["colDetailItem"].Style.BackColor = Color.Red;

                                if (!string.IsNullOrEmpty(messageError))
                                    messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                else
                                    messageError = rows.Cells["colIndex"].Value.ToString();
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageError))
            {
                ShowMessageBoxWarning("Tên nội dung hàng gửi không được sử dụng các ký tự đặc biệt | ; @ # $ ^ & * " + "\r\n" + messageError);
                return false;
            }

            return resullt;
        }

        private bool CheckItemCodeOriginalExists()
        {
            bool resullt = true;

            string messageFormatError = "";

            ItemDAO daoItem = new ItemDAO();

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colItemOriginal"].Value != null)
                {
                    if (!string.IsNullOrEmpty(rows.Cells["colItemOriginal"].Value.ToString()))
                    {
                        ItemEntity enItem = daoItem.SelectOne(rows.Cells["colItemOriginal"].Value.ToString());

                        if (enItem != null && !enItem.IsNullItemCode)
                        {
                        }
                        else
                        {
                            rows.Cells["colItemOriginal"].Style.BackColor = Color.Red;

                            if (!string.IsNullOrEmpty(messageFormatError))
                                messageFormatError = messageFormatError + "\r\n" + rows.Cells["colItemOriginal"].Value.ToString();
                            else
                                messageFormatError = rows.Cells["colItemOriginal"].Value.ToString();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(messageFormatError))
            {
                ShowMessageBoxWarning("Số hiệu bưu gửi gốc không tồn tại trong cơ sở dữ liệu" + messageFormatError);
                return false;
            }

            return resullt;
        }

        private bool CheckReceiverPOSCode()
        {
            bool result = true;
            string messageError = "";
            try
            {
                ConfigurationEntity enConfiguration = new ConfigurationDAO().SelectOne("CheckServiceCodeUsePostal");
                ConfigurationEntity enConfigurationWeight = new ConfigurationDAO().SelectOne("CheckWeightUsePostal");
                ConfigurationEntity enConfigurationSize = new ConfigurationDAO().SelectOne("CheckSizeUsePostal");
                GetSortingCode gsc = new GetSortingCode();

                foreach (DataGridViewRow rows in dgvListItems.Rows)
                {
                    var strReceiverPostCode = rows.Cells["colReceiverPOSCode"].Value.ToString();
                    if (strReceiverPostCode.Trim() == null || strReceiverPostCode.Trim() == "")
                    {
                        if (enConfiguration.ConfigValue != null)
                        {
                            if (enConfiguration.ConfigValue.ToString().IndexOf(cboService.SelectedValue.ToString()) >= 0)
                            {

                                if (rows.Cells["colProvinceCode"].Value != null)
                                {
                                    //check service 
                                    bool isSorting = false;
                                    var ds = gsc.GetPrintSortingDirectionByProvinceAndPosCode(rows.Cells["colProvinceCode"].Value.ToString(), this.POSCode);
                                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                    {
                                        isSorting = (bool)ds.Tables[0].Rows[0]["IsSorting"];
                                        if (isSorting)
                                        {
                                            var strWeight = rows.Cells["colWeight"].Value.ToString();
                                            var strLength = rows.Cells["colLength"].Value.ToString();
                                            var strWidth = rows.Cells["colWidth"].Value.ToString();
                                            var strHeight = rows.Cells["colHeight"].Value.ToString();

                                            if (enConfigurationWeight.ConfigValue != null)
                                            {
                                                if (!string.IsNullOrEmpty(strWeight))
                                                {
                                                    double dWeight = double.Parse(strWeight);
                                                    double dWeightConfig = double.Parse(enConfigurationWeight.ConfigValue.ToString());
                                                    if (dWeight > dWeightConfig)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }


                                            if (enConfigurationSize.ConfigValue != null)
                                            {
                                                if (!string.IsNullOrEmpty(strLength))
                                                {
                                                    double dLength = double.Parse(strLength);
                                                    double dSizeConfig = double.Parse(enConfigurationSize.ConfigValue.ToString());
                                                    if (dLength > dSizeConfig)
                                                    {
                                                        continue;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(strWidth))
                                                {
                                                    double dWidth = double.Parse(strWidth);
                                                    double dSizeConfig = double.Parse(enConfigurationSize.ConfigValue.ToString());
                                                    if (dWidth > dSizeConfig)
                                                    {
                                                        continue;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(strHeight))
                                                {
                                                    double dHeight = double.Parse(strHeight);
                                                    double dSizeConfig = double.Parse(enConfigurationSize.ConfigValue.ToString());
                                                    if (dHeight > dSizeConfig)
                                                    {
                                                        continue;
                                                    }
                                                }

                                                rows.Cells["colReceiverPOSCode"].Style.BackColor = Color.Red;
                                                if (!string.IsNullOrEmpty(messageError))
                                                    messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                                else
                                                    messageError = rows.Cells["colIndex"].Value.ToString();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        rows.Cells["colReceiverPOSCode"].Style.BackColor = Color.Red;
                                        if (!string.IsNullOrEmpty(messageError))
                                            messageError = messageError + "\r\n" + rows.Cells["colIndex"].Value.ToString();
                                        else
                                            messageError = rows.Cells["colIndex"].Value.ToString();

                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(messageError))
                {
                    ShowMessageBoxWarning("Mã bưu chính không được để trống " + "\r\n" + messageError);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.Log(ex.Message, ErrorSource + "CheckUsePostal");
                return result;
            }
        }

        private void GetData()
        {
            DateTime datenow = DateTime.Now;
            if (dgvListItems.Rows.Count > 0)
            {
                foreach (DataGridViewRow rows in dgvListItems.Rows)
                {
                    ItemEntity entityItem = new ItemEntity();

                    if (rows.Cells["colBarcode"].Value != null)
                        entityItem.ItemCode = rows.Cells["colBarcode"].Value.ToString().ToUpper();
                    else
                        entityItem.ItemCode = "";

                    entityItem.AcceptancePOSCode = this.POSCode;

                    entityItem.IsCollection = false;

                    entityItem.CustomerAccountNo = "";

                    if (rows.Cells["colCustomerCode"].Value != null)
                        entityItem.CustomerCode = rows.Cells["colCustomerCode"].Value.ToString();
                    else
                        entityItem.CustomerCode = "";

                    if (rows.Cells["colCustomerGroup"].Value != null)
                        entityItem.CustomerGroupCode = rows.Cells["colCustomerGroup"].Value.ToString();
                    else
                        entityItem.CustomerGroupCode = "";

                    if (rows.Cells["colSenderFullName"].Value != null)
                        entityItem.SenderFullname = rows.Cells["colSenderFullName"].Value.ToString();
                    else
                        entityItem.SenderFullname = "";

                    if (rows.Cells["colSenderAddress"].Value != null)
                        entityItem.SenderAddress = rows.Cells["colSenderAddress"].Value.ToString();
                    else
                        entityItem.SenderAddress = "";

                    if (rows.Cells["colSenderTel"].Value != null)
                        entityItem.SenderTel = rows.Cells["colSenderTel"].Value.ToString();
                    else
                        entityItem.SenderTel = "";

                    if (rows.Cells["colSenderEmail"].Value != null)
                        entityItem.SenderEmail = rows.Cells["colSenderEmail"].Value.ToString();
                    else
                        entityItem.SenderEmail = "";

                    if (rows.Cells["colSenderPOSCode"].Value != null)
                        entityItem.SenderAddressCode = rows.Cells["colSenderPOSCode"].Value.ToString();
                    else
                        entityItem.SenderAddressCode = "";

                    if (rows.Cells["colSenderTaxCode"].Value != null)
                        entityItem.SenderTaxCode = rows.Cells["colSenderTaxCode"].Value.ToString();
                    else
                        entityItem.SenderTaxCode = "";

                    if (rows.Cells["colSenderID"].Value != null)
                        entityItem.SenderIdentification = rows.Cells["colSenderID"].Value.ToString();
                    else
                        entityItem.SenderIdentification = "";

                    if (rows.Cells["colReceiverCustomerCode"].Value != null)
                        entityItem.ReceiverCustomerCode = rows.Cells["colReceiverCustomerCode"].Value.ToString();
                    else
                        entityItem.ReceiverCustomerCode = "";

                    if (rows.Cells["colReceiverFullName"].Value != null)
                        entityItem.ReceiverFullname = rows.Cells["colReceiverFullName"].Value.ToString();
                    else
                        entityItem.ReceiverFullname = "";

                    if (rows.Cells["colReceiverAddress"].Value != null)
                        entityItem.ReceiverAddress = rows.Cells["colReceiverAddress"].Value.ToString();
                    else
                        entityItem.ReceiverAddress = "";

                    if (rows.Cells["colReceiverTel"].Value != null)
                        entityItem.ReceiverTel = rows.Cells["colReceiverTel"].Value.ToString();
                    else
                        entityItem.ReceiverTel = "";

                    if (rows.Cells["colReceiverEmail"].Value != null)
                        entityItem.ReceiverEmail = rows.Cells["colReceiverEmail"].Value.ToString();
                    else
                        entityItem.ReceiverEmail = "";

                    if (rows.Cells["colReceiverPOSCode"].Value != null)
                        entityItem.ReceiverAddressCode = rows.Cells["colReceiverPOSCode"].Value.ToString();
                    else
                        entityItem.ReceiverAddressCode = "";

                    if (rows.Cells["colReceiverTaxCode"].Value != null)
                        entityItem.ReceiverTaxCode = rows.Cells["colReceiverTaxCode"].Value.ToString();
                    else
                        entityItem.ReceiverTaxCode = "";

                    if (rows.Cells["colReceiverID"].Value != null)
                        entityItem.ReceiverIdentification = rows.Cells["colReceiverID"].Value.ToString();
                    else
                        entityItem.ReceiverIdentification = "";

                    entityItem.isDomestic = true;

                    if (rows.Cells["colDataCode"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colDataCode"].Value.ToString()))
                        {
                            entityItem.DataCode = rows.Cells["colDataCode"].Value.ToString();
                        }
                    }

                    if (rows.Cells["colCountryCode"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colCountryCode"].Value.ToString()))
                        {
                            entityItem.CountryCode = rows.Cells["colCountryCode"].Value.ToString();

                            entityItem.isDomestic = false;
                        }
                    }

                    if (rows.Cells["colProvinceCode"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                        {
                            entityItem.ProvinceCode = rows.Cells["colProvinceCode"].Value.ToString();

                            entityItem.isDomestic = true;
                        }
                    }

                    if (rows.Cells["colDistrictCode"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colDistrictCode"].Value.ToString()))
                        {
                            entityItem.ReceiverDistrictCode = rows.Cells["colDistrictCode"].Value.ToString();
                        }
                    }

                    if (rows.Cells["colCommuneCode"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colCommuneCode"].Value.ToString()))
                        {
                            entityItem.ReceiverCommuneCode = rows.Cells["colCommuneCode"].Value.ToString();
                        }
                    }

                    if (rows.Cells["colCountryCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colCountryCode"].Value.ToString()))
                    {

                    }
                    else
                    {
                        if (rows.Cells["colDistrictCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colDistrictCode"].Value.ToString()))
                        {
                            entityItem.ReceiverDistrictCode = rows.Cells["colDistrictCode"].Value.ToString();

                            POSDAO daoPOS = new POSDAO();

                            DataTable dtPOSDistrictGD1 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];
                            if (dtPOSDistrictGD1.Rows.Count > 0)
                            {
                                entityItem.POSCode = dtPOSDistrictGD1.Rows[0]["POSCode"].ToString();
                            }
                            else
                            {
                                DataTable dtPOSDistrictGD2 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                                if (dtPOSDistrictGD2.Rows.Count > 0)
                                {
                                    entityItem.POSCode = dtPOSDistrictGD2.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtPOSDistrictGD3 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];
                                    if (dtPOSDistrictGD3.Rows.Count > 0)
                                    {
                                        entityItem.POSCode = dtPOSDistrictGD3.Rows[0]["POSCode"].ToString();
                                    }
                                    else
                                    {
                                        DataTable dtPOSDistrictKT1 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];
                                        if (dtPOSDistrictKT1.Rows.Count > 0)
                                        {
                                            entityItem.POSCode = dtPOSDistrictKT1.Rows[0]["POSCode"].ToString();
                                        }
                                        else
                                        {
                                            DataTable dtPOSDistrictKT2 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];
                                            if (dtPOSDistrictKT2.Rows.Count > 0)
                                            {
                                                entityItem.POSCode = dtPOSDistrictKT2.Rows[0]["POSCode"].ToString();
                                            }
                                            else
                                            {
                                                DataTable dtPOSDistrict = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "'").Tables[0];
                                                if (dtPOSDistrict.Rows.Count > 0)
                                                {
                                                    entityItem.POSCode = dtPOSDistrict.Rows[0]["POSCode"].ToString();
                                                }
                                                else
                                                {
                                                    if (rows.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                                                    {
                                                        DataTable dtProvinceGD1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];
                                                        if (dtProvinceGD1.Rows.Count > 0)
                                                        {
                                                            entityItem.POSCode = dtProvinceGD1.Rows[0]["POSCode"].ToString();
                                                        }
                                                        else
                                                        {
                                                            DataTable dtProvinceGD2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                                                            if (dtProvinceGD2.Rows.Count > 0)
                                                            {
                                                                entityItem.POSCode = dtProvinceGD2.Rows[0]["POSCode"].ToString();
                                                            }
                                                            else
                                                            {
                                                                DataTable dtProvinceGD3 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];
                                                                if (dtProvinceGD3.Rows.Count > 0)
                                                                {
                                                                    entityItem.POSCode = dtProvinceGD3.Rows[0]["POSCode"].ToString();
                                                                }
                                                                else
                                                                {
                                                                    DataTable dtProvinceKT1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];
                                                                    if (dtProvinceKT1.Rows.Count > 0)
                                                                    {
                                                                        entityItem.POSCode = dtProvinceKT1.Rows[0]["POSCode"].ToString();
                                                                    }
                                                                    else
                                                                    {
                                                                        DataTable dtProvinceKT2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];
                                                                        if (dtProvinceKT2.Rows.Count > 0)
                                                                        {
                                                                            entityItem.POSCode = dtProvinceKT2.Rows[0]["POSCode"].ToString();
                                                                        }
                                                                        else
                                                                        {
                                                                            DataTable dtProvince = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "'").Tables[0];
                                                                            if (dtProvince.Rows.Count > 0)
                                                                            {
                                                                                entityItem.POSCode = dtProvince.Rows[0]["POSCode"].ToString();
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (rows.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                            {
                                POSDAO daoPOS = new POSDAO();
                                DataTable dtProvinceGD1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];
                                if (dtProvinceGD1.Rows.Count > 0)
                                {
                                    entityItem.POSCode = dtProvinceGD1.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtProvinceGD2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                                    if (dtProvinceGD2.Rows.Count > 0)
                                    {
                                        entityItem.POSCode = dtProvinceGD2.Rows[0]["POSCode"].ToString();
                                    }
                                    else
                                    {
                                        DataTable dtProvinceGD3 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];
                                        if (dtProvinceGD3.Rows.Count > 0)
                                        {
                                            entityItem.POSCode = dtProvinceGD3.Rows[0]["POSCode"].ToString();
                                        }
                                        else
                                        {
                                            DataTable dtProvinceKT1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];
                                            if (dtProvinceKT1.Rows.Count > 0)
                                            {
                                                entityItem.POSCode = dtProvinceKT1.Rows[0]["POSCode"].ToString();
                                            }
                                            else
                                            {
                                                DataTable dtProvinceKT2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' AND POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];
                                                if (dtProvinceKT2.Rows.Count > 0)
                                                {
                                                    entityItem.POSCode = dtProvinceKT2.Rows[0]["POSCode"].ToString();
                                                }
                                                else
                                                {
                                                    DataTable dtProvince = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "'").Tables[0];
                                                    if (dtProvince.Rows.Count > 0)
                                                    {
                                                        entityItem.POSCode = dtProvince.Rows[0]["POSCode"].ToString();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    entityItem.IsStatePrice = true;

                    entityItem.StatePriceValue = 0;

                    if (rows.Cells["colDetailItem"].Value != null)
                    {
                        entityItem.SendingContent = rows.Cells["colDetailItem"].Value.ToString();
                    }
                    else
                    {
                        entityItem.SendingContent = "";
                    }

                    entityItem.Note = "";

                    if (rows.Cells["colCustomerGroup"].Value != null)
                        entityItem.Note = rows.Cells["colCustomerGroup"].Value.ToString();

                    if (rows.Cells["colItemType"].Value != null)
                    {
                        entityItem.ItemTypeCode = rows.Cells["colItemType"].Value.ToString();
                    }

                    entityItem.SendingTime = datenow;

                    entityItem.IsAirmail = false;

                    if (rows.Cells["colisAir"].Value != null)
                    {
                        entityItem.IsAirmail = Convert.ToBoolean(rows.Cells["colisAir"].Value);
                    }

                    entityItem.Weight = 0;

                    if (rows.Cells["colWeight"].Value != null && rows.Cells["colWeight"].Value.ToString() != "")
                    {
                        entityItem.Weight = Convert.ToDouble(rows.Cells["colWeight"].Value.ToString());
                    }

                    entityItem.IsAffair = false;

                    if (rows.Cells["colAffair"].Value != null)
                    {
                        entityItem.IsAffair = Convert.ToBoolean(rows.Cells["colAffair"].Value);
                    }

                    entityItem.Status = ItemConstance.StatusAccepted;

                    entityItem.EmployeeCode = this.Username;

                    entityItem.SenderJob = "";

                    entityItem.LightItem = 0;

                    entityItem.SectionCode = "IML";

                    entityItem.ReceiverJob = DateTimeServer.Now.ToString("HHmmssddMMyy");

                    entityItem.IsOpened = false;

                    entityItem.CertificateNumber = "";

                    entityItem.LicenseNumber = this.ApplicationVersion;

                    entityItem.InvoiceNumber = "";

                    entityItem.SenderIssueDate = DateTimeServer.Now;

                    entityItem.SenderIssueCountry = null;

                    entityItem.ReceiverIssueDate = DateTimeServer.Now;

                    entityItem.ReceiverIssueCountry = null;

                    entityItem.MainFreight = 0;//
                    entityItem.SubFreight = 0;

                    entityItem.FuelSurchargeFreight = 0;
                    entityItem.AirSurchargeFreight = 0;
                    entityItem.FarRegionFreight = 0;

                    entityItem.VATPercentage = 0;
                    entityItem.VATFreight = 0;

                    entityItem.TotalFreight = 0;
                    entityItem.TotalFreightVAT = 0;
                    entityItem.TotalFreightDiscount = 0;
                    entityItem.TotalFreightDiscountVAT = 0;

                    entityItem.PaymentFreight = 0;
                    entityItem.PaymentFreightVAT = 0;
                    entityItem.PaymentFreightDiscount = 0;
                    entityItem.PaymentFreightDiscountVAT = 0;

                    entityItem.RemainingFreight = 0;
                    entityItem.RemainingFreightVAT = 0;
                    entityItem.RemainingFreightDiscount = 0;
                    entityItem.RemainingFreightDiscountVAT = 0;

                    if (rows.Cells["colMainFreight"].Value != null && rows.Cells["colMainFreight"].Value.ToString() != "")
                    {
                        entityItem.MainFreight = Math.Round(Convert.ToDouble(rows.Cells["colMainFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colSubFreight"].Value != null && rows.Cells["colSubFreight"].Value.ToString() != "")
                    {
                        entityItem.SubFreight = Math.Round(Convert.ToDouble(rows.Cells["colSubFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colFuelSurchargeFreight"].Value != null && rows.Cells["colFuelSurchargeFreight"].Value.ToString() != "")
                    {
                        entityItem.FuelSurchargeFreight = Math.Round(Convert.ToDouble(rows.Cells["colFuelSurchargeFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colAirSurchargeFreight"].Value != null && rows.Cells["colAirSurchargeFreight"].Value.ToString() != "")
                    {
                        entityItem.AirSurchargeFreight = Math.Round(Convert.ToDouble(rows.Cells["colAirSurchargeFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colFarRegionFreight"].Value != null && rows.Cells["colFarRegionFreight"].Value.ToString() != "")
                    {
                        entityItem.FarRegionFreight = Math.Round(Convert.ToDouble(rows.Cells["colFarRegionFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colTotalFreight"].Value != null && rows.Cells["colTotalFreight"].Value.ToString() != "")
                    {
                        entityItem.TotalFreight = Math.Round(Convert.ToDouble(rows.Cells["colTotalFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colTotalFreightVAT"].Value != null && rows.Cells["colTotalFreightVAT"].Value.ToString() != "")
                    {
                        entityItem.TotalFreightVAT = Math.Round(Convert.ToDouble(rows.Cells["colTotalFreightVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colTotalFreightDiscount"].Value != null && rows.Cells["colTotalFreightDiscount"].Value.ToString() != "")
                    {
                        entityItem.TotalFreightDiscount = Math.Round(Convert.ToDouble(rows.Cells["colTotalFreightDiscount"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colTotalFreightDiscountVAT"].Value != null && rows.Cells["colTotalFreightDiscountVAT"].Value.ToString() != "")
                    {
                        entityItem.TotalFreightDiscountVAT = Math.Round(Convert.ToDouble(rows.Cells["colTotalFreightDiscountVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colVATPercentage"].Value != null && rows.Cells["colVATPercentage"].Value.ToString() != "")
                    {
                        entityItem.VATPercentage = Math.Round(Convert.ToDouble(rows.Cells["colVATPercentage"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colVATFreight"].Value != null && rows.Cells["colVATFreight"].Value.ToString() != "")
                    {
                        entityItem.VATFreight = Math.Round(Convert.ToDouble(rows.Cells["colVATFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colPaymentFreight"].Value != null && rows.Cells["colPaymentFreight"].Value.ToString() != "")
                    {
                        entityItem.PaymentFreight = Math.Round(Convert.ToDouble(rows.Cells["colPaymentFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colPaymentFreightVAT"].Value != null && rows.Cells["colPaymentFreightVAT"].Value.ToString() != "")
                    {
                        entityItem.PaymentFreightVAT = Math.Round(Convert.ToDouble(rows.Cells["colPaymentFreightVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colPaymentFreightDiscount"].Value != null && rows.Cells["colPaymentFreightDiscount"].Value.ToString() != "")
                    {
                        entityItem.PaymentFreightDiscount = Math.Round(Convert.ToDouble(rows.Cells["colPaymentFreightDiscount"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colPaymentFreightDiscountVAT"].Value != null && rows.Cells["colPaymentFreightDiscountVAT"].Value.ToString() != "")
                    {
                        entityItem.PaymentFreightDiscountVAT = Math.Round(Convert.ToDouble(rows.Cells["colPaymentFreightDiscountVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colRemainingFreight"].Value != null && rows.Cells["colRemainingFreight"].Value.ToString() != "")
                    {
                        entityItem.RemainingFreight = Math.Round(Convert.ToDouble(rows.Cells["colRemainingFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colRemainingFreightVAT"].Value != null && rows.Cells["colRemainingFreightVAT"].Value.ToString() != "")
                    {
                        entityItem.RemainingFreightVAT = Math.Round(Convert.ToDouble(rows.Cells["colRemainingFreightVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colRemainingFreightDiscount"].Value != null && rows.Cells["colRemainingFreightDiscount"].Value.ToString() != "")
                    {
                        entityItem.RemainingFreightDiscount = Math.Round(Convert.ToDouble(rows.Cells["colRemainingFreightDiscount"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colRemainingFreightDiscountVAT"].Value != null && rows.Cells["colRemainingFreightDiscountVAT"].Value.ToString() != "")
                    {
                        entityItem.RemainingFreightDiscountVAT = Math.Round(Convert.ToDouble(rows.Cells["colRemainingFreightDiscountVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    entityItem.OriginalMainFreight = 0;
                    entityItem.OriginalSubFreight = 0;

                    entityItem.OriginalFuelSurchargeFreight = 0;
                    entityItem.OriginalFarRegionFreight = 0;
                    entityItem.OriginalAirSurchargeFreight = 0;

                    entityItem.OriginalVATFreight = 0;
                    entityItem.OriginalVATPercentage = 0;

                    entityItem.OriginalTotalFreight = 0;
                    entityItem.OriginalTotalFreightVAT = 0;
                    entityItem.OriginalTotalFreightDiscount = 0;
                    entityItem.OriginalTotalFreightDiscountVAT = 0;

                    entityItem.OriginalPaymentFreight = 0;
                    entityItem.OriginalPaymentFreightVAT = 0;
                    entityItem.OriginalPaymentFreightDiscount = 0;
                    entityItem.OriginalPaymentFreightDiscountVAT = 0;

                    entityItem.OriginalRemainingFreight = 0;
                    entityItem.OriginalRemainingFreightVAT = 0;
                    entityItem.OriginalRemainingFreightDiscount = 0;
                    entityItem.OriginalRemainingFreightDiscountVAT = 0;

                    entityItem.OtherFreight = 0;
                    entityItem.OrtherFreight = 0;

                    if (rows.Cells["colOriginalMainFreight"].Value != null && rows.Cells["colOriginalMainFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalMainFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalMainFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalSubFreight"].Value != null && rows.Cells["colOriginalSubFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalSubFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalSubFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalFuelSurchargeFreight"].Value != null && rows.Cells["colOriginalFuelSurchargeFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalFuelSurchargeFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalFuelSurchargeFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalFarRegionFreight"].Value != null && rows.Cells["colOriginalFarRegionFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalFarRegionFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalFarRegionFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalAirSurchargeFreight"].Value != null && rows.Cells["colOriginalAirSurchargeFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalAirSurchargeFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalAirSurchargeFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalVATFreight"].Value != null && rows.Cells["colOriginalVATFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalVATFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalVATFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalVATPercentage"].Value != null && rows.Cells["colOriginalVATPercentage"].Value.ToString() != "")
                    {
                        entityItem.OriginalVATPercentage = Math.Round(Convert.ToDouble(rows.Cells["colOriginalVATPercentage"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalTotalFreight"].Value != null && rows.Cells["colOriginalTotalFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalTotalFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalTotalFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalTotalFreightVAT"].Value != null && rows.Cells["colOriginalTotalFreightVAT"].Value.ToString() != "")
                    {
                        entityItem.OriginalTotalFreightVAT = Math.Round(Convert.ToDouble(rows.Cells["colOriginalTotalFreightVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalTotalFreightDiscount"].Value != null && rows.Cells["colOriginalTotalFreightDiscount"].Value.ToString() != "")
                    {
                        entityItem.OriginalTotalFreightDiscount = Math.Round(Convert.ToDouble(rows.Cells["colOriginalTotalFreightDiscount"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalTotalFreightDiscountVAT"].Value != null && rows.Cells["colOriginalTotalFreightDiscountVAT"].Value.ToString() != "")
                    {
                        entityItem.OriginalTotalFreightDiscountVAT = Math.Round(Convert.ToDouble(rows.Cells["colOriginalTotalFreightDiscountVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalPaymentFreight"].Value != null && rows.Cells["colOriginalPaymentFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalPaymentFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalPaymentFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalPaymentFreightVAT"].Value != null && rows.Cells["colOriginalPaymentFreightVAT"].Value.ToString() != "")
                    {
                        entityItem.OriginalPaymentFreightVAT = Math.Round(Convert.ToDouble(rows.Cells["colOriginalPaymentFreightVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalPaymentFreightDiscount"].Value != null && rows.Cells["colOriginalPaymentFreightDiscount"].Value.ToString() != "")
                    {
                        entityItem.OriginalPaymentFreightDiscount = Math.Round(Convert.ToDouble(rows.Cells["colOriginalPaymentFreightDiscount"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalPaymentFreightDiscountVAT"].Value != null && rows.Cells["colOriginalPaymentFreightDiscountVAT"].Value.ToString() != "")
                    {
                        entityItem.OriginalPaymentFreightDiscountVAT = Math.Round(Convert.ToDouble(rows.Cells["colOriginalPaymentFreightDiscountVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalRemainingFreight"].Value != null && rows.Cells["colOriginalRemainingFreight"].Value.ToString() != "")
                    {
                        entityItem.OriginalRemainingFreight = Math.Round(Convert.ToDouble(rows.Cells["colOriginalRemainingFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalRemainingFreightVAT"].Value != null && rows.Cells["colOriginalRemainingFreightVAT"].Value.ToString() != "")
                    {
                        entityItem.OriginalRemainingFreightVAT = Math.Round(Convert.ToDouble(rows.Cells["colOriginalRemainingFreightVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalRemainingFreightDiscount"].Value != null && rows.Cells["colOriginalRemainingFreightDiscount"].Value.ToString() != "")
                    {
                        entityItem.OriginalRemainingFreightDiscount = Math.Round(Convert.ToDouble(rows.Cells["colOriginalRemainingFreightDiscount"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colOriginalRemainingFreightDiscountVAT"].Value != null && rows.Cells["colOriginalRemainingFreightDiscountVAT"].Value.ToString() != "")
                    {
                        entityItem.OriginalRemainingFreightDiscountVAT = Math.Round(Convert.ToDouble(rows.Cells["colOriginalRemainingFreightDiscountVAT"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colFundFreight"].Value != null && rows.Cells["colFundFreight"].Value.ToString() != "")
                    {
                        entityItem.OrtherFreight = Math.Round(Convert.ToDouble(rows.Cells["colFundFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    if (rows.Cells["colFundVASFreight"].Value != null && rows.Cells["colFundVASFreight"].Value.ToString() != "")
                    {
                        entityItem.OtherFreight = Math.Round(Convert.ToDouble(rows.Cells["colFundVASFreight"].Value.ToString()), MidpointRounding.AwayFromZero);
                    }

                    entityItem.IsPostFree = false;

                    if (rows.Cells["colFreePost"].Value != null)
                    {
                        entityItem.IsPostFree = Convert.ToBoolean(rows.Cells["colFreePost"].Value);
                    }

                    entityItem.StatePriceFreight = 0;

                    entityItem.PrintedNumber = 0;

                    entityItem.SenderCustomReference = "";

                    entityItem.ReceiverCustomReference = "";

                    entityItem.IsReturn = false;

                    entityItem.IsCompensate = false;

                    entityItem.IsForward = false;

                    entityItem.IsAirmailForward = false;

                    entityItem.IsAirmailReturn = false;

                    entityItem.IsDebt = false;

                    if (rows.Cells["colDebt"].Value != null)
                    {
                        entityItem.IsDebt = Convert.ToBoolean(rows.Cells["colDebt"].Value);
                    }

                    entityItem.MachineName = System.Net.Dns.GetHostName();

                    entityItem.AcceptedIndex = rows.Index + 1;

                    entityItem.BC16Index = 0;

                    entityItem.IncomingIndex = 0;

                    if (cboService.SelectedValue != null)
                        entityItem.ServiceCode = cboService.SelectedValue.ToString();

                    entityItem.LetterMoneyOrderFreight = 0;

                    entityItem.ValueAddedServiceFreightTotalFreight = 0;

                    entityItem.OrderCode = null;

                    if (rows.Cells["colReceiverPOSCode"].Value != null)
                        entityItem.ReceiverAddressCode = rows.Cells["colReceiverPOSCode"].Value.ToString();
                    else
                        entityItem.ReceiverAddressCode = "";

                    if (rows.Cells["colReceiverAddress"].Value != null)
                        entityItem.ReceiverAddress = rows.Cells["colReceiverAddress"].Value.ToString();

                    entityItem.SenderFax = "";

                    entityItem.ReceiverMobile = "";

                    if (rows.Cells["colReceiverTel"].Value != null)
                        entityItem.ReceiverMobile = rows.Cells["colReceiverTel"].Value.ToString();

                    entityItem.ReceiverFax = "";

                    entityItem.ReceiverEmail = "";

                    if (rows.Cells["colReceiverEmail"].Value != null)
                        entityItem.ReceiverEmail = rows.Cells["colReceiverEmail"].Value.ToString();

                    entityItem.Discount = 0;

                    entityItem.Abatement = 0;

                    if (rows.Cells["colUndeliveryIndicator"].Value != null && !string.IsNullOrEmpty(rows.Cells["colUndeliveryIndicator"].Value.ToString()))
                    {
                        entityItem.UndeliverableGuide = Convert.ToByte(rows.Cells["colUndeliveryIndicator"].Value.ToString());
                    }

                    if (rows.Cells["colDeliveryNote"].Value != null)
                        entityItem.DeliveryNote = rows.Cells["colDeliveryNote"].Value.ToString();
                    else
                        entityItem.DeliveryNote = "";

                    entityItem.Width = 0;

                    entityItem.Height = 0;

                    entityItem.Length = 0;

                    if (rows.Cells["colWidth"].Value != null && rows.Cells["colWidth"].Value.ToString() != "")
                        entityItem.Width = Math.Round(Convert.ToDouble(rows.Cells["colWidth"].Value.ToString()), MidpointRounding.AwayFromZero);

                    if (rows.Cells["colHeight"].Value != null && rows.Cells["colHeight"].Value.ToString() != "")
                        entityItem.Height = Math.Round(Convert.ToDouble(rows.Cells["colHeight"].Value.ToString()), MidpointRounding.AwayFromZero);

                    if (rows.Cells["colLength"].Value != null && rows.Cells["colLength"].Value.ToString() != "")
                        entityItem.Length = Math.Round(Convert.ToDouble(rows.Cells["colLength"].Value.ToString()), MidpointRounding.AwayFromZero);

                    entityItem.WeightConvert = 0;

                    if (rows.Cells["colConvertWeight"].Value != null && rows.Cells["colConvertWeight"].Value.ToString() != "")
                    {
                        double dConvertResult;

                        if (double.TryParse(rows.Cells["colConvertWeight"].Value.ToString(), out dConvertResult))
                        {
                            entityItem.WeightConvert = dConvertResult;
                        }
                    }

                    if (entityItem.WeightConvert == 0)
                    {
                        entityItem.WeightConvert = entityItem.Weight;
                    }

                    entityItem.CheckSum = "";

                    entityItem.ItemNumber = "";

                    entityItem.ExchangeRateCode = null;

                    entityItem.CODAddress = "";

                    entityItem.CODPayment = false;

                    entityItem.SenderDistrictCode = "";

                    entityItem.AcceptedType = AcceptanceTypeConstance.BUU_GUI_SLL;

                    if (rows.Cells["colReceiverTaxCode"].Value != null)
                        entityItem.ReceiverTaxCode = rows.Cells["colReceiverTaxCode"].Value.ToString();
                    else
                        entityItem.ReceiverTaxCode = "";

                    entityItem.FarRegion = false;

                    if (rows.Cells["colFarRegion"].Value != null)
                    {
                        entityItem.FarRegion = Convert.ToBoolean(rows.Cells["colFarRegion"].Value);
                    }

                    if (rows.Cells["colDestinationPOSCode"].Value != null)
                        entityItem.DestinationPOSCode = rows.Cells["colDestinationPOSCode"].Value.ToString();
                    else
                        entityItem.DestinationPOSCode = "";

                    entityItem.IsEcommerce = false;

                    List<ItemCommodityTypeEntity> entityItemCommodityTypeList = new List<ItemCommodityTypeEntity>();

                    if (rows.Cells["colDestinationPOSCode"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colComodityType"].Value.ToString()))
                        {
                            foreach (string strCommodityType in rows.Cells["colComodityType"].Value.ToString().Split(Convert.ToChar(";")))
                            {
                                ItemCommodityTypeEntity entityItemCommodityType = new ItemCommodityTypeEntity();
                                entityItemCommodityType.ItemCode = entityItem.ItemCode;
                                entityItemCommodityType.CommodityTypeCode = strCommodityType;
                                entityItemCommodityTypeList.Add(entityItemCommodityType);
                            }
                        }
                    }

                    List<DetailItemEntity> entityDetailItemList = new List<DetailItemEntity>();

                    if (rows.Cells["colDetailItem"].Tag != null)
                    {
                        entityDetailItemList = (List<DetailItemEntity>)rows.Cells["colDetailItem"].Tag;

                        foreach (DetailItemEntity detailItem in entityDetailItemList)
                        {
                            detailItem.ItemCode = entityItem.ItemCode;
                        }
                    }

                    if (rows.Cells["colReceiverContact"].Value != null)
                        entityItem.ReceiverContact = rows.Cells["colReceiverContact"].Value.ToString();
                    else
                        entityItem.ReceiverContact = "";

                    if (rows.Cells["colExecuteOrder"].Value != null)
                        entityItem.ExecuteOrder = rows.Cells["colExecuteOrder"].Value.ToString();
                    else
                        entityItem.ExecuteOrder = "";

                    if (rows.Cells["colInvoice"].Value != null)
                    {
                        entityItem.InvoiceAttached = Convert.ToBoolean(rows.Cells["colInvoice"].Value);
                    }
                    else
                    {
                        entityItem.InvoiceAttached = false;
                    }

                    if (rows.Cells["colOther"].Value != null)
                    {
                        entityItem.OtherAttached = Convert.ToBoolean(rows.Cells["colOther"].Value);
                    }
                    else
                    {
                        entityItem.OtherAttached = false;
                    }

                    if (rows.Cells["colOtherInfo"].Value != null)
                        entityItem.OtherAttachedInfor = rows.Cells["colOtherInfo"].Value.ToString();
                    else
                        entityItem.OtherAttachedInfor = "";


                    if (rows.Cells["colItemOriginal"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(rows.Cells["colItemOriginal"].Value.ToString()))
                        {
                            ItemAdviceOfReceiptEntity enItemAdviceOfReceipt = new ItemAdviceOfReceiptEntity();

                            enItemAdviceOfReceipt.ItemCode = rows.Cells["colItemOriginal"].Value.ToString();

                            enItemAdviceOfReceipt.AdviceOfReceiptCode = entityItem.ItemCode;

                            enItemAdviceOfReceipt.TransferStatus = false;

                            if (entityItemAdviceOfReceiptListGlobal == null)
                            {
                                entityItemAdviceOfReceiptListGlobal = new List<ItemAdviceOfReceiptEntity>();
                            }

                            entityItemAdviceOfReceiptListGlobal.Add(enItemAdviceOfReceipt);
                        }
                    }
                    List<SortingItemEntity> entitySortingItemEntityList = new List<SortingItemEntity>();

                    if (rows.Cells["colReceiverPOSCode"].Value != null && rows.Cells["colBusinessId"].Value != null && rows.Cells["colBusinessId"].Value.ToString() != "1")
                    {
                        GetSortingCode oSc = new GetSortingCode();
                        string strSortingCode = string.Empty;
                        string ProvinceCode = string.Empty;
                        string DistrictCode = string.Empty;
                        string CommuneCode = string.Empty;

                        if (rows.Cells["colProvinceCode"].Value != null)
                            ProvinceCode = rows.Cells["colProvinceCode"].Value.ToString();
                        if (rows.Cells["colDistrictCode"].Value != null)
                            DistrictCode = rows.Cells["colDistrictCode"].Value.ToString();
                        if (rows.Cells["colCommuneCode"].Value != null)
                            CommuneCode = rows.Cells["colCommuneCode"].Value.ToString();

                        DataSet ds = oSc.SelectSortingCodeByAll(ProvinceCode, DistrictCode, CommuneCode);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            strSortingCode = ds.Tables[0].Rows[0]["SortingCode"].ToString();
                        }
                        SortingItemEntity enSortingItem = new SortingItemEntity();
                        enSortingItem.POSCode = this.POSCode;
                        enSortingItem.SortingCode = strSortingCode;
                        enSortingItem.ItemCode = entityItem.ItemCode;
                        enSortingItem.Type = 1;
                        enSortingItem.CreateTime = (System.DateTime)DateTime.Now;
                        enSortingItem.LastUpdatedTime = (System.DateTime)DateTime.Now;

                        entitySortingItemEntityList.Add(enSortingItem);
                    }

                    List<ValueAddedServiceItemEntity> entityVASIList = new List<ValueAddedServiceItemEntity>();

                    List<ItemVASPropertyValueEntity> entityIVASPropertyList = new List<ItemVASPropertyValueEntity>();

                    List<TransactionsCollectionEntity> entityTransactionsCollectionList = new List<TransactionsCollectionEntity>();

                    List<TransactionsCollectionDetailEntity> entityTransactionsCollectionDetailList = new List<TransactionsCollectionDetailEntity>();

                    List<KT1ExpectedTimeEntity> entityKT1ExpectedTimeEntityList = new List<KT1ExpectedTimeEntity>();

                    List<AttachDocumentsItemEntity> entityAttachDocumentsItemList = new List<AttachDocumentsItemEntity>();

                    if (rows.Cells["colValueAddedService"].Tag != null)
                    {
                        entityVASIList = (List<ValueAddedServiceItemEntity>)rows.Cells["colValueAddedService"].Tag;

                        bool isCOD = false;

                        double codAmount = 0;

                        if (rows.Cells["colVASPropertyValue"].Tag != null)
                        {
                            entityIVASPropertyList = (List<ItemVASPropertyValueEntity>)rows.Cells["colVASPropertyValue"].Tag;

                            foreach (ItemVASPropertyValueEntity enIVASProperty in entityIVASPropertyList)
                            {
                                enIVASProperty.ItemCode = entityItem.ItemCode;

                                if (enIVASProperty.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_HANG_THU_TIEN) && enIVASProperty.PropertyCode.Equals("Amount"))
                                {
                                    double amountResult;

                                    if (double.TryParse(enIVASProperty.Value, out amountResult))
                                    {
                                        codAmount = amountResult;
                                    }
                                }
                            }
                        }

                        if (entityVASIList != null && entityVASIList.Count > 0)
                        {
                            foreach (ValueAddedServiceItemEntity value in entityVASIList)
                            {
                                value.ItemCode = entityItem.ItemCode;

                                if (!string.IsNullOrEmpty(entityItem.Note))
                                {
                                    entityItem.Note = entityItem.Note + ";" + value.ValueAddedServiceCode;
                                }
                                else
                                {
                                    entityItem.Note = value.ValueAddedServiceCode;
                                }

                                if (value.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_HANG_THU_TIEN))
                                {
                                    isCOD = true;
                                }
                            }
                        }

                        if (isCOD)
                        {
                            TransactionsCollectionEntity enTransactionsCollection = new TransactionsCollectionEntity();
                            enTransactionsCollection.TransactionsCollectionCode = this.POSCode + entityItem.ItemCode + DateTimeServer.Now.ToString("yyyyMMddHHmmssfff");
                            enTransactionsCollection.ItemCode = entityItem.ItemCode;
                            enTransactionsCollection.TransactionsCollectionDate = DateTimeServer.Now;
                            enTransactionsCollection.POSCode = this.POSCode;
                            enTransactionsCollection.TransactionsCollectionChannel = "BCCP";
                            enTransactionsCollection.CODAmount = codAmount;
                            enTransactionsCollection.CODPostage = entityItem.RemainingFreightDiscountVAT;
                            enTransactionsCollection.Status = 0;

                            enTransactionsCollection.ReceiverCustomerCode = entityItem.CustomerCode;
                            enTransactionsCollection.ReceiverFullName = entityItem.SenderFullname;
                            enTransactionsCollection.ReceiverAddress = entityItem.SenderAddress;
                            enTransactionsCollection.ReceiverTel = entityItem.SenderTel;
                            enTransactionsCollection.ReceiverCertificateType = 0;
                            enTransactionsCollection.ReceiverCertificateNumber = entityItem.SenderIdentification;
                            //enTransactionsCollection.ReceiverCertificateIssueDate = ;
                            //enTransactionsCollection.ReceiverCertificatePlace = ;
                            //enTransactionsCollection.ReceiverCertificateOtherName = ;

                            TransactionsCollectionDetailEntity entityTransactionsCollectionDetail = new TransactionsCollectionDetailEntity();
                            entityTransactionsCollectionDetail.TransactionsCollectionIndex = 1;
                            entityTransactionsCollectionDetail.TransactionsCollectionCode = enTransactionsCollection.TransactionsCollectionCode;
                            entityTransactionsCollectionDetail.ItemCode = enTransactionsCollection.ItemCode;
                            entityTransactionsCollectionDetail.TransactionsCollectionType = 0;
                            entityTransactionsCollectionDetail.CODAmount = codAmount;
                            entityTransactionsCollectionDetail.CODPostage = entityItem.RemainingFreightDiscountVAT;
                            entityTransactionsCollectionDetail.CODPostagePerson = 0;

                            entityTransactionsCollectionList.Add(enTransactionsCollection);

                            entityTransactionsCollectionDetailList.Add(entityTransactionsCollectionDetail);
                        }
                    }
                    else
                    {
                        if (rows.Cells["colCOD"].Value != null && Convert.ToBoolean(rows.Cells["colCOD"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            }

                            Hashtable htCOD = new Hashtable();

                            if (rows.Cells["colCOD"].Tag != null)
                            {
                                try
                                {
                                    htCOD = (Hashtable)rows.Cells["colCOD"].Tag;
                                }
                                catch (Exception x) { }
                            }

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htCOD.ContainsKey("Freight"))
                            {
                                double dFreightResult;
                                if (double.TryParse(htCOD["Freight"].ToString(), out dFreightResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dFreightResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htCOD.ContainsKey("FreightVAT"))
                            {
                                double dFreightResultVAT;
                                if (double.TryParse(htCOD["FreightVAT"].ToString(), out dFreightResultVAT))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dFreightResultVAT, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htCOD.ContainsKey("OriginalFreight"))
                            {
                                double dResultOriginalFreight;
                                if (double.TryParse(htCOD["OriginalFreight"].ToString(), out dResultOriginalFreight))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResultOriginalFreight, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htCOD.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htCOD["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htCOD.ContainsKey("SubFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htCOD["SubFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.SubFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htCOD.ContainsKey("SubFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htCOD["SubFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.SubFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            //if (htCOD.ContainsKey("OriginalSubFreight"))
                            //{
                            //    double dResult;
                            //    if (double.TryParse(htCOD["SubFreight"].ToString(), out dResult))
                            //    {
                            //        enValueAddedServiceItem.OriginalSubFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                            //    }
                            //}

                            //if (htCOD.ContainsKey("OriginalSubFreightVAT"))
                            //{
                            //    double dResult;
                            //    if (double.TryParse(htCOD["SubFreightVAT"].ToString(), out dResult))
                            //    {
                            //        enValueAddedServiceItem.OriginalSubFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                            //    }
                            //}

                            entityVASIList.Add(enValueAddedServiceItem);

                            double dSoTienCOD = 0;

                            bool bNguoiGuiCP = true;
                            bool bNguoiGuiTH = true;

                            bool bTienMat = true;
                            bool bTraTaiBC = true;
                            bool bTraTaiDC = false;

                            bool bChuyenKhoan = false;
                            string sSoTaiKhoan = "";
                            string sNganHang = "";
                            string sChiNhanh = "";
                            double dPhiChuyenKhoan = 0;

                            if (rows.Cells["colAmount"].Value != null && !string.IsNullOrEmpty(rows.Cells["colAmount"].Value.ToString()))
                            {
                                double dSoTienCODResult;
                                if (double.TryParse(rows.Cells["colAmount"].Value.ToString(), out dSoTienCODResult))
                                {
                                    if (dSoTienCODResult > 0)
                                    {
                                        dSoTienCOD = dSoTienCODResult;
                                    }
                                }
                            }

                            if (rows.Cells["colSenderPostage"].Value != null)
                            {
                                if (Convert.ToBoolean(rows.Cells["colSenderPostage"].Value.ToString()))
                                { }
                                else
                                {
                                    bNguoiGuiCP = false;
                                }
                            }

                            if (rows.Cells["colSenderCODPostage"].Value != null)
                            {
                                if (Convert.ToBoolean(rows.Cells["colSenderCODPostage"].Value.ToString()))
                                { }
                                else
                                {
                                    bNguoiGuiTH = false;
                                }
                            }

                            if (rows.Cells["colCash"].Value != null)
                            {
                                if (Convert.ToBoolean(rows.Cells["colCash"].Value.ToString()))
                                { }
                                else
                                {
                                    bTienMat = false;
                                }
                            }

                            if (rows.Cells["colPayPOS"].Value != null)
                            {
                                if (Convert.ToBoolean(rows.Cells["colPayPOS"].Value.ToString()))
                                { }
                                else
                                {
                                    bTraTaiBC = false;
                                }
                            }

                            if (rows.Cells["colAccount"].Value != null)
                            {
                                sSoTaiKhoan = rows.Cells["colAccount"].Value.ToString();
                            }

                            if (rows.Cells["colBank"].Value != null)
                            {
                                sNganHang = rows.Cells["colBank"].Value.ToString();
                            }

                            if (rows.Cells["colBranch"].Value != null)
                            {
                                sChiNhanh = rows.Cells["colBranch"].Value.ToString();
                            }

                            //if (rows.Cells["colChargeTransfer"].Value != null && !string.IsNullOrEmpty(rows.Cells["colChargeTransfer"].Value.ToString()))
                            //{
                            //    double dPhiChuyenKhoanResult;
                            //    if (double.TryParse(rows.Cells["colChargeTransfer"].Value.ToString(), out dPhiChuyenKhoanResult))
                            //    {
                            //        if (dPhiChuyenKhoanResult > 0)
                            //        {
                            //            dPhiChuyenKhoan = dPhiChuyenKhoanResult;
                            //        }
                            //    }
                            //}

                            if (bTienMat)
                            {
                                if (bTraTaiBC)
                                    bTraTaiDC = false;
                                else
                                    bTraTaiDC = true;

                                bChuyenKhoan = false;

                                sSoTaiKhoan = "";
                                sNganHang = "";
                                sChiNhanh = "";
                                dPhiChuyenKhoan = 0;
                            }
                            else
                            {
                                bTienMat = false;
                                bTraTaiBC = false;
                                bTraTaiDC = false;

                                bChuyenKhoan = true;
                            }

                            ItemVASPropertyValueEntity enAmountForBatch = new ItemVASPropertyValueEntity();
                            enAmountForBatch.ItemCode = entityItem.ItemCode;
                            enAmountForBatch.PropertyCode = "AmountForBatch";
                            enAmountForBatch.Value = "True";
                            enAmountForBatch.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enAmountForBatch);

                            ItemVASPropertyValueEntity enAmount = new ItemVASPropertyValueEntity();
                            enAmount.ItemCode = entityItem.ItemCode;
                            enAmount.PropertyCode = "Amount";
                            enAmount.Value = dSoTienCOD.ToString();
                            enAmount.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enAmount);

                            ItemVASPropertyValueEntity enTransfer = new ItemVASPropertyValueEntity();
                            enTransfer.ItemCode = entityItem.ItemCode;
                            enTransfer.PropertyCode = "Transfer";
                            enTransfer.Value = bChuyenKhoan.ToString();
                            enTransfer.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enTransfer);

                            ItemVASPropertyValueEntity enAccount = new ItemVASPropertyValueEntity();
                            enAccount.ItemCode = entityItem.ItemCode;
                            enAccount.PropertyCode = "Account";
                            enAccount.Value = sSoTaiKhoan;
                            enAccount.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enAccount);

                            ItemVASPropertyValueEntity enBank = new ItemVASPropertyValueEntity();
                            enBank.ItemCode = entityItem.ItemCode;
                            enBank.PropertyCode = "Bank";
                            enBank.Value = sNganHang;
                            enBank.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enBank);

                            ItemVASPropertyValueEntity enBranch = new ItemVASPropertyValueEntity();
                            enBranch.ItemCode = entityItem.ItemCode;
                            enBranch.PropertyCode = "Branch";
                            enBranch.Value = sChiNhanh;
                            enBranch.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enBranch);

                            ItemVASPropertyValueEntity enChargeTransfer = new ItemVASPropertyValueEntity();
                            enChargeTransfer.ItemCode = entityItem.ItemCode;
                            enChargeTransfer.PropertyCode = "ChargeTransfer";
                            enChargeTransfer.Value = dPhiChuyenKhoan.ToString();
                            enChargeTransfer.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enChargeTransfer);

                            ItemVASPropertyValueEntity enCash = new ItemVASPropertyValueEntity();
                            enCash.ItemCode = entityItem.ItemCode;
                            enCash.PropertyCode = "Cash";
                            enCash.Value = bTienMat.ToString();
                            enCash.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enCash);

                            ItemVASPropertyValueEntity enPayPOS = new ItemVASPropertyValueEntity();
                            enPayPOS.ItemCode = entityItem.ItemCode;
                            enPayPOS.PropertyCode = "PayPOS";
                            enPayPOS.Value = bTraTaiBC.ToString();
                            enPayPOS.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enPayPOS);

                            ItemVASPropertyValueEntity enPayAddress = new ItemVASPropertyValueEntity();
                            enPayAddress.ItemCode = entityItem.ItemCode;
                            enPayAddress.PropertyCode = "PayAddress";
                            enPayAddress.Value = bTraTaiDC.ToString();
                            enPayAddress.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enPayAddress);

                            ItemVASPropertyValueEntity enSenderPostage = new ItemVASPropertyValueEntity();
                            enSenderPostage.ItemCode = entityItem.ItemCode;
                            enSenderPostage.PropertyCode = "SenderPostage";
                            enSenderPostage.Value = bNguoiGuiCP.ToString();
                            enSenderPostage.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enSenderPostage);

                            ItemVASPropertyValueEntity enSenderCODPostage = new ItemVASPropertyValueEntity();
                            enSenderCODPostage.ItemCode = entityItem.ItemCode;
                            enSenderCODPostage.PropertyCode = "SenderCODPostage";
                            enSenderCODPostage.Value = bNguoiGuiTH.ToString();
                            enSenderCODPostage.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_HANG_THU_TIEN;
                            entityIVASPropertyList.Add(enSenderCODPostage);

                            TransactionsCollectionEntity enTransactionsCollection = new TransactionsCollectionEntity();
                            enTransactionsCollection.TransactionsCollectionCode = this.POSCode + entityItem.ItemCode + DateTimeServer.Now.ToString("yyyyMMddHHmmssfff");
                            enTransactionsCollection.ItemCode = entityItem.ItemCode;
                            enTransactionsCollection.TransactionsCollectionDate = DateTimeServer.Now;
                            enTransactionsCollection.POSCode = this.POSCode;
                            enTransactionsCollection.TransactionsCollectionChannel = "BCCP";
                            enTransactionsCollection.CODAmount = dSoTienCOD;
                            enTransactionsCollection.CODPostage = entityItem.RemainingFreightDiscountVAT;
                            enTransactionsCollection.Status = 0;

                            enTransactionsCollection.ReceiverCustomerCode = entityItem.CustomerCode;
                            enTransactionsCollection.ReceiverFullName = entityItem.SenderFullname;
                            enTransactionsCollection.ReceiverAddress = entityItem.SenderAddress;
                            enTransactionsCollection.ReceiverTel = entityItem.SenderTel;
                            enTransactionsCollection.ReceiverCertificateType = 0;
                            enTransactionsCollection.ReceiverCertificateNumber = entityItem.SenderIdentification;
                            //enTransactionsCollection.ReceiverCertificateIssueDate = ;
                            //enTransactionsCollection.ReceiverCertificatePlace = ;
                            //enTransactionsCollection.ReceiverCertificateOtherName = ;

                            TransactionsCollectionDetailEntity entityTransactionsCollectionDetail = new TransactionsCollectionDetailEntity();
                            entityTransactionsCollectionDetail.TransactionsCollectionIndex = 1;
                            entityTransactionsCollectionDetail.TransactionsCollectionCode = enTransactionsCollection.TransactionsCollectionCode;
                            entityTransactionsCollectionDetail.ItemCode = enTransactionsCollection.ItemCode;
                            entityTransactionsCollectionDetail.TransactionsCollectionType = 0;
                            entityTransactionsCollectionDetail.CODAmount = dSoTienCOD;
                            entityTransactionsCollectionDetail.CODPostage = entityItem.RemainingFreightDiscountVAT;
                            entityTransactionsCollectionDetail.CODPostagePerson = 0;

                            entityTransactionsCollectionList.Add(enTransactionsCollection);

                            entityTransactionsCollectionDetailList.Add(entityTransactionsCollectionDetail);
                        }

                        if (rows.Cells["colPDK"].Value != null && Convert.ToBoolean(rows.Cells["colPDK"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            }

                            Hashtable htPDK = new Hashtable();
                            if (rows.Cells["colPDK"].Tag != null)
                                htPDK = (Hashtable)rows.Cells["colPDK"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htPDK.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htPDK["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPDK.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htPDK["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPDK.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htPDK["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPDK.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htPDK["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                            ItemVASPropertyValueEntity enPDKPostForm = new ItemVASPropertyValueEntity();
                            enPDKPostForm.ItemCode = entityItem.ItemCode;
                            enPDKPostForm.PropertyCode = "PDKPostForm";
                            enPDKPostForm.Value = "True";
                            enPDKPostForm.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enPDKPostForm);

                            ItemVASPropertyValueEntity enCheckNumber = new ItemVASPropertyValueEntity();
                            enCheckNumber.ItemCode = entityItem.ItemCode;
                            enCheckNumber.PropertyCode = "CheckNumber";
                            enCheckNumber.Value = "True";
                            enCheckNumber.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enCheckNumber);

                            ItemVASPropertyValueEntity enSenderNumber = new ItemVASPropertyValueEntity();
                            enSenderNumber.ItemCode = entityItem.ItemCode;
                            enSenderNumber.PropertyCode = "SenderNumber";
                            enSenderNumber.Value = "1";
                            enSenderNumber.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enSenderNumber);

                            ItemVASPropertyValueEntity enReceiverNumber = new ItemVASPropertyValueEntity();
                            enReceiverNumber.ItemCode = entityItem.ItemCode;
                            enReceiverNumber.PropertyCode = "ReceiverNumber";
                            enReceiverNumber.Value = "1";
                            enReceiverNumber.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enReceiverNumber);

                            ItemVASPropertyValueEntity enAcceptancePOSNumber = new ItemVASPropertyValueEntity();
                            enAcceptancePOSNumber.ItemCode = entityItem.ItemCode;
                            enAcceptancePOSNumber.PropertyCode = "AcceptancePOSNumber";
                            enAcceptancePOSNumber.Value = "1";
                            enAcceptancePOSNumber.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enAcceptancePOSNumber);

                            ItemVASPropertyValueEntity enDeliveryPOSNumber = new ItemVASPropertyValueEntity();
                            enDeliveryPOSNumber.ItemCode = entityItem.ItemCode;
                            enDeliveryPOSNumber.PropertyCode = "DeliveryPOSNumber";
                            enDeliveryPOSNumber.Value = "1";
                            enDeliveryPOSNumber.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enDeliveryPOSNumber);

                            ItemVASPropertyValueEntity enConfirm = new ItemVASPropertyValueEntity();
                            enConfirm.ItemCode = entityItem.ItemCode;
                            enConfirm.PropertyCode = "Confirm";
                            enConfirm.Value = "False";
                            enConfirm.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enConfirm);

                            ItemVASPropertyValueEntity enContent = new ItemVASPropertyValueEntity();
                            enContent.ItemCode = entityItem.ItemCode;
                            enContent.PropertyCode = "Content";
                            enContent.Value = "";
                            enContent.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_DONG_KIEM;
                            entityIVASPropertyList.Add(enContent);
                        }

                        if (rows.Cells["colAR"].Value != null && Convert.ToBoolean(rows.Cells["colAR"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.BAO_PHAT;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.BAO_PHAT;
                            }

                            Hashtable htAR = new Hashtable();
                            if (rows.Cells["colAR"].Tag != null)
                                htAR = (Hashtable)rows.Cells["colAR"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.BAO_PHAT;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htAR.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htAR["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htAR.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htAR["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htAR.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htAR["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htAR.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htAR["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                            ItemVASPropertyValueEntity enItemVASPropertyValue = new ItemVASPropertyValueEntity();

                            enItemVASPropertyValue.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyValue.PropertyCode = "ARPostForm";
                            enItemVASPropertyValue.Value = "True";
                            enItemVASPropertyValue.ValueAddedServiceCode = ValueAddedServiceConstance.BAO_PHAT;
                        }

                        if (rows.Cells["colAREmail"].Value != null && Convert.ToBoolean(rows.Cells["colAREmail"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.BAO_PHAT_EMAIL;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.BAO_PHAT_EMAIL;
                            }

                            Hashtable htAREmail = new Hashtable();
                            if (rows.Cells["colAREmail"].Tag != null)
                                htAREmail = (Hashtable)rows.Cells["colAREmail"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.BAO_PHAT_EMAIL;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htAREmail.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htAREmail["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htAREmail.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htAREmail["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htAREmail.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htAREmail["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htAREmail.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htAREmail["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                        }

                        if (rows.Cells["colARSMS"].Value != null && Convert.ToBoolean(rows.Cells["colARSMS"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.BAO_PHAT_SMS;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.BAO_PHAT_SMS;
                            }

                            Hashtable htARSMS = new Hashtable();
                            if (rows.Cells["colARSMS"].Tag != null)
                                htARSMS = (Hashtable)rows.Cells["colARSMS"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.BAO_PHAT_SMS;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htARSMS.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htARSMS["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htARSMS.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htARSMS["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htARSMS.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htARSMS["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htARSMS.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htARSMS["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                        }

                        if (rows.Cells["colPTT"].Value != null && Convert.ToBoolean(rows.Cells["colPTT"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.PHAT_TAN_TAY;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.PHAT_TAN_TAY;
                            }

                            Hashtable htPTT = new Hashtable();
                            if (rows.Cells["colPTT"].Tag != null)
                                htPTT = (Hashtable)rows.Cells["colPTT"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.PHAT_TAN_TAY;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htPTT.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htPTT["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPTT.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htPTT["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPTT.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htPTT["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPTT.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htPTT["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colVUN"].Value != null && Convert.ToBoolean(rows.Cells["colVUN"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.HANG_NHAY_CAM_VUN;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.HANG_NHAY_CAM_VUN;
                            }

                            Hashtable htVUN = new Hashtable();
                            if (rows.Cells["colVUN"].Tag != null)
                                htVUN = (Hashtable)rows.Cells["colVUN"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.HANG_NHAY_CAM_VUN;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htVUN.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htVUN["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htVUN.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htVUN["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htVUN.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htVUN["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htVUN.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htVUN["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        /*================================ Added by Quangnd ================================*/

                        if (rows.Cells["colKA"].Value != null && Convert.ToBoolean(rows.Cells["colKA"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.TUYET_MAT;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.TUYET_MAT;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colKA"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colKA"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.TUYET_MAT, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colKB"].Value != null && Convert.ToBoolean(rows.Cells["colKB"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.TOI_MAT;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.TOI_MAT;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colKB"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colKB"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.TOI_MAT, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colKC"].Value != null && Convert.ToBoolean(rows.Cells["colKC"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.MAT;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.MAT;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colKC"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colKC"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.MAT, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colHGN"].Value != null && Convert.ToBoolean(rows.Cells["colHGN"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.HEN_GIO_NOI_TINH;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.HEN_GIO_NOI_TINH;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colHGN"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colHGN"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.HEN_GIO_NOI_TINH, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colHGL"].Value != null && Convert.ToBoolean(rows.Cells["colHGL"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.HEN_GIO_LIEN_TINH;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.HEN_GIO_LIEN_TINH;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colHGL"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colHGL"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.HEN_GIO_LIEN_TINH, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colHTN"].Value != null && Convert.ToBoolean(rows.Cells["colHTN"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.HOA_TOC_NOI_TINH;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.HOA_TOC_NOI_TINH;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colHTN"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colHTN"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.HOA_TOC_NOI_TINH, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        if (rows.Cells["colHTL"].Value != null && Convert.ToBoolean(rows.Cells["colHTL"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.HOA_TOC_LIEN_TINH;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.HOA_TOC_LIEN_TINH;
                            }

                            Hashtable htVAS = new Hashtable();
                            if (rows.Cells["colHTL"].Tag != null)
                                htVAS = (Hashtable)rows.Cells["colHTL"].Tag;

                            var enValueAddedServiceItem = CreateValueAddedServiceItem(entityItem.ServiceCode, ValueAddedServiceConstance.HOA_TOC_LIEN_TINH, entityItem.ItemCode, entityItem.SendingTime, entityItem.AcceptancePOSCode, htVAS);

                            entityVASIList.Add(enValueAddedServiceItem);
                        }

                        /*======================== END Quangnd =========================*/

                        if (rows.Cells["colV"].Value != null && Convert.ToBoolean(rows.Cells["colV"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.KHAI_GIA;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.KHAI_GIA;
                            }

                            Hashtable htV = new Hashtable();
                            if (rows.Cells["colV"].Tag != null)
                                htV = (Hashtable)rows.Cells["colV"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.KHAI_GIA;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htV.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htV["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htV.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htV["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htV.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htV["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htV.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htV["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                            double dGiaTriKhaiGia = 0;

                            bool bUyQuyenChoNguoiNhan = false;

                            if (rows.Cells["colGiaTriKhaiGia"].Value != null && !string.IsNullOrEmpty(rows.Cells["colGiaTriKhaiGia"].Value.ToString()))
                            {
                                double dGiaTriKhaiGiaResult;
                                if (double.TryParse(rows.Cells["colGiaTriKhaiGia"].Value.ToString(), out dGiaTriKhaiGiaResult))
                                {
                                    if (dGiaTriKhaiGiaResult > 0)
                                    {
                                        dGiaTriKhaiGia = dGiaTriKhaiGiaResult;
                                    }
                                }
                            }

                            if (rows.Cells["colAuthorReceiver"].Value != null)
                            {
                                if (Convert.ToBoolean(rows.Cells["colAuthorReceiver"].Value.ToString()))
                                {
                                    bUyQuyenChoNguoiNhan = true;
                                }
                                else
                                {
                                    bUyQuyenChoNguoiNhan = false;
                                }
                            }

                            ItemVASPropertyValueEntity enItemVASPropertyDeclaredValue = new ItemVASPropertyValueEntity();

                            enItemVASPropertyDeclaredValue.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyDeclaredValue.PropertyCode = "DeclaredValue";
                            enItemVASPropertyDeclaredValue.Value = dGiaTriKhaiGia.ToString().Replace(",", "").Replace(".", "");
                            enItemVASPropertyDeclaredValue.ValueAddedServiceCode = ValueAddedServiceConstance.KHAI_GIA;
                            entityIVASPropertyList.Add(enItemVASPropertyDeclaredValue);

                            ItemVASPropertyValueEntity enItemVASPropertyAuthorReceiver = new ItemVASPropertyValueEntity();

                            enItemVASPropertyAuthorReceiver.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyAuthorReceiver.PropertyCode = "AuthorReceiver";
                            enItemVASPropertyAuthorReceiver.Value = bUyQuyenChoNguoiNhan.ToString();
                            enItemVASPropertyAuthorReceiver.ValueAddedServiceCode = ValueAddedServiceConstance.KHAI_GIA;
                            entityIVASPropertyList.Add(enItemVASPropertyAuthorReceiver);
                        }

                        if (rows.Cells["colPPA"].Value != null && Convert.ToBoolean(rows.Cells["colPPA"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            }

                            Hashtable htPPA = new Hashtable();
                            if (rows.Cells["colPPA"].Tag != null)
                                htPPA = (Hashtable)rows.Cells["colPPA"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htPPA.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htPPA["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPPA.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htPPA["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPPA.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htPPA["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htPPA.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htPPA["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                            string SoHopDongPPA = "";
                            DateTime HanHopDongPPA = DateTimeServer.Now;

                            if (rows.Cells["colContractNumberPPA"].Value != null)
                            {
                                SoHopDongPPA = rows.Cells["colContractNumberPPA"].Value.ToString();
                            }

                            if (rows.Cells["colContractDatePPA"].Value != null)
                            {
                                if (!string.IsNullOrEmpty(rows.Cells["colContractDatePPA"].Value.ToString()))
                                {
                                    DateTime HanHDPPA;

                                    if (DateTime.TryParseExact(rows.Cells["colContractDatePPA"].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDPPA))
                                    {
                                        HanHopDongPPA = HanHDPPA;
                                    }
                                }
                            }

                            ItemVASPropertyValueEntity enItemVASPropertyContractNumberPPA = new ItemVASPropertyValueEntity();

                            enItemVASPropertyContractNumberPPA.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyContractNumberPPA.PropertyCode = "ContractNumber";
                            enItemVASPropertyContractNumberPPA.Value = SoHopDongPPA;
                            enItemVASPropertyContractNumberPPA.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            entityIVASPropertyList.Add(enItemVASPropertyContractNumberPPA);

                            ItemVASPropertyValueEntity enItemVASPropertyContractDatePPA = new ItemVASPropertyValueEntity();

                            enItemVASPropertyContractDatePPA.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyContractDatePPA.PropertyCode = "ContractDate";
                            enItemVASPropertyContractDatePPA.Value = HanHopDongPPA.ToString();
                            enItemVASPropertyContractDatePPA.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            entityIVASPropertyList.Add(enItemVASPropertyContractDatePPA);

                            ItemVASPropertyValueEntity enItemVASPropertyThirdPartyPPA = new ItemVASPropertyValueEntity();

                            enItemVASPropertyThirdPartyPPA.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyThirdPartyPPA.PropertyCode = "ThirdParty";
                            enItemVASPropertyThirdPartyPPA.Value = "False";
                            enItemVASPropertyThirdPartyPPA.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            entityIVASPropertyList.Add(enItemVASPropertyThirdPartyPPA);

                            ItemVASPropertyValueEntity enItemVASPropertyThirdPartyNamePPA = new ItemVASPropertyValueEntity();

                            enItemVASPropertyThirdPartyNamePPA.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyThirdPartyNamePPA.PropertyCode = "ThirdPartyName";
                            enItemVASPropertyThirdPartyNamePPA.Value = "";
                            enItemVASPropertyThirdPartyNamePPA.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI;
                            entityIVASPropertyList.Add(enItemVASPropertyThirdPartyNamePPA);
                        }

                        if (rows.Cells["colC"].Value != null && Convert.ToBoolean(rows.Cells["colC"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            }

                            Hashtable htC = new Hashtable();
                            if (rows.Cells["colC"].Tag != null)
                                htC = (Hashtable)rows.Cells["colC"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htC.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htC["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htC.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htC["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htC.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htC["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htC.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htC["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                            string SoHopDongC = "";
                            DateTime HanHopDongC = DateTimeServer.Now;

                            if (rows.Cells["colContractNumberC"].Value != null)
                            {
                                SoHopDongC = rows.Cells["colContractNumberC"].Value.ToString();
                            }

                            if (rows.Cells["colContractDateC"].Value != null)
                            {
                                if (!string.IsNullOrEmpty(rows.Cells["colContractDateC"].Value.ToString()))
                                {
                                    DateTime HanHDC;

                                    if (DateTime.TryParseExact(rows.Cells["colContractDateC"].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDC))
                                    {
                                        HanHopDongC = HanHDC;
                                    }
                                }
                            }

                            ItemVASPropertyValueEntity enItemVASPropertyContractNumberC = new ItemVASPropertyValueEntity();

                            enItemVASPropertyContractNumberC.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyContractNumberC.PropertyCode = "ContractNumber";
                            enItemVASPropertyContractNumberC.Value = SoHopDongC;
                            enItemVASPropertyContractNumberC.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            entityIVASPropertyList.Add(enItemVASPropertyContractNumberC);

                            ItemVASPropertyValueEntity enItemVASPropertyContractDateC = new ItemVASPropertyValueEntity();

                            enItemVASPropertyContractDateC.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyContractDateC.PropertyCode = "ContractDate";
                            enItemVASPropertyContractDateC.Value = HanHopDongC.ToString();
                            enItemVASPropertyContractDateC.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            entityIVASPropertyList.Add(enItemVASPropertyContractDateC);

                            ItemVASPropertyValueEntity enItemVASPropertyThirdPartyC = new ItemVASPropertyValueEntity();

                            enItemVASPropertyThirdPartyC.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyThirdPartyC.PropertyCode = "ThirdParty";
                            enItemVASPropertyThirdPartyC.Value = "False";
                            enItemVASPropertyThirdPartyC.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            entityIVASPropertyList.Add(enItemVASPropertyThirdPartyC);

                            ItemVASPropertyValueEntity enItemVASPropertyThirdPartyNameC = new ItemVASPropertyValueEntity();

                            enItemVASPropertyThirdPartyNameC.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyThirdPartyNameC.PropertyCode = "ThirdPartyName";
                            enItemVASPropertyThirdPartyNameC.Value = "";
                            enItemVASPropertyThirdPartyNameC.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN;
                            entityIVASPropertyList.Add(enItemVASPropertyThirdPartyNameC);
                        }

                        if (rows.Cells["colBenThu3"].Value != null && Convert.ToBoolean(rows.Cells["colBenThu3"].Value))
                        {
                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            }
                            else
                            {
                                entityItem.Note = ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            }

                            Hashtable htT3 = new Hashtable();
                            if (rows.Cells["colBenThu3"].Tag != null)
                                htT3 = (Hashtable)rows.Cells["colBenThu3"].Tag;

                            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                            enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                            enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                            enValueAddedServiceItem.Freight = 0;
                            enValueAddedServiceItem.FreightVAT = 0;
                            enValueAddedServiceItem.OriginalFreight = 0;
                            enValueAddedServiceItem.OriginalFreightVAT = 0;
                            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                            enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                            enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                            enValueAddedServiceItem.SubFreight = 0;
                            enValueAddedServiceItem.SubFreightVAT = 0;
                            enValueAddedServiceItem.OriginalSubFreight = 0;
                            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                            if (htT3.ContainsKey("Freight"))
                            {
                                double dResult;
                                if (double.TryParse(htT3["Freight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htT3.ContainsKey("FreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htT3["FreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htT3.ContainsKey("OriginalFreight"))
                            {
                                double dResult;
                                if (double.TryParse(htT3["OriginalFreight"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            if (htT3.ContainsKey("OriginalFreightVAT"))
                            {
                                double dResult;
                                if (double.TryParse(htT3["OriginalFreightVAT"].ToString(), out dResult))
                                {
                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                                }
                            }

                            entityVASIList.Add(enValueAddedServiceItem);

                            string SoHopDongT3 = "";
                            DateTime HanHopDongT3 = DateTimeServer.Now;
                            string TenT3 = "";
                            if (rows.Cells["colContractNumberT3"].Value != null)
                            {
                                SoHopDongT3 = rows.Cells["colContractNumberT3"].Value.ToString();
                            }

                            if (rows.Cells["colContractDateT3"].Value != null)
                            {
                                if (!string.IsNullOrEmpty(rows.Cells["colContractDateT3"].Value.ToString()))
                                {
                                    DateTime HanHDT3;

                                    if (DateTime.TryParseExact(rows.Cells["colContractDateT3"].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDT3))
                                    {
                                        HanHopDongT3 = HanHDT3;
                                    }
                                }
                            }

                            if (rows.Cells["colThirdPartyName"].Value != null)
                            {
                                TenT3 = rows.Cells["colThirdPartyName"].Value.ToString();
                            }

                            ItemVASPropertyValueEntity enItemVASPropertyContractNumberT3 = new ItemVASPropertyValueEntity();

                            enItemVASPropertyContractNumberT3.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyContractNumberT3.PropertyCode = "ContractNumber";
                            enItemVASPropertyContractNumberT3.Value = SoHopDongT3;
                            enItemVASPropertyContractNumberT3.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            entityIVASPropertyList.Add(enItemVASPropertyContractNumberT3);

                            ItemVASPropertyValueEntity enItemVASPropertyContractDateT3 = new ItemVASPropertyValueEntity();

                            enItemVASPropertyContractDateT3.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyContractDateT3.PropertyCode = "ContractDate";
                            enItemVASPropertyContractDateT3.Value = HanHopDongT3.ToString();
                            enItemVASPropertyContractDateT3.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            entityIVASPropertyList.Add(enItemVASPropertyContractDateT3);

                            ItemVASPropertyValueEntity enItemVASPropertyThirdPartyT3 = new ItemVASPropertyValueEntity();

                            enItemVASPropertyThirdPartyT3.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyThirdPartyT3.PropertyCode = "ThirdParty";
                            enItemVASPropertyThirdPartyT3.Value = "True";
                            enItemVASPropertyThirdPartyT3.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            entityIVASPropertyList.Add(enItemVASPropertyThirdPartyT3);

                            ItemVASPropertyValueEntity enItemVASPropertyThirdPartyNameT3 = new ItemVASPropertyValueEntity();

                            enItemVASPropertyThirdPartyNameT3.ItemCode = entityItem.ItemCode;
                            enItemVASPropertyThirdPartyNameT3.PropertyCode = "ThirdPartyName";
                            enItemVASPropertyThirdPartyNameT3.Value = TenT3;
                            enItemVASPropertyThirdPartyNameT3.ValueAddedServiceCode = ValueAddedServiceConstance.THU_CUOC_BEN_THU_3;
                            entityIVASPropertyList.Add(enItemVASPropertyThirdPartyNameT3);
                        }
                        //Dungnt 
                        if (rows.Cells["colVASService"].Value != null && rows.Cells["colVASService"].Value.ToString().Length > 0)
                        {
                            var listVAS = rows.Cells["colVASService"].Value.ToString().Split(Convert.ToChar(";"));

                            if (!string.IsNullOrEmpty(entityItem.Note))
                            {
                                entityItem.Note = entityItem.Note + ";" + rows.Cells["colVASService"].Value.ToString();
                            }
                            else
                            {
                                entityItem.Note = rows.Cells["colVASService"].Value.ToString();
                            }
                            foreach (var itemVAS in listVAS)
                            {
                                ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                                enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                                enValueAddedServiceItem.ValueAddedServiceCode = itemVAS;
                                enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                                enValueAddedServiceItem.Freight = 0;
                                enValueAddedServiceItem.FreightVAT = 0;
                                enValueAddedServiceItem.OriginalFreight = 0;
                                enValueAddedServiceItem.OriginalFreightVAT = 0;
                                enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                                enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                                enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                                enValueAddedServiceItem.SubFreight = 0;
                                enValueAddedServiceItem.SubFreightVAT = 0;
                                enValueAddedServiceItem.OriginalSubFreight = 0;
                                enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                                if (VASFreightList != null)
                                {
                                    foreach (ValueAddedServiceFreight VASValue in VASFreightList)
                                    {
                                        if (itemVAS.Equals(VASValue.ValueAddedServiceCode))
                                        {
                                            enValueAddedServiceItem.Freight = Math.Round(VASValue.Freight, MidpointRounding.AwayFromZero);
                                            enValueAddedServiceItem.FreightVAT = Math.Round(VASValue.Freight + (VASValue.Freight * c_VATPercentage / 100), MidpointRounding.AwayFromZero);
                                        }
                                    }
                                }
                                if (VASFreightListOriginal != null)
                                {
                                    foreach (ValueAddedServiceFreight VASValue in VASFreightListOriginal)
                                    {
                                        if (itemVAS.Equals(VASValue.ValueAddedServiceCode))
                                        {
                                            enValueAddedServiceItem.OriginalFreight = Math.Round(VASValue.Freight, MidpointRounding.AwayFromZero);
                                            enValueAddedServiceItem.OriginalFreightVAT = Math.Round(VASValue.Freight + (VASValue.Freight * c_VATPercentage / 100), MidpointRounding.AwayFromZero);

                                        }
                                    }
                                }
                                entityVASIList.Add(enValueAddedServiceItem);
                            }
                        }
                        if (cboService.SelectedValue.ToString().Equals(ServiceConstance.KT1))
                        {
                            ItemTypeDAO daoItemTypeDAO = new ItemTypeDAO();
                            ItemTypeEntity enItemTypeEntity = daoItemTypeDAO.SelectOne(entityItem.ItemTypeCode);
                            if (enItemTypeEntity.ItemTypeName.IndexOf("Hẹn giờ") >= 0)
                            {
                                entityKT1ExpectedTimeEntityList = new List<KT1ExpectedTimeEntity>();
                                KT1ExpectedTimeEntity c_KT1ExpectedTimeEntity = new KT1ExpectedTimeEntity();
                                c_KT1ExpectedTimeEntity.POSCode = this.POSCode;
                                c_KT1ExpectedTimeEntity.DeliveryTime = DateTime.Parse(rows.Cells["colDeliveryTime"].Value.ToString());

                                c_KT1ExpectedTimeEntity.ItemCode = entityItem.ItemCode;

                                c_KT1ExpectedTimeEntity.CreateTime = (System.DateTime)DateTime.Now;
                                c_KT1ExpectedTimeEntity.LastUpdatedTime = (System.DateTime)DateTime.Now;

                                entityKT1ExpectedTimeEntityList.Add(c_KT1ExpectedTimeEntity);
                            }
                        }


                        //End Dungnt

                        //hiepvb 05/12/2018 get buu gui quoc te

                        if (rows.Cells["colExtendData"].Value != null && rows.Cells["colExtendData"].Value.ToString().Length > 0 && rows.Cells["colBusinessId"].Value != null && rows.Cells["colBusinessId"].Value.ToString().Equals(DieutinBussinessConstance.QUOC_TE))
                        {
                            if (cboService.SelectedValue.ToString().Equals(ServiceConstance.BCUT) || cboService.SelectedValue.ToString().Equals(ServiceConstance.BPBD))
                            {
                                ExtendDataQT extendData = Newtonsoft.Json.JsonConvert.DeserializeObject<ExtendDataQT>(rows.Cells["colExtendData"].Value.ToString());
                                AttachDocumentsItemEntity enAttachDocumentsItem = new AttachDocumentsItemEntity();

                                enAttachDocumentsItem.POSCode = this.POSCode;
                                enAttachDocumentsItem.ItemCode = entityItem.ItemCode;
                                enAttachDocumentsItem.Certificate = extendData.AttachDocument.Certificate;
                                enAttachDocumentsItem.CertificationNumber = extendData.AttachDocument.CertificationNumber;
                                enAttachDocumentsItem.License = extendData.AttachDocument.License;
                                enAttachDocumentsItem.LicenseNumber = extendData.AttachDocument.LicenseNumber;
                                enAttachDocumentsItem.Invoice = extendData.AttachDocument.Invoice;
                                enAttachDocumentsItem.InvoiceNumber = extendData.AttachDocument.InvoiceNumber;
                                enAttachDocumentsItem.SenderAddress2 = extendData.DiaChiNguoiGui2;
                                if (rows.Cells["colProvinceCode"].Value != null && rows.Cells["colProvinceCode"].Value.ToString().Length > 0)
                                {
                                    ProvinceEntity enProvince = new ProvinceDAO().SelectOne(rows.Cells["colProvinceCode"].Value.ToString());
                                    enAttachDocumentsItem.SenderCity = enProvince.ProvinceName.ToString().Trim();
                                }
                                enAttachDocumentsItem.ReceiverAddress2 = extendData.DiaChiNguoiNhan2;
                                enAttachDocumentsItem.ReceiverState = extendData.BangNguoiNhan;
                                enAttachDocumentsItem.ReceiverCity = extendData.TPNguoiNhan;
                                enAttachDocumentsItem.CreateTime = DateTimeServer.Now;
                                enAttachDocumentsItem.LastUpdatedTime = DateTimeServer.Now;
                                entityAttachDocumentsItemList.Add(enAttachDocumentsItem);

                                if (!string.IsNullOrEmpty(extendData.ListCommodityCode))
                                {
                                    foreach (string strCommodityType in extendData.ListCommodityCode.ToString().Trim().Split(Convert.ToChar(";")))
                                    {
                                        ItemCommodityTypeEntity entityItemCommodityType = new ItemCommodityTypeEntity();
                                        entityItemCommodityType.ItemCode = entityItem.ItemCode;
                                        entityItemCommodityType.CommodityTypeCode = strCommodityType;
                                        entityItemCommodityTypeList.Add(entityItemCommodityType);
                                    }
                                }

                                if (extendData.ItemDetail != null && extendData.ItemDetail.Count > 0)
                                {
                                    entityDetailItemList.Clear();
                                    foreach (ItemDetails itemDetails in extendData.ItemDetail)
                                    {

                                        DetailItemEntity enDetailItem = new DetailItemEntity();
                                        enDetailItem.ItemIndex = itemDetails.Order;
                                        enDetailItem.ItemCode = entityItem.ItemCode;
                                        enDetailItem.Quantity = itemDetails.Quantity;
                                        enDetailItem.Amount = itemDetails.Amount;
                                        enDetailItem.Weight = itemDetails.Weight;
                                        enDetailItem.DetailItemName = itemDetails.ContentVN;
                                        enDetailItem.EnContent = itemDetails.ContentEn;
                                        enDetailItem.HsId = itemDetails.HsCode;
                                        enDetailItem.OriginalCountryCode = itemDetails.Origin;
                                        enDetailItem.CreateTime = DateTimeServer.Now;
                                        enDetailItem.LastUpdatedTime = DateTimeServer.Now;
                                        entityDetailItemList.Add(enDetailItem);
                                    }
                                }

                                if (!string.IsNullOrEmpty(extendData.ListServiceCode) != null)
                                {
                                    foreach (string strValueAddedServiceCode in extendData.ListServiceCode.ToString().Trim().Split(Convert.ToChar(";")))
                                    {
                                        ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                                        enValueAddedServiceItem.ServiceCode = entityItem.ServiceCode;
                                        enValueAddedServiceItem.ValueAddedServiceCode = strValueAddedServiceCode;
                                        enValueAddedServiceItem.ItemCode = entityItem.ItemCode;
                                        enValueAddedServiceItem.Freight = 0;
                                        enValueAddedServiceItem.FreightVAT = 0;
                                        enValueAddedServiceItem.OriginalFreight = 0;
                                        enValueAddedServiceItem.OriginalFreightVAT = 0;
                                        enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                                        enValueAddedServiceItem.AddedDate = entityItem.SendingTime;
                                        enValueAddedServiceItem.POSCode = entityItem.AcceptancePOSCode;

                                        enValueAddedServiceItem.SubFreight = 0;
                                        enValueAddedServiceItem.SubFreightVAT = 0;
                                        enValueAddedServiceItem.OriginalSubFreight = 0;
                                        enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                                        if (VASFreightList != null)
                                        {
                                            foreach (ValueAddedServiceFreight VASValue in VASFreightList)
                                            {
                                                if (strValueAddedServiceCode.Equals(VASValue.ValueAddedServiceCode))
                                                {
                                                    enValueAddedServiceItem.Freight = Math.Round(VASValue.Freight, MidpointRounding.AwayFromZero);
                                                    enValueAddedServiceItem.FreightVAT = Math.Round(VASValue.Freight + (VASValue.Freight * c_VATPercentage / 100), MidpointRounding.AwayFromZero);
                                                }
                                            }
                                        }
                                        if (VASFreightListOriginal != null)
                                        {
                                            foreach (ValueAddedServiceFreight VASValue in VASFreightListOriginal)
                                            {
                                                if (strValueAddedServiceCode.Equals(VASValue.ValueAddedServiceCode))
                                                {
                                                    enValueAddedServiceItem.OriginalFreight = Math.Round(VASValue.Freight, MidpointRounding.AwayFromZero);
                                                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(VASValue.Freight + (VASValue.Freight * c_VATPercentage / 100), MidpointRounding.AwayFromZero);

                                                }
                                            }
                                        }
                                        entityVASIList.Add(enValueAddedServiceItem);
                                    }
                                }
                            }
                        }
                        //end hiepvb
                    }

                    ExchangeRateDAO daoExchangeRate = new ExchangeRateDAO();

                    ExchangeRateEntity entityExchangeRate = daoExchangeRate.SelectExchangeRate("VND");

                    if (entityExchangeRate != null && !entityExchangeRate.IsNullExchangeRateCode)
                    {
                        entityItem.ExchangeRateCode = entityExchangeRate.ExchangeRateCode;
                    }

                    //Thông tin bưu gửi trong ca làm việc
                    ShiftHandoverItemEntity entityShiftHandoverItem = new ShiftHandoverItemEntity();
                    entityShiftHandoverItem.HandoverIndex = this.ShiftHandover.HandoverIndex;
                    entityShiftHandoverItem.ShiftCode = this.ShiftHandover.ShiftCode;
                    entityShiftHandoverItem.POSCode = this.ShiftHandover.POSCode;
                    entityShiftHandoverItem.ItemCode = entityItem.ItemCode;
                    entityShiftHandoverItem.Status = ItemConstance.StatusAccepted;
                    entityShiftHandoverItem.CounterCode = this.ShiftHandover.CounterCode;
                    entityShiftHandoverItem.Phase = TraceItemConstance.StatusAccepted;

                    //Định vị bưu gửi
                    if (!string.IsNullOrEmpty(OriginalPOSCode))
                    {
                        if (entityItem.AcceptancePOSCode.Equals(this.OriginalPOSCode))
                        {
                            TraceItemEntity entityTraceItemOriginal = new TraceItemEntity();
                            entityTraceItemOriginal.TraceIndex = 1;
                            entityTraceItemOriginal.ItemCode = entityItem.ItemCode;
                            entityTraceItemOriginal.TraceDate = DateTimeServer.Now;
                            entityTraceItemOriginal.StatusDesc = "BCCP.NG.IML";
                            entityTraceItemOriginal.TransferUser = this.Username;
                            entityTraceItemOriginal.TransferMachine = System.Net.Dns.GetHostName();
                            entityTraceItemOriginal.Note = "";
                            entityTraceItemOriginal.POSCode = this.OriginalPOSCode;
                            entityTraceItemOriginal.Status = TraceItemConstance.StatusAccepted;

                            if (entityTraceItemListGlobal == null)
                                entityTraceItemListGlobal = new List<TraceItemEntity>();
                            entityTraceItemListGlobal.Add(entityTraceItemOriginal);
                        }
                        else
                        {
                            TraceItemEntity entityTraceItemOriginal = new TraceItemEntity();
                            entityTraceItemOriginal.TraceIndex = 1;
                            entityTraceItemOriginal.ItemCode = entityItem.ItemCode;
                            entityTraceItemOriginal.TraceDate = DateTimeServer.Now;
                            entityTraceItemOriginal.StatusDesc = "BCCP.NG.IML";
                            entityTraceItemOriginal.TransferUser = this.Username;
                            entityTraceItemOriginal.TransferMachine = System.Net.Dns.GetHostName();
                            entityTraceItemOriginal.POSCode = this.OriginalPOSCode;
                            entityTraceItemOriginal.Status = TraceItemConstance.StatusAccepted;
                            entityTraceItemOriginal.Note = "Nhập thay thế";

                            TraceItemEntity entityTraceItemReplace = new TraceItemEntity();
                            entityTraceItemReplace.TraceIndex = 2;
                            entityTraceItemReplace.ItemCode = entityItem.ItemCode;
                            entityTraceItemReplace.TraceDate = DateTimeServer.Now;
                            entityTraceItemReplace.StatusDesc = "BCCP.NG.IML";
                            entityTraceItemReplace.TransferUser = this.Username;
                            entityTraceItemReplace.TransferMachine = System.Net.Dns.GetHostName();
                            entityTraceItemReplace.POSCode = this.OriginalPOSCode;
                            entityTraceItemReplace.Status = TraceItemConstance.StatusToPOS;
                            entityTraceItemReplace.Note = "Nhập thay thế";

                            if (entityTraceItemListGlobal == null)
                                entityTraceItemListGlobal = new List<TraceItemEntity>();
                            entityTraceItemListGlobal.Add(entityTraceItemOriginal);
                            entityTraceItemListGlobal.Add(entityTraceItemReplace);
                        }
                    }
                    else
                    {

                        TraceItemEntity entityTraceItemOriginal = new TraceItemEntity();
                        entityTraceItemOriginal.TraceIndex = 1;
                        entityTraceItemOriginal.ItemCode = entityItem.ItemCode;
                        entityTraceItemOriginal.TraceDate = DateTimeServer.Now;
                        entityTraceItemOriginal.StatusDesc = "BCCP.NG.IML";
                        entityTraceItemOriginal.TransferUser = this.Username;
                        entityTraceItemOriginal.TransferMachine = System.Net.Dns.GetHostName();
                        entityTraceItemOriginal.Note = "";
                        entityTraceItemOriginal.POSCode = this.POSCode;
                        entityTraceItemOriginal.Status = TraceItemConstance.StatusAccepted;

                        if (entityTraceItemListGlobal == null)
                        {
                            entityTraceItemListGlobal = new List<TraceItemEntity>();
                        }
                        entityTraceItemListGlobal.Add(entityTraceItemOriginal);
                    }

                    if (entityItemListGlobal == null)
                    {
                        entityItemListGlobal = new List<ItemEntity>();
                    }

                    entityItemListGlobal.Add(entityItem);

                    if (entityItemCommodityTypeListGlobal == null)
                    {
                        entityItemCommodityTypeListGlobal = new List<ItemCommodityTypeEntity>();
                    }

                    foreach (ItemCommodityTypeEntity eItemCommodity in entityItemCommodityTypeList)
                    {
                        //eItemCommodity.ItemCode = entityItem.ItemCode;

                        entityItemCommodityTypeListGlobal.Add(eItemCommodity);
                    }

                    if (entityDetailItemListGlobal == null)
                    {
                        entityDetailItemListGlobal = new List<DetailItemEntity>();
                    }

                    foreach (DetailItemEntity eDetailItem in entityDetailItemList)
                    {
                        eDetailItem.ItemCode = entityItem.ItemCode;

                        entityDetailItemListGlobal.Add(eDetailItem);
                    }

                    if (entityValueAddedServiceItemListGlobal == null)
                    {
                        entityValueAddedServiceItemListGlobal = new List<ValueAddedServiceItemEntity>();
                    }

                    foreach (ValueAddedServiceItemEntity enVASI in entityVASIList)
                    {
                        entityValueAddedServiceItemListGlobal.Add(enVASI);
                    }

                    if (entityItemVASPropertyValueListGlobal == null)
                    {
                        entityItemVASPropertyValueListGlobal = new List<ItemVASPropertyValueEntity>();
                    }

                    foreach (ItemVASPropertyValueEntity enIVASPro in entityIVASPropertyList)
                    {
                        entityItemVASPropertyValueListGlobal.Add(enIVASPro);
                    }

                    if (entityShiftHandoverItemListGlobal == null)
                    {
                        entityShiftHandoverItemListGlobal = new List<ShiftHandoverItemEntity>();
                    }

                    entityShiftHandoverItemListGlobal.Add(entityShiftHandoverItem);

                    if (entityTransactionsCollectionListGlobal == null)
                    {
                        entityTransactionsCollectionListGlobal = new List<TransactionsCollectionEntity>();
                    }

                    foreach (TransactionsCollectionEntity enTransaction in entityTransactionsCollectionList)
                    {
                        entityTransactionsCollectionListGlobal.Add(enTransaction);
                    }

                    if (entityTransactionsCollectionDetailListGlobal == null)
                    {
                        entityTransactionsCollectionDetailListGlobal = new List<TransactionsCollectionDetailEntity>();
                    }

                    foreach (TransactionsCollectionDetailEntity enTransactionDetail in entityTransactionsCollectionDetailList)
                    {
                        entityTransactionsCollectionDetailListGlobal.Add(enTransactionDetail);
                    }
                    if (entityKT1ExpectedTimeEntityListGlobal == null)
                    {
                        entityKT1ExpectedTimeEntityListGlobal = new List<KT1ExpectedTimeEntity>();
                    }
                    foreach (KT1ExpectedTimeEntity enKT1ExpectedTimeEntity in entityKT1ExpectedTimeEntityList)
                    {
                        entityKT1ExpectedTimeEntityListGlobal.Add(enKT1ExpectedTimeEntity);
                    }

                    if (entitySortingItemListGlobal == null)
                    {
                        entitySortingItemListGlobal = new List<SortingItemEntity>();
                    }
                    foreach (SortingItemEntity enSortingItemEntity in entitySortingItemEntityList)
                    {
                        entitySortingItemListGlobal.Add(enSortingItemEntity);
                    }

                    if (entityAttachDocumentsItemListGlobal == null)
                    {
                        entityAttachDocumentsItemListGlobal = new List<AttachDocumentsItemEntity>();
                    }
                    foreach (AttachDocumentsItemEntity enAttachDocumentsItem in entityAttachDocumentsItemList)
                    {
                        entityAttachDocumentsItemListGlobal.Add(enAttachDocumentsItem);
                    }
                }
            }
            else
            {
                ShowMessageBoxWarning("Không có bưu gửi. Yêu cầu import bưu gửi");
            }
        }

        //private void Accept()
        //{
        //    if (CheckShifted())
        //    {
        //        return;
        //    }
        //    if (CheckSendingTime())
        //    {
        //        if (CheckItemCode())
        //        {
        //            if (CheckSymbolItem())
        //            {
        //                if (CheckSumItem())
        //                {
        //                    if (CheckDataCode())
        //                    {
        //                        if (CheckCustomerCode())
        //                        {
        //                            if (CheckSenderFullName())
        //                            {
        //                                if (CheckSenderFullNameSymbol())
        //                                {
        //                                    if (CheckSenderAddress())
        //                                    {
        //                                        if (CheckSenderAddressSymbol())
        //                                        {
        //                                            if (CheckReceiverCustomerCode())
        //                                            {
        //                                                if (CheckReceiverCustomerCodeByItemType())
        //                                                {
        //                                                    if (CheckReceiverFullName())
        //                                                    {
        //                                                        if (CheckReceiverFullNameSymbol())
        //                                                        {
        //                                                            if (CheckReceiverAddress())
        //                                                            {
        //                                                                if (CheckReceiverAddressSymbol())
        //                                                                {
        //                                                                    if (CheckCountryProvince())
        //                                                                    {
        //                                                                        if (CheckItemType())
        //                                                                        {
        //                                                                            if (CheckItemContent())
        //                                                                            {
        //                                                                                if (CheckUndeliveryGuide())
        //                                                                                {
        //                                                                                    if (CheckWeight())
        //                                                                                    {
        //                                                                                        if (CheckLength())
        //                                                                                        {
        //                                                                                            if (CheckWidth())
        //                                                                                            {
        //                                                                                                if (CheckHeight())
        //                                                                                                {
        //                                                                                                    if (CheckCOD())
        //                                                                                                    {
        //                                                                                                        if (CheckDetailItemNameSymbol())
        //                                                                                                        {
        //                                                                                                            if (CheckContractNumberPPA())
        //                                                                                                            {
        //                                                                                                                if (CheckContractDatePPA())
        //                                                                                                                {
        //                                                                                                                    if (CheckContractNumberC())
        //                                                                                                                    {
        //                                                                                                                        if (CheckContractDateC())
        //                                                                                                                        {
        //                                                                                                                            if (CheckContractNumberT3())
        //                                                                                                                            {
        //                                                                                                                                if (CheckContractDateT3())
        //                                                                                                                                {
        //                                                                                                                                    if (CheckItemCodeOriginalExists())
        //                                                                                                                                    {
        //                                                                                                                                        if (CheckReceiverPOSCode())
        //                                                                                                                                        {

        //                                                                                                                                            ItemDAO daoItem = new ItemDAO();

        //                                                                                                                                            ItemCommodityTypeDAO daoItemCommodityType = new ItemCommodityTypeDAO();

        //                                                                                                                                            DetailItemDAO daoDetailItem = new DetailItemDAO();

        //                                                                                                                                            ValueAddedServiceItemDAO daoValueAddedServiceItem = new ValueAddedServiceItemDAO();

        //                                                                                                                                            ItemVASPropertyValueDAO daoItemVASPropertyValue = new ItemVASPropertyValueDAO();

        //                                                                                                                                            ShiftHandoverItemDAO daoShiftHandoverItem = new ShiftHandoverItemDAO();

        //                                                                                                                                            TraceItemDAO daoTraceItem = new TraceItemDAO();

        //                                                                                                                                            TransactionsCollectionDAO daoTransactionsCollection = new TransactionsCollectionDAO();

        //                                                                                                                                            TransactionsCollectionDetailDAO daoTransactionsCollectionDetail = new TransactionsCollectionDetailDAO();

        //                                                                                                                                            ItemAdviceOfReceiptDAO daoItemAdviceOfReceipt = new ItemAdviceOfReceiptDAO();

        //                                                                                                                                            KT1ExpectedTimeDAO daoKT1ExpectedTimeDAO = new KT1ExpectedTimeDAO();

        //                                                                                                                                            SortingItemDAO daoSortingItem = new SortingItemDAO();

        //                                                                                                                                            AttachDocumentsItemDAO daoAttachDocumentsItem = new AttachDocumentsItemDAO();
        //                                                                                                                                            GetData();

        //                                                                                                                                            if (entityItemListGlobal != null && entityItemListGlobal.Count > 0)
        //                                                                                                                                            {
        //                                                                                                                                                List<string> ItemSuccess = new List<string>();

        //                                                                                                                                                List<string> ItemError = new List<string>();

        //                                                                                                                                                foreach (ItemEntity ItemValue in entityItemListGlobal)
        //                                                                                                                                                {
        //                                                                                                                                                    if (daoItem.Save(ItemValue))
        //                                                                                                                                                    {
        //                                                                                                                                                        ItemSuccess.Add(ItemValue.ItemCode);
        //                                                                                                                                                    }
        //                                                                                                                                                    else
        //                                                                                                                                                    {
        //                                                                                                                                                        ItemError.Add(ItemValue.ItemCode);
        //                                                                                                                                                    }
        //                                                                                                                                                }

        //                                                                                                                                                if (ItemError.Count == 0)
        //                                                                                                                                                {
        //                                                                                                                                                    List<string> TraceItemSuccess = new List<string>();

        //                                                                                                                                                    List<string> TraceItemError = new List<string>();

        //                                                                                                                                                    if (entityTraceItemListGlobal != null && entityTraceItemListGlobal.Count > 0)
        //                                                                                                                                                    {
        //                                                                                                                                                        foreach (TraceItemEntity enTraceItemTemp in entityTraceItemListGlobal)
        //                                                                                                                                                        {
        //                                                                                                                                                            if (daoTraceItem.Save(enTraceItemTemp))
        //                                                                                                                                                            {
        //                                                                                                                                                                TraceItemSuccess.Add(enTraceItemTemp.ItemCode);
        //                                                                                                                                                            }
        //                                                                                                                                                            else
        //                                                                                                                                                            {
        //                                                                                                                                                                TraceItemError.Add(enTraceItemTemp.ItemCode);
        //                                                                                                                                                            }
        //                                                                                                                                                        }
        //                                                                                                                                                    }

        //                                                                                                                                                    if (TraceItemError.Count == 0)
        //                                                                                                                                                    {
        //                                                                                                                                                        List<string> ShiftHandoverItemSuccess = new List<string>();

        //                                                                                                                                                        List<string> ShiftHandoverItemError = new List<string>();

        //                                                                                                                                                        if (entityShiftHandoverItemListGlobal != null && entityShiftHandoverItemListGlobal.Count > 0)
        //                                                                                                                                                        {
        //                                                                                                                                                            foreach (ShiftHandoverItemEntity enShiftHandoverItemTemp in entityShiftHandoverItemListGlobal)
        //                                                                                                                                                            {
        //                                                                                                                                                                if (daoShiftHandoverItem.Save(enShiftHandoverItemTemp))
        //                                                                                                                                                                {
        //                                                                                                                                                                    ShiftHandoverItemSuccess.Add(enShiftHandoverItemTemp.ItemCode);
        //                                                                                                                                                                }
        //                                                                                                                                                                else
        //                                                                                                                                                                {
        //                                                                                                                                                                    ShiftHandoverItemError.Add(enShiftHandoverItemTemp.ItemCode);
        //                                                                                                                                                                }
        //                                                                                                                                                            }
        //                                                                                                                                                        }

        //                                                                                                                                                        if (ShiftHandoverItemError.Count == 0)
        //                                                                                                                                                        {
        //                                                                                                                                                            List<string> VASISuccess = new List<string>();

        //                                                                                                                                                            List<string> VASIError = new List<string>();

        //                                                                                                                                                            if (entityValueAddedServiceItemListGlobal != null && entityValueAddedServiceItemListGlobal.Count > 0)
        //                                                                                                                                                            {
        //                                                                                                                                                                foreach (ValueAddedServiceItemEntity enVASITemp in entityValueAddedServiceItemListGlobal)
        //                                                                                                                                                                {
        //                                                                                                                                                                    if (daoValueAddedServiceItem.Save(enVASITemp))
        //                                                                                                                                                                    {
        //                                                                                                                                                                        if (!VASISuccess.Contains(enVASITemp.ItemCode))
        //                                                                                                                                                                        {
        //                                                                                                                                                                            VASISuccess.Add(enVASITemp.ItemCode);
        //                                                                                                                                                                        }
        //                                                                                                                                                                    }
        //                                                                                                                                                                    else
        //                                                                                                                                                                    {
        //                                                                                                                                                                        if (!VASIError.Contains(enVASITemp.ItemCode))
        //                                                                                                                                                                        {
        //                                                                                                                                                                            VASIError.Add(enVASITemp.ItemCode);
        //                                                                                                                                                                        }
        //                                                                                                                                                                    }
        //                                                                                                                                                                }
        //                                                                                                                                                            }

        //                                                                                                                                                            if (VASIError.Count == 0)
        //                                                                                                                                                            {
        //                                                                                                                                                                ErrorLog.Log(entityItemVASPropertyValueListGlobal.Count.ToString(), "frmAcceptanceFromDieuTin.Accept");
        //                                                                                                                                                                List<string> IVASPropertySuccess = new List<string>();
        //                                                                                                                                                                List<string> IVASPropertyError = new List<string>();

        //                                                                                                                                                                if (entityItemVASPropertyValueListGlobal != null && entityItemVASPropertyValueListGlobal.Count > 0)
        //                                                                                                                                                                {
        //                                                                                                                                                                    foreach (ItemVASPropertyValueEntity enIVASPropertyTemp in entityItemVASPropertyValueListGlobal)
        //                                                                                                                                                                    {
        //                                                                                                                                                                        ErrorLog.Log("ItemCode:" + enIVASPropertyTemp.ItemCode + ",PropertyCode:" + enIVASPropertyTemp.PropertyCode + ",Value:" + enIVASPropertyTemp.Value + ",ValueAddedServiceCode:" + enIVASPropertyTemp.ValueAddedServiceCode
        //                                                                                                                                                                            , "frmAcceptanceFromDieuTin.Accept");
        //                                                                                                                                                                        if (daoItemVASPropertyValue.Save(enIVASPropertyTemp))
        //                                                                                                                                                                        {
        //                                                                                                                                                                            if (!IVASPropertySuccess.Contains(enIVASPropertyTemp.ItemCode))
        //                                                                                                                                                                            {
        //                                                                                                                                                                                IVASPropertySuccess.Add(enIVASPropertyTemp.ItemCode);
        //                                                                                                                                                                            }
        //                                                                                                                                                                        }
        //                                                                                                                                                                        else
        //                                                                                                                                                                        {
        //                                                                                                                                                                            if (!IVASPropertyError.Contains(enIVASPropertyTemp.ItemCode))
        //                                                                                                                                                                            {
        //                                                                                                                                                                                IVASPropertyError.Add(enIVASPropertyTemp.ItemCode);
        //                                                                                                                                                                            }
        //                                                                                                                                                                        }
        //                                                                                                                                                                    }
        //                                                                                                                                                                }
        //                                                                                                                                                                ErrorLog.Log(IVASPropertySuccess.Count.ToString() + " - " + IVASPropertyError.Count.ToString(), "frmAcceptanceFromDieuTin.Accept");

        //                                                                                                                                                                if (IVASPropertyError.Count == 0)
        //                                                                                                                                                                {
        //                                                                                                                                                                    List<string> CollectionSuccess = new List<string>();

        //                                                                                                                                                                    List<string> CollectionError = new List<string>();

        //                                                                                                                                                                    if (entityTransactionsCollectionListGlobal != null && entityTransactionsCollectionListGlobal.Count > 0)
        //                                                                                                                                                                    {
        //                                                                                                                                                                        foreach (TransactionsCollectionEntity enCollectionTemp in entityTransactionsCollectionListGlobal)
        //                                                                                                                                                                        {
        //                                                                                                                                                                            if (daoTransactionsCollection.Save(enCollectionTemp))
        //                                                                                                                                                                            {
        //                                                                                                                                                                                if (!CollectionSuccess.Contains(enCollectionTemp.ItemCode))
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    CollectionSuccess.Add(enCollectionTemp.ItemCode);
        //                                                                                                                                                                                }
        //                                                                                                                                                                            }
        //                                                                                                                                                                            else
        //                                                                                                                                                                            {
        //                                                                                                                                                                                if (!CollectionError.Contains(enCollectionTemp.ItemCode))
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    CollectionError.Add(enCollectionTemp.ItemCode);
        //                                                                                                                                                                                }
        //                                                                                                                                                                            }
        //                                                                                                                                                                        }
        //                                                                                                                                                                    }

        //                                                                                                                                                                    if (CollectionError.Count == 0)
        //                                                                                                                                                                    {
        //                                                                                                                                                                        List<string> CollectionDetailSuccess = new List<string>();

        //                                                                                                                                                                        List<string> CollectionDetailError = new List<string>();

        //                                                                                                                                                                        if (entityTransactionsCollectionDetailListGlobal != null && entityTransactionsCollectionDetailListGlobal.Count > 0)
        //                                                                                                                                                                        {
        //                                                                                                                                                                            foreach (TransactionsCollectionDetailEntity enCollectionDetailTemp in entityTransactionsCollectionDetailListGlobal)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                if (daoTransactionsCollectionDetail.Save(enCollectionDetailTemp))
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    if (!CollectionDetailSuccess.Contains(enCollectionDetailTemp.ItemCode))
        //                                                                                                                                                                                    {
        //                                                                                                                                                                                        CollectionDetailSuccess.Add(enCollectionDetailTemp.ItemCode);
        //                                                                                                                                                                                    }
        //                                                                                                                                                                                }
        //                                                                                                                                                                                else
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    if (!CollectionDetailError.Contains(enCollectionDetailTemp.ItemCode))
        //                                                                                                                                                                                    {
        //                                                                                                                                                                                        CollectionDetailError.Add(enCollectionDetailTemp.ItemCode);
        //                                                                                                                                                                                    }
        //                                                                                                                                                                                }
        //                                                                                                                                                                            }
        //                                                                                                                                                                        }

        //                                                                                                                                                                        if (CollectionDetailError.Count == 0)
        //                                                                                                                                                                        {
        //                                                                                                                                                                            if (entityItemCommodityTypeListGlobal != null && entityItemCommodityTypeListGlobal.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoItemCommodityType.SaveList(entityItemCommodityTypeListGlobal);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            foreach (ItemEntity enItemDetail in entityItemListGlobal)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoDetailItem.DeleteByItemCode(enItemDetail.ItemCode);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            if (entityDetailItemListGlobal != null && entityDetailItemListGlobal.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoDetailItem.SaveList(entityDetailItemListGlobal);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            if (entityItemAdviceOfReceiptListGlobal != null && entityItemAdviceOfReceiptListGlobal.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoItemAdviceOfReceipt.SaveList(entityItemAdviceOfReceiptListGlobal);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            if (entityKT1ExpectedTimeEntityListGlobal != null && entityKT1ExpectedTimeEntityListGlobal.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoKT1ExpectedTimeDAO.SaveList(entityKT1ExpectedTimeEntityListGlobal);
        //                                                                                                                                                                            }


        //                                                                                                                                                                            if (entitySortingItemListGlobal != null && entitySortingItemListGlobal.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                //tam thoi remove het ban ghi trang o day 
        //                                                                                                                                                                                foreach (SortingItemEntity enSortingItem in entitySortingItemListGlobal)
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    if (enSortingItem.SortingCode != null && enSortingItem.SortingCode.Trim() == "")
        //                                                                                                                                                                                    {
        //                                                                                                                                                                                        entitySortingItemListGlobal.Remove(enSortingItem);
        //                                                                                                                                                                                    }
        //                                                                                                                                                                                }
        //                                                                                                                                                                                daoSortingItem.SaveList(entitySortingItemListGlobal);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            foreach (AttachDocumentsItemEntity enAttachDocumentsItem in entityAttachDocumentsItemListGlobal)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoAttachDocumentsItem.DeleteAttachDocumentsItemByPOSCodeItemCode(this.POSCode, enAttachDocumentsItem.ItemCode);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            if (entityAttachDocumentsItemListGlobal != null && entityAttachDocumentsItemListGlobal.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoAttachDocumentsItem.SaveList(entityAttachDocumentsItemListGlobal);
        //                                                                                                                                                                            }

        //                                                                                                                                                                            if (ItemSuccess.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                foreach (string ItemTransfer in ItemSuccess)
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    itemListTransferWait.Add(ItemTransfer);
        //                                                                                                                                                                                }
        //                                                                                                                                                                            }

        //                                                                                                                                                                            lblWaitCount.Text = itemListTransferWait.Count.ToString();

        //                                                                                                                                                                            ShowMessageBoxInformation("Thêm mới bưu gửi thành công");

        //                                                                                                                                                                            updateDieutin();

        //                                                                                                                                                                            frmPrintOption frmOption = new frmPrintOption();
        //                                                                                                                                                                            frmOption.POSCode = this.POSCode;
        //                                                                                                                                                                            frmOption.OriginalPOSCode = this.OriginalPOSCode;
        //                                                                                                                                                                            frmOption.ServiceCode = cboService.SelectedValue.ToString();
        //                                                                                                                                                                            frmOption.Username = this.Username;
        //                                                                                                                                                                            frmOption.PhaseCode = PhaseConstance.NHAN_GUI_SLL;
        //                                                                                                                                                                            frmOption.AcceptanceType = AcceptanceTypeConstance.BUU_GUI_SLL;
        //                                                                                                                                                                            //frmOption.ItemList = entityItemListGlobal;
        //                                                                                                                                                                            //if (entityItemListGlobal.Count > 0)
        //                                                                                                                                                                            //{
        //                                                                                                                                                                            //    ItemEntity eItem = new ItemEntity();
        //                                                                                                                                                                            //    eItem = entityItemListGlobal[0];
        //                                                                                                                                                                            //}
        //                                                                                                                                                                            frmOption.EntityItem = entityItemListGlobal[0];
        //                                                                                                                                                                            frmOption.DieuTin = true;
        //                                                                                                                                                                            frmOption.ShowDialog();
        //                                                                                                                                                                            alwaysAsk = false;
        //                                                                                                                                                                            this.Close();
        //                                                                                                                                                                        }
        //                                                                                                                                                                        else
        //                                                                                                                                                                        {
        //                                                                                                                                                                            if (CollectionDetailSuccess.Count > 0)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                foreach (string ItemDelete in CollectionDetailSuccess)
        //                                                                                                                                                                                {
        //                                                                                                                                                                                    daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                                                }
        //                                                                                                                                                                            }

        //                                                                                                                                                                            ShowMessageBoxWarning("Lỗi khi thêm thông tin giao dịch nhờ thu");
        //                                                                                                                                                                        }
        //                                                                                                                                                                    }
        //                                                                                                                                                                    else
        //                                                                                                                                                                    {
        //                                                                                                                                                                        if (CollectionSuccess.Count > 0)
        //                                                                                                                                                                        {
        //                                                                                                                                                                            foreach (string ItemDelete in CollectionSuccess)
        //                                                                                                                                                                            {
        //                                                                                                                                                                                daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                                            }
        //                                                                                                                                                                        }

        //                                                                                                                                                                        ShowMessageBoxWarning("Lỗi khi thêm thông tin giao dịch nhờ thu");
        //                                                                                                                                                                    }
        //                                                                                                                                                                }
        //                                                                                                                                                                else
        //                                                                                                                                                                {
        //                                                                                                                                                                    if (IVASPropertySuccess.Count > 0)
        //                                                                                                                                                                    {
        //                                                                                                                                                                        foreach (string ItemDelete in IVASPropertySuccess)
        //                                                                                                                                                                        {
        //                                                                                                                                                                            daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                                        }
        //                                                                                                                                                                    }

        //                                                                                                                                                                    ShowMessageBoxWarning("Lỗi khi thêm thông tin DV GTGT");
        //                                                                                                                                                                }
        //                                                                                                                                                            }
        //                                                                                                                                                            else
        //                                                                                                                                                            {
        //                                                                                                                                                                if (VASISuccess.Count > 0)
        //                                                                                                                                                                {
        //                                                                                                                                                                    foreach (string ItemDelete in VASISuccess)
        //                                                                                                                                                                    {
        //                                                                                                                                                                        daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                                    }
        //                                                                                                                                                                }

        //                                                                                                                                                                ShowMessageBoxWarning("Lỗi khi thêm DV GTGT");
        //                                                                                                                                                            }
        //                                                                                                                                                        }
        //                                                                                                                                                        else
        //                                                                                                                                                        {
        //                                                                                                                                                            if (ShiftHandoverItemSuccess.Count > 0)
        //                                                                                                                                                            {
        //                                                                                                                                                                foreach (string ItemDelete in ShiftHandoverItemSuccess)
        //                                                                                                                                                                {
        //                                                                                                                                                                    daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                                }
        //                                                                                                                                                            }

        //                                                                                                                                                            ShowMessageBoxWarning("Lỗi khi thêm bưu gửi vào ca làm việc");
        //                                                                                                                                                        }
        //                                                                                                                                                    }
        //                                                                                                                                                    else
        //                                                                                                                                                    {
        //                                                                                                                                                        if (TraceItemSuccess.Count > 0)
        //                                                                                                                                                        {
        //                                                                                                                                                            foreach (string ItemDelete in TraceItemSuccess)
        //                                                                                                                                                            {
        //                                                                                                                                                                daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                            }
        //                                                                                                                                                        }

        //                                                                                                                                                        ShowMessageBoxWarning("Lỗi khi thêm trạng thái bưu gửi");
        //                                                                                                                                                    }
        //                                                                                                                                                }
        //                                                                                                                                                else
        //                                                                                                                                                {
        //                                                                                                                                                    if (ItemSuccess.Count > 0)
        //                                                                                                                                                    {
        //                                                                                                                                                        foreach (string ItemDelete in ItemSuccess)
        //                                                                                                                                                        {
        //                                                                                                                                                            daoItem.DeleteItemAllBy(ItemDelete);
        //                                                                                                                                                        }
        //                                                                                                                                                    }

        //                                                                                                                                                    ShowMessageBoxWarning("Lỗi khi thêm bưu gửi");
        //                                                                                                                                                }
        //                                                                                                                                            }
        //                                                                                                                                            else
        //                                                                                                                                            {
        //                                                                                                                                                ShowMessageBoxWarning("Lỗi không có thông tin bưu gửi");
        //                                                                                                                                            }
        //                                                                                                                                        }
        //                                                                                                                                    }
        //                                                                                                                                }
        //                                                                                                                            }
        //                                                                                                                        }
        //                                                                                                                    }
        //                                                                                                                }
        //                                                                                                            }
        //                                                                                                        }
        //                                                                                                    }
        //                                                                                                }
        //                                                                                            }
        //                                                                                        }
        //                                                                                    }
        //                                                                                }
        //                                                                            }
        //                                                                        }
        //                                                                    }
        //                                                                }
        //                                                            }
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private void Accept()
        {
            if (CheckShifted())
            {
                return;
            }

            if (!ValidateData())
            {
                return;
            }

            ItemDAO daoItem = new ItemDAO();

            ItemCommodityTypeDAO daoItemCommodityType = new ItemCommodityTypeDAO();

            DetailItemDAO daoDetailItem = new DetailItemDAO();

            ValueAddedServiceItemDAO daoValueAddedServiceItem = new ValueAddedServiceItemDAO();

            ItemVASPropertyValueDAO daoItemVASPropertyValue = new ItemVASPropertyValueDAO();

            ShiftHandoverItemDAO daoShiftHandoverItem = new ShiftHandoverItemDAO();

            TraceItemDAO daoTraceItem = new TraceItemDAO();

            TransactionsCollectionDAO daoTransactionsCollection = new TransactionsCollectionDAO();

            TransactionsCollectionDetailDAO daoTransactionsCollectionDetail = new TransactionsCollectionDetailDAO();

            ItemAdviceOfReceiptDAO daoItemAdviceOfReceipt = new ItemAdviceOfReceiptDAO();

            KT1ExpectedTimeDAO daoKT1ExpectedTimeDAO = new KT1ExpectedTimeDAO();

            SortingItemDAO daoSortingItem = new SortingItemDAO();

            AttachDocumentsItemDAO daoAttachDocumentsItem = new AttachDocumentsItemDAO();
            GetData();

            if (entityItemListGlobal != null && entityItemListGlobal.Count > 0)
            {
                List<string> ItemSuccess = new List<string>();

                List<string> ItemError = new List<string>();

                foreach (ItemEntity ItemValue in entityItemListGlobal)
                {
                    if (daoItem.Save(ItemValue))
                    {
                        ItemSuccess.Add(ItemValue.ItemCode);
                    }
                    else
                    {
                        ItemError.Add(ItemValue.ItemCode);
                    }
                }

                if (ItemError.Count == 0)
                {
                    List<string> TraceItemSuccess = new List<string>();

                    List<string> TraceItemError = new List<string>();

                    if (entityTraceItemListGlobal != null && entityTraceItemListGlobal.Count > 0)
                    {
                        foreach (TraceItemEntity enTraceItemTemp in entityTraceItemListGlobal)
                        {
                            if (daoTraceItem.Save(enTraceItemTemp))
                            {
                                TraceItemSuccess.Add(enTraceItemTemp.ItemCode);
                            }
                            else
                            {
                                TraceItemError.Add(enTraceItemTemp.ItemCode);
                            }
                        }
                    }

                    if (TraceItemError.Count == 0)
                    {
                        List<string> ShiftHandoverItemSuccess = new List<string>();

                        List<string> ShiftHandoverItemError = new List<string>();

                        if (entityShiftHandoverItemListGlobal != null && entityShiftHandoverItemListGlobal.Count > 0)
                        {
                            foreach (ShiftHandoverItemEntity enShiftHandoverItemTemp in entityShiftHandoverItemListGlobal)
                            {
                                if (daoShiftHandoverItem.Save(enShiftHandoverItemTemp))
                                {
                                    ShiftHandoverItemSuccess.Add(enShiftHandoverItemTemp.ItemCode);
                                }
                                else
                                {
                                    ShiftHandoverItemError.Add(enShiftHandoverItemTemp.ItemCode);
                                }
                            }
                        }

                        if (ShiftHandoverItemError.Count == 0)
                        {
                            List<string> VASISuccess = new List<string>();

                            List<string> VASIError = new List<string>();

                            if (entityValueAddedServiceItemListGlobal != null && entityValueAddedServiceItemListGlobal.Count > 0)
                            {
                                foreach (ValueAddedServiceItemEntity enVASITemp in entityValueAddedServiceItemListGlobal)
                                {
                                    if (daoValueAddedServiceItem.Save(enVASITemp))
                                    {
                                        if (!VASISuccess.Contains(enVASITemp.ItemCode))
                                        {
                                            VASISuccess.Add(enVASITemp.ItemCode);
                                        }
                                    }
                                    else
                                    {
                                        if (!VASIError.Contains(enVASITemp.ItemCode))
                                        {
                                            VASIError.Add(enVASITemp.ItemCode);
                                        }
                                    }
                                }
                            }

                            if (VASIError.Count == 0)
                            {
                                List<string> IVASPropertySuccess = new List<string>();

                                List<string> IVASPropertyError = new List<string>();

                                if (entityItemVASPropertyValueListGlobal != null && entityItemVASPropertyValueListGlobal.Count > 0)
                                {
                                    foreach (ItemVASPropertyValueEntity enIVASPropertyTemp in entityItemVASPropertyValueListGlobal)
                                    {
                                        if (daoItemVASPropertyValue.Save(enIVASPropertyTemp))
                                        {
                                            if (!IVASPropertySuccess.Contains(enIVASPropertyTemp.ItemCode))
                                            {
                                                IVASPropertySuccess.Add(enIVASPropertyTemp.ItemCode);
                                            }
                                        }
                                        else
                                        {
                                            if (!IVASPropertyError.Contains(enIVASPropertyTemp.ItemCode))
                                            {
                                                IVASPropertyError.Add(enIVASPropertyTemp.ItemCode);
                                            }
                                        }
                                    }
                                }

                                if (IVASPropertyError.Count == 0)
                                {
                                    List<string> CollectionSuccess = new List<string>();

                                    List<string> CollectionError = new List<string>();

                                    if (entityTransactionsCollectionListGlobal != null && entityTransactionsCollectionListGlobal.Count > 0)
                                    {
                                        foreach (TransactionsCollectionEntity enCollectionTemp in entityTransactionsCollectionListGlobal)
                                        {
                                            if (daoTransactionsCollection.Save(enCollectionTemp))
                                            {
                                                if (!CollectionSuccess.Contains(enCollectionTemp.ItemCode))
                                                {
                                                    CollectionSuccess.Add(enCollectionTemp.ItemCode);
                                                }
                                            }
                                            else
                                            {
                                                if (!CollectionError.Contains(enCollectionTemp.ItemCode))
                                                {
                                                    CollectionError.Add(enCollectionTemp.ItemCode);
                                                }
                                            }
                                        }
                                    }

                                    if (CollectionError.Count == 0)
                                    {
                                        List<string> CollectionDetailSuccess = new List<string>();

                                        List<string> CollectionDetailError = new List<string>();

                                        if (entityTransactionsCollectionDetailListGlobal != null && entityTransactionsCollectionDetailListGlobal.Count > 0)
                                        {
                                            foreach (TransactionsCollectionDetailEntity enCollectionDetailTemp in entityTransactionsCollectionDetailListGlobal)
                                            {
                                                if (daoTransactionsCollectionDetail.Save(enCollectionDetailTemp))
                                                {
                                                    if (!CollectionDetailSuccess.Contains(enCollectionDetailTemp.ItemCode))
                                                    {
                                                        CollectionDetailSuccess.Add(enCollectionDetailTemp.ItemCode);
                                                    }
                                                }
                                                else
                                                {
                                                    if (!CollectionDetailError.Contains(enCollectionDetailTemp.ItemCode))
                                                    {
                                                        CollectionDetailError.Add(enCollectionDetailTemp.ItemCode);
                                                    }
                                                }
                                            }
                                        }

                                        if (CollectionDetailError.Count == 0)
                                        {
                                            List<string> ItemDetailSuccess = new List<string>();

                                            List<string> ItemDetailError = new List<string>();

                                            if (entityDetailItemListGlobal != null && entityDetailItemListGlobal.Count > 0)
                                            {
                                                foreach (DetailItemEntity enItemDetailTemp in entityDetailItemListGlobal.Count > 0)
                                                {
                                                    if (daoDetailItem.Save(enItemDetailTemp))
                                                    {
                                                        if (!DetailItemSuccess.Contains(enItemDetailTemp.ItemCode))
                                                        {
                                                            DetailItemSuccess.Add(enItemDetailTemp.ItemCode);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!DetailItemError.Contains(enItemDetailTemp.ItemCode))
                                                        {
                                                            DetailItemError.Add(enItemDetailTemp.ItemCode);
                                                        }
                                                    }
                                                }
                                            }

                                            if (ItemDetailError.Count == 0)
                                            {
                                                List<string> ItemCommodityTypeSuccess = new List<string>();

                                                List<string> ItemCommodityTypeError = new List<string>();

                                                if (entityItemCommodityTypeListGlobal != null && entityItemCommodityTypeListGlobal.Count > 0)
                                                {
                                                    foreach (ItemCommodityTypeEntity enItemCommodityTypeTemp in entityItemCommodityTypeListGlobal)
                                                    {
                                                        if (daoItemCommodityType.Save(enItemCommodityTypeTemp))
                                                        {
                                                            if (!ItemCommodityTypeSuccess.Contains(enItemCommodityTypeTemp.ItemCode))
                                                            {
                                                                ItemCommodityTypeSuccess.Add(enItemCommodityTypeTemp.ItemCode);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!ItemCommodityTypeError.Contains(enItemCommodityTypeTemp.ItemCode))
                                                            {
                                                                ItemCommodityTypeError.Add(enItemCommodityTypeTemp.ItemCode);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (ItemCommodityTypeError.Count == 0)
                                                {
                                                    List<string> ItemAdviceOfReceiptSuccess = new List<string>();

                                                    List<string> ItemAdviceOfReceiptError = new List<string>();

                                                    if (entityItemAdviceOfReceiptListGlobal != null && entityItemAdviceOfReceiptListGlobal.Count > 0)
                                                    {
                                                        foreach (ItemAdviceOfReceiptEntity entityItemAdviceOfReceiptTemp in entityItemAdviceOfReceiptListGlobal)
                                                        {
                                                            if (daoItemAdviceOfReceipt.Save(entityItemAdviceOfReceiptTemp))
                                                            {
                                                                if (!ItemAdviceOfReceiptSuccess.Contains(entityItemAdviceOfReceiptTemp.ItemCode))
                                                                {
                                                                    ItemAdviceOfReceiptSuccess.Add(entityItemAdviceOfReceiptTemp.ItemCode);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (!ItemAdviceOfReceiptError.Contains(entityItemAdviceOfReceiptTemp.ItemCode))
                                                                {
                                                                    ItemAdviceOfReceiptError.Add(entityItemAdviceOfReceiptTemp.ItemCode);
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (ItemAdviceOfReceiptError.Count == 0)
                                                    {
                                                        List<string> KT1ExpectedTimeSuccess = new List<string>();

                                                        List<string> KT1ExpectedTimeError = new List<string>();

                                                        if (entityKT1ExpectedTimeEntityListGlobal != null && entityKT1ExpectedTimeEntityListGlobal.Count > 0)
                                                        {
                                                            foreach (KT1ExpectedTimeEntity entityKT1ExpectedTimeTemp in entityKT1ExpectedTimeEntityListGlobal)
                                                            {
                                                                if (daoKT1ExpectedTimeDAO.Save(entityKT1ExpectedTimeTemp))
                                                                {
                                                                    if (!KT1ExpectedTimeSuccess.Contains(entityKT1ExpectedTimeTemp.ItemCode))
                                                                    {
                                                                        KT1ExpectedTimeSuccess.Add(entityKT1ExpectedTimeTemp.ItemCode);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (!KT1ExpectedTimeError.Contains(entityKT1ExpectedTimeTemp.ItemCode))
                                                                    {
                                                                        KT1ExpectedTimeError.Add(entityKT1ExpectedTimeTemp.ItemCode);
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (KT1ExpectedTimeError.Count == 0)
                                                        {

                                                            List<string> SortingItemSuccess = new List<string>();

                                                            List<string> SortingItemError = new List<string>();

                                                            if (entitySortingItemListGlobal != null && entitySortingItemListGlobal.Count > 0)
                                                            {
                                                                foreach (SortingItemEntity enSortingItem in entitySortingItemListGlobal)
                                                                {
                                                                    if (enSortingItem.SortingCode != null && enSortingItem.SortingCode.Trim() == "")
                                                                    {
                                                                        entitySortingItemListGlobal.Remove(enSortingItem);
                                                                    }
                                                                }

                                                                foreach (SortingItemEntity entitySortingItemTemp in entitySortingItemListGlobal)
                                                                {
                                                                    if (daoSortingItem.Save(entitySortingItemTemp))
                                                                    {
                                                                        if (!SortingItemSuccess.Contains(entitySortingItemTemp.ItemCode))
                                                                        {
                                                                            SortingItemSuccess.Add(entitySortingItemTemp.ItemCode);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (!SortingItemError.Contains(entitySortingItemTemp.ItemCode))
                                                                        {
                                                                            SortingItemError.Add(entitySortingItemTemp.ItemCode);
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (SortingItemError.Count == 0)
                                                            {

                                                                List<string> AttachDocumentsItemSuccess = new List<string>();

                                                                List<string> AttachDocumentsItemError = new List<string>();

                                                                if (entityAttachDocumentsItemListGlobal != null && entityAttachDocumentsItemListGlobal.Count > 0)
                                                                {
                                                                    foreach (AttachDocumentsItemEntity entityAttachDocumentsItemTemp in entityAttachDocumentsItemListGlobal)
                                                                    {
                                                                        if (daoAttachDocumentsItem.Save(entityAttachDocumentsItemTemp))
                                                                        {
                                                                            if (!AttachDocumentsItemSuccess.Contains(entityAttachDocumentsItemTemp.ItemCode))
                                                                            {
                                                                                AttachDocumentsItemSuccess.Add(entityAttachDocumentsItemTemp.ItemCode);
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            if (!AttachDocumentsItemError.Contains(entityAttachDocumentsItemTemp.ItemCode))
                                                                            {
                                                                                AttachDocumentsItemError.Add(entityAttachDocumentsItemTemp.ItemCode);
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                if (AttachDocumentsItemError.Count == 0)
                                                                {
                                                                    if (ItemSuccess.Count > 0)
                                                                    {
                                                                        foreach (string ItemTransfer in ItemSuccess)
                                                                        {
                                                                            itemListTransferWait.Add(ItemTransfer);
                                                                        }
                                                                    }

                                                                    lblWaitCount.Text = itemListTransferWait.Count.ToString();

                                                                    ShowMessageBoxInformation("Thêm mới bưu gửi thành công");

                                                                    updateDieutin();

                                                                    frmPrintOption frmOption = new frmPrintOption();
                                                                    frmOption.POSCode = this.POSCode;
                                                                    frmOption.OriginalPOSCode = this.OriginalPOSCode;
                                                                    frmOption.ServiceCode = cboService.SelectedValue.ToString();
                                                                    frmOption.Username = this.Username;
                                                                    frmOption.PhaseCode = PhaseConstance.NHAN_GUI_SLL;
                                                                    frmOption.AcceptanceType = AcceptanceTypeConstance.BUU_GUI_SLL;
                                                                    //frmOption.ItemList = entityItemListGlobal;
                                                                    //if (entityItemListGlobal.Count > 0)
                                                                    //{
                                                                    //    ItemEntity eItem = new ItemEntity();
                                                                    //    eItem = entityItemListGlobal[0];
                                                                    //}
                                                                    frmOption.EntityItem = entityItemListGlobal[0];
                                                                    frmOption.DieuTin = true;
                                                                    frmOption.ShowDialog();
                                                                    alwaysAsk = false;
                                                                    this.Close();

                                                                }
                                                                else
                                                                {
                                                                    if (AttachDocumentsItemSuccess.Count > 0)
                                                                    {
                                                                        foreach (string ItemDelete in AttachDocumentsItemSuccess)
                                                                        {
                                                                            daoItem.DeleteItemAllBy(ItemDelete);
                                                                        }
                                                                    }

                                                                    ShowMessageBoxWarning("Lỗi khi thêm thông tin chứng từ hóa đơn");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (SortingItemSuccess.Count > 0)
                                                                {
                                                                    foreach (string ItemDelete in SortingItemSuccess)
                                                                    {
                                                                        daoItem.DeleteItemAllBy(ItemDelete);
                                                                    }
                                                                }

                                                                ShowMessageBoxWarning("Lỗi khi thêm thông tin mã chia bưu gửi");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (KT1ExpectedTimeSuccess.Count > 0)
                                                            {
                                                                foreach (string ItemDelete in KT1ExpectedTimeSuccess)
                                                                {
                                                                    daoItem.DeleteItemAllBy(ItemDelete);
                                                                }
                                                            }

                                                            ShowMessageBoxWarning("Lỗi khi thêm thông tin thời gian KT1");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (ItemAdviceOfReceiptSuccess.Count > 0)
                                                        {
                                                            foreach (string ItemDelete in ItemAdviceOfReceiptSuccess)
                                                            {
                                                                daoItem.DeleteItemAllBy(ItemDelete);
                                                            }
                                                        }

                                                        ShowMessageBoxWarning("Lỗi khi thêm thông tin chứng từ chuyển trả");
                                                    }
                                                }
                                                else
                                                {
                                                    if (ItemCommodityTypeSuccess.Count > 0)
                                                    {
                                                        foreach (string ItemDelete in ItemCommodityTypeSuccess)
                                                        {
                                                            daoItem.DeleteItemAllBy(ItemDelete);
                                                        }
                                                    }

                                                    ShowMessageBoxWarning("Lỗi khi thêm thông tin dịch vụ đặc biệt bưu gửi");
                                                }
                                            }
                                            else
                                            {
                                                if (ItemDetailSuccess.Count > 0)
                                                {
                                                    foreach (string ItemDelete in ItemDetailSuccess)
                                                    {
                                                        daoItem.DeleteItemAllBy(ItemDelete);
                                                    }
                                                }

                                                ShowMessageBoxWarning("Lỗi khi thêm thông tin chi tiết bưu gửi");
                                            }
                                        }
                                        else
                                        {
                                            if (CollectionDetailSuccess.Count > 0)
                                            {
                                                foreach (string ItemDelete in CollectionDetailSuccess)
                                                {
                                                    daoItem.DeleteItemAllBy(ItemDelete);
                                                }
                                            }

                                            ShowMessageBoxWarning("Lỗi khi thêm thông tin giao dịch nhờ thu");
                                        }
                                    }
                                    else
                                    {
                                        if (CollectionSuccess.Count > 0)
                                        {
                                            foreach (string ItemDelete in CollectionSuccess)
                                            {
                                                daoItem.DeleteItemAllBy(ItemDelete);
                                            }
                                        }

                                        ShowMessageBoxWarning("Lỗi khi thêm thông tin giao dịch nhờ thu");
                                    }
                                }
                                else
                                {
                                    if (IVASPropertySuccess.Count > 0)
                                    {
                                        foreach (string ItemDelete in IVASPropertySuccess)
                                        {
                                            daoItem.DeleteItemAllBy(ItemDelete);
                                        }
                                    }

                                    ShowMessageBoxWarning("Lỗi khi thêm thông tin DV GTGT");
                                }
                            }
                            else
                            {
                                if (VASISuccess.Count > 0)
                                {
                                    foreach (string ItemDelete in VASISuccess)
                                    {
                                        daoItem.DeleteItemAllBy(ItemDelete);
                                    }
                                }

                                ShowMessageBoxWarning("Lỗi khi thêm DV GTGT");
                            }
                        }
                        else
                        {
                            if (ShiftHandoverItemSuccess.Count > 0)
                            {
                                foreach (string ItemDelete in ShiftHandoverItemSuccess)
                                {
                                    daoItem.DeleteItemAllBy(ItemDelete);
                                }
                            }

                            ShowMessageBoxWarning("Lỗi khi thêm bưu gửi vào ca làm việc");
                        }
                    }
                    else
                    {
                        if (TraceItemSuccess.Count > 0)
                        {
                            foreach (string ItemDelete in TraceItemSuccess)
                            {
                                daoItem.DeleteItemAllBy(ItemDelete);
                            }
                        }

                        ShowMessageBoxWarning("Lỗi khi thêm trạng thái bưu gửi");
                    }
                }
                else
                {
                    if (ItemSuccess.Count > 0)
                    {
                        foreach (string ItemDelete in ItemSuccess)
                        {
                            daoItem.DeleteItemAllBy(ItemDelete);
                        }
                    }

                    ShowMessageBoxWarning("Lỗi khi thêm bưu gửi");
                }
            }
            else
            {
                ShowMessageBoxWarning("Lỗi không có thông tin bưu gửi");
            }

        }


        static DataTable GetTable()
        {
            // Here we create a DataTable with four columns.
            DataTable table = new DataTable();
            table.Columns.Add("itemCode", typeof(string));
            table.Columns.Add("customerCode", typeof(string));
            table.Columns.Add("dataCode", typeof(string));
            return table;
        }

        private void updateDieutin()
        {
            try
            {
                DataTable dtItem = GetTable();
                foreach (var item in entityItemListGlobal)
                {

                    dtItem.Rows.Add(item.ItemCode, item.CustomerCode, item.DataCode);

                }
                if (dtItem.Rows.Count > 0)
                {
                    string postData = JsonConvert.SerializeObject(dtItem);
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);



                    string ulr = string.Format(Base_ulr_get + "/serviceApi/v1/UpdateListDataToBccp?POSCode={0}", this.POSCode);
                    var request = WebRequest.Create(ulr) as HttpWebRequest;
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse response = request.GetResponse();



                    dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();

                    reader.Close();
                    response.Close();
                    dataStream.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool CheckShifted()
        {
            bool result = false;

            try
            {
                if (ShiftHandover != null)
                {
                    ShiftHandoverDAO oShiftHandOver = new ShiftHandoverDAO();

                    ShiftHandoverEntity eShiftHandover = oShiftHandOver.SelectOne(this.ShiftHandover.HandoverIndex, this.ShiftHandover.ShiftCode, this.ShiftHandover.CounterCode, this.ShiftHandover.POSCode);

                    if (eShiftHandover != null)
                    {
                        if (eShiftHandover.HandoverTime != null && eShiftHandover.HandoverTime > new DateTime(2000, 01, 01))
                        {
                            ShowMessageBoxWarning("Ca làm việc hiện tại đã chốt. Yêu cầu đăng xuất và đăng nhập vào ca đang làm việc.");

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            { }

            return result;
        }

        private void ConvertWeight()
        {
            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                //Tinh khoi luong quy doi
                double iLongResult = 0;
                double iWideResult = 0;
                double iHighResult = 0;

                bool bLongResult = false;
                bool bWideResult = false;
                bool bHighResult = false;

                if (rows.Cells["colLength"].Value != null)
                {
                    bLongResult = double.TryParse(rows.Cells["colLength"].Value.ToString(), out iLongResult);
                }

                if (rows.Cells["colWidth"].Value != null)
                {
                    bWideResult = double.TryParse(rows.Cells["colWidth"].Value.ToString(), out iWideResult);
                }

                if (rows.Cells["colHeight"].Value != null)
                {
                    bHighResult = double.TryParse(rows.Cells["colHeight"].Value.ToString(), out iHighResult);
                }

                if (bLongResult && bWideResult && bHighResult)
                {
                    if (iLongResult != 0 && iWideResult != 0 && iHighResult != 0)
                    {
                        double dConvert = (iLongResult * iWideResult * iHighResult);

                        ServiceDAO daoServie = new ServiceDAO();
                        ServiceEntity eService = daoServie.SelectOne(cboService.SelectedValue.ToString());

                        if (eService != null)
                        {
                            if (rows.Cells["colisAir"].Value != null)
                            {
                                if (Convert.ToBoolean(rows.Cells["colisAir"].Value.ToString()))
                                {
                                    if (!eService.IsNullAirCoefficientConvertWeight && eService.AirCoefficientConvertWeight != 0)
                                    {
                                        rows.Cells["colConvertWeight"].Value = NumberFormat(Math.Round((dConvert / eService.AirCoefficientConvertWeight) * 1000, MidpointRounding.AwayFromZero));
                                    }
                                    else
                                    {
                                        rows.Cells["colConvertWeight"].Value = "";
                                    }
                                }
                                else
                                {
                                    if (!eService.IsNullSurfaceCoefficientConvertWeight && eService.SurfaceCoefficientConvertWeight != 0)
                                    {
                                        rows.Cells["colConvertWeight"].Value = NumberFormat(Math.Round((dConvert / eService.SurfaceCoefficientConvertWeight) * 1000, MidpointRounding.AwayFromZero));
                                    }
                                    else
                                    {
                                        rows.Cells["colConvertWeight"].Value = "";
                                    }
                                }
                            }
                            else
                            {
                                if (!eService.IsNullSurfaceCoefficientConvertWeight && eService.SurfaceCoefficientConvertWeight != 0)
                                {
                                    rows.Cells["colConvertWeight"].Value = NumberFormat(Math.Round((dConvert / eService.SurfaceCoefficientConvertWeight) * 1000, MidpointRounding.AwayFromZero));
                                }
                                else
                                {
                                    rows.Cells["colConvertWeight"].Value = "";
                                }
                            }
                        }
                        else
                        {
                            rows.Cells["colConvertWeight"].Value = "";
                        }
                    }
                    else
                    {
                        rows.Cells["colConvertWeight"].Value = "";
                    }

                }
                else
                {
                    rows.Cells["colConvertWeight"].Value = "";
                }
            }

        }

        private void CalculatorTotalItem()
        {
            lblTotalItems.Text = NumberFormat(dgvListItems.Rows.Count);
        }

        private void CalculatorTotalWeight()
        {
            double dTotalWeight = 0;

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colWeight"].Value != null && !string.IsNullOrEmpty(rows.Cells["colWeight"].Value.ToString()))
                {
                    double dResult;
                    if (double.TryParse(rows.Cells["colWeight"].Value.ToString(), out dResult))
                    {
                        dTotalWeight += dResult;
                    }
                }
            }

            lblTotalWeights.Text = NumberFormat(dTotalWeight) + " (gr)";
        }

        private void CalculatorTotalFreight()
        {
            double dTotalFreight = 0;

            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                if (rows.Cells["colTotalFreightDiscountVAT"].Value != null && !string.IsNullOrEmpty(rows.Cells["colTotalFreightDiscountVAT"].Value.ToString()))
                {
                    double dResult;
                    if (double.TryParse(rows.Cells["colTotalFreightDiscountVAT"].Value.ToString(), out dResult))
                    {
                        dTotalFreight += dResult;
                    }
                }
            }

            lblTotalFreight.Text = NumberFormat(dTotalFreight) + " - Bằng chữ: " + Common.ReadMoney(dTotalFreight); ;
        }

        private string GetDestinationCode(string CommuneCode, string DistrictCode, string ProvinceCode)
        {
            //Lấy bưu cục nhận tính cước
            string strDestinationCode = "";

            if (!string.IsNullOrEmpty(CommuneCode))
            {
                string destinationCommune = GetDestinationCodeByCommune(CommuneCode);

                if (!string.IsNullOrEmpty(destinationCommune))
                {
                    strDestinationCode = destinationCommune;
                }
                else
                {
                    if (!string.IsNullOrEmpty(DistrictCode))
                    {
                        string destinationDistrict = GetDestinationCodeByDistrict(DistrictCode);

                        if (!string.IsNullOrEmpty(destinationDistrict))
                        {
                            strDestinationCode = destinationDistrict;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ProvinceCode))
                            {
                                string destionationProvince = GetDestinationCodeByProvince(ProvinceCode);

                                if (!string.IsNullOrEmpty(destionationProvince))
                                {
                                    strDestinationCode = destionationProvince;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ProvinceCode))
                        {
                            string destionationProvince = GetDestinationCodeByProvince(ProvinceCode);

                            if (!string.IsNullOrEmpty(destionationProvince))
                            {
                                strDestinationCode = destionationProvince;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(DistrictCode))
                {
                    string destinationDistrict = GetDestinationCodeByDistrict(DistrictCode);

                    if (!string.IsNullOrEmpty(destinationDistrict))
                    {
                        strDestinationCode = destinationDistrict;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ProvinceCode))
                        {
                            string destionationProvince = GetDestinationCodeByProvince(ProvinceCode);

                            if (!string.IsNullOrEmpty(destionationProvince))
                            {
                                strDestinationCode = destionationProvince;
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ProvinceCode))
                    {
                        string destionationProvince = GetDestinationCodeByProvince(ProvinceCode);

                        if (!string.IsNullOrEmpty(destionationProvince))
                        {
                            strDestinationCode = destionationProvince;
                        }
                    }
                }
            }

            return strDestinationCode;
        }

        private string GetDestinationCodeByCommune(string CommuneCode)
        {
            //Lấy bưu cục nhận tính cước theo xa
            string strDestinationCode = "";

            if (!string.IsNullOrEmpty(CommuneCode))
            {
                POSDAO daoPOS = new POSDAO();

                DataTable dtPOSCommuneGD1 = daoPOS.SelectAllDSFilter("CommuneCode = '" + CommuneCode + "' AND POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];

                if (dtPOSCommuneGD1.Rows.Count > 0)
                {
                    strDestinationCode = dtPOSCommuneGD1.Rows[0]["POSCode"].ToString();
                }
                else
                {
                    DataTable dtPOSCommuneGD2 = daoPOS.SelectAllDSFilter("CommuneCode = '" + CommuneCode + "' AND POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                    if (dtPOSCommuneGD2.Rows.Count > 0)
                    {
                        strDestinationCode = dtPOSCommuneGD2.Rows[0]["POSCode"].ToString();
                    }
                    else
                    {
                        DataTable dtPOSCommuneGD3 = daoPOS.SelectAllDSFilter("CommuneCode = '" + CommuneCode + "' AND POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];

                        if (dtPOSCommuneGD3.Rows.Count > 0)
                        {
                            strDestinationCode = dtPOSCommuneGD3.Rows[0]["POSCode"].ToString();
                        }
                        else
                        {
                            DataTable dtPOSCommuneKT1 = daoPOS.SelectAllDSFilter("CommuneCode = '" + CommuneCode + "' AND POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];

                            if (dtPOSCommuneKT1.Rows.Count > 0)
                            {
                                strDestinationCode = dtPOSCommuneKT1.Rows[0]["POSCode"].ToString();
                            }
                            else
                            {
                                DataTable dtPOSCommuneKT2 = daoPOS.SelectAllDSFilter("CommuneCode = '" + CommuneCode + "' AND POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];

                                if (dtPOSCommuneKT2.Rows.Count > 0)
                                {
                                    strDestinationCode = dtPOSCommuneKT2.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtPOSCommune = daoPOS.SelectAllDSFilter("CommuneCode = '" + CommuneCode + "'").Tables[0];

                                    if (dtPOSCommune.Rows.Count > 0)
                                    {
                                        strDestinationCode = dtPOSCommune.Rows[0]["POSCode"].ToString();
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                        }

                    }
                }
            }

            return strDestinationCode;
        }

        private string GetDestinationCodeByDistrict(string DistrictCode)
        {
            //Lấy bưu cục nhận tính cước theo huyen
            string strDestinationCode = "";

            if (!string.IsNullOrEmpty(DistrictCode))
            {
                POSDAO daoPOS = new POSDAO();

                DataTable dtPOSDistrictGD1 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + DistrictCode + "' AND POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];

                if (dtPOSDistrictGD1.Rows.Count > 0)
                {
                    strDestinationCode = dtPOSDistrictGD1.Rows[0]["POSCode"].ToString();
                }
                else
                {
                    DataTable dtPOSDistrictGD2 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + DistrictCode + "' AND POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                    if (dtPOSDistrictGD2.Rows.Count > 0)
                    {
                        strDestinationCode = dtPOSDistrictGD2.Rows[0]["POSCode"].ToString();
                    }
                    else
                    {
                        DataTable dtPOSDistrictGD3 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + DistrictCode + "' AND POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];

                        if (dtPOSDistrictGD3.Rows.Count > 0)
                        {
                            strDestinationCode = dtPOSDistrictGD3.Rows[0]["POSCode"].ToString();
                        }
                        else
                        {
                            DataTable dtPOSDistrictKT1 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + DistrictCode + "' AND POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];

                            if (dtPOSDistrictKT1.Rows.Count > 0)
                            {
                                strDestinationCode = dtPOSDistrictKT1.Rows[0]["POSCode"].ToString();
                            }
                            else
                            {
                                DataTable dtPOSDistrictKT2 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + DistrictCode + "' AND POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];

                                if (dtPOSDistrictKT2.Rows.Count > 0)
                                {
                                    strDestinationCode = dtPOSDistrictKT2.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtPOSDistrict = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + DistrictCode + "'").Tables[0];

                                    if (dtPOSDistrict.Rows.Count > 0)
                                    {
                                        strDestinationCode = dtPOSDistrict.Rows[0]["POSCode"].ToString();
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }

                    }
                }
            }

            return strDestinationCode;
        }

        private string GetDestinationCodeByProvince(string ProvinceCode)
        {
            //Lấy bưu cục nhận tính cước theo tinh
            string strDestinationCode = "";

            if (!string.IsNullOrEmpty(ProvinceCode))
            {
                POSDAO daoPOS = new POSDAO();

                DataTable dtProvinceGD1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + ProvinceCode + "' AND POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];

                if (dtProvinceGD1.Rows.Count > 0)
                {
                    strDestinationCode = dtProvinceGD1.Rows[0]["POSCode"].ToString();
                }
                else
                {
                    DataTable dtProvinceGD2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + ProvinceCode + "' AND POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];

                    if (dtProvinceGD2.Rows.Count > 0)
                    {
                        strDestinationCode = dtProvinceGD2.Rows[0]["POSCode"].ToString();
                    }
                    else
                    {
                        DataTable dtProvinceGD3 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + ProvinceCode + "' AND POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];

                        if (dtProvinceGD3.Rows.Count > 0)
                        {
                            strDestinationCode = dtProvinceGD3.Rows[0]["POSCode"].ToString();
                        }
                        else
                        {
                            DataTable dtProvinceKT1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + ProvinceCode + "' AND POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];

                            if (dtProvinceKT1.Rows.Count > 0)
                            {
                                strDestinationCode = dtProvinceKT1.Rows[0]["POSCode"].ToString();
                            }
                            else
                            {
                                DataTable dtProvinceKT2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + ProvinceCode + "' AND POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];

                                if (dtProvinceKT2.Rows.Count > 0)
                                {
                                    strDestinationCode = dtProvinceKT2.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtProvince = daoPOS.SelectAllDSFilter("ProvinceCode = '" + ProvinceCode + "'").Tables[0];

                                    if (dtProvince.Rows.Count > 0)
                                    {
                                        strDestinationCode = dtProvince.Rows[0]["POSCode"].ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return strDestinationCode;
        }

        private void displayFreight()
        {
            foreach (DataGridViewRow rows in dgvListItems.Rows)
            {
                List<ValueAddedServiceItemEntity> entityValueAddedServiceItemList = new List<ValueAddedServiceItemEntity>();

                bool _domestic = true;

                double _VATFreight = 0;//Biến dùng để lưu thuế VAT
                double _VATDiscountFreight = 0;//Biến dùng để lưu thuế VAT sau chiet khau
                double _VATPercentage = 0; //Biến dùng để lưu %VAT

                double _MainFreight = 0;//Biến dùng để lưu cước chính
                double _AddedFreight = 0;//Biến dùng để lưu cước cộng thêm

                double _CODAmount = 0; //So tien thu ho
                double _CODFreight = 0;//Biến dùng để lưu cước cộng thêm
                double _CODVATFreight = 0;//Biến dùng để lưu cước cộng thêm
                double _CODSubFreight = 0;
                double _CODVATSubFreight = 0;

                double _FuelSurchargeFreight = 0;//Biến dùng để lưu phụ phí xang dau
                double _FarRegionFreight = 0;//Biến dùng để lưu phụ phí vung sau, vung xa
                double _AirSurchargeFreight = 0;//Biến dùng để lưu phụ phí máy bay

                double _DiscountFreight = 0; //Cuoc chiet khau

                double _TotalFreight = 0;//Biến dùng để lưu tổng tiền cước chưa vat, chưa chiết khấu
                double _TotalFreightVAT = 0;//Biến dùng để lưu tổng tiền cước có vat, chưa chiết khấu
                double _TotalFreightDiscount = 0;//Biến dùng để lưu tổng tiền cước  chưa vat, có chiết khấu
                double _TotalFreightDiscountVAT = 0;//Biến dùng để lưu tổng tiền cước có vat, có chiết khấu

                double _PaymentFreight = 0; //Biến dùng để lưu tổng tiền phải thu của người gửi chua vat chua chiet khau
                double _PaymentFreightVAT = 0; //Biến dùng để lưu tổng tiền phải thu của người gửi co vat chua chiet khau
                double _PaymentFreightDiscount = 0;//Biến dùng để lưu tổng tiền phải thu của người gửi chua vat co chiet khau
                double _PaymentFreightDiscountVAT = 0;//Biến dùng để lưu tổng tiền phải thu của người gửi co vat co chiet khau

                double _RemainingFreight = 0; //Biến dùng để lưu tổng tiền phải thu của người nhận
                double _RemainingFreightVAT = 0; //Biến dùng để lưu tổng tiền phải thu của người nhận co vat
                double _RemainingFreightDiscount = 0;//Biến dùng để lưu tổng tiền phải thu của người nhận chua vat co chiet khau
                double _RemainingFreightDiscountVAT = 0;//Biến dùng để lưu tổng tiền phải thu của người nhận co vat co chiet khau

                // Khai báo biến lưu cước theo bảng cước gốc

                double _OriginalVATFreight = 0;//Biến dùng để lưu thuế VAT
                double _OriginalVATDiscountFreight = 0;//Biến dùng để lưu thuế VAT sau chiet khau
                double _OriginalVATPercentage = 0; //Biến dùng để lưu %VAT

                double _OriginalMainFreight = 0;//Biến dùng để lưu cước chính
                double _OriginalAddedFreight = 0;//Biến dùng để lưu cước cộng thêm

                double _OriginalCODAmount = 0; //So tien thu ho
                double _OriginalCODFreight = 0;//Biến dùng để lưu cước cộng thêm
                double _OriginalCODVATFreight = 0;//Biến dùng để lưu cước cộng thêm
                double _OriginalCODSubFreight = 0;
                double _OriginalCODVATSubFreight = 0;

                double _OriginalFuelSurchargeFreight = 0;//Biến dùng để lưu phụ phí xang dau
                double _OriginalFarRegionFreight = 0;//Biến dùng để lưu phụ phí vung sau, vung xa
                double _OriginalAirSurchargeFreight = 0;//Biến dùng để lưu phụ phí máy bay
                double _OriginalOtherFreight = 0;//Biến dùng để lưu cac loai cuoc khac

                double _OriginalDiscountFreight = 0; //Cuoc chiet khau
                double _OriginalFeedbackFreight = 0; //Cuoc trich thuong

                double _OriginalTotalFreight = 0;//Biến dùng để lưu tổng tiền cước chưa vat, chưa chiết khấu
                double _OriginalTotalFreightVAT = 0;//Biến dùng để lưu tổng tiền cước có vat, chưa chiết khấu
                double _OriginalTotalFreightDiscount = 0;//Biến dùng để lưu tổng tiền cước  chưa vat, có chiết khấu
                double _OriginalTotalFreightDiscountVAT = 0;//Biến dùng để lưu tổng tiền cước có vat, có chiết khấu

                double _OriginalPaymentFreight = 0; //Biến dùng để lưu tổng tiền phải thu của người gửi chua vat chua chiet khau
                double _OriginalPaymentFreightVAT = 0; //Biến dùng để lưu tổng tiền phải thu của người gửi co vat chua chiet khau
                double _OriginalPaymentFreightDiscount = 0;//Biến dùng để lưu tổng tiền phải thu của người gửi chua vat co chiet khau
                double _OriginalPaymentFreightDiscountVAT = 0;//Biến dùng để lưu tổng tiền phải thu của người gửi co vat co chiet khau

                double _OriginalRemainingFreight = 0; //Biến dùng để lưu tổng tiền phải thu của người nhận
                double _OriginalRemainingFreightVAT = 0; //Biến dùng để lưu tổng tiền phải thu của người nhận co vat
                double _OriginalRemainingFreightDiscount = 0;//Biến dùng để lưu tổng tiền phải thu của người nhận chua vat co chiet khau
                double _OriginalRemainingFreightDiscountVAT = 0;//Biến dùng để lưu tổng tiền phải thu của người nhận co vat co chiet khau

                double _FundFreight = 0; //Cước giá vốn của cước chính
                double _FundVASFreight = 0; //Cước giá vốn của DV GTGT

                bool bSenderPostage = true;
                bool bSenderCODPostage = true;
                bool bThuCuocNguoiNhan = false;

                rows.Cells["colMainFreight"].Value = NumberFormat(_MainFreight);
                rows.Cells["colSubFreight"].Value = NumberFormat(_AddedFreight);

                rows.Cells["colFuelSurchargeFreight"].Value = NumberFormat(_FuelSurchargeFreight);
                rows.Cells["colFarRegionFreight"].Value = NumberFormat(_FarRegionFreight);
                rows.Cells["colAirSurchargeFreight"].Value = NumberFormat(_AirSurchargeFreight);

                rows.Cells["colTotalFreight"].Value = NumberFormat(_TotalFreight);
                rows.Cells["colTotalFreightVAT"].Value = NumberFormat(_TotalFreightVAT);
                rows.Cells["colTotalFreightDiscount"].Value = NumberFormat(_TotalFreightDiscount);
                rows.Cells["colTotalFreightDiscountVAT"].Value = NumberFormat(_TotalFreightDiscountVAT);

                rows.Cells["colVATPercentage"].Value = NumberFormat(_VATPercentage);
                rows.Cells["colVATFreight"].Value = NumberFormat(_VATFreight);

                rows.Cells["colPaymentFreight"].Value = NumberFormat(_PaymentFreight);
                rows.Cells["colPaymentFreightVAT"].Value = NumberFormat(_PaymentFreightVAT);
                rows.Cells["colPaymentFreightDiscount"].Value = NumberFormat(_PaymentFreightDiscount);
                rows.Cells["colPaymentFreightDiscountVAT"].Value = NumberFormat(_PaymentFreightDiscountVAT);

                rows.Cells["colRemainingFreight"].Value = NumberFormat(_RemainingFreight);
                rows.Cells["colRemainingFreightVAT"].Value = NumberFormat(_RemainingFreightVAT);
                rows.Cells["colRemainingFreightDiscount"].Value = NumberFormat(_RemainingFreightDiscount);
                rows.Cells["colRemainingFreightDiscountVAT"].Value = NumberFormat(_RemainingFreightDiscountVAT);

                rows.Cells["colOriginalMainFreight"].Value = NumberFormat(_OriginalMainFreight);
                rows.Cells["colOriginalSubFreight"].Value = NumberFormat(_OriginalAddedFreight);

                rows.Cells["colOriginalFuelSurchargeFreight"].Value = NumberFormat(_OriginalFuelSurchargeFreight);
                rows.Cells["colOriginalFarRegionFreight"].Value = NumberFormat(_OriginalFarRegionFreight);
                rows.Cells["colOriginalAirSurchargeFreight"].Value = NumberFormat(_OriginalAirSurchargeFreight);

                rows.Cells["colOriginalVATFreight"].Value = NumberFormat(_OriginalVATFreight);
                rows.Cells["colOriginalVATPercentage"].Value = NumberFormat(_OriginalVATPercentage);

                rows.Cells["colOriginalTotalFreight"].Value = NumberFormat(_OriginalTotalFreight);
                rows.Cells["colOriginalTotalFreightVAT"].Value = NumberFormat(_OriginalTotalFreightVAT);
                rows.Cells["colOriginalTotalFreightDiscount"].Value = NumberFormat(_OriginalTotalFreightDiscount);
                rows.Cells["colOriginalTotalFreightDiscountVAT"].Value = NumberFormat(_OriginalTotalFreightDiscountVAT);

                rows.Cells["colOriginalPaymentFreight"].Value = NumberFormat(_OriginalPaymentFreight);
                rows.Cells["colOriginalPaymentFreightVAT"].Value = NumberFormat(_OriginalPaymentFreightVAT);
                rows.Cells["colOriginalPaymentFreightDiscount"].Value = NumberFormat(_OriginalPaymentFreightDiscount);
                rows.Cells["colOriginalPaymentFreightDiscountVAT"].Value = NumberFormat(_OriginalPaymentFreightDiscountVAT);

                rows.Cells["colOriginalRemainingFreight"].Value = NumberFormat(_OriginalRemainingFreight);
                rows.Cells["colOriginalRemainingFreightVAT"].Value = NumberFormat(_OriginalRemainingFreightVAT);
                rows.Cells["colOriginalRemainingFreightDiscount"].Value = NumberFormat(_OriginalRemainingFreightDiscount);
                rows.Cells["colOriginalRemainingFreightDiscountVAT"].Value = NumberFormat(_OriginalRemainingFreightDiscountVAT);

                rows.Cells["colFundFreight"].Value = NumberFormat(_FundFreight);
                rows.Cells["colFundVASFreight"].Value = NumberFormat(_FundVASFreight);

                string serviceCode = "";
                if (cboService.SelectedValue != null)
                    serviceCode = cboService.SelectedValue.ToString();

                string itemType = "";
                if (rows.Cells["colItemType"].Value != null && !string.IsNullOrEmpty(rows.Cells["colItemType"].Value.ToString()))
                    itemType = rows.Cells["colItemType"].Value.ToString();

                double weight = 0;
                if (rows.Cells["colWeight"].Value != null && !string.IsNullOrEmpty(rows.Cells["colWeight"].Value.ToString()))
                {
                    double dWeightResult;
                    if (double.TryParse(rows.Cells["colWeight"].Value.ToString(), out dWeightResult))
                        weight = dWeightResult;
                }

                double convertWeight = 0;
                if (rows.Cells["colConvertWeight"].Value != null && !string.IsNullOrEmpty(rows.Cells["colConvertWeight"].Value.ToString()))
                {
                    double dConvertWeightResult;
                    if (double.TryParse(rows.Cells["colConvertWeight"].Value.ToString(), out dConvertWeightResult))
                    {
                        convertWeight = dConvertWeightResult;
                    }
                }

                //Dungnt - Them loai hang nhe
                List<string> commodityTypeList = new List<string>();

                if (rows.Cells["colComodityType"].Value != null && !string.IsNullOrEmpty(rows.Cells["colComodityType"].Value.ToString()))
                {
                    if (itemType.Equals(ItemTypeConstance.CMTND) ||
                    itemType.Equals(ItemTypeConstance.KET_QUA_XET_NGHIEM) ||
                    itemType.Equals(ItemTypeConstance.DANG_KY_XE_CO_GIOI) ||
                    itemType.Equals(ItemTypeConstance.HO_KHAU) ||
                    itemType.Equals(ItemTypeConstance.HO_SO_VA_GIAY_PHEP_LAI_XE) ||
                    itemType.Equals(ItemTypeConstance.HO_SO_XET_TUYEN) ||
                    itemType.Equals(ItemTypeConstance.HO_SO_TU_PHAP) ||
                    itemType.Equals(ItemTypeConstance.CDHC))
                    {
                        commodityTypeList = new List<string>();
                    }
                    else
                    {
                        if (rows.Cells["colComodityType"].Value.ToString().Contains(CommodityTypeConstance.HANG_NHE))
                        {
                            commodityTypeList.Add(CommodityTypeConstance.HANG_NHE);
                        }
                        else
                        {
                            foreach (var itemComodityType in rows.Cells["colComodityType"].Value.ToString().Split(Convert.ToChar(";")))
                            {
                                commodityTypeList.Add(itemComodityType);
                            }
                        }
                    }
                }

                string transport = TransportIndicatorConstance.THUY_BO;

                if (rows.Cells["colisAir"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colisAir"].Value.ToString()))
                    {
                        transport = TransportIndicatorConstance.MAY_BAY;
                    }
                }

                bool farRegion = false;
                if (rows.Cells["colFarRegion"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colFarRegion"].Value.ToString()))
                    {
                        farRegion = true;
                    }
                }

                string destinationCode = "";
                string provinceCode = "";
                string districtCode = "";
                string communeCode = "";

                if (rows.Cells["colCountryCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colCountryCode"].Value.ToString()))
                {
                    destinationCode = rows.Cells["colCountryCode"].Value.ToString();

                    _domestic = false;
                }
                else
                {
                    #region cach cu
                    /*
                    if (rows.Cells["colDistrictCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colDistrictCode"].Value.ToString()))
                    {
                        districtCode = rows.Cells["colDistrictCode"].Value.ToString();

                        if (rows.Cells["colCommuneCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colCommuneCode"].Value.ToString()))
                        {
                            communeCode = rows.Cells["colCommuneCode"].Value.ToString();
                        }

                        POSDAO daoPOS = new POSDAO();

                        DataTable dtPOSDistrictGD1 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];
                        if (dtPOSDistrictGD1.Rows.Count > 0)
                        {
                            destinationCode = dtPOSDistrictGD1.Rows[0]["POSCode"].ToString();
                        }
                        else
                        {
                            DataTable dtPOSDistrictGD2 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                            if (dtPOSDistrictGD2.Rows.Count > 0)
                            {
                                destinationCode = dtPOSDistrictGD2.Rows[0]["POSCode"].ToString();
                            }
                            else
                            {
                                DataTable dtPOSDistrictGD3 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD3 + "'").Tables[0];
                                if (dtPOSDistrictGD3.Rows.Count > 0)
                                {
                                    destinationCode = dtPOSDistrictGD3.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtPOSDistrictKT1 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];
                                    if (dtPOSDistrictKT1.Rows.Count > 0)
                                    {
                                        destinationCode = dtPOSDistrictKT1.Rows[0]["POSCode"].ToString();
                                    }
                                    else
                                    {
                                        DataTable dtPOSDistrictKT2 = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];
                                        if (dtPOSDistrictKT2.Rows.Count > 0)
                                        {
                                            destinationCode = dtPOSDistrictKT2.Rows[0]["POSCode"].ToString();
                                        }
                                        else
                                        {
                                            DataTable dtPOSDistrict = daoPOS.SelectAllDSWithCommuneFilter("DistrictCode = '" + rows.Cells["colDistrictCode"].Value.ToString() + "'").Tables[0];
                                            if (dtPOSDistrict.Rows.Count > 0)
                                            {
                                                destinationCode = dtPOSDistrict.Rows[0]["POSCode"].ToString();
                                            }
                                            else
                                            {
                                                if (rows.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                                                {
                                                    DataTable dtProvinceGD1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];
                                                    if (dtProvinceGD1.Rows.Count > 0)
                                                    {
                                                        destinationCode = dtProvinceGD1.Rows[0]["POSCode"].ToString();
                                                    }
                                                    else
                                                    {
                                                        DataTable dtProvinceGD2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                                                        if (dtProvinceGD2.Rows.Count > 0)
                                                        {
                                                            destinationCode = dtProvinceGD2.Rows[0]["POSCode"].ToString();
                                                        }
                                                        else
                                                        {
                                                            DataTable dtProvinceKT1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];
                                                            if (dtProvinceKT1.Rows.Count > 0)
                                                            {
                                                                destinationCode = dtProvinceKT1.Rows[0]["POSCode"].ToString();
                                                            }
                                                            else
                                                            {
                                                                DataTable dtProvinceKT2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];
                                                                if (dtProvinceKT2.Rows.Count > 0)
                                                                {
                                                                    destinationCode = dtProvinceKT2.Rows[0]["POSCode"].ToString();
                                                                }
                                                                else
                                                                {
                                                                    DataTable dtProvince = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "'").Tables[0];
                                                                    if (dtProvince.Rows.Count > 0)
                                                                    {
                                                                        destinationCode = dtProvince.Rows[0]["POSCode"].ToString();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rows.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                        {
                            POSDAO daoPOS = new POSDAO();

                            DataTable dtProvinceGD1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD1 + "'").Tables[0];
                            if (dtProvinceGD1.Rows.Count > 0)
                            {
                                destinationCode = dtProvinceGD1.Rows[0]["POSCode"].ToString();
                            }
                            else
                            {
                                DataTable dtProvinceGD2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.GD2 + "'").Tables[0];
                                if (dtProvinceGD2.Rows.Count > 0)
                                {
                                    destinationCode = dtProvinceGD2.Rows[0]["POSCode"].ToString();
                                }
                                else
                                {
                                    DataTable dtProvinceKT1 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT1 + "'").Tables[0];
                                    if (dtProvinceKT1.Rows.Count > 0)
                                    {
                                        destinationCode = dtProvinceKT1.Rows[0]["POSCode"].ToString();
                                    }
                                    else
                                    {
                                        DataTable dtProvinceKT2 = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "' And POSLevelCode = '" + POSLevelConstance.KT2 + "'").Tables[0];
                                        if (dtProvinceKT2.Rows.Count > 0)
                                        {
                                            destinationCode = dtProvinceKT2.Rows[0]["POSCode"].ToString();
                                        }
                                        else
                                        {
                                            DataTable dtProvince = daoPOS.SelectAllDSFilter("ProvinceCode = '" + rows.Cells["colProvinceCode"].Value.ToString() + "'").Tables[0];
                                            if (dtProvince.Rows.Count > 0)
                                            {
                                                destinationCode = dtProvince.Rows[0]["POSCode"].ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    */
                    #endregion

                    if (rows.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colProvinceCode"].Value.ToString()))
                    {
                        provinceCode = rows.Cells["colProvinceCode"].Value.ToString();
                    }

                    if (rows.Cells["colDistrictCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colDistrictCode"].Value.ToString()))
                    {
                        districtCode = rows.Cells["colDistrictCode"].Value.ToString();
                    }

                    if (rows.Cells["colCommuneCode"].Value != null && !string.IsNullOrEmpty(rows.Cells["colCommuneCode"].Value.ToString()))
                    {
                        communeCode = rows.Cells["colCommuneCode"].Value.ToString();
                    }

                    destinationCode = GetDestinationCode(communeCode, districtCode, provinceCode);
                }

                eBillingInput.ServiceCode = cboService.SelectedValue.ToString();
                eBillingInput.ItemType = itemType;

                eBillingInput.StartingCode = this.POSCode;
                eBillingInput.NumberItems = 1;

                List<Items> listWeight = new List<Items>();
                Items item = new Items();
                if (weight > convertWeight)
                {
                    eBillingInput.Weight = weight;

                    item.Weight = weight;
                }
                else
                {
                    eBillingInput.Weight = convertWeight;

                    item.Weight = convertWeight;
                }

                listWeight.Add(item);

                eBillingInput.lsItems = listWeight.ToArray();

                eBillingInput.SendingTime = dtpFromDate.Value;

                eBillingInput.CommodityTypeCodes = commodityTypeList.ToArray();

                if (rows.Cells["colCustomerCode"].Value != null)
                    eBillingInput.CustomerCode = rows.Cells["colCustomerCode"].Value.ToString();
                else
                    eBillingInput.CustomerCode = "";

                eBillingInput.CommodityNumbers = 0;
                eBillingInput.Transport = transport;
                eBillingInput.DestinationCode = destinationCode;
                eBillingInput.CODAmount = 0;
                eBillingInput.DistrictDestination = districtCode;
                eBillingInput.CommuneDestination = communeCode;
                eBillingInput.FarRegion = farRegion;

                List<string> lsValueAddedService = new List<string>();
                if (eBillingInput.Transport.Equals(TransportIndicatorConstance.MAY_BAY))
                {
                    if (eBillingInput.ServiceCode.Equals(ServiceConstance.BK) && _domestic == false)
                    { }
                    else
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.MAY_BAY);
                    }
                }

                if (rows.Cells["colCOD"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colCOD"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.PHAT_HANG_THU_TIEN);

                        eBillingInput.CODAmount = 0;

                        if (rows.Cells["colAmount"].Value != null)
                        {
                            double dAmountResult;
                            if (double.TryParse(rows.Cells["colAmount"].Value.ToString(), out dAmountResult))
                            {
                                if (dAmountResult > 0)
                                {
                                    eBillingInput.CODAmount = dAmountResult;
                                }
                            }
                        }

                        if (rows.Cells["colCash"].Value != null)
                        {
                            bool bCashResult;
                            if (bool.TryParse(rows.Cells["colCash"].Value.ToString(), out bCashResult))
                            {
                                if (bCashResult)
                                {
                                    if (rows.Cells["colPayPOS"].Value != null)
                                    {
                                        bool bPayPOSResult;
                                        if (bool.TryParse(rows.Cells["colPayPOS"].Value.ToString(), out bPayPOSResult))
                                        {
                                            if (bPayPOSResult)
                                            {
                                            }
                                            else
                                            {
                                                lsValueAddedService.Add(ValueAddedServiceConstance.TRA_TIEN_TAI_DIA_CHI);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    lsValueAddedService.Add(ValueAddedServiceConstance.CHUYEN_KHOAN);
                                }
                            }
                        }

                        if (rows.Cells["colSenderPostage"].Value != null)
                        {
                            bool bSenderPostageResult;
                            if (bool.TryParse(rows.Cells["colSenderPostage"].Value.ToString(), out bSenderPostageResult))
                            {
                                if (bSenderPostageResult)
                                {
                                    bSenderPostage = true;
                                }
                                else
                                {
                                    bSenderPostage = false;
                                }
                            }
                        }

                        if (rows.Cells["colSenderCODPostage"].Value != null)
                        {
                            bool bSenderCODPostageResult;
                            if (bool.TryParse(rows.Cells["colSenderCODPostage"].Value.ToString(), out bSenderCODPostageResult))
                            {
                                if (bSenderCODPostageResult)
                                {
                                    bSenderCODPostage = true;
                                }
                                else
                                {
                                    bSenderCODPostage = false;
                                }
                            }
                        }
                    }
                }

                if (rows.Cells["colPDK"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colPDK"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.PHAT_DONG_KIEM);

                        eBillingInput.CommodityNumbers = 1;
                    }
                }

                if (rows.Cells["colAR"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colAR"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.BAO_PHAT);
                    }
                }

                if (rows.Cells["colAREmail"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colAREmail"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.BAO_PHAT_EMAIL);
                    }
                }

                if (rows.Cells["colARSMS"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colARSMS"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.BAO_PHAT_SMS);
                    }
                }

                if (rows.Cells["colPTT"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colPTT"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.PHAT_TAN_TAY);
                    }
                }

                if (rows.Cells["colVUN"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colVUN"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.HANG_NHAY_CAM_VUN);
                    }
                }

                if (rows.Cells["colKA"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colKA"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.TUYET_MAT);
                    }
                }

                if (rows.Cells["colKB"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colKB"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.TOI_MAT);
                    }
                }

                if (rows.Cells["colKC"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colKC"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.MAT);
                    }
                }

                if (rows.Cells["colHGN"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colHGN"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.HEN_GIO_NOI_TINH);
                    }
                }

                if (rows.Cells["colHGL"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colHGL"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.HEN_GIO_LIEN_TINH);
                    }
                }

                if (rows.Cells["colHTN"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colHTN"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.HOA_TOC_NOI_TINH);
                    }
                }

                if (rows.Cells["colHTL"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colHTL"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.HOA_TOC_LIEN_TINH);
                    }
                }

                if (rows.Cells["colV"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colV"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.KHAI_GIA);

                        eBillingInput.StateValue = 0;

                        if (rows.Cells["colGiaTriKhaiGia"].Value != null)
                        {
                            double dGiaTriKhaiGia;
                            if (double.TryParse(rows.Cells["colGiaTriKhaiGia"].Value.ToString(), out dGiaTriKhaiGia))
                            {
                                if (dGiaTriKhaiGia > 0)
                                {
                                    eBillingInput.StateValue = dGiaTriKhaiGia;
                                }
                            }
                        }
                    }
                }

                if (rows.Cells["colPPA"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colPPA"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.THU_CUOC_NGUOI_GUI);
                    }
                }

                if (rows.Cells["colC"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colC"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.THU_CUOC_NGUOI_NHAN);

                        bThuCuocNguoiNhan = true;
                    }
                }

                if (rows.Cells["colBenThu3"].Value != null)
                {
                    if (Convert.ToBoolean(rows.Cells["colBenThu3"].Value))
                    {
                        lsValueAddedService.Add(ValueAddedServiceConstance.THU_CUOC_BEN_THU_3);
                    }
                }
                //Dungnt them cot dich vu
                if (rows.Cells["colVASService"].Value != null)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(rows.Cells["colVASService"].Value)))
                    {
                        var ListVAS = Convert.ToString(rows.Cells["colVASService"].Value).Split(Convert.ToChar(";"));

                        lsValueAddedService.AddRange(ListVAS);
                    }
                }



                eBillingInput.ValueAddedServiceCodes = lsValueAddedService.Distinct().ToArray();



                bool isAffair = false;//Là bưu gửi công vụ
                bool isFreePost = false;//Bưu gửi miễn cước
                if (rows.Cells["colAffair"].Value != null)
                {
                    isAffair = Convert.ToBoolean(rows.Cells["colAffair"].Value);
                }
                if (rows.Cells["colFreePost"].Value != null)
                {
                    isFreePost = Convert.ToBoolean(rows.Cells["colFreePost"].Value);
                }

                if (isAffair || isFreePost) // Bưu gửi sự vụ, miễn cước thì cước là 0
                {

                }
                else
                {
                    BillingOutput eOutput = new BillingOutput();
                    if (eBillingInput.DestinationCode != null && eBillingInput.DestinationCode != "")
                        eOutput = oBilling.ItemBilling(eBillingInput);

                    //Tỷ lệ VAT Bán hàng
                    if (eOutput.VATFreight != 0)
                    {
                        _VATPercentage = eOutput.VATFreight;
                        c_VATPercentage = eOutput.VATFreight;
                    }

                    //Tỷ lệ VAT gốc
                    if (eOutput.VATFreight_Origin != 0)
                    {
                        _OriginalVATPercentage = eOutput.VATFreight_Origin;
                    }

                    //Cước giá vốn
                    if (eOutput.FundFreight != 0)
                    {
                        _FundFreight = eOutput.FundFreight;
                    }

                    //Cước giá vốn DV GTGT
                    if (eOutput.FundVASFreight != 0)
                    {
                        _FundVASFreight = eOutput.FundVASFreight;
                    }

                    if (eBillingInput.ItemType.Equals(ItemTypeConstance.CMTND) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.KET_QUA_XET_NGHIEM) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.DANG_KY_XE_CO_GIOI) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.HO_KHAU) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.HO_SO_VA_GIAY_PHEP_LAI_XE) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.HO_SO_XET_TUYEN) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.HO_SO_TU_PHAP) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.CDHC) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.EMS_HO_CHIEU_1_CHIEU) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.EMS_HO_CHIEU_2_CHIEU) ||
                        eBillingInput.ItemType.Equals(ItemTypeConstance.EMS_VISA))
                    {
                        //Cước bán hàng
                        if (eOutput.HasVAT)
                        {
                            if (eOutput.VATFreight != 0)
                            {
                                double dMainVAT = eOutput.MainFreight - eOutput.MainFreight / (1 + eOutput.VATFreight / 100);
                                _MainFreight = eOutput.MainFreight - dMainVAT;
                            }
                        }
                        else
                        {
                            _MainFreight = eOutput.MainFreight;
                        }

                        //Cước gốc
                        if (eOutput.HasVAT_Origin)
                        {
                            if (eOutput.VATFreight_Origin != 0)
                            {
                                double dMainVATOriginal = eOutput.MainFreight_Origin - eOutput.MainFreight_Origin / (1 + eOutput.VATFreight_Origin / 100);
                                _OriginalMainFreight = eOutput.MainFreight_Origin - dMainVATOriginal;
                            }
                        }
                        else
                        {
                            _OriginalMainFreight = eOutput.MainFreight_Origin;
                        }

                        //Cước trọn gói thì reset tất cả các loại cước khác cước chính về 0 -------------
                        eOutput.AirSurcharge = 0;
                        eOutput.FuelSurcharge = 0;
                        eOutput.FarRegionFreight = 0;

                        if (eOutput.ValueAddedServiceFreights != null && eOutput.ValueAddedServiceFreights.Count > 0)
                        {
                            foreach (ValueAddedServiceFreight eVASF in eOutput.ValueAddedServiceFreights)
                            {
                                eVASF.Freight = 0;
                                eVASF.SurchangeFreight = 0;
                            }
                        }

                        //eOutput.AirSurcharge_Origin = 0;
                        //eOutput.FuelSurcharge_Origin = 0;
                        //eOutput.FarRegionFreight_Origin = 0;

                        //if (eOutput.ValueAddedServiceFreights_Origin != null && eOutput.ValueAddedServiceFreights_Origin.Count > 0)
                        //{
                        //    foreach (ValueAddedServiceFreight eVASF in eOutput.ValueAddedServiceFreights_Origin)
                        //    {
                        //        eVASF.Freight = 0;
                        //        eVASF.SurchangeFreight = 0;
                        //    }
                        //}
                        // ------------------

                    }
                    else
                    {
                        //Cước bán hàng
                        if (eOutput.HasVAT)
                        {
                            if (eOutput.VATFreight != 0)
                            {
                                if (eOutput.CommodityCoefficient != 0)
                                {
                                    double dMainVAT = eOutput.MainFreight * eOutput.CommodityCoefficient - Math.Round(eOutput.MainFreight * eOutput.CommodityCoefficient / (1 + eOutput.VATFreight / 100), MidpointRounding.AwayFromZero);
                                    _MainFreight = Math.Round(eOutput.MainFreight * eOutput.CommodityCoefficient, MidpointRounding.AwayFromZero) - dMainVAT;
                                }
                                else
                                {
                                    double dMainVAT = eOutput.MainFreight - Math.Round(eOutput.MainFreight / (1 + eOutput.VATFreight / 100), MidpointRounding.AwayFromZero);
                                    _MainFreight = Math.Round(eOutput.MainFreight, MidpointRounding.AwayFromZero) - dMainVAT;
                                }
                            }
                        }
                        else
                        {
                            if (eOutput.CommodityCoefficient != 0)
                            {
                                _MainFreight = eOutput.MainFreight * eOutput.CommodityCoefficient;
                            }
                            else
                            {
                                _MainFreight = eOutput.MainFreight;
                            }
                        }

                        //Cước gốc
                        if (eOutput.HasVAT_Origin)
                        {
                            if (eOutput.VATFreight_Origin != 0)
                            {
                                if (eOutput.CommodityCoefficient_Origin != 0)
                                {
                                    double dMainVATOriginal = eOutput.MainFreight_Origin * eOutput.CommodityCoefficient_Origin - eOutput.MainFreight_Origin * eOutput.CommodityCoefficient_Origin / (1 + eOutput.VATFreight_Origin / 100);
                                    _OriginalMainFreight = eOutput.MainFreight_Origin * eOutput.CommodityCoefficient_Origin - dMainVATOriginal;
                                }
                                else
                                {
                                    double dMainVATOriginal = eOutput.MainFreight_Origin - eOutput.MainFreight_Origin / (1 + eOutput.VATFreight_Origin / 100);
                                    _OriginalMainFreight = eOutput.MainFreight_Origin - dMainVATOriginal;
                                }
                            }
                        }
                        else
                        {
                            if (eOutput.CommodityCoefficient_Origin != 0)
                            {
                                _OriginalMainFreight = eOutput.MainFreight_Origin * eOutput.CommodityCoefficient_Origin;
                            }
                            else
                            {
                                _OriginalMainFreight = eOutput.MainFreight_Origin;
                            }
                        }
                    }

                    //Phụ phí máy bay bán hàng
                    if (eOutput.AirSurcharge != 0)
                    {
                        if (eOutput.HasVAT)
                        {
                            if (eOutput.VATFreight != 0)
                            {
                                double dAirSurchargeVAT = eOutput.AirSurcharge - Math.Round(eOutput.AirSurcharge / (1 + eOutput.VATFreight / 100), MidpointRounding.AwayFromZero);
                                _AirSurchargeFreight = eOutput.AirSurcharge - dAirSurchargeVAT;
                            }
                        }
                        else
                        {
                            _AirSurchargeFreight = eOutput.AirSurcharge;
                        }
                    }

                    //Phụ phí máy bay gốc
                    if (eOutput.AirSurcharge_Origin != 0)
                    {
                        if (eOutput.HasVAT_Origin)
                        {
                            if (eOutput.VATFreight_Origin != 0)
                            {
                                double dAirSurchargeVATOriginal = eOutput.AirSurcharge_Origin - eOutput.AirSurcharge_Origin / (1 + eOutput.VATFreight_Origin / 100);
                                _OriginalAirSurchargeFreight = eOutput.AirSurcharge_Origin - dAirSurchargeVATOriginal;
                            }
                        }
                        else
                        {
                            _OriginalAirSurchargeFreight = eOutput.AirSurcharge_Origin;
                        }
                    }

                    //Phụ phí xăng dầu bán hàng
                    if (eOutput.FuelSurcharge != 0)
                    {
                        if (eOutput.HasVAT)
                        {
                            if (eOutput.VATFreight != 0)
                            {
                                double dFuelSurchargeVAT = eOutput.FuelSurcharge - Math.Round(eOutput.FuelSurcharge / (1 + eOutput.VATFreight / 100), MidpointRounding.AwayFromZero);
                                _FuelSurchargeFreight = eOutput.FuelSurcharge - dFuelSurchargeVAT;
                            }
                        }
                        else
                        {
                            _FuelSurchargeFreight = eOutput.FuelSurcharge;
                        }
                    }

                    //Phụ phí xăng dầu gốc
                    if (eOutput.FuelSurcharge_Origin != 0)
                    {

                        if (eOutput.HasVAT_Origin)
                        {
                            if (eOutput.VATFreight_Origin != 0)
                            {
                                double dFuelSurchargeVATOriginal = eOutput.FuelSurcharge_Origin - eOutput.FuelSurcharge_Origin / (1 + eOutput.VATFreight_Origin / 100);
                                _OriginalFuelSurchargeFreight = eOutput.FuelSurcharge_Origin - dFuelSurchargeVATOriginal;
                            }
                        }
                        else
                        {
                            _OriginalFuelSurchargeFreight = eOutput.FuelSurcharge_Origin;
                        }
                    }

                    //Phụ phí vùng xa bán hàng
                    if (eOutput.FarRegionFreight != 0)
                    {
                        if (eOutput.HasVAT)
                        {
                            if (eOutput.VATFreight != 0)
                            {
                                double dFarRegionFreightVAT = eOutput.FarRegionFreight - Math.Round(eOutput.FarRegionFreight / (1 + eOutput.VATFreight / 100), MidpointRounding.AwayFromZero);
                                _FarRegionFreight = eOutput.FarRegionFreight - dFarRegionFreightVAT;
                            }
                        }
                        else
                        {
                            _FarRegionFreight = eOutput.FarRegionFreight;
                        }
                    }

                    //Phụ phí vùng xa gốc
                    if (eOutput.FarRegionFreight_Origin != 0)
                    {
                        if (eOutput.HasVAT_Origin)
                        {
                            if (eOutput.VATFreight_Origin != 0)
                            {
                                double dFarRegionFreightVATOriginal = eOutput.FarRegionFreight_Origin - eOutput.FarRegionFreight_Origin / (1 + eOutput.VATFreight_Origin / 100);
                                _OriginalFarRegionFreight = eOutput.FarRegionFreight_Origin - dFarRegionFreightVATOriginal;
                            }
                        }
                        else
                        {
                            _OriginalFarRegionFreight = eOutput.FarRegionFreight_Origin;
                        }
                    }

                    //Dịch vụ GTGT bán hàng
                    DataTable dt = new DataTable();
                    dt.Columns.Add("FreightName");
                    dt.Columns.Add("Freight");
                    if (eOutput.ValueAddedServiceFreights != null)
                    {
                        bool UseCOD = false;
                        VASFreightList = new List<ValueAddedServiceFreight>();
                        foreach (ValueAddedServiceFreight eVASF in eOutput.ValueAddedServiceFreights)
                        {
                            if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.MAY_BAY))
                            {
                                ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
                                enValueAddedServiceItem.ServiceCode = cboService.SelectedValue.ToString();
                                enValueAddedServiceItem.ValueAddedServiceCode = ValueAddedServiceConstance.MAY_BAY;
                                //enValueAddedServiceItem.ItemCode = rows.Cells["colBarcode"].Value.ToString();
                                enValueAddedServiceItem.Freight = 0;
                                enValueAddedServiceItem.FreightVAT = 0;
                                enValueAddedServiceItem.OriginalFreight = 0;
                                enValueAddedServiceItem.OriginalFreightVAT = 0;
                                enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
                                enValueAddedServiceItem.AddedDate = dtpFromDate.Value;
                                enValueAddedServiceItem.POSCode = this.POSCode;

                                enValueAddedServiceItem.SubFreight = 0;
                                enValueAddedServiceItem.SubFreightVAT = 0;
                                enValueAddedServiceItem.OriginalSubFreight = 0;
                                enValueAddedServiceItem.OriginalSubFreightVAT = 0;

                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        enValueAddedServiceItem.Freight = eVASF.Freight - dAddedVAT;
                                        enValueAddedServiceItem.FreightVAT = eVASF.Freight;
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    _AddedFreight += eVASF.Freight;

                                    enValueAddedServiceItem.Freight = eVASF.Freight;
                                    enValueAddedServiceItem.FreightVAT = Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero);

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                entityValueAddedServiceItemList.Add(enValueAddedServiceItem);

                                rows.Cells["colSubFreight"].Tag = entityValueAddedServiceItemList;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.BAO_PHAT))
                            {
                                Hashtable htAR = new Hashtable();

                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htAR.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htAR.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htAR.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htAR.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colAR"].Tag = htAR;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.BAO_PHAT_EMAIL))
                            {
                                Hashtable htAREmail = new Hashtable();

                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htAREmail.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htAREmail.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htAREmail.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htAREmail.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colAREmail"].Tag = htAREmail;

                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.BAO_PHAT_SMS))
                            {
                                Hashtable htARSMS = new Hashtable();

                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htARSMS.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htARSMS.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htARSMS.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htARSMS.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colARSMS"].Tag = htARSMS;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_TAN_TAY))
                            {
                                Hashtable htPTT = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htPTT.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htPTT.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htPTT.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htPTT.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colPTT"].Tag = htPTT;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HANG_NHAY_CAM_VUN))
                            {
                                Hashtable htVUN = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htVUN.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htVUN.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htVUN.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htVUN.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colVUN"].Tag = htVUN;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.TUYET_MAT))
                            {
                                Hashtable htKA = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htKA.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htKA.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htKA.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htKA.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colKA"].Tag = htKA;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.TOI_MAT))
                            {
                                Hashtable htKB = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htKB.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htKB.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htKB.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htKB.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colKB"].Tag = htKB;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.MAT))
                            {
                                Hashtable htKC = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htKC.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htKC.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htKC.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htKC.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colKC"].Tag = htKC;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HEN_GIO_NOI_TINH))
                            {
                                Hashtable htHGN = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htHGN.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHGN.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHGN.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHGN.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHGN"].Tag = htHGN;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HEN_GIO_LIEN_TINH))
                            {
                                Hashtable htHGL = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htHGL.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHGL.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHGL.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHGL.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHGL"].Tag = htHGL;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HOA_TOC_NOI_TINH))
                            {
                                Hashtable htHTN = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htHTN.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHTN.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHTN.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHTN.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHTN"].Tag = htHTN;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HOA_TOC_LIEN_TINH))
                            {
                                Hashtable htHTL = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htHTL.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHTL.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHTL.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHTL.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHTL"].Tag = htHTL;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.KHAI_GIA))
                            {
                                Hashtable htV = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htV.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htV.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htV.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htV.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colV"].Tag = htV;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_DONG_KIEM))
                            {
                                Hashtable htPDK = new Hashtable();
                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        htPDK.Add("Freight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htPDK.Add("FreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htPDK.Add("Freight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htPDK.Add("FreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _VATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _AddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colPDK"].Tag = htPDK;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_HANG_THU_TIEN))
                            {
                                UseCOD = true;

                                if (eVASF.Freight != 0)
                                {
                                    double dAddedVAT = eVASF.Freight - eVASF.Freight / 1.1;
                                    //_AddedFreight += eVASF.Freight - dAddedVAT;
                                    _CODVATFreight += dAddedVAT;
                                    _CODFreight += eVASF.Freight - dAddedVAT;
                                }

                                if (rows.Cells["colCash"].Value != null)
                                {
                                    bool bTraTienMatResult;
                                    if (bool.TryParse(rows.Cells["colCash"].Value.ToString(), out bTraTienMatResult))
                                    {
                                        if (bTraTienMatResult)
                                        {

                                        }
                                        else
                                        {
                                            //if (rows.Cells["colChargeTransfer"].Value != null)
                                            //{
                                            //    double dPhiCKResult;
                                            //    if (double.TryParse(rows.Cells["colChargeTransfer"].Value.ToString(), out dPhiCKResult))
                                            //    {
                                            //        if (dPhiCKResult > 0)
                                            //        {
                                            //            double dPhiCKVAT = dPhiCKResult - dPhiCKResult / 1.1;
                                            //            _CODVATFreight += dPhiCKVAT;
                                            //            _CODFreight += dPhiCKResult - dPhiCKVAT;

                                            //            //Cước dịch vụ GTGT của dịch vụ COD
                                            //            _CODVATSubFreight += dPhiCKVAT;
                                            //            _CODSubFreight += dPhiCKResult - dPhiCKVAT;
                                            //        }
                                            //    }
                                            //}
                                        }
                                    }
                                }

                                //Phụ phí xăng dầu của dịch vụ GTGT
                                if (eVASF.SurchangeFreight != 0)
                                {
                                    double dSurchangeVAT = 0;
                                    dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / 1.1;
                                    _FuelSurchargeFreight += eVASF.Freight - dSurchangeVAT;
                                }
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.CHUYEN_KHOAN))
                            {
                                if (eVASF.Freight != 0)
                                {
                                    double dAddedVAT = eVASF.Freight - eVASF.Freight / 1.1;
                                    _CODVATFreight += dAddedVAT;
                                    _CODFreight += eVASF.Freight - dAddedVAT;

                                    //Cước dịch vụ GTGT của dịch vụ COD
                                    _CODVATSubFreight += dAddedVAT;
                                    _CODSubFreight += eVASF.Freight - dAddedVAT;
                                }

                                //Phụ phí xăng dầu của dịch vụ GTGT
                                if (eVASF.SurchangeFreight != 0)
                                {
                                    double dSurchangeVAT = 0;
                                    dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / 1.1;
                                    _FuelSurchargeFreight += eVASF.Freight - dSurchangeVAT;
                                }
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.TRA_TIEN_TAI_DIA_CHI))
                            {
                                if (eVASF.Freight != 0)
                                {
                                    double dAddedVAT = eVASF.Freight - eVASF.Freight / 1.1;
                                    _CODVATFreight += dAddedVAT;
                                    _CODFreight += eVASF.Freight - dAddedVAT;

                                    //Cước dịch vụ GTGT của dịch vụ COD
                                    _CODVATSubFreight += dAddedVAT;
                                    _CODSubFreight += eVASF.Freight - dAddedVAT;
                                }

                                //Phụ phí xăng dầu của dịch vụ GTGT
                                if (eVASF.SurchangeFreight != 0)
                                {
                                    double dSurchangeVAT = 0;
                                    dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / 1.1;
                                    _FuelSurchargeFreight += eVASF.Freight - dSurchangeVAT;
                                }
                            }
                            else
                            {

                                //Dungnt
                                ValueAddedServiceFreight enVASFTemp = new ValueAddedServiceFreight();
                                enVASFTemp.ValueAddedServiceCode = eVASF.ValueAddedServiceCode;

                                DataRow rVASF = dt.NewRow();
                                rVASF["FreightName"] = "  " + eVASF.ValueAddedServiceName;
                                rVASF["Freight"] = "0";

                                if (eOutput.HasVAT)
                                {
                                    if (eOutput.VATFreight != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight / 100);

                                        _AddedFreight += eVASF.Freight - dAddedVAT;

                                        rVASF["Freight"] = NumberFormat(Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));

                                        enVASFTemp.Freight = eVASF.Freight - dAddedVAT;

                                        VASFreightList.Add(enVASFTemp);
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight / 100);

                                        _FuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    _AddedFreight += eVASF.Freight;

                                    rVASF["Freight"] = NumberFormat(Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));

                                    enVASFTemp.Freight = eVASF.Freight;

                                    VASFreightList.Add(enVASFTemp);

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _FuelSurchargeFreight += eVASF.SurchangeFreight;
                                }
                            }
                        }

                        if (UseCOD)
                        {
                            Hashtable htCOD = new Hashtable();

                            htCOD.Add("Freight", Math.Round(_CODFreight, MidpointRounding.AwayFromZero));
                            htCOD.Add("FreightVAT", Math.Round(_CODFreight + _CODVATFreight, MidpointRounding.AwayFromZero));

                            htCOD.Add("SubFreight", Math.Round(_CODSubFreight, MidpointRounding.AwayFromZero));
                            htCOD.Add("SubFreightVAT", Math.Round(_CODSubFreight + _CODVATSubFreight, MidpointRounding.AwayFromZero));

                            rows.Cells["colCOD"].Tag = htCOD;
                        }
                    }

                    //Dịch vụ GTGT gốc
                    if (eOutput.ValueAddedServiceFreights_Origin != null)
                    {
                        VASFreightListOriginal = new List<ValueAddedServiceFreight>();
                        foreach (ValueAddedServiceFreight eVASF in eOutput.ValueAddedServiceFreights_Origin)
                        {
                            if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.MAY_BAY))
                            {
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.BAO_PHAT))
                            {
                                Hashtable htAR = new Hashtable();
                                if (rows.Cells["colAR"].Tag != null)
                                {
                                    htAR = (Hashtable)rows.Cells["colAR"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htAR.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htAR.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htAR.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htAR.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colAR"].Tag = htAR;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.BAO_PHAT_EMAIL))
                            {
                                Hashtable htAREmail = new Hashtable();
                                if (rows.Cells["colAREmail"].Tag != null)
                                {
                                    htAREmail = (Hashtable)rows.Cells["colAREmail"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htAREmail.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htAREmail.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htAREmail.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htAREmail.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colAREMail"].Tag = htAREmail;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.BAO_PHAT_SMS))
                            {
                                Hashtable htARSMS = new Hashtable();
                                if (rows.Cells["colARSMS"].Tag != null)
                                {
                                    htARSMS = (Hashtable)rows.Cells["colARSMS"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htARSMS.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htARSMS.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htARSMS.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htARSMS.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colARSMS"].Tag = htARSMS;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_TAN_TAY))
                            {
                                Hashtable htPTT = new Hashtable();
                                if (rows.Cells["colPTT"].Tag != null)
                                {
                                    htPTT = (Hashtable)rows.Cells["colPTT"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htPTT.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htPTT.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htPTT.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htPTT.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colPTT"].Tag = htPTT;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HANG_NHAY_CAM_VUN))
                            {
                                Hashtable htVUN = new Hashtable();
                                if (rows.Cells["colVUN"].Tag != null)
                                {
                                    htVUN = (Hashtable)rows.Cells["colVUN"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htVUN.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htVUN.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htVUN.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htVUN.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colVUN"].Tag = htVUN;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.TUYET_MAT))
                            {
                                Hashtable htKA = new Hashtable();
                                if (rows.Cells["colKA"].Tag != null)
                                {
                                    htKA = (Hashtable)rows.Cells["colKA"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htKA.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htKA.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htKA.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htKA.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colKA"].Tag = htKA;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.TOI_MAT))
                            {
                                Hashtable htKB = new Hashtable();
                                if (rows.Cells["colKB"].Tag != null)
                                {
                                    htKB = (Hashtable)rows.Cells["colKB"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htKB.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htKB.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htKB.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htKB.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colKB"].Tag = htKB;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.MAT))
                            {
                                Hashtable htKC = new Hashtable();
                                if (rows.Cells["colKC"].Tag != null)
                                {
                                    htKC = (Hashtable)rows.Cells["colKC"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htKC.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htKC.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htKC.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htKC.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colKC"].Tag = htKC;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HEN_GIO_NOI_TINH))
                            {
                                Hashtable htHGN = new Hashtable();
                                if (rows.Cells["colHGN"].Tag != null)
                                {
                                    htHGN = (Hashtable)rows.Cells["colHGN"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htHGN.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHGN.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHGN.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHGN.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHGN"].Tag = htHGN;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HEN_GIO_LIEN_TINH))
                            {
                                Hashtable htHGL = new Hashtable();
                                if (rows.Cells["colHGL"].Tag != null)
                                {
                                    htHGL = (Hashtable)rows.Cells["colHGL"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htHGL.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHGL.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHGL.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHGL.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHGL"].Tag = htHGL;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HOA_TOC_NOI_TINH))
                            {
                                Hashtable htHTN = new Hashtable();
                                if (rows.Cells["colHTN"].Tag != null)
                                {
                                    htHTN = (Hashtable)rows.Cells["colHTN"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htHTN.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHTN.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHTN.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHTN.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHTN"].Tag = htHTN;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.HOA_TOC_LIEN_TINH))
                            {
                                Hashtable htHTL = new Hashtable();
                                if (rows.Cells["colHTL"].Tag != null)
                                {
                                    htHTL = (Hashtable)rows.Cells["colHTL"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htHTL.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htHTL.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htHTL.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htHTL.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colHTL"].Tag = htHTL;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_DONG_KIEM))
                            {
                                Hashtable htPDK = new Hashtable();
                                if (rows.Cells["colPDK"].Tag != null)
                                {
                                    htPDK = (Hashtable)rows.Cells["colPDK"].Tag;
                                }

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        htPDK.Add("OriginalFreight", Math.Round(eVASF.Freight - dAddedVAT, MidpointRounding.AwayFromZero));
                                        htPDK.Add("OriginalFreightVAT", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    htPDK.Add("OriginalFreight", Math.Round(eVASF.Freight, MidpointRounding.AwayFromZero));
                                    htPDK.Add("OriginalFreightVAT", Math.Round(eVASF.Freight + (eVASF.Freight * _OriginalVATPercentage / 100), MidpointRounding.AwayFromZero));

                                    _OriginalAddedFreight += eVASF.Freight;

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }

                                rows.Cells["colPDK"].Tag = htPDK;
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.PHAT_HANG_THU_TIEN))
                            {
                                if (eVASF.Freight != 0)
                                {
                                    double dAddedVAT = eVASF.Freight - eVASF.Freight / 1.1;

                                    _OriginalCODVATFreight += dAddedVAT;
                                    _OriginalCODFreight += eVASF.Freight - dAddedVAT;
                                }

                                //Phụ phí xăng dầu của dịch vụ GTGT
                                if (eVASF.SurchangeFreight != 0)
                                {
                                    double dSurchangeVAT = 0;
                                    dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / 1.1;
                                    _OriginalFuelSurchargeFreight += eVASF.Freight - dSurchangeVAT;
                                }
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.CHUYEN_KHOAN))
                            {
                                if (eVASF.Freight != 0)
                                {
                                    double dAddedVAT = eVASF.Freight - eVASF.Freight / 1.1;

                                    _OriginalCODVATFreight += dAddedVAT;
                                    _OriginalCODFreight += eVASF.Freight - dAddedVAT;

                                    ////Cước dịch vụ GTGT của dịch vụ COD
                                    //_OriginalCODVATSubFreight += dAddedVAT;
                                    //_OriginalCODSubFreight += eVASF.Freight - dAddedVAT;
                                }

                                //Phụ phí xăng dầu của dịch vụ GTGT
                                if (eVASF.SurchangeFreight != 0)
                                {
                                    double dSurchangeVAT = 0;
                                    dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / 1.1;
                                    _OriginalFuelSurchargeFreight += eVASF.Freight - dSurchangeVAT;
                                }
                            }
                            else if (eVASF.ValueAddedServiceCode.Equals(ValueAddedServiceConstance.TRA_TIEN_TAI_DIA_CHI))
                            {
                                if (eVASF.Freight != 0)
                                {
                                    double dAddedVAT = eVASF.Freight - eVASF.Freight / 1.1;

                                    _OriginalCODVATFreight += dAddedVAT;
                                    _OriginalCODFreight += eVASF.Freight - dAddedVAT;

                                    ////Cước dịch vụ GTGT của dịch vụ COD
                                    //_OriginalCODVATSubFreight += dAddedVAT;
                                    //_OriginalCODSubFreight += eVASF.Freight - dAddedVAT;
                                }

                                //Phụ phí xăng dầu của dịch vụ GTGT
                                if (eVASF.SurchangeFreight != 0)
                                {
                                    double dSurchangeVAT = 0;
                                    dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / 1.1;
                                    _OriginalFuelSurchargeFreight += eVASF.Freight - dSurchangeVAT;
                                }
                            }
                            else
                            {
                                ValueAddedServiceFreight enVASFTemp = new ValueAddedServiceFreight();
                                enVASFTemp.ValueAddedServiceCode = eVASF.ValueAddedServiceCode;

                                if (eOutput.HasVAT_Origin)
                                {
                                    if (eOutput.VATFreight_Origin != 0)
                                    {
                                        double dAddedVAT = eVASF.Freight - eVASF.Freight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalAddedFreight += eVASF.Freight - dAddedVAT;

                                        enVASFTemp.Freight = eVASF.Freight - dAddedVAT;

                                        VASFreightListOriginal.Add(enVASFTemp);
                                    }

                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    if (eVASF.SurchangeFreight != 0)
                                    {
                                        double dSurchangeVAT = eVASF.SurchangeFreight - eVASF.SurchangeFreight / (1 + eOutput.VATFreight_Origin / 100);

                                        _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight - dSurchangeVAT;
                                    }
                                }
                                else
                                {
                                    _OriginalAddedFreight += eVASF.Freight;

                                    enVASFTemp.Freight = eVASF.Freight;

                                    VASFreightListOriginal.Add(enVASFTemp);
                                    //Phụ phí xăng dầu cho từng chuyến dịch vụ GTGT
                                    _OriginalFuelSurchargeFreight += eVASF.SurchangeFreight;
                                }
                            }
                        }

                        Hashtable htCOD = new Hashtable();
                        if (rows.Cells["colCOD"].Tag != null)
                        {
                            htCOD = (Hashtable)rows.Cells["colCOD"].Tag;

                            htCOD.Add("OriginalFreight", Math.Round(_OriginalCODFreight, MidpointRounding.AwayFromZero));
                            htCOD.Add("OriginalFreightVAT", Math.Round(_OriginalCODFreight + _OriginalCODVATFreight, MidpointRounding.AwayFromZero));

                            //htCOD.Add("OriginalSubFreight", Math.Round(_OriginalCODSubFreight, MidpointRounding.AwayFromZero));
                            //htCOD.Add("OriginalSubFreightVAT", Math.Round(_OriginalCODSubFreight + _OriginalCODVATSubFreight, MidpointRounding.AwayFromZero));

                            rows.Cells["colCOD"].Tag = htCOD;
                        }
                    }

                    _TotalFreight = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight;
                    _OriginalTotalFreight = _OriginalMainFreight + _OriginalFuelSurchargeFreight + _OriginalFarRegionFreight + _OriginalAirSurchargeFreight + _OriginalAddedFreight + _OriginalCODFreight;

                    rows.Cells["colIsDiscount"].Value = false;
                    rows.Cells["colDiscountPercent"].Value = "0";
                    rows.Cells["colDiscountAmount"].Value = "0";
                    rows.Cells["colIsFeedback"].Value = false;
                    rows.Cells["colFeedbackPercent"].Value = "0";
                    rows.Cells["colFeedbackAmount"].Value = "0";


                    //_VATFreight = Math.Round(_TotalFreight * eOutput.VATFreight / 100, MidpointRounding.AwayFromZero);
                    //_OriginalVATFreight = Math.Round(_OriginalTotalFreight * eOutput.VATFreight_Origin / 100, MidpointRounding.AwayFromZero);
                    _VATFreight = _TotalFreight * eOutput.VATFreight / 100;
                    _OriginalVATFreight = _OriginalTotalFreight * eOutput.VATFreight_Origin / 100;

                    _TotalFreightVAT = _TotalFreight + _VATFreight;
                    _OriginalTotalFreightVAT = _OriginalTotalFreight + _OriginalVATFreight;

                    _TotalFreightDiscount = _TotalFreight - _DiscountFreight;
                    _OriginalTotalFreightDiscount = _OriginalTotalFreight;

                    _VATDiscountFreight = _TotalFreightDiscount * eOutput.VATFreight / 100;
                    _OriginalVATDiscountFreight = _OriginalTotalFreightDiscount * eOutput.VATFreight_Origin / 100;

                    _TotalFreightDiscountVAT = _TotalFreightDiscount + _VATDiscountFreight;
                    _OriginalTotalFreightDiscountVAT = _OriginalTotalFreightDiscount + _OriginalVATDiscountFreight;

                    if (bThuCuocNguoiNhan)
                    {
                        _PaymentFreight = 0;
                        _PaymentFreightVAT = 0;
                        _PaymentFreightDiscount = 0;
                        _PaymentFreightDiscountVAT = 0;

                        _RemainingFreight = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight;
                        _RemainingFreightVAT = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight + _VATDiscountFreight;
                        _RemainingFreightDiscount = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight - _DiscountFreight;
                        _RemainingFreightDiscountVAT = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight - _DiscountFreight + _VATDiscountFreight;

                    }
                    else
                    {
                        if (bSenderPostage)
                        {
                            if (bSenderCODPostage)
                            {
                                _PaymentFreight = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight;
                                _PaymentFreightVAT = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight + _VATDiscountFreight;
                                _PaymentFreightDiscount = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight - _DiscountFreight;
                                _PaymentFreightDiscountVAT = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight - _DiscountFreight + _VATDiscountFreight;

                                _RemainingFreight = 0;
                                _RemainingFreightVAT = 0;
                                _RemainingFreightDiscount = 0;
                                _RemainingFreightDiscountVAT = 0;
                            }
                            else
                            {
                                _PaymentFreight = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight;
                                _PaymentFreightVAT = _PaymentFreight + _PaymentFreight * _VATPercentage / 100;
                                _PaymentFreightDiscount = _PaymentFreight - _DiscountFreight;
                                _PaymentFreightDiscountVAT = _PaymentFreightDiscount + _PaymentFreightDiscount * _VATPercentage / 100;

                                _RemainingFreight = _CODFreight;
                                _RemainingFreightVAT = _CODFreight + _CODVATFreight;
                                _RemainingFreightDiscount = _CODFreight;
                                _RemainingFreightDiscountVAT = _CODFreight + _CODVATFreight;
                            }
                        }
                        else
                        {
                            if (bSenderCODPostage)
                            {
                                _PaymentFreight = _CODFreight;
                                _PaymentFreightVAT = _CODFreight + _CODVATFreight;
                                _PaymentFreightDiscount = _CODFreight;
                                _PaymentFreightDiscountVAT = _CODFreight + _CODVATFreight;

                                _RemainingFreight = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight;
                                _RemainingFreightVAT = _RemainingFreight + _RemainingFreight * _VATPercentage / 100;
                                _RemainingFreightDiscount = _RemainingFreight - _DiscountFreight;
                                _RemainingFreightDiscountVAT = _RemainingFreightDiscount + _RemainingFreightDiscount * _VATPercentage / 100;

                            }
                            else
                            {
                                _PaymentFreight = 0;
                                _PaymentFreightVAT = 0;
                                _PaymentFreightDiscount = 0;
                                _PaymentFreightDiscountVAT = 0;

                                _RemainingFreight = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight;
                                _RemainingFreightVAT = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight + _VATDiscountFreight;
                                _RemainingFreightDiscount = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight - _DiscountFreight;
                                _RemainingFreightDiscountVAT = _MainFreight + _FuelSurchargeFreight + _FarRegionFreight + _AirSurchargeFreight + _AddedFreight + _CODFreight - _DiscountFreight + _VATDiscountFreight;

                            }
                        }
                    }
                }

                if (_FarRegionFreight != 0)
                {
                    rows.Cells["colFarRegion"].Value = true;
                }

                rows.Cells["colMainFreight"].Value = NumberFormat(Math.Round(_MainFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colSubFreight"].Value = NumberFormat(Math.Round(_AddedFreight + _CODFreight, MidpointRounding.AwayFromZero));

                rows.Cells["colFuelSurchargeFreight"].Value = NumberFormat(Math.Round(_FuelSurchargeFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colFarRegionFreight"].Value = NumberFormat(Math.Round(_FarRegionFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colAirSurchargeFreight"].Value = NumberFormat(Math.Round(_AirSurchargeFreight, MidpointRounding.AwayFromZero));

                rows.Cells["colTotalFreight"].Value = NumberFormat(Math.Round(_TotalFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colTotalFreightVAT"].Value = NumberFormat(Math.Round(_TotalFreightVAT, MidpointRounding.AwayFromZero));
                rows.Cells["colTotalFreightDiscount"].Value = NumberFormat(Math.Round(_TotalFreightDiscount, MidpointRounding.AwayFromZero));
                rows.Cells["colTotalFreightDiscountVAT"].Value = NumberFormat(Math.Round(_TotalFreightDiscountVAT, MidpointRounding.AwayFromZero));

                rows.Cells["colVATPercentage"].Value = NumberFormat(Math.Round(_VATPercentage, MidpointRounding.AwayFromZero));
                rows.Cells["colVATFreight"].Value = NumberFormat(Math.Round(_VATFreight, MidpointRounding.AwayFromZero));

                rows.Cells["colPaymentFreight"].Value = NumberFormat(Math.Round(_PaymentFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colPaymentFreightVAT"].Value = NumberFormat(Math.Round(_PaymentFreightVAT, MidpointRounding.AwayFromZero));
                rows.Cells["colPaymentFreightDiscount"].Value = NumberFormat(Math.Round(_PaymentFreightDiscount, MidpointRounding.AwayFromZero));
                rows.Cells["colPaymentFreightDiscountVAT"].Value = NumberFormat(Math.Round(_PaymentFreightDiscountVAT, MidpointRounding.AwayFromZero));

                rows.Cells["colRemainingFreight"].Value = NumberFormat(Math.Round(_RemainingFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colRemainingFreightVAT"].Value = NumberFormat(Math.Round(_RemainingFreightVAT, MidpointRounding.AwayFromZero));
                rows.Cells["colRemainingFreightDiscount"].Value = NumberFormat(Math.Round(_RemainingFreightDiscount, MidpointRounding.AwayFromZero));
                rows.Cells["colRemainingFreightDiscountVAT"].Value = NumberFormat(Math.Round(_RemainingFreightDiscountVAT, MidpointRounding.AwayFromZero));


                rows.Cells["colOriginalMainFreight"].Value = NumberFormat(Math.Round(_OriginalMainFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalSubFreight"].Value = NumberFormat(Math.Round(_OriginalAddedFreight, MidpointRounding.AwayFromZero));

                rows.Cells["colOriginalFuelSurchargeFreight"].Value = NumberFormat(Math.Round(_OriginalFuelSurchargeFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalFarRegionFreight"].Value = NumberFormat(Math.Round(_OriginalFarRegionFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalAirSurchargeFreight"].Value = NumberFormat(Math.Round(_OriginalAirSurchargeFreight, MidpointRounding.AwayFromZero));

                rows.Cells["colOriginalVATFreight"].Value = NumberFormat(Math.Round(_OriginalVATFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalVATPercentage"].Value = NumberFormat(Math.Round(_OriginalVATPercentage, MidpointRounding.AwayFromZero));

                rows.Cells["colOriginalTotalFreight"].Value = NumberFormat(Math.Round(_OriginalTotalFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalTotalFreightVAT"].Value = NumberFormat(Math.Round(_OriginalTotalFreightVAT, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalTotalFreightDiscount"].Value = NumberFormat(Math.Round(_OriginalTotalFreightDiscount, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalTotalFreightDiscountVAT"].Value = NumberFormat(Math.Round(_OriginalTotalFreightDiscountVAT, MidpointRounding.AwayFromZero));

                rows.Cells["colOriginalPaymentFreight"].Value = NumberFormat(Math.Round(_OriginalPaymentFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalPaymentFreightVAT"].Value = NumberFormat(Math.Round(_OriginalPaymentFreightVAT, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalPaymentFreightDiscount"].Value = NumberFormat(Math.Round(_OriginalPaymentFreightDiscount, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalPaymentFreightDiscountVAT"].Value = NumberFormat(Math.Round(_OriginalPaymentFreightDiscountVAT, MidpointRounding.AwayFromZero));

                rows.Cells["colOriginalRemainingFreight"].Value = NumberFormat(Math.Round(_OriginalRemainingFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalRemainingFreightVAT"].Value = NumberFormat(Math.Round(_OriginalRemainingFreightVAT, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalRemainingFreightDiscount"].Value = NumberFormat(Math.Round(_OriginalRemainingFreightDiscount, MidpointRounding.AwayFromZero));
                rows.Cells["colOriginalRemainingFreightDiscountVAT"].Value = NumberFormat(Math.Round(_OriginalRemainingFreightDiscountVAT, MidpointRounding.AwayFromZero));

                rows.Cells["colFundFreight"].Value = NumberFormat(Math.Round(_FundFreight, MidpointRounding.AwayFromZero));
                rows.Cells["colFundVASFreight"].Value = NumberFormat(Math.Round(_FundVASFreight, MidpointRounding.AwayFromZero));
            }

            CalculatorTotalWeight();

            CalculatorTotalFreight();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (CheckShifted())
            {
                ShowMessageBoxWarning("Ca làm việc hiện tại đã được chốt. Không cho phép điều chỉnh dữ liệu trong ca");
                return;
            }

            Hashtable hasItemInfo = new Hashtable();

            hasItemInfo.Add("ServiceCode", cboService.SelectedValue != null ? cboService.SelectedValue.ToString() : "");
            hasItemInfo.Add("SendingTime", dtpFromDate.Value.ToString());

            frmAcceptanceSingleInput frm = new frmAcceptanceSingleInput();
            frm.POSCode = this.POSCode;
            frm.OriginalPOSCode = this.OriginalPOSCode;
            frm.Username = this.Username;
            frm.ShiftHandover = this.ShiftHandover;
            frm.ServiceCode = cboService.SelectedValue.ToString();
            frm.FormStatus = FormStatus.AddSingle;
            frm.HasSingleItemInfo = hasItemInfo;
            frm.AddSingleItem += new frmAcceptanceSingleInput.AddSingleItemEventHandler(frm_AddSingleItem);
            frm.ShowDialog();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (CheckShifted())
            {
                ShowMessageBoxWarning("Ca làm việc hiện tại đã được chốt. Không cho phép điều chỉnh dữ liệu trong ca");
                return;
            }

            Hashtable hasItemInfo = new Hashtable();

            hasItemInfo.Add("ServiceCode", cboService.SelectedValue != null ? cboService.SelectedValue.ToString() : "");
            hasItemInfo.Add("SendingTime", dtpFromDate.Value.ToString());

            //hasItemInfo.Add("CustomerCode", txtCustomerCode.Text.Trim());
            //hasItemInfo.Add("CustomerGroupCode", cboCustomerGroup.SelectedValue != null ? cboCustomerGroup.SelectedValue.ToString() : "");
            //hasItemInfo.Add("SenderFullName", txtSenderFullName.Text.Trim());
            //hasItemInfo.Add("SenderAddress", txtSenderAddress.Text.Trim());
            //hasItemInfo.Add("SenderTel", txtSenderTel.Text.Trim());
            //hasItemInfo.Add("SenderEmail", txtSenderEmail.Text.Trim());
            //hasItemInfo.Add("SenderPostCode", txtSenderPostCode.Text.Trim());
            //hasItemInfo.Add("SenderTaxCode", txtSenderTaxCode.Text.Trim());
            //hasItemInfo.Add("SenderIdentificationNumber", txtSenderIdentificationNumber.Text.Trim());

            hasItemInfo.Add("DestinationPOSCode", dgvListItems.CurrentRow.Cells["colDestinationPOSCode"].Value != null ? dgvListItems.CurrentRow.Cells["colDestinationPOSCode"].Value.ToString() : "");

            hasItemInfo.Add("ItemCode", dgvListItems.CurrentRow.Cells["colBarcode"].Value != null ? dgvListItems.CurrentRow.Cells["colBarcode"].Value.ToString() : "");
            hasItemInfo.Add("DataCode", dgvListItems.CurrentRow.Cells["colDataCode"].Value != null ? dgvListItems.CurrentRow.Cells["colDataCode"].Value.ToString() : "");
            hasItemInfo.Add("Affair", dgvListItems.CurrentRow.Cells["colAffair"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colAffair"].Value : false);
            hasItemInfo.Add("Collection", dgvListItems.CurrentRow.Cells["colIsCollection"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colIsCollection"].Value : false);
            hasItemInfo.Add("CustomerAccountNo", dgvListItems.CurrentRow.Cells["colCustomerAccountNo"].Value != null ? dgvListItems.CurrentRow.Cells["colCustomerAccountNo"].Value.ToString() : "");

            hasItemInfo.Add("CustomerCode", dgvListItems.CurrentRow.Cells["colCustomerCode"].Value != null ? dgvListItems.CurrentRow.Cells["colCustomerCode"].Value.ToString() : "");
            hasItemInfo.Add("CustomerGroupCode", dgvListItems.CurrentRow.Cells["colCustomerGroup"].Value != null ? dgvListItems.CurrentRow.Cells["colCustomerGroup"].Value.ToString() : "");
            hasItemInfo.Add("SenderFullName", dgvListItems.CurrentRow.Cells["colSenderFullName"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderFullName"].Value.ToString() : "");
            hasItemInfo.Add("SenderAddress", dgvListItems.CurrentRow.Cells["colSenderAddress"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderAddress"].Value.ToString() : "");
            hasItemInfo.Add("SenderTel", dgvListItems.CurrentRow.Cells["colSenderTel"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderTel"].Value.ToString() : "");
            hasItemInfo.Add("SenderEmail", dgvListItems.CurrentRow.Cells["colSenderEmail"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderEmail"].Value.ToString() : "");
            hasItemInfo.Add("SenderPostCode", dgvListItems.CurrentRow.Cells["colSenderPOSCode"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderPOSCode"].Value.ToString() : "");
            hasItemInfo.Add("SenderTaxCode", dgvListItems.CurrentRow.Cells["colSenderTaxCode"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderTaxCode"].Value.ToString() : "");
            hasItemInfo.Add("SenderIdentificationNumber", dgvListItems.CurrentRow.Cells["colSenderID"].Value != null ? dgvListItems.CurrentRow.Cells["colSenderID"].Value.ToString() : "");

            hasItemInfo.Add("ReceiverCustomerCode", dgvListItems.CurrentRow.Cells["colReceiverCustomerCode"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverCustomerCode"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverFullName", dgvListItems.CurrentRow.Cells["colReceiverFullName"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverFullName"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverAddress", dgvListItems.CurrentRow.Cells["colReceiverAddress"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverAddress"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverTel", dgvListItems.CurrentRow.Cells["colReceiverTel"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverTel"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverEmail", dgvListItems.CurrentRow.Cells["colReceiverEmail"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverEmail"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverContact", dgvListItems.CurrentRow.Cells["colReceiverContact"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverContact"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverPostCode", dgvListItems.CurrentRow.Cells["colReceiverPOSCode"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverPOSCode"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverTaxCode", dgvListItems.CurrentRow.Cells["colReceiverTaxCode"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverTaxCode"].Value.ToString() : "");
            hasItemInfo.Add("ReceiverIdentificationNumber", dgvListItems.CurrentRow.Cells["colReceiverID"].Value != null ? dgvListItems.CurrentRow.Cells["colReceiverID"].Value.ToString() : "");

            hasItemInfo.Add("Country", "");
            hasItemInfo.Add("Province", "");
            hasItemInfo.Add("District", "");
            hasItemInfo.Add("Commnue", "");
            if (dgvListItems.CurrentRow.Cells["colCountryCode"].Value != null && !string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colCountryCode"].Value.ToString()))
            {
                hasItemInfo["Country"] = dgvListItems.CurrentRow.Cells["colCountryCode"].Value.ToString();
            }
            else
            {
                if (dgvListItems.CurrentRow.Cells["colProvinceCode"].Value != null && !string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colProvinceCode"].Value.ToString()))
                {
                    hasItemInfo["Province"] = dgvListItems.CurrentRow.Cells["colProvinceCode"].Value.ToString();
                }

                if (dgvListItems.CurrentRow.Cells["colDistrictCode"].Value != null && !string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colDistrictCode"].Value.ToString()))
                {
                    hasItemInfo["District"] = dgvListItems.CurrentRow.Cells["colDistrictCode"].Value.ToString();
                }

                if (dgvListItems.CurrentRow.Cells["colCommuneCode"].Value != null && !string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colCommuneCode"].Value.ToString()))
                {
                    hasItemInfo["Commune"] = dgvListItems.CurrentRow.Cells["colCommuneCode"].Value.ToString();
                }
            }

            hasItemInfo.Add("FarRegion", dgvListItems.CurrentRow.Cells["colFarRegion"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colFarRegion"].Value : false);
            hasItemInfo.Add("Air", dgvListItems.CurrentRow.Cells["colIsAir"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colIsAir"].Value : false);
            hasItemInfo.Add("ExecuteOrder", dgvListItems.CurrentRow.Cells["colExecuteOrder"].Value != null ? dgvListItems.CurrentRow.Cells["colExecuteOrder"].Value.ToString() : "");
            hasItemInfo.Add("Invoice", dgvListItems.CurrentRow.Cells["colInvoice"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colInvoice"].Value : false);
            hasItemInfo.Add("OtherPaper", dgvListItems.CurrentRow.Cells["colOther"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colOther"].Value : false);
            hasItemInfo.Add("OtherPaperInfo", dgvListItems.CurrentRow.Cells["colOtherInfo"].Value != null ? dgvListItems.CurrentRow.Cells["colOtherInfo"].Value.ToString() : "");
            hasItemInfo.Add("DetailItem", dgvListItems.CurrentRow.Cells["colDetailItem"].Value != null ? dgvListItems.CurrentRow.Cells["colDetailItem"].Value.ToString() : "");
            hasItemInfo.Add("DetailItemList", dgvListItems.CurrentRow.Cells["colDetailItem"].Tag);
            hasItemInfo.Add("ItemType", dgvListItems.CurrentRow.Cells["colItemType"].Value != null ? dgvListItems.CurrentRow.Cells["colItemType"].Value.ToString() : "");
            hasItemInfo.Add("CommodityType", dgvListItems.CurrentRow.Cells["colComodityType"].Value != null ? dgvListItems.CurrentRow.Cells["colComodityType"].Value.ToString() : "");
            hasItemInfo.Add("UndeliveryGuide", dgvListItems.CurrentRow.Cells["colUndeliveryIndicator"].Value != null ? dgvListItems.CurrentRow.Cells["colUndeliveryIndicator"].Value.ToString() : "");
            hasItemInfo.Add("DeliveryNote", dgvListItems.CurrentRow.Cells["colDeliveryNote"].Value != null ? dgvListItems.CurrentRow.Cells["colDeliveryNote"].Value.ToString() : "");
            hasItemInfo.Add("Weight", dgvListItems.CurrentRow.Cells["colWeight"].Value != null ? dgvListItems.CurrentRow.Cells["colWeight"].Value.ToString() : "");
            hasItemInfo.Add("Length", dgvListItems.CurrentRow.Cells["colLength"].Value != null ? dgvListItems.CurrentRow.Cells["colLength"].Value.ToString() : "");
            hasItemInfo.Add("Width", dgvListItems.CurrentRow.Cells["colWidth"].Value != null ? dgvListItems.CurrentRow.Cells["colWidth"].Value.ToString() : "");
            hasItemInfo.Add("Height", dgvListItems.CurrentRow.Cells["colHeight"].Value != null ? dgvListItems.CurrentRow.Cells["colHeight"].Value.ToString() : "");
            hasItemInfo.Add("WeightConvert", "");
            hasItemInfo.Add("PostFree", dgvListItems.CurrentRow.Cells["colFreePost"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colFreePost"].Value : false);
            hasItemInfo.Add("Debt", dgvListItems.CurrentRow.Cells["colDebt"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colDebt"].Value : false);
            hasItemInfo.Add("InvoiceExport", dgvListItems.CurrentRow.Cells["colInvoiceExport"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colInvoiceExport"].Value : false);

            if (dgvListItems.CurrentRow.Cells["colValueAddedService"].Tag != null)
            {
                hasItemInfo.Add("ValueAddedService", dgvListItems.CurrentRow.Cells["colValueAddedService"].Tag);

                if (dgvListItems.CurrentRow.Cells["colVASPropertyValue"].Tag != null)
                {
                    hasItemInfo.Add("VASPropertyValue", dgvListItems.CurrentRow.Cells["colVASPropertyValue"].Tag);
                }
            }

            hasItemInfo.Add("COD", dgvListItems.CurrentRow.Cells["colCOD"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colCOD"].Value : false);

            if (dgvListItems.CurrentRow.Cells["colCOD"].Value != null)
            {
                if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colCOD"].Value.ToString()))
                {
                    hasItemInfo.Add("AmountForBatch", true);

                    if (dgvListItems.CurrentRow.Cells["colAmount"].Value != null)
                    {
                        double dSoTienCOD;
                        if (double.TryParse(dgvListItems.CurrentRow.Cells["colAmount"].Value.ToString(), out dSoTienCOD))
                        {
                            if (dSoTienCOD > 0)
                            {
                                hasItemInfo.Add("Amount", dSoTienCOD.ToString());
                            }
                            else
                            {
                                hasItemInfo.Add("Amount", 0);
                            }
                        }
                    }

                    bool bNguoiGuiCP = true;
                    bool bNguoiGuiTH = true;

                    bool bTienMat = true;
                    bool bTraTaiBC = true;
                    bool bTraTaiDC = false;

                    bool bChuyenKhoan = false;
                    string SoTaiKhoan = "";
                    string TenNganHang = "";
                    string ChiNhanhNganHang = "";
                    double PhiChuyenKhoan = 0;

                    if (dgvListItems.CurrentRow.Cells["colSenderPostage"].Value != null)
                    {
                        if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colSenderPostage"].Value.ToString()))
                        {
                            bNguoiGuiCP = true;
                        }
                        else
                        {
                            bNguoiGuiCP = false;
                        }
                    }

                    if (dgvListItems.CurrentRow.Cells["colSenderCODPostage"].Value != null)
                    {
                        if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colSenderCODPostage"].Value.ToString()))
                        {
                            bNguoiGuiTH = true;
                        }
                        else
                        {
                            bNguoiGuiTH = false;
                        }
                    }

                    if (dgvListItems.CurrentRow.Cells["colCash"].Value != null)
                    {
                        if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colCash"].Value.ToString()))
                        {
                            bTienMat = true;
                        }
                        else
                        {
                            bTienMat = false;
                        }
                    }

                    if (bTienMat)
                    {
                        if (dgvListItems.CurrentRow.Cells["colPayPOS"].Value != null)
                        {
                            if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colPayPOS"].Value.ToString()))
                            {
                                bTraTaiBC = true;
                            }
                            else
                            {
                                bTraTaiBC = false;
                                bTraTaiDC = true;
                            }
                        }
                    }
                    else
                    {
                        bChuyenKhoan = true;
                        bTraTaiBC = false;

                        if (dgvListItems.CurrentRow.Cells["colAccount"].Value != null)
                        {
                            if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colAccount"].Value.ToString()))
                            {
                                SoTaiKhoan = dgvListItems.CurrentRow.Cells["colAccount"].Value.ToString();
                            }
                        }

                        if (dgvListItems.CurrentRow.Cells["colBank"].Value != null)
                        {
                            if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colBank"].Value.ToString()))
                            {
                                TenNganHang = dgvListItems.CurrentRow.Cells["colBank"].Value.ToString();
                            }
                        }

                        if (dgvListItems.CurrentRow.Cells["colBranch"].Value != null)
                        {
                            if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colBranch"].Value.ToString()))
                            {
                                ChiNhanhNganHang = dgvListItems.CurrentRow.Cells["colBranch"].Value.ToString();
                            }
                        }

                        //if (dgvListItems.CurrentRow.Cells["colChargeTransfer"].Value != null)
                        //{
                        //    if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colChargeTransfer"].Value.ToString()))
                        //    {
                        //        double PhiResult;
                        //        if (double.TryParse(dgvListItems.CurrentRow.Cells["colChargeTransfer"].Value.ToString(), out PhiResult))
                        //        {
                        //            if (PhiResult > 0)
                        //            {
                        //                PhiChuyenKhoan = PhiResult;
                        //            }
                        //        }
                        //    }
                        //}
                    }

                    hasItemInfo.Add("SenderPostage", bNguoiGuiCP);
                    hasItemInfo.Add("SenderCODPostage", bNguoiGuiTH);

                    hasItemInfo.Add("Cash", bTienMat);
                    hasItemInfo.Add("PayPOS", bTraTaiBC);
                    hasItemInfo.Add("PayAddress", bTraTaiDC);

                    hasItemInfo.Add("Transfer", bChuyenKhoan);
                    hasItemInfo.Add("Account", SoTaiKhoan);
                    hasItemInfo.Add("Bank", TenNganHang);
                    hasItemInfo.Add("Branch", ChiNhanhNganHang);
                    hasItemInfo.Add("ChargeTransfer", PhiChuyenKhoan);
                }
            }

            hasItemInfo.Add("PDK", dgvListItems.CurrentRow.Cells["colPDK"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colPDK"].Value : false);
            hasItemInfo.Add("AR", dgvListItems.CurrentRow.Cells["colAR"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colAR"].Value : false);
            hasItemInfo.Add("AREmail", dgvListItems.CurrentRow.Cells["colAREmail"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colAREmail"].Value : false);
            hasItemInfo.Add("ARSMS", dgvListItems.CurrentRow.Cells["colARSMS"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colARSMS"].Value : false);
            hasItemInfo.Add("PTT", dgvListItems.CurrentRow.Cells["colPTT"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colPTT"].Value : false);
            hasItemInfo.Add("VUN", dgvListItems.CurrentRow.Cells["colVUN"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colVUN"].Value : false);
            hasItemInfo.Add("KA", dgvListItems.CurrentRow.Cells["colKA"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colKA"].Value : false);
            hasItemInfo.Add("KB", dgvListItems.CurrentRow.Cells["colKB"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colKB"].Value : false);
            hasItemInfo.Add("KC", dgvListItems.CurrentRow.Cells["colKC"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colKC"].Value : false);
            hasItemInfo.Add("HGN", dgvListItems.CurrentRow.Cells["colHGN"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colHGN"].Value : false);
            hasItemInfo.Add("HGL", dgvListItems.CurrentRow.Cells["colHGL"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colHGL"].Value : false);
            hasItemInfo.Add("HTN", dgvListItems.CurrentRow.Cells["colHTN"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colHTN"].Value : false);
            hasItemInfo.Add("HTL", dgvListItems.CurrentRow.Cells["colHTL"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colHTL"].Value : false);
            hasItemInfo.Add("V", dgvListItems.CurrentRow.Cells["colV"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colV"].Value : false);
            if (dgvListItems.CurrentRow.Cells["colV"].Value != null)
            {
                if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colV"].Value.ToString()))
                {
                    if (dgvListItems.CurrentRow.Cells["colGiaTriKhaiGia"].Value != null)
                    {
                        double dGiaTriKhaiGia;
                        if (double.TryParse(dgvListItems.CurrentRow.Cells["colGiaTriKhaiGia"].Value.ToString(), out dGiaTriKhaiGia))
                        {
                            if (dGiaTriKhaiGia > 0)
                            {
                                hasItemInfo.Add("DeclaredValue", dGiaTriKhaiGia.ToString());
                            }
                            else
                            {
                                hasItemInfo.Add("DeclaredValue", 0);
                            }
                        }
                    }

                    bool bUyQuyenNguoiNhan = false;

                    if (dgvListItems.CurrentRow.Cells["colAuthorReceiver"].Value != null)
                    {
                        if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colAuthorReceiver"].Value.ToString()))
                        {
                            bUyQuyenNguoiNhan = true;
                        }
                        else
                        {
                            bUyQuyenNguoiNhan = false;
                        }
                    }

                    hasItemInfo.Add("AuthorReceiver", bUyQuyenNguoiNhan);
                }
            }

            hasItemInfo.Add("PPA", dgvListItems.CurrentRow.Cells["colPPA"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colPPA"].Value : false);

            if (dgvListItems.CurrentRow.Cells["colPPA"].Value != null)
            {
                if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colPPA"].Value.ToString()))
                {
                    string contractNumberPPA = "";

                    if (dgvListItems.CurrentRow.Cells["colContractNumberPPA"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colContractNumberPPA"].Value.ToString()))
                        {
                            contractNumberPPA = dgvListItems.CurrentRow.Cells["colContractNumberPPA"].Value.ToString();
                        }
                    }

                    hasItemInfo.Add("ContractNumberPPA", contractNumberPPA);



                    DateTime contractDatePPA = DateTimeServer.Now;

                    if (dgvListItems.CurrentRow.Cells["colContractDatePPA"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colContractDatePPA"].Value.ToString()))
                        {
                            DateTime HanHDPPA;

                            if (DateTime.TryParseExact(dgvListItems.CurrentRow.Cells["colContractDatePPA"].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDPPA))
                            {
                                contractDatePPA = HanHDPPA;
                            }
                        }
                    }

                    hasItemInfo.Add("ContractDatePPA", contractDatePPA);
                }
            }

            hasItemInfo.Add("C", dgvListItems.CurrentRow.Cells["colC"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colC"].Value : false);

            if (dgvListItems.CurrentRow.Cells["colC"].Value != null)
            {
                if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colC"].Value.ToString()))
                {
                    string contractNumberC = "";

                    if (dgvListItems.CurrentRow.Cells["colContractNumberC"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colContractNumberC"].Value.ToString()))
                        {
                            contractNumberC = dgvListItems.CurrentRow.Cells["colContractNumberC"].Value.ToString();
                        }
                    }

                    hasItemInfo.Add("ContractNumberC", contractNumberC);



                    DateTime contractDateC = DateTimeServer.Now;

                    if (dgvListItems.CurrentRow.Cells["colContractDateC"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colContractDateC"].Value.ToString()))
                        {
                            DateTime HanHDC;

                            if (DateTime.TryParseExact(dgvListItems.CurrentRow.Cells["colContractDateC"].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDC))
                            {
                                contractDateC = HanHDC;
                            }
                        }
                    }

                    hasItemInfo.Add("ContractDateC", contractDateC);
                }
            }

            hasItemInfo.Add("BenThu3", dgvListItems.CurrentRow.Cells["colBenThu3"].Value != null ? (bool)dgvListItems.CurrentRow.Cells["colBenThu3"].Value : false);

            if (dgvListItems.CurrentRow.Cells["colBenThu3"].Value != null)
            {
                if (Convert.ToBoolean(dgvListItems.CurrentRow.Cells["colBenThu3"].Value.ToString()))
                {
                    string contractNumberT3 = "";

                    if (dgvListItems.CurrentRow.Cells["colContractNumberT3"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colContractNumberT3"].Value.ToString()))
                        {
                            contractNumberT3 = dgvListItems.CurrentRow.Cells["colContractNumberT3"].Value.ToString();
                        }
                    }

                    hasItemInfo.Add("ContractNumberT3", contractNumberT3);



                    DateTime contractDateT3 = DateTimeServer.Now;

                    if (dgvListItems.CurrentRow.Cells["colContractDateT3"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colContractDateT3"].Value.ToString()))
                        {
                            DateTime HanHDT3;

                            if (DateTime.TryParseExact(dgvListItems.CurrentRow.Cells["colContractDateT3"].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out HanHDT3))
                            {
                                contractDateT3 = HanHDT3;
                            }
                        }
                    }

                    hasItemInfo.Add("ContractDateT3", contractDateT3);

                    string TenT3 = "";

                    if (dgvListItems.CurrentRow.Cells["colThirdPartyName"].Value != null)
                    {
                        if (!string.IsNullOrEmpty(dgvListItems.CurrentRow.Cells["colThirdPartyName"].Value.ToString()))
                        {
                            TenT3 = dgvListItems.CurrentRow.Cells["colThirdPartyName"].Value.ToString();
                        }
                    }

                    hasItemInfo.Add("ThirdPartyName", TenT3);
                }
            }
            hasItemInfo.Add("VASService", dgvListItems.CurrentRow.Cells["colVASService"].Value != null ? dgvListItems.CurrentRow.Cells["colVASService"].Value.ToString() : "");
            hasItemInfo.Add("DeliveryTime", dgvListItems.CurrentRow.Cells["colDeliveryTime"].Value != null ? dgvListItems.CurrentRow.Cells["colDeliveryTime"].Value.ToString() : "");
            frmAcceptanceSingleInput frm = new frmAcceptanceSingleInput();
            frm.POSCode = this.POSCode;
            frm.OriginalPOSCode = this.OriginalPOSCode;
            frm.Username = this.Username;
            frm.ShiftHandover = this.ShiftHandover;
            frm.ServiceCode = cboService.SelectedValue.ToString();
            frm.FormStatus = FormStatus.EditSingle;
            frm.EditSingleItem += new frmAcceptanceSingleInput.EditSingleItemEventHandler(frm_EditSingleItem);
            frm.HasSingleItemInfo = hasItemInfo;
            frm.ShowDialog();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (CheckShifted())
            {
                ShowMessageBoxWarning("Ca làm việc hiện tại đã được chốt. Không cho phép điều chỉnh dữ liệu trong ca");
                return;
            }

            dgvListItems.EndEdit();

            if (dgvListItems.CurrentRow != null)
            {
                dgvListItems.Rows.RemoveAt(dgvListItems.CurrentRow.Index);

                foreach (DataGridViewRow row in dgvListItems.Rows)
                {
                    row.Cells["colIndex"].Value = row.Index + 1;
                }

                CalculatorTotalItem();

                CalculatorTotalWeight();

                CalculatorTotalFreight();
            }
        }

        void frm_AddSingleItem(Hashtable hasItemInfo)
        {
            if (hasItemInfo != null)
            {
                dgvListItems.Rows.Add();

                if (hasItemInfo.ContainsKey("ServiceCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ServiceCode"].ToString()))
                    {
                        cboService.SelectedValue = hasItemInfo["ServiceCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIndex"].Value = dgvListItems.Rows.Count;

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBarCode"].Value = "";

                if (hasItemInfo.ContainsKey("ItemCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ItemCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBarCode"].Value = hasItemInfo["ItemCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDataCode"].Value = "";
                if (hasItemInfo.ContainsKey("DataCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DataCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDataCode"].Value = hasItemInfo["DataCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAffair"].Value = false;
                if (hasItemInfo.ContainsKey("Affair"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Affair"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAffair"].Value = (bool)hasItemInfo["Affair"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colIsCollection"].Value = false;
                if (hasItemInfo.ContainsKey("Collection"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Collection"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colIsCollection"].Value = (bool)hasItemInfo["Collection"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colCustomerAccountNo"].Value = "";
                if (hasItemInfo.ContainsKey("CustomerAccountNo"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CustomerAccountNo"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCustomerAccountNo"].Value = hasItemInfo["CustomerAccountNo"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerCode"].Value = "";
                if (hasItemInfo.ContainsKey("CustomerCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CustomerCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerCode"].Value = hasItemInfo["CustomerCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerGroup"].Value = "";
                if (hasItemInfo.ContainsKey("CustomerGroupCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CustomerGroupCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCustomerGroup"].Value = hasItemInfo["CustomerGroupCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderFullName"].Value = "";
                if (hasItemInfo.ContainsKey("SenderFullName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderFullName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderFullName"].Value = hasItemInfo["SenderFullName"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderAddress"].Value = "";
                if (hasItemInfo.ContainsKey("SenderAddress"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderAddress"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderAddress"].Value = hasItemInfo["SenderAddress"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderTel"].Value = "";
                if (hasItemInfo.ContainsKey("SenderTel"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderTel"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderTel"].Value = hasItemInfo["SenderTel"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderEmail"].Value = "";
                if (hasItemInfo.ContainsKey("SenderEmail"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderEmail"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderEmail"].Value = hasItemInfo["SenderEmail"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderPOSCode"].Value = "";
                if (hasItemInfo.ContainsKey("SenderPostCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderPostCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderPOSCode"].Value = hasItemInfo["SenderPostCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderTaxCode"].Value = "";
                if (hasItemInfo.ContainsKey("SenderTaxCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderTaxCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderTaxCode"].Value = hasItemInfo["SenderTaxCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderID"].Value = "";
                if (hasItemInfo.ContainsKey("SenderIdentificationNumber"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderIdentificationNumber"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSenderID"].Value = hasItemInfo["SenderIdentificationNumber"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverCustomerCode"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverCustomerCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverCustomerCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverCustomerCode"].Value = hasItemInfo["ReceiverCustomerCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverFullName"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverFullName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverFullName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverFullName"].Value = hasItemInfo["ReceiverFullName"].ToString();
                    }
                }
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverAddress"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverAddress"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverAddress"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverAddress"].Value = hasItemInfo["ReceiverAddress"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverTel"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverTel"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverTel"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverTel"].Value = hasItemInfo["ReceiverTel"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverEmail"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverEmail"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverEmail"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverEmail"].Value = hasItemInfo["ReceiverEmail"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverContact"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverContact"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverContact"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverContact"].Value = hasItemInfo["ReceiverContact"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverPOSCode"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverPostCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverPostCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverPOSCode"].Value = hasItemInfo["ReceiverPostCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverTaxCode"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverTaxCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverTaxCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverTaxCode"].Value = hasItemInfo["ReceiverTaxCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverID"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverIdentificationNumber"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverIdentificationNumber"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colReceiverID"].Value = hasItemInfo["ReceiverIdentificationNumber"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCountryCode"].Value = "";
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colProvinceCode"].Value = "";
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDistrictCode"].Value = "";
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCommuneCode"].Value = "";

                if (hasItemInfo.ContainsKey("Country"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Country"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCountryCode"].Value = hasItemInfo["Countryr"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("CountryName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CountryName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCountryCodeName"].Value = hasItemInfo["CountryName"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("Province"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Province"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colProvinceCode"].Value = hasItemInfo["Province"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("ProvinceName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ProvinceName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colProvinceName"].Value = hasItemInfo["ProvinceName"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("District"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["District"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDistrictCode"].Value = hasItemInfo["District"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("DistrictName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DistrictName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDistrictName"].Value = hasItemInfo["DistrictName"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("Commnue"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Commnue"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCommuneCode"].Value = hasItemInfo["Commnue"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("CommnueName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CommnueName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCommuneName"].Value = hasItemInfo["CommnueName"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFarRegion"].Value = false;
                if (hasItemInfo.ContainsKey("FarRegion"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FarRegion"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFarRegion"].Value = (bool)hasItemInfo["FarRegion"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colisAir"].Value = false;
                if (hasItemInfo.ContainsKey("Air"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Air"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colisAir"].Value = (bool)hasItemInfo["Air"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colExecuteOrder"].Value = "";
                if (hasItemInfo.ContainsKey("ExecuteOrder"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ExecuteOrder"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colExecuteOrder"].Value = hasItemInfo["ExecuteOrder"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colInvoice"].Value = false;
                if (hasItemInfo.ContainsKey("Invoice"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Invoice"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colInvoice"].Value = (bool)hasItemInfo["Invoice"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOther"].Value = false;
                if (hasItemInfo.ContainsKey("OtherPaper"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OtherPaper"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOther"].Value = (bool)hasItemInfo["OtherPaper"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOtherInfo"].Value = "";
                if (hasItemInfo.ContainsKey("OtherPaperInfo"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OtherPaperInfo"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOtherInfo"].Value = hasItemInfo["OtherPaperInfo"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colValueAddedService"].Value = "";
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colValueAddedService"].Tag = null;

                if (hasItemInfo.ContainsKey("ValueAddedService"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ValueAddedService"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colValueAddedService"].Tag = hasItemInfo["ValueAddedService"];

                        List<ValueAddedServiceItemEntity> enVASIList = (List<ValueAddedServiceItemEntity>)hasItemInfo["ValueAddedService"];
                        string VASName = "";
                        foreach (ValueAddedServiceItemEntity enVASI in enVASIList)
                        {
                            if (!string.IsNullOrEmpty(VASName))
                                VASName = VASName + ";" + enVASI.ValueAddedServiceCode;
                            else
                                VASName = enVASI.ValueAddedServiceCode;
                        }
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colValueAddedService"].Value = VASName;
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVASPropertyValue"].Tag = null;
                if (hasItemInfo.ContainsKey("VASPropertyValue"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["VASPropertyValue"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVASPropertyValue"].Tag = hasItemInfo["VASPropertyValue"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDetailItem"].Value = "";
                if (hasItemInfo.ContainsKey("DetailItem"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DetailItem"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDetailItem"].Value = hasItemInfo["DetailItem"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDetailItem"].Tag = null;
                if (hasItemInfo.ContainsKey("DetailItemList"))
                {
                    if (hasItemInfo["DetailItemList"] != null && !string.IsNullOrEmpty(hasItemInfo["DetailItemList"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDetailItem"].Tag = hasItemInfo["DetailItemList"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemType"].Value = "";
                if (hasItemInfo.ContainsKey("ItemType"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ItemType"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemType"].Value = hasItemInfo["ItemType"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemTypeName"].Value = "";
                if (hasItemInfo.ContainsKey("ItemTypeName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ItemTypeName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colItemTypeName"].Value = hasItemInfo["ItemTypeName"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colComodityType"].Value = "";
                if (hasItemInfo.ContainsKey("CommodityType"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CommodityType"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colComodityType"].Value = hasItemInfo["CommodityType"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colComodityType"].Tag = null;
                if (hasItemInfo.ContainsKey("CommodityTypeList"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CommodityTypeList"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colComodityType"].Tag = hasItemInfo["CommodityTypeList"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colUndeliveryIndicator"].Value = "";
                if (hasItemInfo.ContainsKey("UndeliveryGuide"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["UndeliveryGuide"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colUndeliveryIndicator"].Value = hasItemInfo["UndeliveryGuide"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colUndeliveryIndicatorName"].Value = "";
                if (hasItemInfo.ContainsKey("UndeliveryGuideName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["UndeliveryGuideName"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colUndeliveryIndicatorName"].Value = hasItemInfo["UndeliveryGuideName"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDeliveryNote"].Value = "";
                if (hasItemInfo.ContainsKey("DeliveryNote"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DeliveryNote"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDeliveryNote"].Value = hasItemInfo["DeliveryNote"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colWeight"].Value = "";
                if (hasItemInfo.ContainsKey("Weight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Weight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colWeight"].Value = hasItemInfo["Weight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colLength"].Value = "";
                if (hasItemInfo.ContainsKey("Length"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Length"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colLength"].Value = hasItemInfo["Length"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colWidth"].Value = "";
                if (hasItemInfo.ContainsKey("Width"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Width"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colWidth"].Value = hasItemInfo["Width"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHeight"].Value = "";
                if (hasItemInfo.ContainsKey("Height"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Height"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHeight"].Value = hasItemInfo["Height"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colConvertWeight"].Value = "";
                if (hasItemInfo.ContainsKey("WeightConvert"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["WeightConvert"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colConvertWeight"].Value = hasItemInfo["WeightConvert"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFreePost"].Value = false;
                if (hasItemInfo.ContainsKey("PostFree"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PostFree"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFreePost"].Value = (bool)hasItemInfo["PostFree"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDebt"].Value = false;
                if (hasItemInfo.ContainsKey("Debt"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Debt"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDebt"].Value = (bool)hasItemInfo["Debt"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colInvoiceExport"].Value = false;
                if (hasItemInfo.ContainsKey("InvoiceExport"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["InvoiceExport"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colInvoiceExport"].Value = (bool)hasItemInfo["InvoiceExport"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDestinationPOSCode"].Value = "";
                if (hasItemInfo.ContainsKey("DestinationPOSCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DestinationPOSCode"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDestinationPOSCode"].Value = hasItemInfo["DestinationPOSCode"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIsDiscount"].Value = false;
                if (hasItemInfo.ContainsKey("IsDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["IsDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIsDiscount"].Value = (bool)hasItemInfo["IsDiscount"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDiscountPercent"].Value = "0";
                if (hasItemInfo.ContainsKey("DiscountPercent"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DiscountPercent"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDiscountPercent"].Value = hasItemInfo["DiscountPercent"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDiscountAmount"].Value = "0";
                if (hasItemInfo.ContainsKey("DiscountAmount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DiscountAmount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colDiscountAmount"].Value = hasItemInfo["DiscountAmount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIsFeedback"].Value = false;
                if (hasItemInfo.ContainsKey("IsFeedback"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["IsFeedback"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colIsFeedback"].Value = (bool)hasItemInfo["IsFeedback"];
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFeedbackPercent"].Value = "0";
                if (hasItemInfo.ContainsKey("FeedbackPercent"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FeedbackPercent"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFeedbackPercent"].Value = hasItemInfo["FeedbackPercent"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFeedbackAmount"].Value = "0";
                if (hasItemInfo.ContainsKey("FeedbackAmount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FeedbackAmount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFeedbackAmount"].Value = hasItemInfo["FeedbackAmount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colMainFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("MainFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["MainFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colMainFreight"].Value = hasItemInfo["MainFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSubFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("SubFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SubFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colSubFreight"].Value = hasItemInfo["SubFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFuelSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FuelSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FuelSurchargeFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFuelSurchargeFreight"].Value = hasItemInfo["FuelSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFarRegionFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FarRegionFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FarRegionFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFarRegionFreight"].Value = hasItemInfo["FarRegionFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAirSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("AirSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["AirSurchargeFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAirSurchargeFreight"].Value = hasItemInfo["AirSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreight"].Value = hasItemInfo["TotalFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightVAT"].Value = hasItemInfo["TotalFreightVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightDiscount"].Value = hasItemInfo["TotalFreightDiscount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightDiscountVAT"].Value = hasItemInfo["TotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colTotalFreightDiscountVAT"].Value = hasItemInfo["TotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVATPercentage"].Value = "0";
                if (hasItemInfo.ContainsKey("VATPercentage"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["VATPercentage"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVATPercentage"].Value = hasItemInfo["VATPercentage"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVATFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("VATFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["VATFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVATFreight"].Value = hasItemInfo["VATFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreight"].Value = hasItemInfo["RemainingFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreightVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreightVAT"].Value = hasItemInfo["RemainingFreightVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreightDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreightDiscount"].Value = hasItemInfo["RemainingFreightDiscount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colRemainingFreightDiscountVAT"].Value = hasItemInfo["RemainingFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreight"].Value = hasItemInfo["PaymentFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreightVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreightVAT"].Value = hasItemInfo["PaymentFreightVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreightDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreightDiscount"].Value = hasItemInfo["PaymentFreightDiscount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPaymentFreightDiscountVAT"].Value = hasItemInfo["PaymentFreightDiscountVAT"].ToString();
                    }
                }

                //--------------------------------------

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalMainFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalMainFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalMainFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalMainFreight"].Value = hasItemInfo["OriginalMainFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalSubFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalSubFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalSubFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalSubFreight"].Value = hasItemInfo["OriginalSubFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalFuelSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalFuelSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalFuelSurchargeFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalFuelSurchargeFreight"].Value = hasItemInfo["OriginalFuelSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalFarRegionFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalFarRegionFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalFarRegionFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalFarRegionFreight"].Value = hasItemInfo["OriginalFarRegionFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalAirSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalAirSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalAirSurchargeFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalAirSurchargeFreight"].Value = hasItemInfo["OriginalAirSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreight"].Value = hasItemInfo["OriginalTotalFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightVAT"].Value = hasItemInfo["OriginalTotalFreightVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightDiscount"].Value = hasItemInfo["OriginalTotalFreightDiscount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightDiscountVAT"].Value = hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalTotalFreightDiscountVAT"].Value = hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalVATPercentage"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalVATPercentage"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalVATPercentage"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalVATPercentage"].Value = hasItemInfo["OriginalVATPercentage"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalVATFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalVATFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalVATFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalVATFreight"].Value = hasItemInfo["OriginalVATFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreight"].Value = hasItemInfo["OriginalRemainingFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreightVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreightVAT"].Value = hasItemInfo["OriginalRemainingFreightVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreightDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreightDiscount"].Value = hasItemInfo["OriginalRemainingFreightDiscount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalRemainingFreightDiscountVAT"].Value = hasItemInfo["OriginalRemainingFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreight"].Value = hasItemInfo["OriginalPaymentFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreightVAT"].Value = hasItemInfo["OriginalPaymentFreightVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightDiscount"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreightDiscount"].Value = hasItemInfo["OriginalPaymentFreightDiscount"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colOriginalPaymentFreightDiscountVAT"].Value = hasItemInfo["OriginalPaymentFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFundFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FundFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FundFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFundFreight"].Value = hasItemInfo["FundFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFundVASFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FundVASFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FundVASFreight"].ToString()))
                    {
                        dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colFundVASFreight"].Value = hasItemInfo["FundVASFreight"].ToString();
                    }
                }

                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colCOD"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPDK"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAR"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colAREmail"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colARSMS"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPTT"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colVUN"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colKA"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colKB"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colKC"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHGN"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHGL"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHTN"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colHTL"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colV"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colPPA"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colC"].Value = false;
                dgvListItems.Rows[dgvListItems.Rows.Count - 1].Cells["colBenThu3"].Value = false;

                CalculatorTotalItem();

                CalculatorTotalWeight();

                CalculatorTotalFreight();
            }
        }

        void frm_EditSingleItem(Hashtable hasItemInfo)
        {
            if (hasItemInfo != null)
            {
                if (hasItemInfo.ContainsKey("ServiceCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ServiceCode"].ToString()))
                    {
                        cboService.SelectedValue = hasItemInfo["ServiceCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colBarCode"].Value = "";
                if (hasItemInfo.ContainsKey("ItemCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ItemCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colBarCode"].Value = hasItemInfo["ItemCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colDataCode"].Value = "";
                if (hasItemInfo.ContainsKey("DataCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DataCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDataCode"].Value = hasItemInfo["DataCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colAffair"].Value = false;
                if (hasItemInfo.ContainsKey("Affair"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Affair"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colAffair"].Value = (bool)hasItemInfo["Affair"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colIsCollection"].Value = false;
                if (hasItemInfo.ContainsKey("Collection"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Collection"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colIsCollection"].Value = (bool)hasItemInfo["Collection"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colCustomerAccountNo"].Value = "";
                if (hasItemInfo.ContainsKey("CustomerAccountNo"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CustomerAccountNo"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCustomerAccountNo"].Value = hasItemInfo["CustomerAccountNo"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colCustomerCode"].Value = "";
                if (hasItemInfo.ContainsKey("CustomerCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CustomerCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCustomerCode"].Value = hasItemInfo["CustomerCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colCustomerGroup"].Value = "";
                if (hasItemInfo.ContainsKey("CustomerGroupCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CustomerGroupCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCustomerGroup"].Value = hasItemInfo["CustomerGroupCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderFullName"].Value = "";
                if (hasItemInfo.ContainsKey("SenderFullName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderFullName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderFullName"].Value = hasItemInfo["SenderFullName"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderAddress"].Value = "";
                if (hasItemInfo.ContainsKey("SenderAddress"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderAddress"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderAddress"].Value = hasItemInfo["SenderAddress"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderTel"].Value = "";
                if (hasItemInfo.ContainsKey("SenderTel"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderTel"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderTel"].Value = hasItemInfo["SenderTel"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderEmail"].Value = "";
                if (hasItemInfo.ContainsKey("SenderEmail"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderEmail"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderEmail"].Value = hasItemInfo["SenderEmail"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderTaxCode"].Value = "";
                if (hasItemInfo.ContainsKey("SenderTaxCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderTaxCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderTaxCode"].Value = hasItemInfo["SenderTaxCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderPOSCode"].Value = "";
                if (hasItemInfo.ContainsKey("SenderPostCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderPostCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderPOSCode"].Value = hasItemInfo["SenderPostCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSenderID"].Value = "";
                if (hasItemInfo.ContainsKey("SenderIdentificationNumber"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SenderIdentificationNumber"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSenderID"].Value = hasItemInfo["SenderIdentificationNumber"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverCustomerCode"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverCustomerCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverCustomerCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverCustomerCode"].Value = hasItemInfo["ReceiverCustomerCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverFullName"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverFullName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverFullName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverFullName"].Value = hasItemInfo["ReceiverFullName"].ToString();
                    }
                }
                dgvListItems.CurrentRow.Cells["colReceiverAddress"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverAddress"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverAddress"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverAddress"].Value = hasItemInfo["ReceiverAddress"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverTel"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverTel"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverTel"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverTel"].Value = hasItemInfo["ReceiverTel"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverEmail"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverEmail"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverEmail"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverEmail"].Value = hasItemInfo["ReceiverEmail"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverContact"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverContact"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverContact"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverContact"].Value = hasItemInfo["ReceiverContact"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverPOSCode"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverPostCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverPostCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverPOSCode"].Value = hasItemInfo["ReceiverPostCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverTaxCode"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverTaxCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverTaxCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverTaxCode"].Value = hasItemInfo["ReceiverTaxCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colReceiverID"].Value = "";
                if (hasItemInfo.ContainsKey("ReceiverIdentificationNumber"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ReceiverIdentificationNumber"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colReceiverID"].Value = hasItemInfo["ReceiverIdentificationNumber"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colCountryCode"].Value = "";
                dgvListItems.CurrentRow.Cells["colCountryName"].Value = "";

                dgvListItems.CurrentRow.Cells["colProvinceCode"].Value = "";
                dgvListItems.CurrentRow.Cells["colProvinceCode"].Value = "";

                dgvListItems.CurrentRow.Cells["colDistrictCode"].Value = "";
                dgvListItems.CurrentRow.Cells["colDistrictCode"].Value = "";

                dgvListItems.CurrentRow.Cells["colCommuneCode"].Value = "";
                dgvListItems.CurrentRow.Cells["colCommuneName"].Value = "";

                if (hasItemInfo.ContainsKey("Country"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Country"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCountryCode"].Value = hasItemInfo["Country"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("CountryName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CountryName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCountryName"].Value = hasItemInfo["CountryName"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("Province"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Province"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colProvinceCode"].Value = hasItemInfo["Province"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("ProvinceName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ProvinceName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colProvinceName"].Value = hasItemInfo["ProvinceName"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("District"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["District"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDistrictCode"].Value = hasItemInfo["District"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("DistrictName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DistrictName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDistrictName"].Value = hasItemInfo["DistrictName"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("Commune"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Commune"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCommuneCode"].Value = hasItemInfo["Commune"].ToString();
                    }
                }

                if (hasItemInfo.ContainsKey("CommuneName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CommuneName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colCommuneName"].Value = hasItemInfo["CommuneName"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFarRegion"].Value = false;
                if (hasItemInfo.ContainsKey("FarRegion"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FarRegion"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFarRegion"].Value = (bool)hasItemInfo["FarRegion"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colisAir"].Value = false;
                if (hasItemInfo.ContainsKey("Air"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Air"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colisAir"].Value = (bool)hasItemInfo["Air"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colExecuteOrder"].Value = "";
                if (hasItemInfo.ContainsKey("ExecuteOrder"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ExecuteOrder"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colExecuteOrder"].Value = hasItemInfo["ExecuteOrder"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colInvoice"].Value = false;
                if (hasItemInfo.ContainsKey("Invoice"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Invoice"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colInvoice"].Value = (bool)hasItemInfo["Invoice"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colOther"].Value = false;
                if (hasItemInfo.ContainsKey("OtherPaper"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OtherPaper"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOther"].Value = (bool)hasItemInfo["OtherPaper"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colOtherInfo"].Value = "";
                if (hasItemInfo.ContainsKey("OtherPaperInfo"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OtherPaperInfo"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOtherInfo"].Value = hasItemInfo["OtherPaperInfo"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colValueAddedService"].Value = "";
                dgvListItems.CurrentRow.Cells["colValueAddedService"].Tag = null;

                if (hasItemInfo.ContainsKey("ValueAddedService"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ValueAddedService"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colValueAddedService"].Tag = hasItemInfo["ValueAddedService"];

                        List<ValueAddedServiceItemEntity> enVASIList = (List<ValueAddedServiceItemEntity>)hasItemInfo["ValueAddedService"];
                        string VASName = "";
                        foreach (ValueAddedServiceItemEntity enVASI in enVASIList)
                        {
                            if (!string.IsNullOrEmpty(VASName))
                                VASName = VASName + ";" + enVASI.ValueAddedServiceCode;
                            else
                                VASName = enVASI.ValueAddedServiceCode;
                        }
                        dgvListItems.CurrentRow.Cells["colValueAddedService"].Value = VASName;
                    }
                }

                dgvListItems.CurrentRow.Cells["colVASPropertyValue"].Tag = null;
                if (hasItemInfo.ContainsKey("VASPropertyValue"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["VASPropertyValue"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colVASPropertyValue"].Tag = hasItemInfo["VASPropertyValue"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colDetailItem"].Value = "";
                if (hasItemInfo.ContainsKey("DetailItem"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DetailItem"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDetailItem"].Value = hasItemInfo["DetailItem"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colDetailItem"].Tag = null;
                if (hasItemInfo.ContainsKey("DetailItemList"))
                {
                    if (hasItemInfo["DetailItemList"] != null && !string.IsNullOrEmpty(hasItemInfo["DetailItemList"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDetailItem"].Tag = hasItemInfo["DetailItemList"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colItemType"].Value = "";
                if (hasItemInfo.ContainsKey("ItemType"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ItemType"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colItemType"].Value = hasItemInfo["ItemType"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colItemTypeName"].Value = "";
                if (hasItemInfo.ContainsKey("ItemTypeName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["ItemTypeName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colItemTypeName"].Value = hasItemInfo["ItemTypeName"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colComodityType"].Value = "";
                if (hasItemInfo.ContainsKey("CommodityType"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CommodityType"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colComodityType"].Value = hasItemInfo["CommodityType"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colComodityType"].Tag = null;
                if (hasItemInfo.ContainsKey("CommodityTypeList"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["CommodityTypeList"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colComodityType"].Tag = hasItemInfo["CommodityTypeList"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colUndeliveryIndicator"].Value = "";
                if (hasItemInfo.ContainsKey("UndeliveryGuide"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["UndeliveryGuide"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colUndeliveryIndicator"].Value = hasItemInfo["UndeliveryGuide"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colUndeliveryIndicatorName"].Value = "";
                if (hasItemInfo.ContainsKey("UndeliveryGuideName"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["UndeliveryGuideName"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colUndeliveryIndicatorName"].Value = hasItemInfo["UndeliveryGuideName"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colDeliveryNote"].Value = "";
                if (hasItemInfo.ContainsKey("DeliveryNote"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DeliveryNote"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDeliveryNote"].Value = hasItemInfo["DeliveryNote"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colWeight"].Value = "";
                if (hasItemInfo.ContainsKey("Weight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Weight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colWeight"].Value = hasItemInfo["Weight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colLength"].Value = "";
                if (hasItemInfo.ContainsKey("Length"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Length"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colLength"].Value = hasItemInfo["Length"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colWidth"].Value = "";
                if (hasItemInfo.ContainsKey("Width"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Width"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colWidth"].Value = hasItemInfo["Width"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colHeight"].Value = "";
                if (hasItemInfo.ContainsKey("Height"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Height"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colHeight"].Value = hasItemInfo["Height"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colConvertWeight"].Value = "";
                if (hasItemInfo.ContainsKey("WeightConvert"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["WeightConvert"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colConvertWeight"].Value = hasItemInfo["WeightConvert"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFreePost"].Value = false;
                if (hasItemInfo.ContainsKey("PostFree"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PostFree"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFreePost"].Value = (bool)hasItemInfo["PostFree"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colDebt"].Value = false;
                if (hasItemInfo.ContainsKey("Debt"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["Debt"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDebt"].Value = (bool)hasItemInfo["Debt"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colInvoiceExport"].Value = false;
                if (hasItemInfo.ContainsKey("InvoiceExport"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["InvoiceExport"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colInvoiceExport"].Value = (bool)hasItemInfo["InvoiceExport"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colDestinationPOSCode"].Value = "";
                if (hasItemInfo.ContainsKey("DestinationPOSCode"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DestinationPOSCode"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDestinationPOSCode"].Value = hasItemInfo["DestinationPOSCode"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colIsDiscount"].Value = false;
                if (hasItemInfo.ContainsKey("IsDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["IsDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colIsDiscount"].Value = (bool)hasItemInfo["IsDiscount"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colDiscountPercent"].Value = "0";
                if (hasItemInfo.ContainsKey("DiscountPercent"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DiscountPercent"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDiscountPercent"].Value = hasItemInfo["DiscountPercent"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colDiscountAmount"].Value = "0";
                if (hasItemInfo.ContainsKey("DiscountAmount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["DiscountAmount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colDiscountAmount"].Value = hasItemInfo["DiscountAmount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colIsFeedback"].Value = false;
                if (hasItemInfo.ContainsKey("IsFeedback"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["IsFeedback"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colIsFeedback"].Value = (bool)hasItemInfo["IsFeedback"];
                    }
                }

                dgvListItems.CurrentRow.Cells["colFeedbackPercent"].Value = "0";
                if (hasItemInfo.ContainsKey("FeedbackPercent"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FeedbackPercent"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFeedbackPercent"].Value = hasItemInfo["FeedbackPercent"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFeedbackAmount"].Value = "0";
                if (hasItemInfo.ContainsKey("FeedbackAmount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FeedbackAmount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFeedbackAmount"].Value = hasItemInfo["FeedbackAmount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colMainFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("MainFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["MainFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colMainFreight"].Value = hasItemInfo["MainFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colSubFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("SubFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["SubFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colSubFreight"].Value = hasItemInfo["SubFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFuelSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FuelSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FuelSurchargeFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFuelSurchargeFreight"].Value = hasItemInfo["FuelSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFarRegionFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FarRegionFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FarRegionFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFarRegionFreight"].Value = hasItemInfo["FarRegionFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colAirSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("AirSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["AirSurchargeFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colAirSurchargeFreight"].Value = hasItemInfo["AirSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colVATPercentage"].Value = "0";
                if (hasItemInfo.ContainsKey("VATPercentage"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["VATPercentage"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colVATPercentage"].Value = hasItemInfo["VATPercentage"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colVATFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("VATFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["VATFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colVATFreight"].Value = hasItemInfo["VATFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colTotalFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colTotalFreightVAT"].Value = hasItemInfo["TotalFreightVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colTotalFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colTotalFreightDiscount"].Value = hasItemInfo["TotalFreightDiscount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colTotalFreightDiscountVAT"].Value = hasItemInfo["TotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("TotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["TotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colTotalFreightDiscountVAT"].Value = hasItemInfo["TotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colRemainingFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colRemainingFreight"].Value = hasItemInfo["RemainingFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colRemainingFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreightVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colRemainingFreightVAT"].Value = hasItemInfo["RemainingFreightVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colRemainingFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreightDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colRemainingFreightDiscount"].Value = hasItemInfo["RemainingFreightDiscount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colRemainingFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("RemainingFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["RemainingFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colRemainingFreightDiscountVAT"].Value = hasItemInfo["RemainingFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colPaymentFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colPaymentFreight"].Value = hasItemInfo["PaymentFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colPaymentFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreightVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colPaymentFreightVAT"].Value = hasItemInfo["PaymentFreightVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colPaymentFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreightDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colPaymentFreightDiscount"].Value = hasItemInfo["PaymentFreightDiscount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colPaymentFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("PaymentFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["PaymentFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colPaymentFreightDiscountVAT"].Value = hasItemInfo["PaymentFreightDiscountVAT"].ToString();
                    }
                }

                //--------------

                dgvListItems.CurrentRow.Cells["colOriginalMainFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalMainFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalMainFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalMainFreight"].Value = hasItemInfo["OriginalMainFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalSubFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalSubFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalSubFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalSubFreight"].Value = hasItemInfo["OriginalSubFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalFuelSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalFuelSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalFuelSurchargeFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalFuelSurchargeFreight"].Value = hasItemInfo["OriginalFuelSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalFarRegionFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalFarRegionFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalFarRegionFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalFarRegionFreight"].Value = hasItemInfo["OriginalFarRegionFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalAirSurchargeFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalAirSurchargeFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalAirSurchargeFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalAirSurchargeFreight"].Value = hasItemInfo["OriginalAirSurchargeFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalVATPercentage"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalVATPercentage"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalVATPercentage"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalVATPercentage"].Value = hasItemInfo["OriginalVATPercentage"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalVATFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalVATFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalVATFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalVATFreight"].Value = hasItemInfo["OriginalVATFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalTotalFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalTotalFreightVAT"].Value = hasItemInfo["OriginalTotalFreightVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalTotalFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalTotalFreightDiscount"].Value = hasItemInfo["OriginalTotalFreightDiscount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalTotalFreightDiscountVAT"].Value = hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalTotalFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalTotalFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalTotalFreightDiscountVAT"].Value = hasItemInfo["OriginalTotalFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalRemainingFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalRemainingFreight"].Value = hasItemInfo["OriginalRemainingFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalRemainingFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreightVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalRemainingFreightVAT"].Value = hasItemInfo["OriginalRemainingFreightVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalRemainingFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreightDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalRemainingFreightDiscount"].Value = hasItemInfo["OriginalRemainingFreightDiscount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalRemainingFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalRemainingFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalRemainingFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalRemainingFreightDiscountVAT"].Value = hasItemInfo["OriginalRemainingFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalPaymentFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalPaymentFreight"].Value = hasItemInfo["OriginalPaymentFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightVAT"].Value = hasItemInfo["OriginalPaymentFreightVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightDiscount"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightDiscount"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightDiscount"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightDiscount"].Value = hasItemInfo["OriginalPaymentFreightDiscount"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightDiscountVAT"].Value = hasItemInfo["OriginalPaymentFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightDiscountVAT"].Value = "0";
                if (hasItemInfo.ContainsKey("OriginalPaymentFreightDiscountVAT"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["OriginalPaymentFreightDiscountVAT"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colOriginalPaymentFreightDiscountVAT"].Value = hasItemInfo["OriginalPaymentFreightDiscountVAT"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFundFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FundFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FundFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFundFreight"].Value = hasItemInfo["FundFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colFundVASFreight"].Value = "0";
                if (hasItemInfo.ContainsKey("FundVASFreight"))
                {
                    if (!string.IsNullOrEmpty(hasItemInfo["FundVASFreight"].ToString()))
                    {
                        dgvListItems.CurrentRow.Cells["colFundVASFreight"].Value = hasItemInfo["FundVASFreight"].ToString();
                    }
                }

                dgvListItems.CurrentRow.Cells["colCOD"].Value = false;
                dgvListItems.CurrentRow.Cells["colPDK"].Value = false;
                dgvListItems.CurrentRow.Cells["colAR"].Value = false;
                dgvListItems.CurrentRow.Cells["colAREmail"].Value = false;
                dgvListItems.CurrentRow.Cells["colARSMS"].Value = false;
                dgvListItems.CurrentRow.Cells["colPTT"].Value = false;
                dgvListItems.CurrentRow.Cells["colVUN"].Value = false;
                dgvListItems.CurrentRow.Cells["colKA"].Value = false;
                dgvListItems.CurrentRow.Cells["colKB"].Value = false;
                dgvListItems.CurrentRow.Cells["colKC"].Value = false;
                dgvListItems.CurrentRow.Cells["colHGN"].Value = false;
                dgvListItems.CurrentRow.Cells["colHGL"].Value = false;
                dgvListItems.CurrentRow.Cells["colHTN"].Value = false;
                dgvListItems.CurrentRow.Cells["colHTL"].Value = false;
                dgvListItems.CurrentRow.Cells["colV"].Value = false;
                dgvListItems.CurrentRow.Cells["colPPA"].Value = false;
                dgvListItems.CurrentRow.Cells["colC"].Value = false;
                dgvListItems.CurrentRow.Cells["colBenThu3"].Value = false;

                CalculatorTotalItem();

                CalculatorTotalWeight();

                CalculatorTotalFreight();
            }
        }

        private void cboService_SelectedValueChanged(object sender, EventArgs e)
        {
            displayFreight();
        }

        private void dtpSendingTime_ValueChanged(object sender, EventArgs e)
        {
            DateTime dtCurrentDateTime = new DateTime(dtpFromDate.Value.Year, dtpFromDate.Value.Month, dtpFromDate.Value.Day);

            if (dtCurrentDateTime != dtOldDateTime)
            {
                dtOldDateTime = dtCurrentDateTime;

                displayFreight();
            }
        }

        private ValueAddedServiceItemEntity CreateValueAddedServiceItem(string serviceCode, string vasCode, string itemCode, DateTime sendingTime, string acceptancePosCode, Hashtable htVAS)
        {
            ValueAddedServiceItemEntity enValueAddedServiceItem = new ValueAddedServiceItemEntity();
            enValueAddedServiceItem.ServiceCode = serviceCode;
            enValueAddedServiceItem.ValueAddedServiceCode = vasCode;
            enValueAddedServiceItem.ItemCode = itemCode;
            enValueAddedServiceItem.Freight = 0;
            enValueAddedServiceItem.FreightVAT = 0;
            enValueAddedServiceItem.OriginalFreight = 0;
            enValueAddedServiceItem.OriginalFreightVAT = 0;
            enValueAddedServiceItem.PhaseCode = PhaseConstance.NHAN_GUI;
            enValueAddedServiceItem.AddedDate = sendingTime;
            enValueAddedServiceItem.POSCode = acceptancePosCode;

            enValueAddedServiceItem.SubFreight = 0;
            enValueAddedServiceItem.SubFreightVAT = 0;
            enValueAddedServiceItem.OriginalSubFreight = 0;
            enValueAddedServiceItem.OriginalSubFreightVAT = 0;

            if (htVAS.ContainsKey("Freight"))
            {
                double dResult;
                if (double.TryParse(htVAS["Freight"].ToString(), out dResult))
                {
                    enValueAddedServiceItem.Freight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                }
            }

            if (htVAS.ContainsKey("FreightVAT"))
            {
                double dResult;
                if (double.TryParse(htVAS["FreightVAT"].ToString(), out dResult))
                {
                    enValueAddedServiceItem.FreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                }
            }

            if (htVAS.ContainsKey("OriginalFreight"))
            {
                double dResult;
                if (double.TryParse(htVAS["OriginalFreight"].ToString(), out dResult))
                {
                    enValueAddedServiceItem.OriginalFreight = Math.Round(dResult, MidpointRounding.AwayFromZero);
                }
            }

            if (htVAS.ContainsKey("OriginalFreightVAT"))
            {
                double dResult;
                if (double.TryParse(htVAS["OriginalFreightVAT"].ToString(), out dResult))
                {
                    enValueAddedServiceItem.OriginalFreightVAT = Math.Round(dResult, MidpointRounding.AwayFromZero);
                }
            }

            return enValueAddedServiceItem;
        }

        private bool ValidateData()
        {
            bool result = false;
            try
            {
                if (CheckSendingTime())
                    result = true;

                if (CheckItemCode())
                    result = true;

                if (CheckSymbolItem())
                    result = true;

                if (CheckSumItem())
                    result = true;

                if (CheckDataCode())
                    result = true;

                if (CheckCustomerCode())
                    result = true;

                if (CheckSenderFullName())
                    result = true;

                if (CheckSenderFullNameSymbol())
                    result = true;

                if (CheckSenderAddress())
                    result = true;

                if (CheckSenderAddressSymbol())
                    result = true;

                if (CheckReceiverCustomerCode())
                    result = true;

                if (CheckReceiverCustomerCodeByItemType())
                    result = true;

                if (CheckReceiverFullName())
                    result = true;

                if (CheckReceiverFullNameSymbol())
                    result = true;

                if (CheckReceiverAddress())
                    result = true;

                if (CheckReceiverAddressSymbol())
                    result = true;

                if (CheckCountryProvince())
                    result = true;

                if (CheckItemType())
                    result = true;

                if (CheckItemContent())
                    result = true;

                if (CheckUndeliveryGuide())
                    result = true;

                if (CheckWeight())
                    result = true;

                if (CheckLength())
                    result = true;

                if (CheckWidth())
                    result = true;

                if (CheckHeight())
                    result = true;

                if (CheckCOD())
                    result = true;

                if (CheckDetailItemNameSymbol())
                    result = true;

                if (CheckContractNumberPPA())
                    result = true;

                if (CheckContractDatePPA())
                    result = true;

                if (CheckContractNumberC())
                    result = true;

                if (CheckContractDateC())
                    result = true;

                if (CheckContractNumberT3())
                    result = true;

                if (CheckContractDateT3())
                    result = true;

                if (CheckItemCodeOriginalExists())
                    result = true;

                if (CheckReceiverPOSCode())
                    result = true;

                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.Log(ex.Message, ErrorSource + "ValidateData");
                return result;
            }
        }
    }
}
