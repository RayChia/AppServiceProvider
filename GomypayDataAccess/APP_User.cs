//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class APP_User
    {
        public int ID { get; set; }
        public string System_ID { get; set; }
        public string User_Account { get; set; }
        public string User_Password { get; set; }
        public string User_Name { get; set; }
        public string Identifier { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string RN_verified { get; set; }
        public string IsDuplicate { get; set; }
        public string Address_City { get; set; }
        public string Address_Zone { get; set; }
        public string Address_ZIP { get; set; }
        public string Address { get; set; }
        public Nullable<long> Bonus_Point { get; set; }
        public string Invoice_Carrier { get; set; }
        public string Referrer_Type { get; set; }
        public string Referrer_ID { get; set; }
        public Nullable<System.DateTime> Build_Date { get; set; }
        public string Builder { get; set; }
        public Nullable<System.DateTime> Modify_Date { get; set; }
        public string Modifier { get; set; }
    }
}
