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
    
    public partial class DieuTinDbEntities : DbContext
    {
        public DieuTinDbEntities()
            : base("name=DieuTinDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CollectItem> CollectItems { get; set; }
        public virtual DbSet<ExploitUser> ExploitUsers { get; set; }
        public virtual DbSet<User> Users { get; set; }
    
        public virtual ObjectResult<SMP_GetCollectItem_Result> SMP_GetCollectItem(string postmanCode, string posCode, Nullable<short> days)
        {
            var postmanCodeParameter = postmanCode != null ?
                new ObjectParameter("PostmanCode", postmanCode) :
                new ObjectParameter("PostmanCode", typeof(string));
    
            var posCodeParameter = posCode != null ?
                new ObjectParameter("PosCode", posCode) :
                new ObjectParameter("PosCode", typeof(string));
    
            var daysParameter = days.HasValue ?
                new ObjectParameter("Days", days) :
                new ObjectParameter("Days", typeof(short));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SMP_GetCollectItem_Result>("SMP_GetCollectItem", postmanCodeParameter, posCodeParameter, daysParameter);
        }
    
        public virtual ObjectResult<SyncPost_GetProvince_Result> SyncPost_GetProvince(string postId, string keyword)
        {
            var postIdParameter = postId != null ?
                new ObjectParameter("PostId", postId) :
                new ObjectParameter("PostId", typeof(string));
    
            var keywordParameter = keyword != null ?
                new ObjectParameter("Keyword", keyword) :
                new ObjectParameter("Keyword", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SyncPost_GetProvince_Result>("SyncPost_GetProvince", postIdParameter, keywordParameter);
        }
    }
}