﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace GomypayDataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Gomypay_AppEntities : DbContext
    {
        public Gomypay_AppEntities()
            : base("name=Gomypay_AppEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<APP_Activity> APP_Activity { get; set; }
        public virtual DbSet<APP_Category_Big> APP_Category_Big { get; set; }
        public virtual DbSet<APP_Category_Small> APP_Category_Small { get; set; }
        public virtual DbSet<APP_Customer> APP_Customer { get; set; }
        public virtual DbSet<APP_Grade> APP_Grade { get; set; }
        public virtual DbSet<APP_Mission> APP_Mission { get; set; }
        public virtual DbSet<APP_Mission_Item> APP_Mission_Item { get; set; }
        public virtual DbSet<APP_Misson_Mode> APP_Misson_Mode { get; set; }
        public virtual DbSet<APP_News> APP_News { get; set; }
        public virtual DbSet<APP_Notice> APP_Notice { get; set; }
        public virtual DbSet<APP_Order> APP_Order { get; set; }
        public virtual DbSet<APP_Product> APP_Product { get; set; }
        public virtual DbSet<APP_Product_Big> APP_Product_Big { get; set; }
        public virtual DbSet<APP_Product_Small> APP_Product_Small { get; set; }
        public virtual DbSet<APP_Promotion> APP_Promotion { get; set; }
        public virtual DbSet<APP_SlideShow> APP_SlideShow { get; set; }
        public virtual DbSet<APP_SurPrise_Item> APP_SurPrise_Item { get; set; }
        public virtual DbSet<APP_SurpriseBox> APP_SurpriseBox { get; set; }
        public virtual DbSet<APP_TCash> APP_TCash { get; set; }
        public virtual DbSet<APP_User> APP_User { get; set; }
    }
}