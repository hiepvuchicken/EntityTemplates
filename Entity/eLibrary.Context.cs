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
    
    public partial class eLibraryEntities : DbContext
    {
        public eLibraryEntities()
            : base("name=eLibraryEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ALLCODE> ALLCODES { get; set; }
        public virtual DbSet<AUTHOR> AUTHORS { get; set; }
        public virtual DbSet<BOOK> BOOKS { get; set; }
        public virtual DbSet<BOOKTYPy> BOOKTYPIES { get; set; }
        public virtual DbSet<FIELD> FIELDS { get; set; }
        public virtual DbSet<LIBRARYCARD> LIBRARYCARDS { get; set; }
        public virtual DbSet<MENU> MENUS { get; set; }
        public virtual DbSet<PUBLISHER> PUBLISHERS { get; set; }
        public virtual DbSet<SLIDE> SLIDES { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<TLPROFILE> TLPROFILES { get; set; }
        public virtual DbSet<VISITOR_STATISTICS> VISITOR_STATISTICS { get; set; }
        public virtual DbSet<MENUGROUP> MENUGROUPS { get; set; }
    }
}
