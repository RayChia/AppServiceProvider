using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppServiceProvider.Models
{
    public class GoMyPayOrder
    {
        /// <summary>
        /// 傳送型態請填(0.信用卡 1.銀聯卡 2.超商條碼 3.WebAtm 4.虛擬帳號 5.定期扣款 6.超商代碼7.LinePay)
        /// </summary>
        public string Send_Type { get; set; } = "";
        /// <summary>
        /// 付款模式請填2
        /// </summary>
        public string Pay_Mode_No { get; set; } = "";
        /// <summary>
        /// 商店代號(統編/身份證/系統商店代號20碼)
        /// </summary>
        public string CustomerId { get; set; } = "";
        /// <summary>
        /// 交易單號，如無則自動帶入系統預設交易單號 若使用系統預設交易畫面，交易單號不可為無
        /// </summary>
        public string Order_No { get; set; } = "";
        /// <summary>
        /// 交易金額
        /// </summary>
        [Required(ErrorMessage = "信用卡號為必填")]
        public string Amount { get; set; } = "";
        /// <summary>
        /// 交易類別請填00(授權)
        /// </summary>
        public string TransCode { get; set; } = "";
        /// <summary>
        /// 消費者姓名，如無將自動轉入系統預設付款頁面
        /// </summary>
        public string Buyer_Name { get; set; } = "";
        /// <summary>
        /// 消費者手機(數字，不可全形) ，如無將自動轉入系統預設付款頁面
        /// </summary>
        public string Buyer_Telm { get; set; } = "";
        /// <summary>
        /// 消費者Email(不可全形) ，如無將自動轉入系統預設付款頁面
        /// </summary>
        [EmailAddress]
        public string Buyer_Mail { get; set; } = "";
        /// <summary>
        /// 消費備註(交易內容) ，如無將自動轉入系統預設付款頁面
        /// </summary>
        public string Buyer_Memo { get; set; } = "";
        /// <summary>
        /// 授權結果回傳網址：如無則自動轉入系統預設授權頁面
        /// 註:如果要用JSON回傳請勿帶此參數
        /// </summary>
        public string Return_url { get; set; } = "";
        /// <summary>
        /// 背景對帳網址，如未填寫默認不進行背景對帳
        /// </summary>
        public string Callback_Url { get; set; } = "";
    }
    public class CreditCard: GoMyPayOrder
    {
        /// <summary>
        /// 信用卡號，如無將自動轉入系統預設付款頁面
        /// </summary>
        [Required(ErrorMessage = "信用卡號為必填")]
        public string CardNo { get; set; } = "";
        /// <summary>
        /// 卡片有效日期(YYMM) ，如無將自動轉入系統預設付款頁面
        /// </summary>
        [Required(ErrorMessage = "信用卡有效日期為必填")]
        public string ExpireDate { get; set; } = "";
        /// <summary>
        /// 卡片認證碼，如無將自動轉入系統預設付款頁面
        /// </summary>
        [Required(ErrorMessage = "信用卡認證碼為必填")]
        public string CVV { get; set; } = "";
        /// <summary>
        /// 交易模式一般請填(1)、分期請填(2)
        /// </summary>
        public string TransMode { get; set; } = "";
        /// <summary>
        /// 期數，無期數請填0
        /// </summary>
        public string Installment { get; set; } = "";
        /// <summary>
        /// 使用json回傳是否交易成功(限用非3D驗證) ，請填1
        /// 註:如果要用預設交易頁面請勿帶此參數
        /// </summary>
        public string e_return { get; set; } = "";
        /// <summary>
        /// 交易驗證密碼，如果檢查不符合無法交易(使用Json回傳才為必填欄位)
        /// </summary>
        public string Str_Check { get; set; } = "";
        /// <summary>
        /// 紅利扣點
        /// </summary>
        public string Bonus { get; set; } = "";
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