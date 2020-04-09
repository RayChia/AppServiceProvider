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
using System.Net.Mail;
using GomypayDataAccess;
using MailKit.Security;
using MimeKit;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AppServiceProvider.Service
{
    public class MemberService
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static MemoryCache _cacheOTP = MemoryCache.Default;// ID , OTP
        private static MemoryCache _cacheQS = MemoryCache.Default;// Phone , Query String
        private static MemoryCache _cacheFogetPW = MemoryCache.Default;// email , OTP

        private string MailFrom = ConfigurationManager.AppSettings["MailFrom"];
        private string MailAddress = ConfigurationManager.AppSettings["MailAccount"];
        private string MailPwd = ConfigurationManager.AppSettings["MailPwd"];
        private string SMTPServer = ConfigurationManager.AppSettings["smtpServer"];
        private string SMTPPort = ConfigurationManager.AppSettings["smtpPort"];
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
                
                SMSBody =
                    $"yhy={yhy}&dc2a={dc2a}&movetel={body.phone}&name={body.name}" +
                    $"&bf={DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}" +
                    $"&sb=您的4位驗證碼是[{GetCacheData(body.Id)}]  GoMyPay手機APP，使用者註冊系統。";
                
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 20, 0) };
                _cacheQS.Set(body.phone, body, cacheItemPolicy);//body 存起來 重發簡訊使用
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

        /// <summary>
        /// 寄送忘記密碼Email
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public bool SendEmail(string mail)
        {
            /*
            //string[] MailTo = null;
            string otp = string.Empty;
            try
            {
                otp = GetNewCache(6);
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 30, 0) };
                //更新 cache
                _cacheFogetPW.Set(mail, otp, cacheItemPolicy);

                MailMessage msg = new MailMessage();
                msg.To.Add(mail);
                msg.From = new MailAddress(MailAddress, "GoMyPa", System.Text.Encoding.UTF8);
                msg.Subject = "GoMyPay 手機APP 忘記密碼認證碼";
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                string Body = "GoMyPay 手機APP 忘記密碼認證碼\n" + otp + "\n請在30分鐘內輸入認證碼";
                msg.Body = Body;
                msg.IsBodyHtml = false;
                msg.Priority = MailPriority.High;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                //msg.IsBodyHtml = true;
                //using (SmtpClient client = new SmtpClient(SMTPServer, int.Parse(SMTPPort)))
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 465))
                {
                    client.Credentials = new NetworkCredential("tm80013554@gmail.com", "0930517121");
                    //client.Credentials = new NetworkCredential(MailAddress, MailPassword); //這裡要填正確的帳號跟密碼
                    //client.Host = MailServer; //設定smtp Server
                    //client.Port = int.Parse(MailPort); //設定Port
                    client.EnableSsl = true; //gmail預設開啟驗證
                    client.Send(msg); //寄出信件
                    client.Dispose();
                    msg.Dispose();
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("==Email 發送== : " + e.Message.ToString());
                _logger.Error("==Email 發送== : " + e.StackTrace.ToString());
                return false;
            }*/


            string otp = string.Empty;
            try
            {
                otp = GetNewCache(6);
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 30, 0) };
                //更新 cache
                _cacheFogetPW.Set(mail, otp, cacheItemPolicy);

                MailMessage msg = new MailMessage();
                msg.To.Add(mail);
                msg.From = new MailAddress(MailFrom, "GoMyPa", System.Text.Encoding.UTF8);
                msg.Subject = "GoMyPay 手機APP 忘記密碼認證碼";
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                string Body = "GoMyPay 手機APP 忘記密碼認證碼\n" + otp + "\n請在30分鐘內輸入認證碼";
                msg.Body = Body;
                msg.IsBodyHtml = false;
                msg.Priority = MailPriority.High;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                //msg.IsBodyHtml = true;
                //using (SmtpClient client = new SmtpClient(SMTPServer, int.Parse(SMTPPort)))
                var mime = (MimeMessage)msg;
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                    //client.Connect(SMTP_Server, SMTP_Port, SecureSocketOptions.SslOnConnect);
                    client.Connect(SMTPServer, int.Parse(SMTPPort), SecureSocketOptions.StartTls);
                    client.Authenticate(MailAddress, MailPwd);
                    client.Timeout = 10000;
                    client.Send(mime);
                    client.Disconnect(true);
                    client.Dispose();
                }
                /*
                using (SmtpClient client = new SmtpClient(SMTPServer, int.Parse(SMTPPort)))
                {
                    client.Credentials = new NetworkCredential(MailAddress, MailPwd);
                    //client.Credentials = new NetworkCredential(MailAddress, MailPassword); //這裡要填正確的帳號跟密碼
                    //client.Host = MailServer; //設定smtp Server
                    //client.Port = int.Parse(MailPort); //設定Port
                    client.EnableSsl = true; //gmail預設開啟驗證
                    client.Send(msg); //寄出信件
                    client.Dispose();
                    msg.Dispose();
                }*/
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("==Email 發送== : " + e.Message.ToString());
                _logger.Error("==Email 發送== : " + e.StackTrace.ToString());
                return false;
            }
        }
        public static bool ValidateServerCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 忘記密碼確認碼驗證
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        public string CheckForgetPWOTP(string mail,string otp)
        {
            //string[] MailTo = null;
            try
            {
                string cacheData = _cacheOTP[mail] as string;
                if (!string.IsNullOrEmpty(cacheData))
                {
                    if (cacheData == otp)
                    {
                        //_cacheOTP.Remove(key); 改密碼後在刪除
                        return "";
                    }
                    else
                    {
                        return "驗證碼錯誤!!";
                    }
                }
                else
                {
                    return "查無此EMAIL!!";
                }
            }
            catch (Exception e)
            {
                _logger.Error("==Email 發送== : " + e.Message.ToString());
                _logger.Error("==Email 發送== : " + e.StackTrace.ToString());
                return "忘記密碼 確認碼驗證失敗!!";
            }
        }

        /// <summary>
        /// 更新密碼
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        public bool GoUpdatePassword(string mail, string otp)
        {
            //string[] MailTo = null;
            try
            {
                string cacheData = _cacheOTP[mail] as string;
                if (string.IsNullOrEmpty(cacheData))
                {
                    return false;
                }
                else
                {
                    _cacheOTP.Remove(mail);
                    return true;
                }
                //"查無此EMAIL忘記密碼申請!!"
            }
            catch (Exception e)
            {
                _logger.Error("==更新密碼== : " + e.Message.ToString());
                _logger.Error("==更新密碼== : " + e.StackTrace.ToString());
                return false;
            }
        }
    }
}