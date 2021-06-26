using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSerialize.BO
{
    public class RequestItem
    {
        public string dataCode { get; set; }
        public string customerCode { get; set; }
        public string itemCode { get; set; }
        public string senderName { get; set; }
        public string senderPhone { get; set; }
        public string senderAddress { get; set; }
        public string senderEmail { get; set; }
        public string receiverName { get; set; }
        public string receiverAddress { get; set; }
        public string receiverPhone { get; set; }
        public string receiverEmail { get; set; }
        public Nullable<decimal> CODAmount { get; set; }
        public decimal totalFreight { get; set; }
        public string sendingContent { get; set; }
        public Nullable<System.DateTime> sendingTime { get; set; }
        public string itemStatus { get; set; }
        public string causeCode { get; set; }
        public string causeName { get; set; }
        public string deliveredNote { get; set; }
        public string syncingID { get; set; }
        public string syncingTime { get; set; }
    }
    public class RequestUpdateItem
    {
        public IList<RequestItem> Items { get; set; }
    }
}
