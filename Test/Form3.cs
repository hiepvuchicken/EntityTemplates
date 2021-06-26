using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.ExchangeServiceDomain;
using Test.ExchangeServiceUAT;


namespace Test
{
    public partial class Form3 : Form
    {
        ExchangeServiceDomain.ItemDataSet itemDataSet = new ExchangeServiceDomain.ItemDataSet();
        ExchangeServiceUAT.MailTripDataSet mailTripDataSet = new ExchangeServiceUAT.MailTripDataSet();
        ExchangeServiceUAT.ItemDataSet itemDataSetUAT = new ExchangeServiceUAT.ItemDataSet();
        ExchangeServiceUAT.BC37DataSet BC37DataSet = new ExchangeServiceUAT.BC37DataSet();

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ExchangeServiceDomain.GetMailTripRequest getMailTripRequest = new ExchangeServiceDomain.GetMailTripRequest();
            //getMailTripRequest.fromPOSCode = "930100";
            //getMailTripRequest.toPOSCode = "700955";
            //getMailTripRequest.mailTripType = "1";
            //getMailTripRequest.mailTripNumber = "672";
            //getMailTripRequest.serviceCode = "C";
            //getMailTripRequest.year = "20190318";
            //getMailTripRequest.username = "BDHCM";
            //getMailTripRequest.password = "BDHCM";

            //var s = new ServiceClient().

            //StartingCode DestinationCode MailtripType ServiceCode Year MailtripNumber
            //700920  700000  1   R   20190128    42

            ExchangeServiceDomain.ServiceClient client = new ExchangeServiceDomain.ServiceClient("httpExchangeService1");
            var s = client.GetMailTrip("563830", "700955", "1", "C", "20190502", "508", "OE", "OE");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s = "CP766637857VN|CP766637891VN|CP766637914VN|CP766637931VN|CP766638441VN";
            string[] s1 = s.Split('|');

            if (s1.Length > 0)
            {
                DataTable dt = itemDataSet.Tables["Item"];
                //dt.Columns.Add(new DataColumn("ItemCode", typeof(string)));

                DataTable dtA = itemDataSet.Tables["AttachDocumentsItem"];//new DataTable("AttachDocumentsItem");
                //dtA.Columns.Add(new DataColumn("ItemCode", typeof(string)));
                //dtA.Columns.Add(new DataColumn("POSCode", typeof(string)));


                foreach (var item in s1)
                {
                    string Itemcode = item.ToString();
                    string POSCode = "700000";

                    DataRow dr = dt.NewRow();
                    dr["ItemCode"] = Itemcode;
                    dr["AcceptancePOSCode"] = "700000";
                    dr["SenderFullname"] = "HIEPVU";
                    dr["SenderAddress"] = "123";
                    dr["ReceiverFullname"] = "CHICKEN";
                    dr["ReceiverAddress"] = "456";
                    dr["SendingContent"] = "TestAPI";
                    dr["ItemTypeCode"] = "BK";
                    dr["IsAirmail"] = false;
                    dr["Weight"] = 1000;
                    dr["ProvinceCode"] = "10";
                    dr["TotalFreight"] = "10";
                    dr["ServiceCode"] = "C";
                    dr["ReceiverAddressCode"] = "700000";

                    dt.Rows.Add(dr);
                    //itemDataSet.Tables.Add(dt);


                    DataRow drA = dtA.NewRow();
                    drA["ItemCode"] = Itemcode;
                    drA["POSCode"] = POSCode;

                    dtA.Rows.Add(drA);
                }

                //itemDataSet.Tables.Add(dtA);

                itemDataSet.Tables.Count.ToString();
            }

        }

        private void AddItem(string Itemcode, string Poscode)
        {




        }

