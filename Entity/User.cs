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
    
    public partial class User
    {
        public long UserId { get; set; }
        public Nullable<int> TypeUser { get; set; }
        public Nullable<long> ParentId { get; set; }
        public Nullable<int> RoleId { get; set; }
        public string PostId { get; set; }
        public Nullable<int> LinkId { get; set; }
        public string PostParent { get; set; }
        public string PostRoot { get; set; }
        public string PayPost { get; set; }
        public string CustomerCode { get; set; }
        public string CRMCode { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string TaxCode { get; set; }
        public string Note { get; set; }
        public string ImageSrc { get; set; }
        public Nullable<System.Guid> ForgotCode { get; set; }
        public Nullable<System.DateTime> ForgotExpired { get; set; }
        public Nullable<System.Guid> Ticket { get; set; }
        public Nullable<bool> IsActived { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}