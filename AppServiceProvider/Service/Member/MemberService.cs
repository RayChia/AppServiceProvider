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
using System.Runtime.Caching;
using AppServiceProvider.Models;

namespace AppServiceProvider.Service
{
    public class MemberService
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static MemoryCache _cacheOTP = MemoryCache.Default;// ID , OTP
        private static MemoryCache _cacheQS = MemoryCache.Default;// Phone , Query String
        public string SendSMS(string QueryString)
        {
            try
            {                   
                string targetUrl = ConfigurationManager.AppSettings["SMSUrl"];
                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    string result = webClient.DownloadString(targetUrl + "?" + QueryString);
                    _logger.Debug("簡訊回應資料 : " + result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception StackTrace: " + ex.Message);
                _logger.Error("Exception StackTrace: " + ex.StackTrace);
                throw ex;
            }

        }

        /// <summary>
        /// 新增/更新 OTP cache data
        /// </summary>
        /// <param name="key">caceh key</param>
        /// <returns>儲存快取OTP  + 回傳OTP</returns>
        public string GetCacheData(string key)
        {
            try
            {
                // lock 以 key 產生的專屬 lock object，如果 object 過期會自動 new 出新的
                // 如果直接 lock cache[key] 會造成無法寫入 cache 資料
                //GetNewCache();
                // 取得 memorycache 是否已有 key 的 cahce 資料
                //string cacheData = _cache[key] as string;
                string otp = GetNewCache(4);
                //OTP Catch 保留10分鐘
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 10, 0) };
                //更新 cache
                _cacheOTP.Set(key, otp, cacheItemPolicy);
                _logger.Debug("GetCacheData otp : " + otp);
                return otp;
            }
            catch (Exception e)
            {
                _logger.Error("Exception : " + e.Message.ToString());
                throw e;
            }
        }

        /// <summary>
        /// 驗證OTP
        /// </summary>
        /// <param name="key"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        public bool CheckCacheData(string key, string otp)
        {
            try
            {
                string cacheData = _cacheOTP[key] as string;
                if (!string.IsNullOrEmpty(cacheData) && cacheData == otp)
                {
                    _cacheOTP.Remove(key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Exception : " + e.Message.ToString());
                throw e;
            }
        }

        /// <summary>
        /// 取得新OTP
        /// </summary>
        /// <param name="num"></param>
        /// <returns>產生num長度 隨機亂數</returns>
        public static string GetNewCache(int num)
        {
            Random rnd = new Random();
            string otp = string.Empty;
            for (int i = 0; i < num; i++)
            {
                otp = otp + rnd.Next(0, 9).ToString();
            }
            return otp;
        }
        public string GetSMSString(AddUser body)
        {
            string SMSUrl = ConfigurationManager.AppSettings["SMSUrl"];
            string yhy = ConfigurationManager.AppSettings["SMSID"];
            string dc2a = ConfigurationManager.AppSettings["SMSPW"];
            string SMSBody = string.Empty;
            try
            {
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 20, 0) };
                SMSBody =
                    $"yhy={yhy}&dc2a={dc2a}&movetel={body.phone}&name={body.name}" +
                    $"&bf={DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}" +
                    $"&sb=您的4位驗證碼是[{GetCacheData(body.Id)}]  GoMyPay手機APP，使用者註冊系統。";
                _cacheQS.Set(body.phone, body, cacheItemPolicy);
                // lock 以 key 產生的專屬 lock object，如果 object 過期會自動 new 出新的
                // 如果直接 lock cache[key] 會造成無法寫入 cache 資料
                //GetNewCache();
                // 取得 memorycache 是否已有 key 的 cahce 資料
                //string cacheData = _cache[key] as string;
                string querystring = string.Empty;

                _logger.Debug("GetSMSString otp : " + SMSBody);
                return SMSBody;
            }
            catch (Exception e)
            {
                _logger.Error("Exception : " + e.Message.ToString());
                throw e;
            }
        }
        public string ReGetSMSString(string phone)
        {
            string SMSUrl = ConfigurationManager.AppSettings["SMSUrl"];
            string yhy = ConfigurationManager.AppSettings["SMSID"];
            string dc2a = ConfigurationManager.AppSettings["SMSPW"];
            string SMSBody = string.Empty;
            AddUser body = _cacheQS[phone] as AddUser;
            try
            {
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 20, 0) };
                SMSBody =
                    $"yhy={yhy}&dc2a={dc2a}&movetel={body.phone}&name={body.name}" +
                    $"&bf={DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}" +
                    $"&sb=您的4位驗證碼是[{GetCacheData(body.Id)}]  GoMyPay手機APP，使用者註冊系統。";
                _cacheQS.Set(phone, body, cacheItemPolicy);

                _logger.Debug("==ReGetSMSString== otp : " + SMSBody);
                return SMSBody;
            }
            catch (Exception e)
            {
                _logger.Error("Exception : " + e.Message.ToString());
                throw e;
            }
        }


        //System_ID Sequence  未使用
        public int GetNextSystemID()
        {
            string connString = ConfigurationManager.AppSettings["DBConnectionStr"];
            string APOrder_ID = string.Empty;//交易唯一識別碼 APP訂單編號(AP+年月日+6碼流水號)
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT NEXT VALUE FOR dbo.System_ID", connection);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    APOrder_ID = reader[0].ToString();
                }
                connection.Close();
            }
            return 0;
        }

    }
}