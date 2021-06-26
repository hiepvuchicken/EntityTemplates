using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSerialize.BO
{
    public class RequestCollectItem
    {
        public string provinceCode { get; set; }
        public string POSCode { get; set; }
        public string PosmanCode { get; set; }
        public string dataCode { get; set; }
        public string itemCode { get; set; }
        public string customerCode { get; set; }
        public string senderAddress { get; set; }
        public string senderTel { get; set; }
        public string senderName { get; set; }
        public string senderDesc { get; set; }
        public string receiverAddress { get; set; }
        public string receiverTel { get; set; }
        public string receiverName { get; set; }
        public Nullable<decimal> COD { get; set; }
        public DateTime receiveDate { get; set; }

    }

    public class RequestAddCollectItem
    {
        public string token { get; set; }
        public List<RequestCollectItem> Items { get; set; }
    }

    public class ResponseAddCollectItem
    {
        public bool status { get; set; }
        public int totalItems { get; set; }
        public int totalSuccess { get; set; }
        public int totalFails { get; set; }
        public List<ResponseCollectItem> successItems { get; set; }
        public List<ResponseCollectItem> failItems { get; set; }
        public string errorContent { get; set; }
    }

    public class ResponseCollectItem
    {
        public string dataCode { get; set; }
        public string customerCode { get; set; }
        public string errorCode { get; set; }
        public string errorContent { get; set; }
    }
}
