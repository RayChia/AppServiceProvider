using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using NLog;
using AppServiceProvider.Models;
using GomypayDataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace AppServiceProvider.Service
{
    public class OrderService
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string CreditCardOrder(CreditCard BodyObj)
        {
            try
            {
                string RespBody = string.Empty;

                //DB
                //DbLink(Body);
                string connString = ConfigurationManager.AppSettings["DBConnectionStr"];
                string APOrder_ID = string.Empty;//交易唯一識別碼 APP訂單編號(AP+年月日+6碼流水號)
                int DBSave = 0;

                //取商家交易密碼
                //using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                //{

                //    var pass = entities.APP_Customer.FirstOrDefault(x => x.Customer_ID == BodyObj.CustomerId);
                //    if (pass != null)
                //    {
                //        BodyObj.Str_Check = pass.Customer_Password;
                //    }
                //    else
                //    {
                //        return JsonToQueryString(new ApiResult<object>("500","查無商家資訊"));
                //    }
                //}



                //取交易流水編號
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT NEXT VALUE FOR dbo.APOrder_ID", connection);
                    
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        APOrder_ID = reader[0].ToString();
                    }
                    connection.Close();
                }
                APOrder_ID = "AP" + DateTime.Now.ToString("yyyyMMdd") + APOrder_ID.PadLeft(6, '0');
                logger.Debug("APOrder_ID : " + APOrder_ID);
                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {
                    //System.Data.Entity.Infrastructure.DbRawSqlQuery<string> rawQuery = entities.Database.SqlQuery<string>("SELECT NEXT VALUE FOR dbo.APOrder_ID");
                    //var results = entities.GetNextAppOrderID();
                    //int? nextSequenceValue = results;
                    //logger.Debug("GetNextAppOrderID : " + nextSequenceValue); 

                    APP_Order NewAppOrder = new APP_Order()
                    {
                        APOrder_ID = APOrder_ID,
                        Order_ID = "",
                        User_System_ID = "",
                        Customer_ID = BodyObj.CustomerId,
                        Customer_Order_ID = "",
                        Promotion_ID = "",
                        Use_Bonus = 0,
                        Return_Bonus = 0,
                        IsInstallment = "N",
                        Installment_Period = 0,//int.Parse(BodyObj.Installment),
                        Amount = int.Parse(BodyObj.Amount),
                        Pay_Amount = 0,//-Use_Bonus
                        Pay_Result = "2",//ResultUpdate
                        Approve_Code = "",
                        InvoiceNumber = "",
                        Goods_Return_Statu = "",
                        Build_Date = DateTime.Now
                    };
                    entities.APP_Order.Add(NewAppOrder);
                    DBSave = entities.SaveChanges();
                    
                    //return entities.APP_Order.ToList();
                }

                //DB新增 OK                
                if (DBSave>0)
                {
                    //送API 物件轉QueryString
                    RespBody = SendOrder(JsonToQueryString(BodyObj));
                    //RespBody = JsonToQueryString(BodyObj);
                }
                else
                {
                    //DBSaveError 缺ERROR回覆 return
                    //RespBody
                }
                CreditCardResult RespObj = new CreditCardResult();
                RespObj = JsonConvert.DeserializeObject<CreditCardResult>(RespBody);
                logger.Debug("RespObj result : " + RespObj.result);
                logger.Debug("RespObj e_date : " + RespObj.e_date);
                logger.Debug("RespObj OrderID : " + RespObj.OrderID);
                logger.Debug("RespObj str_check : " + RespObj.result);
                logger.Debug("RespObj bankname : " + RespObj.bankname);

                if (RespObj.result == "1")//授權成功
                {
                    //update Response where APOrder_ID
                    using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                    {
                        APP_Order UpdateAppOrder = entities.APP_Order.First(c => c.APOrder_ID == APOrder_ID);
                        UpdateAppOrder.IsInstallment = "Y";
                        UpdateAppOrder.Pay_Amount = int.Parse(RespObj.e_money);
                        UpdateAppOrder.Order_ID = RespObj.OrderID;
                        UpdateAppOrder.Pay_Result = RespObj.result;
                        UpdateAppOrder.Approve_Code = RespObj.avcode;
                        UpdateAppOrder.Customer_ID = RespObj.e_no;

                        DBSave = entities.SaveChanges();
                    }
                    if (DBSave == 0)
                    {//更新失敗 
                        logger.Debug("CreditCardOrder : " + "CreditCardOrder  Update fail !!!");
                    }
                }
                else //授權失敗
                { 
                }

                return RespBody;
            }
            catch (Exception ex)
            {
                logger.Error("Exception : " + ex.StackTrace);
            }



            return "";
        }
        public string JsonToQueryString(Object ObjBody)
        {
            var jObj = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(ObjBody));
            //JSON 轉 CURL
            var query = String.Join("&",
                            jObj.Children().Cast<JProperty>()
                            .Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));
            return query;
        }
        public string SendOrder(string QueryString)
        {
            try
            {
                string targetUrl = ConfigurationManager.AppSettings["GoMyPayUrl"];
                //string parame = "CardNo=4907060600015101&ExpireDate=2012&CVV=615&TransMode=1&Installment=0&e_return=1&Str_Check=lm378l3a72lx5sxzg54frkua5e4uathb&Send_Type=0&Pay_Mode_No=2&CustomerId=01478523&Order_No=&Amount=35&TransCode=00&Buyer_Name=BigDish&Buyer_Telm=0987654321&Buyer_Mail=hahaha0417@hotmail.com&Buyer_Memo=moMo&Return_url=&Callback_Url=";
                byte[] postData = Encoding.UTF8.GetBytes(QueryString);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 30000;
                //request.ContentLength = postData.Length;
                // 寫入 Post Body Message 資料流
                using (Stream st = request.GetRequestStream())
                {
                    st.Write(postData, 0, postData.Length);
                }

                string result = "";
                // 取得回應資料
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
                logger.Debug("取得GMP回應資料 : " + result);
                //System.Diagnostics.Debug.WriteLine("取得回應資料 : " + result);

                return result;
            }
            catch (Exception ex)
            {
                logger.Debug("Exception : " + ex.StackTrace);
            }

            return "";
        }


        public string DbLink(string Body)
        {
            try
            {
                string connString = ConfigurationManager.AppSettings["DBConnectionStr"];
                string parame = Body;
                SqlConnection nwindConn = new SqlConnection(connString);
                nwindConn.Open();



                SqlTransaction nwindTxn = nwindConn.BeginTransaction();






            }
            catch
            {
            }

            return "";
        }

    }
}