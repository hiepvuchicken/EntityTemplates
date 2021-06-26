﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class DBBAOPHATEntities : DbContext
    {
        public DBBAOPHATEntities()
            : base("name=DBBAOPHATEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
    
        public virtual int service_process_LoadItemDetails_printf(string postmanCode, string startingCode, string frDate, string toDate, Nullable<int> trangThai)
        {
            var postmanCodeParameter = postmanCode != null ?
                new ObjectParameter("PostmanCode", postmanCode) :
                new ObjectParameter("PostmanCode", typeof(string));
    
            var startingCodeParameter = startingCode != null ?
                new ObjectParameter("StartingCode", startingCode) :
                new ObjectParameter("StartingCode", typeof(string));
    
            var frDateParameter = frDate != null ?
                new ObjectParameter("FrDate", frDate) :
                new ObjectParameter("FrDate", typeof(string));
    
            var toDateParameter = toDate != null ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(string));
    
            var trangThaiParameter = trangThai.HasValue ?
                new ObjectParameter("TrangThai", trangThai) :
                new ObjectParameter("TrangThai", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("service_process_LoadItemDetails_printf", postmanCodeParameter, startingCodeParameter, frDateParameter, toDateParameter, trangThaiParameter);
        }
    
        public virtual ObjectResult<service_process_LoadItemDetails_printf_v3_Result> service_process_LoadItemDetails_printf_v3(string postmanCode, string startingCode, string frDate, string toDate, Nullable<int> trangThai)
        {
            var postmanCodeParameter = postmanCode != null ?
                new ObjectParameter("PostmanCode", postmanCode) :
                new ObjectParameter("PostmanCode", typeof(string));
    
            var startingCodeParameter = startingCode != null ?
                new ObjectParameter("StartingCode", startingCode) :
                new ObjectParameter("StartingCode", typeof(string));
    
            var frDateParameter = frDate != null ?
                new ObjectParameter("FrDate", frDate) :
                new ObjectParameter("FrDate", typeof(string));
    
            var toDateParameter = toDate != null ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(string));
    
            var trangThaiParameter = trangThai.HasValue ?
                new ObjectParameter("TrangThai", trangThai) :
                new ObjectParameter("TrangThai", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<service_process_LoadItemDetails_printf_v3_Result>("service_process_LoadItemDetails_printf_v3", postmanCodeParameter, startingCodeParameter, frDateParameter, toDateParameter, trangThaiParameter);
        }
    
        public virtual ObjectResult<service_process_LoadItemForPosman_printf_v2_dev_Result> service_process_LoadItemForPosman_printf_v2_dev(string postmanCode, string itemcodes)
        {
            var postmanCodeParameter = postmanCode != null ?
                new ObjectParameter("PostmanCode", postmanCode) :
                new ObjectParameter("PostmanCode", typeof(string));
    
            var itemcodesParameter = itemcodes != null ?
                new ObjectParameter("itemcodes", itemcodes) :
                new ObjectParameter("itemcodes", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<service_process_LoadItemForPosman_printf_v2_dev_Result>("service_process_LoadItemForPosman_printf_v2_dev", postmanCodeParameter, itemcodesParameter);
        }
    
        public virtual ObjectResult<service_process_LoadItemList_printf_Result> service_process_LoadItemList_printf(string postmanCode, string startingCode, string date, string sheetNumber)
        {
            var postmanCodeParameter = postmanCode != null ?
                new ObjectParameter("PostmanCode", postmanCode) :
                new ObjectParameter("PostmanCode", typeof(string));
    
            var startingCodeParameter = startingCode != null ?
                new ObjectParameter("StartingCode", startingCode) :
                new ObjectParameter("StartingCode", typeof(string));
    
            var dateParameter = date != null ?
                new ObjectParameter("date", date) :
                new ObjectParameter("date", typeof(string));
    
            var sheetNumberParameter = sheetNumber != null ?
                new ObjectParameter("SheetNumber", sheetNumber) :
                new ObjectParameter("SheetNumber", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<service_process_LoadItemList_printf_Result>("service_process_LoadItemList_printf", postmanCodeParameter, startingCodeParameter, dateParameter, sheetNumberParameter);
        }
    
        public virtual ObjectResult<service_process_LoadItemList_printf_dev_Result> service_process_LoadItemList_printf_dev(string postmanCode, string startingCode, string date, string sheetNumber)
        {
            var postmanCodeParameter = postmanCode != null ?
                new ObjectParameter("PostmanCode", postmanCode) :
                new ObjectParameter("PostmanCode", typeof(string));
    
            var startingCodeParameter = startingCode != null ?
                new ObjectParameter("StartingCode", startingCode) :
                new ObjectParameter("StartingCode", typeof(string));
    
            var dateParameter = date != null ?
                new ObjectParameter("date", date) :
                new ObjectParameter("date", typeof(string));
    
            var sheetNumberParameter = sheetNumber != null ?
                new ObjectParameter("SheetNumber", sheetNumber) :
                new ObjectParameter("SheetNumber", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<service_process_LoadItemList_printf_dev_Result>("service_process_LoadItemList_printf_dev", postmanCodeParameter, startingCodeParameter, dateParameter, sheetNumberParameter);
        }
    }
}
