using JsonSerialize.BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonSerialize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RequestItem requestItem = new RequestItem();
            IList<RequestItem> lsrequestItem = new List<RequestItem>();
            RequestUpdateItem requestUpdateItem = new RequestUpdateItem();

            RequestItem requestItem1 = new RequestItem()
            {
                dataCode = "11111111",
                customerCode = "71001G47001569010",
                itemCode = "EG997860512VN",
                senderName = "hiepvb",
                senderAddress = "HN",
                senderPhone = "",
                senderEmail = "",
                receiverName = "hungvt",
                receiverAddress = "HP",
                receiverPhone = "",
                receiverEmail = "",
                CODAmount = 100000,
                sendingContent = "test json",
                sendingTime = DateTime.Now,
                itemStatus = "success",
                causeCode = "",
                causeName = "",
                deliveredNote = "nội dung test thêm",
                syncingID = "AAAA",
                syncingTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            lsrequestItem.Add(requestItem1);

            RequestItem requestItem2 = new RequestItem()
            {
                dataCode = "11111112",
                customerCode = "71001G47001569010",
                itemCode = "EG997860513VN",
                senderName = "hiepvb",
                senderAddress = "HN",
                senderPhone = "",
                senderEmail = "",
                receiverName = "hungvt",
                receiverAddress = "HP",
                receiverPhone = "",
                receiverEmail = "",
                CODAmount = 100000,
                sendingContent = "test json",
                sendingTime = DateTime.Now,
                itemStatus = "fail",
                causeCode = "0",
                causeName = "Lý do khác - Kh hẹn mai đến lấy",
                deliveredNote = "Kh hẹn mai đến lấy",
                syncingID = "AAAA",
                syncingTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };
            lsrequestItem.Add(requestItem2);

            requestUpdateItem.Items = lsrequestItem;

            string s = Newtonsoft.Json.JsonConvert.SerializeObject(requestUpdateItem);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CollectUser collectUser1 = new CollectUser();
            CollectUser collectUser2 = new CollectUser();
            RequestSyncCollectUser requestSyncCollectUser = new RequestSyncCollectUser();
            List<CollectUser> listCollectUser = new List<CollectUser>();

            collectUser1.POSCode = "756080";
            collectUser1.PosmanCode = "9B7A";
            collectUser1.ShortName = "Hưng";
            collectUser1.FullName = "Tiến Hưng";
            collectUser1.Mobile = "0979262474";

            collectUser2.POSCode = "756080";
            collectUser2.PosmanCode = "9B7B";
            collectUser2.ShortName = "Hiệp";
            collectUser2.FullName = "Vũ Hiệp";
            collectUser2.Mobile = "0979262478";

            listCollectUser.Add(collectUser1);
            listCollectUser.Add(collectUser2);
            requestSyncCollectUser.collectUsers = listCollectUser;

            string s = Newtonsoft.Json.JsonConvert.SerializeObject(requestSyncCollectUser);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RequestCollectItem requestItem1 = new RequestCollectItem();
            RequestCollectItem requestItem2 = new RequestCollectItem();
            RequestAddCollectItem requestAddCollectItem = new RequestAddCollectItem();
            List<RequestCollectItem> listRequestCollectItem = new List<RequestCollectItem>();

            requestItem1.customerCode = "11001G47003807086";
            requestItem1.dataCode = "MPDS-3915195242-2265";
            requestItem1.itemCode = "CC992665371VN";
            requestItem1.senderAddress = "TP HCM";
            requestItem1.senderDesc = "Giao hàng giờ hành chính.";
            requestItem1.senderName = "Vũ Hiệp";
            requestItem1.senderTel = "0979262474";
            requestItem1.receiveDate = DateTime.Now;
            requestItem1.receiverAddress = "Hà Nội";
            requestItem1.receiverName = "Tiến Hưng";
            requestItem1.receiverTel = "0989092093";
            requestItem1.provinceCode = "70";
            requestItem1.POSCode = "756080";
            requestItem1.PosmanCode = "9B7A";
            requestItem1.COD = 1000000;
            

            requestItem2.customerCode = "71001G47001569008";
            requestItem2.dataCode = "0b319350-4329-49cf-9e6a-8451ba295dc8";
            requestItem2.itemCode = null;
            requestItem2.senderAddress = "TP HCM";
            requestItem2.senderDesc = "Giao hàng giờ hành chính.";
            requestItem2.senderName = "Tiến Hưng";
            requestItem2.senderTel = "0979262474";
            requestItem2.receiveDate = DateTime.Now;
            requestItem2.receiverAddress = "Hà Nội";
            requestItem2.receiverName = "Vũ Hiệp";
            requestItem2.receiverTel = "0989092093";
            requestItem2.provinceCode = "70";
            requestItem2.POSCode = "756080";
            requestItem2.PosmanCode = "9B7B";
            requestItem2.COD = 200000;

            listRequestCollectItem.Add(requestItem2);
            listRequestCollectItem.Add(requestItem1);

            requestAddCollectItem.Items = listRequestCollectItem;

            string s = Newtonsoft.Json.JsonConvert.SerializeObject(requestAddCollectItem);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                ex.ServiceClient client = new ex.ServiceClient();
                var s = client.GetMailTrip("600100", "100916", "3", "E", "20180906", "2247", "OE", "OE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SortingDetails sortingDetails = new SortingDetails();
            List<SortingDetails> sortings = new List<SortingDetails>();
            SortingList sortingList = new SortingList();

            sortingDetails.POSCode = "880100";
            sortingDetails.POSName = "KT An Giang";
            sortingDetails.POSFileWav = "880100.wav";
            sortingDetails.SortingCode = "90000/90016/90198/90199";

            sortings.Add(sortingDetails);

            sortingDetails.POSCode = "920100";
            sortingDetails.POSName = "KT Kiên Giang";
            sortingDetails.POSFileWav = "920100.wav";
            sortingDetails.SortingCode = "91000/91016/91152/91198/91199/91752";

            sortings.Add(sortingDetails);
            sortingList.sortingDetails = sortings;
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(sortings);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RequestUpdateItems requestUpdateItems = new RequestUpdateItems();
            ItemMobile itemMobile = new ItemMobile();
            List<ItemMobile> itemMobiles = new List<ItemMobile>();
            itemMobiles.Add(itemMobile);
            requestUpdateItems.Items = itemMobiles;
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(requestUpdateItems);

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            //ColumnMapping columnMapping = new ColumnMapping();
            List<TablesMapping> tablesMappings = new List<TablesMapping>();
            TablesMapping tablesMapping = new TablesMapping();
            List<string> ColumnInDB = new List<string>();

            for (int i = 0; i < 6; i++)
            {

                string gs ="PERSON_PERSON_NO";
                ColumnInDB.Add(gs);
            }

            tablesMapping.TableName = "SF_LMS_LEARNING_HISTORY";
            tablesMapping.FileName = "SF_LMS_Learning history";
            tablesMapping.ColumnInDB = ColumnInDB;
            tablesMappings.Add(tablesMapping);

            string s = Newtonsoft.Json.JsonConvert.SerializeObject(tablesMappings);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ConfigDetail configDetail = new ConfigDetail() { param="#a#",type="string",format=""};
            List<Config> configs = new List<Config>();
            Config config = new Config() { col = "1", configDetail = configDetail };
            configs.Add(config);
            config = new Config() { col = "2", configDetail = configDetail };
            configs.Add(config);
            string s = Newtonsoft.Json.JsonConvert.SerializeObject(configs);
        }
    }

    public class TablesMapping
    {
        public string TableName { get; set; }
        public string FileName { get; set; }
        public List<string> ColumnInDB { get; set; }
    }

    public class Config
    {
        public string col { get; set; }
        public ConfigDetail configDetail { get; set; }
    }

    public class ConfigDetail
    {
        public string param { get; set; }
        public string type { get; set; }
        public string format { get; set; }
    }
}
