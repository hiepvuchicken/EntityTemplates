//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class CollectItem
    {
        public long Id { get; set; }
        public string ProvinceCode { get; set; }
        public string POSCode { get; set; }
        public string PostmanCode { get; set; }
        public string PostmanCodeName { get; set; }
        public string Datacode { get; set; }
        public string ItemCode { get; set; }
        public string CustomerCode { get; set; }
        public string SenderAddress { get; set; }
        public string SenderTel { get; set; }
        public string SenderName { get; set; }
        public string SenderDesc { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverTel { get; set; }
        public string ReceiverName { get; set; }
        public Nullable<decimal> COD { get; set; }
        public Nullable<decimal> CODofSender { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public Nullable<System.DateTime> ReceiveDate { get; set; }
        public Nullable<System.DateTime> CollectDate { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string Year { get; set; }
        public Nullable<long> DieutinId { get; set; }
    }
}