        private void btnAddMailtripUAT_Click(object sender, EventArgs e)
        {
            DataTable dt = mailTripDataSet.Tables["MailTrip"];
            DataRow dr = dt.NewRow();
            dr["StartingCode"] = "700955";
            dr["DestinationCode"] = "590100";
            dr["MailtripType"] = "1";
            dr["ServiceCode"] = "L";
            dr["Year"] = "20190416";
            dr["MailtripNumber"] = "5850";
            dr["OutgoingDate"] = DateTime.Now;
            dr["Status"] = "4";
            dr["Quantity"] = "100";
            dr["Weight"] = "1000";
            dr["PackagingTime"] = DateTime.Now;
            dr["PackagingUser"] = "Hiệp Vũ";
            dr["PackagingMachineName"] = "HIEPVB-PC";
            dr["InitialTime"] = DateTime.Now;
            dr["InitialMachineName"] = "HIEPVB-PC";
            dr["TransferMachine"] = "HIEPVB-PC";
            dr["TransferUser"] = "Hiệp Vũ";
            dr["TransferPOSCode"] = "700955";
            dr["TransferDate"] = DateTime.Now;
            dr["TransferStatus"] = true;
            dr["TransferTimes"] = "1";
            //dt.Rows.Add(dr);

            mailTripDataSet.Tables["MailTrip"].Rows.Add(dr);

            //ExchangeServiceUAT.AddMailtripRequest addMailtripRequest = new ExchangeServiceUAT.AddMailtripRequest();
            //addMailtripRequest.mailTripDataSet = mailTripDataSet;
            //addMailtripRequest.password = "mypostuat@123";
            //addMailtripRequest.username = "mypostuat";

            DataTable dtItem = mailTripDataSet.Tables["Item"];
            DataRow drItem = dtItem.NewRow();
            drItem["ItemCode"] = "EL980009938VN";
            drItem["AcceptancePOSCode"] = "100900";
            drItem["SenderFullname"] = "Hiệp Vũ";
            drItem["ReceiverFullname"] = "aaa";
            drItem["ReceiverAddress"] = "bbb";
            drItem["SendingContent"] = "clgt";
            drItem["ItemTypeCode"] = "E";
            drItem["IsAirmail"] = false;
            drItem["Weight"] = "1000";
            drItem["TotalFreight"] = "5000000";
            drItem["ServiceCode"] = "E";

            mailTripDataSet.Tables["Item"].Rows.Add(drItem);

            ExchangeServiceUAT.ServiceClient client = new ExchangeServiceUAT.ServiceClient("httpExchangeService");
            var s = client.AddMailtrip(mailTripDataSet, "mypostuat", "mypostuat@123");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                
                ExchangeReal.ServiceClient client = new ExchangeReal.ServiceClient("httpExchangeService2");
                var ds = client.GetAllMailRouteSchedule("BDHCM", "BDHCM");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string s = "TRUE\r\n";
            bool bCODResult;

            if (bool.TryParse(s, out bCODResult))
            {
                if (bCODResult)
                {
                    
                }
                else
                {
                    
                }
            }
            else
            {

            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            //DataTable dataTable = itemDataSet.Item;

            //var dr = itemDataSet.Item.NewItemRow();
            //dr.Abatement =1000;

            //itemDataSet.Item.AddItemRow(dr);

            DataTable dtItem = itemDataSetUAT.Tables["Item"];
            DataRow drItem = dtItem.NewRow();
            drItem["ItemCode"] = "EL980009941VN";
            drItem["AcceptancePOSCode"] = "100900";
            drItem["SenderFullname"] = "Hiệp Vũ";
            drItem["ReceiverFullname"] = "aaa";
            drItem["ReceiverAddress"] = "bbb";
            drItem["SendingContent"] = "clgt";
            drItem["ItemTypeCode"] = "E";
            drItem["IsAirmail"] = false;
            drItem["Weight"] = "1000";
            drItem["TotalFreight"] = "5000000";
            drItem["ServiceCode"] = "E";

            itemDataSetUAT.Tables["Item"].Rows.Add(drItem);

            //DataTable dtSortingItem = itemDataSet.Tables["SortingItem"];
            DataRow drSortingItem = itemDataSetUAT.Tables["SortingItem"].NewRow();

            drSortingItem["POSCode"] = "100900";
            drSortingItem["ItemCode"] = "EL980009941VN";
            drSortingItem["SortingCode"] = "123456";
            drSortingItem["Type"] = Byte.Parse("1");
            drSortingItem["CreateTime"] = "2019-12-03 10:58:44.344";
            drSortingItem["LastUpdatedTime"] = DateTime.Now;

            itemDataSetUAT.Tables["SortingItem"].Rows.Add(drSortingItem);
            //itemDataSetUAT.Tables["SortingItem"].Rows.Add(drSortingItem);


            DataRow drValuesAddedServiceItem = itemDataSetUAT.Tables["ValueAddedServiceItem"].NewRow();

            drValuesAddedServiceItem["AddedDate"] = DateTime.Now.ToString();
            drValuesAddedServiceItem["Freight"] = 10000;
            drValuesAddedServiceItem["FreightVAT"] = 11000;
            drValuesAddedServiceItem["ItemCode"] = "EL980009941VN";
            drValuesAddedServiceItem["OriginalFreight"] = 10000;
            drValuesAddedServiceItem["OriginalFreightVAT"] = 11000;
            drValuesAddedServiceItem["PhaseCode"] = "NG";
            drValuesAddedServiceItem["POSCode"] = "100900";
            drValuesAddedServiceItem["ServiceCode"] = "E";
            drValuesAddedServiceItem["ValueAddedServiceCode"] = "AR";

            itemDataSetUAT.Tables["ValueAddedServiceItem"].Rows.Add(drValuesAddedServiceItem);

            ExchangeServiceUAT.ServiceClient client = new ExchangeServiceUAT.ServiceClient("httpExchangeService");
            var s = client.AddItem(itemDataSetUAT, "mypostuat", "mypostuat@123");

            //ExchangeServiceDomain.ServiceClient sv = new ExchangeServiceDomain.ServiceClient("httpExchangeService");
            //var rq = sv.AddMailtrip()
        }

        private void button5_Click(object sender, EventArgs e)
        {
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string signature = string.Empty;
            string Url = string.Empty;
            string pnsIDs = string.Empty;
            string Key_Encrypt = string.Empty;

            Key_Encrypt = "1c2ab110efc8b0f3ee49e06fa8eccd71";
            //Url = "https://test-pns.vnpost.vn";
            Url = "http://103.21.149.238:1238";

            List<string> lstLockOrder = new List<string>();
            lstLockOrder.Add("1296850");
            lstLockOrder.Add("1296815");
            lstLockOrder.Add("1296838");
            lstLockOrder.Add("1296839");
            lstLockOrder.Add("1296840");
            lstLockOrder.Add("1296841");

            pnsIDs = string.Join(";", lstLockOrder.ToArray());
            signature = ComputeSha256Hash(pnsIDs + Key_Encrypt);

            string postData = JsonConvert.SerializeObject(lstLockOrder);

            var client = new RestClient(Url);
            var request = new RestRequest("/BCCP/LockOrder");
            request.Method = Method.GET;
            request.AddParameter("signature", signature);
            request.AddJsonBody(postData);
            request.RequestFormat = DataFormat.Json;
            var result = client.Execute(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show(result.StatusCode.ToString());
            }
            else
            {
                MessageBox.Show(result.StatusCode.ToString() + ":" + result.Content);
            }
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;

            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //var client = new RestClient("https://test-pns.vnpost.vn//BCCP/LockOrder?signature=29f0170d0cf9b7ce9a66551aea1fd5c507b888d09b46d9998c9ff215b47b0d1f");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.GET);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("Cookie", "__Host-SRVNAME=T1");
            //request.AddQueryParameter("application/json", "[\"1296815\",\"1296838\",\"1296839\",\"1296840\",\"1296841\"]");
            //IRestResponse response = client.Execute(request);

            //MessageBox.Show("StatusCode: " + response.StatusCode.ToString());
        }

        private string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
