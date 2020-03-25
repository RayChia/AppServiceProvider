using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppServiceProvider.Models.AppOrder
{
    public class CreditCard
    {
        public class CreditCardOrder
        {
            public string Send_Type { get; set; }
            public string Pay_Mode_No { get; set; }
            public string CustomerId { get; set; }
            public string Order_No { get; set; }
            public string Amount { get; set; }
            public string TransCode { get; set; }
            public string Buyer_Name { get; set; }
            public string Buyer_Telm { get; set; }
            public string Buyer_Mail { get; set; }
            public string Buyer_Memo { get; set; }
            public string CardNo { get; set; }
            public string ExpireDate { get; set; }
            public string CVV { get; set; }
            public string TransMode { get; set; }
            public string Installment { get; set; }

        }
        public class CreditCardResult
        {
            public string result { get; set; }
            public string ret_msg { get; set; }
            public string OrderID { get; set; }
            public string e_Cur { get; set; }
            public string e_money { get; set; }
            public string e_date { get; set; }
            public string e_time { get; set; }
            public string e_orderno { get; set; }
            public string e_no { get; set; }
            public string e_outlay { get; set; }
            public string str_check { get; set; }
            public string bankname { get; set; }
            public string avcode { get; set; }
        }
    }
}