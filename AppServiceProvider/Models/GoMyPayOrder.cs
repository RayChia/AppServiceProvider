using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppServiceProvider.Models
{
    public class GoMyPayOrder
    {
        public string Send_Type { get; set; } = "";
        public string Pay_Mode_No { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string Order_No { get; set; } = "";
        public string Amount { get; set; } = "";
        public string TransCode { get; set; } = "";
        public string Buyer_Name { get; set; } = "";
        public string Buyer_Telm { get; set; } = "";
        public string Buyer_Mail { get; set; } = "";
        public string Buyer_Memo { get; set; } = "";
        public string Return_url { get; set; } = "";
        public string Callback_Url { get; set; } = "";
    }
    public class CreditCard: GoMyPayOrder
    {
        public string CardNo { get; set; } = "";
        public string ExpireDate { get; set; } = "";
        public string CVV { get; set; } = "";
        public string TransMode { get; set; } = "";
        public string Installment { get; set; } = "";
        public string e_return { get; set; } = "";
        public string Str_Check { get; set; } = "";
    }
    public class CreditCardResult
    {
        public string result { get; set; } = "";
        public string ret_msg { get; set; } = "";
        public string OrderID { get; set; } = "";
        public string e_Cur { get; set; } = "";
        public string e_money { get; set; } = "";
        public string e_date { get; set; } = "";
        public string e_time { get; set; } = "";
        public string e_orderno { get; set; } = "";
        public string e_no { get; set; } = "";
        public string e_outlay { get; set; } = "";
        public string str_check { get; set; } = "";
        public string bankname { get; set; } = "";
        public string avcode { get; set; } = "";
        public string Invoice_No { get; set; } = "";
    }
    /*public class CreditCard_DB
    {
        public string APOrder_ID { get; set; }
        public string Order_ID { get; set; }
        public string User_System_ID { get; set; }
        public string Customer_ID { get; set; }
        public string Customer_Order_ID { get; set; }
        public string Promotion_ID { get; set; }
        public string Use_Bonus { get; set; }
        public string Return_Bonus { get; set; }
        public string IsInstallment { get; set; }
        public string Installment_Period { get; set; }
        public string Amount { get; set; }
        public string Pay_Amount { get; set; }
        public string Pay_Result { get; set; }
        public string Approve_Code { get; set; }
        public string InvoiceNumber { get; set; }
        public string Goods_Return_Statu { get; set; }
        public string Build_Date { get; set; }
    }*/
}