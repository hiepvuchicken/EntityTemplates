using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSerialize.BO
{
    public class RequestUpdateItems
    {
        public IList<ItemMobile> Items { get; set; }

        public string token { get; set; }
    }

    public class ItemMobile
    {
        public string itemCode { get; set; }
        public string senderName { get; set; }
        public string senderPhone { get; set; }
        public string senderAddress { get; set; }
        public string receiverName { get; set; }
        public string receiverPhone { get; set; }
        public string receiverAddress { get; set; }
        public string receiverRelation { get; set; }
        public string acceptancePOSName { get; set; }
        public string acceptancePOSCode { get; set; }
        public string acceptancePOSPhone { get; set; }
        public string deliveryRouteCode { get; set; }
        public int? groupByCODHCC { get; set; }
        public string serviceCode { get; set; }
        public decimal? CODAmount { get; set; }
        public int? totalFreight { get; set; }
        public string deliveryNote { get; set; }
        public string sendingContent { get; set; }
        public string sendingTime { get; set; }
        public string itemStatus { get; set; }
        public string realReceiverName { get; set; }
        public string realReceiverId { get; set; }
        public string causeCode { get; set; }
        public string causeName { get; set; }
        public string solutionCode { get; set; }
        public string solutionName { get; set; }
        public string deliveredNote { get; set; }
        public List<string> photos { get; set; }
        public string signature { get; set; }
        public int? rating { get; set; }
        public string deliveredId { get; set; }
        public string deliveredTime { get; set; }
        // DHL Info
        public string ItemCode { get; set; }
        public int? ItemId { get; set; }
        public bool IsSuccess { get; set; }
        public string PosCode { get; set; }
        public string ReceiveName { get; set; }
        public Nullable<int> ReasonId { get; set; }
        public Nullable<int> ResolveId { get; set; }
        public Nullable<byte> CertificateType { get; set; }
        public string CertificateCode { get; set; }
        public string Note { get; set; }
        public string Relationship { get; set; }
        public DateTime DateDelivery { get; set; }
    }
}
