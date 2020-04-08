﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using System.Web.Mvc;
using NLog;
using AppServiceProvider.Models;
using AppServiceProvider.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ServiceModel.Dispatcher;
using System.Web;
using GomypayDataAccess;
using System.Data.Entity.SqlServer;

namespace AppServiceProvider.Controllers
{
    public class CodecApiController : ApiController
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static OrderService _OrderService = new OrderService();
        private static MemberService _MemberService = new MemberService();
        //private class CreditCard = AppOrder.CreditCard;

        [Route("CodecApi/GG123")]
        public IEnumerable<APP_Order> Get()
        {
            _logger.Debug("CodecApi/GG123 : TEST ");
            using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
            {
                return entities.APP_Order.ToList();
            }

        }
        public APP_Order Get(int id)
        {
            using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
            {
                return entities.APP_Order.FirstOrDefault(e => e.ID == id);
            }

        }
        #region App交易

        /// <summary>
        /// 信用卡交易
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/creditcard")]
        [HttpPost]
        //public CreditCardResult CreditCard([FromBody]CreditCard body)
        public IHttpActionResult CreditCard([FromBody]CreditCard body)
        {
            CreditCardResult Result = new CreditCardResult();
            string StrBody = string.Empty;
            try
            {
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Debug("App Request Body : " + StrBody);
                if (!ModelState.IsValid)
                {
                    //Result.ret_msg= ModelState.Values;
                    return Ok(Result);

                }


                //logger.Debug("APP Request Body : " + body);

                //JObject jObj = new JObject(body);

                //Json to QuerryString
                //var jObj = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(body));
                //JSON 轉 CURL
                //var query = String.Join("&",
                //                jObj.Children().Cast<JProperty>()
                //                .Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));
                //logger.Trace("我是追蹤:Trace222 : " + query);

                //SecurityManager.Authorize(Request);
                //return body;
                //JsonQueryStringConverter.ConvertValueToString(body, CreditCard);
                //return query;

                //string tempResult = _OrderService.CreditCardOrder(StrBody);
                string tempResult = _OrderService.CreditCardOrder(body);
                _logger.Debug("APP Resp Body : " + tempResult);
                Result = JsonConvert.DeserializeObject<CreditCardResult>(tempResult);
                //return tempResult;
                return Ok(Result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 銀聯卡付款
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/UnionPaycard")]
        [HttpPost]
        public string UnionPaycard([FromBody]string body)
        {


            //SecurityManager.Authorize(Request);
            return body; //Json(new ApiResult<byte[]>);
        }



        #endregion

        #region 使用者相關
        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/AddUser")]
        [HttpPost]
        //public ApiResult<object> Register([FromBody]AddUser body)
        public IHttpActionResult Register([FromBody]AddUser body)
        {
            //CreditCardResult Result = new CreditCardResult();
            string StrBody = string.Empty;
            int DBSave = 0;
            string otp = string.Empty;
            string SMSResult = string.Empty;
            try
            {
                bool recover = false;
                //logger.Debug("APP Request Body : " + body);
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Debug("AddUser Request Body : " + StrBody);

                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {

                    //驗手機 身分證字號 重複
                    if (entities.APP_User.FirstOrDefault(x => x.User_Account == body.phone) != null)
                    {// Message 手機號碼重複
                        if (entities.APP_User.FirstOrDefault(x => x.OtpCheck == "1") != null)
                        {//已簡訊開通帳號
                            return Ok(new ApiResult<object>("500", "手機號碼重複"));
                        }
                        recover = true;
                    }
                    if (entities.APP_User.FirstOrDefault(x => x.Identifier == body.Id) != null)
                    { // Message 身分證字號重複
                        if (entities.APP_User.FirstOrDefault(x => x.OtpCheck == "1") != null)
                        {//已簡訊開通帳號
                            return Ok(new ApiResult<object>("500", "身分證字號重複"));
                        }
                        recover = true;
                    }

                    if (recover != true)//全新帳號
                    {
                        //取APP_System_ID
                        APP_User APPUser = new APP_User()
                        {
                            System_ID = body.Id,
                            Identifier = body.Id,
                            User_Name = body.name,
                            User_Password = body.Password,
                            User_Account = body.phone,//帳號，默認為手機號碼
                            Sex = body.sex,
                            OtpCheck = "0",

                            Builder = "AppServer",
                            Build_Date = DateTime.Now,
                            Modifier = "AppServer",
                            Modify_Date = DateTime.Now
                        };
                        entities.APP_User.Add(APPUser);
                        DBSave = entities.SaveChanges();
                    }
                    else
                    {
                        APP_User UpdateAppOrder = entities.APP_User.First(c => c.System_ID == body.Id);
                        //UpdateAppOrder.System_ID = body.Id;
                        UpdateAppOrder.Identifier = body.Id;
                        UpdateAppOrder.User_Name = body.name;
                        UpdateAppOrder.User_Password = body.Password;
                        UpdateAppOrder.User_Account = body.phone;//帳號，默認為手機號碼
                        UpdateAppOrder.Sex = body.sex;
                        UpdateAppOrder.OtpCheck = "0";

                        UpdateAppOrder.Modifier = "AppServer";
                        UpdateAppOrder.Modify_Date = DateTime.Now;

                        DBSave = entities.SaveChanges();

                    }
                    if (DBSave > 0)
                    {
                        //otp = _MemberService.GetCacheData(body.Id);
                        //SendSMS
                        SMSResult = _MemberService.SendSMS(_MemberService.GetSMSString(body));
                        //ApiResult<object> dataSet = new ApiResult<object>();
                        return Ok(new ApiResult<object>());
                    }
                    else
                    {
                        return Ok(new ApiResult<object>("500", "資料庫錯誤"));
                    }
                    //return entities.APP_Order.ToList();
                }
                //var opt = @"{opt=1234 }";
                //ApiResult<object> dataSet = new ApiResult<object>();
                //string tempResult = _OrderService.CreditCardOrder(StrBody);

                //ApiResult dataSet = JsonConvert.DeserializeObject<ApiResult>();
                //return tempResult;
                //return dataSet;
            }
            catch (Exception ex)
            {
                _logger.Error("===AddUser Error===: " + ex.Message);
                _logger.Error("===AddUser Error===: " + ex.StackTrace);
                throw ex;
            }
        }


        /// <summary>
        /// 使用者簡訊驗證
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/CheckOTP")]
        [HttpPost]
        public IHttpActionResult CheckOTP([FromBody]CheckOTP body)
        {
            //ApiResult Result = new ApiResult();
            int DBSave = 0;
            string otp = string.Empty;
            try
            {
                if (_MemberService.CheckCacheData(body.ID, body.otp))
                {//OTP Check OK
                    _logger.Info("==CheckOTP OK== : ID : " + body.ID + " otp : " + body.otp);
                    using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                    {
                        APP_User UpdateAppUser = entities.APP_User.FirstOrDefault(c => c.Identifier == body.ID);
                        if (UpdateAppUser != null)
                        {
                            UpdateAppUser.OtpCheck = "1";
                        }
                        else
                        {
                            _logger.Info($"==CheckOTP== : 查無該身分證申請資料 ID = {body.ID}");
                            return Ok(new ApiResult<object>("501", $"查無該身分證申請資料 ID = {body.ID}"));
                        }

                        //APP_User 新增一個帳號狀態欄位???
                        DBSave = entities.SaveChanges();

                        if (DBSave > 0)
                        { _logger.Info("==CheckOTP== DB Update OK !!!"); }
                        else
                        {
                            _logger.Info($"==CheckOTP== : 資料庫更新失敗 ID = {body.ID}");
                            return Ok(new ApiResult<object>("501", "資料庫更新失敗"));
                        }
                        //if (DBSave > 0)
                        //{
                        //    Result.Message = "新增使用者簡訊驗證成功";
                        //    return new ApiResult();
                        //}
                        //else
                        //{// otp update error

                        //}
                        //return entities.APP_Order.ToList();
                    }
                    //Result.Message = "新增使用者簡訊驗證成功";
                    return Ok(new ApiResult<object>());
                }
                else
                {
                    _logger.Info("==CheckOTP== FAILD!!! : ID : " + body.ID + " otp : " + body.otp);
                    return Ok(new ApiResult<object>("500", "驗證碼錯誤"));
                    //return otp wrong
                }

                //return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 重送使用者簡訊驗證
        /// </summary> 
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/ResendSMS")]
        [HttpPost]
        public IHttpActionResult ResendSMS([FromBody]ResendSMS body)
        {
            //ApiResult Result = new ApiResult();
            int DBSave = 0;
            //string otp = string.Empty;
            string result;
            try
            {
                //TODO 增加重送驗證簡訊時效 錯誤判斷 30分鐘內可重送

                //取出Cache中的QueryString 重送簡訊 
                _MemberService.SendSMS(_MemberService.ReGetSMSString(body.phone));
                //_MemberService.GetCacheData(body.ID);
                return Ok(new ApiResult<object>());

                //return otp wrong

                //return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/Login")]
        [HttpPost]
        public IHttpActionResult Login([FromBody]UserLogin body)
        {
            //CreditCardResult Result = new CreditCardResult();
            string StrBody = string.Empty;
            int DBSave = 0;
            string otp = string.Empty;
            string SMSResult = string.Empty;
            try
            {
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Info("Login Request Body : " + StrBody);

                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {
                    if (entities.APP_User.FirstOrDefault(
                        x => x.User_Account == body.account &&
                        x.User_Password == body.password) != null)
                    {
                        return Ok(new ApiResult<object>());
                    }
                    else
                    {
                        return Ok(new ApiResult<object>("500", "帳號或密碼錯誤"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug("==Login== : " + ex.ToString());
                _logger.Debug("==Login== : " + ex.StackTrace.ToString());
                throw ex;
            }
        }

        #endregion
        /// <summary>
        /// 新增店家
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/addStore")]
        [HttpPost]
        public IHttpActionResult AddStore([FromBody]AddStore body)
        {
            string StrBody = string.Empty;
            int DBSave = 0;
            string otp = string.Empty;
            string SMSResult = string.Empty;
            try
            {
                bool recover = false;
                //logger.Debug("APP Request Body : " + body);
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Debug("==AddStore== Request Body : " + StrBody);

                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {

                    //驗手機 身分證字號 重複
                    //if (entities.APP_Customer.FirstOrDefault(x => x.User_Account == body.phone) != null)
                    //{// Message 手機號碼重複
                    //    if (entities.APP_User.FirstOrDefault(x => x.OtpCheck == "1") != null)
                    //    {//已簡訊開通帳號
                    //        return Ok(new ApiResult<object>("500", "手機號碼重複"));
                    //    }
                    //    recover = true;
                    //}
                    //if (entities.APP_User.FirstOrDefault(x => x.Identifier == body.Id) != null)
                    //{ // Message 身分證字號重複
                    //    if (entities.APP_User.FirstOrDefault(x => x.OtpCheck == "1") != null)
                    //    {//已簡訊開通帳號
                    //        return Ok(new ApiResult<object>("500", "身分證字號重複"));
                    //    }
                    //    recover = true;
                    //}

                    if (recover != true)//全新帳號
                    {
                        //取APP_System_ID
                        APP_Customer APPCustomerr = new APP_Customer()
                        {
                            Customer_name = body.Storename,
                            Address = body.Address,
                            Pic = body.Imagedata,
                            Telm = body.Phone,
                            Facebook = body.Fbfans,
                            Line = body.Linefans,
                            BCategory_ID = body.Type,

                            Builder = "AppServer",
                            Build_Date = DateTime.Now,
                            Modifier = "AppServer",
                            Modify_Date = DateTime.Now
                        };
                        entities.APP_Customer.Add(APPCustomerr);
                        DBSave = entities.SaveChanges();
                    }
                    //else
                    //{
                    //    APP_User UpdateAppOrder = entities.APP_User.First(c => c.System_ID == body.Id);
                    //    //UpdateAppOrder.System_ID = body.Id;
                    //    UpdateAppOrder.Identifier = body.Id;
                    //    UpdateAppOrder.User_Name = body.name;
                    //    UpdateAppOrder.User_Password = body.Password;
                    //    UpdateAppOrder.User_Account = body.phone;//帳號，默認為手機號碼
                    //    UpdateAppOrder.Sex = body.sex;
                    //    UpdateAppOrder.OtpCheck = "0";

                    //    UpdateAppOrder.Modifier = "AppServer";
                    //    UpdateAppOrder.Modify_Date = DateTime.Now;

                    //    DBSave = entities.SaveChanges();

                    //}
                    if (DBSave > 0)
                    {
                        //otp = _MemberService.GetCacheData(body.Id);
                        //SendSMS
                        //SMSResult = _MemberService.SendSMS(_MemberService.GetSMSString(body));
                        //ApiResult<object> dataSet = new ApiResult<object>();
                        return Ok(new ApiResult<object>());
                    }
                    else
                    {
                        return Ok(new ApiResult<object>("500", "資料庫錯誤"));
                    }
                    //return entities.APP_Order.ToList();
                }
                //var opt = @"{opt=1234 }";
                //ApiResult<object> dataSet = new ApiResult<object>();
                //string tempResult = _OrderService.CreditCardOrder(StrBody);

                //ApiResult dataSet = JsonConvert.DeserializeObject<ApiResult>();
                //return tempResult;
                //return dataSet;
            }
            catch (Exception ex)
            {
                _logger.Error("===AddUser Error===: " + ex.Message);
                _logger.Error("===AddUser Error===: " + ex.StackTrace);
                throw ex;
            }

        }

        /// <summary>
        /// 取得所有最新消息
        /// </summary>
        /// <returns></returns>
        [Route("CodecApi/getAllnews")]
        [HttpPost]
        public IHttpActionResult getAllnews()
        {
            string StrBody = string.Empty;
            //int DBSave = 0;
            string otp = string.Empty;
            string SMSResult = string.Empty;
            try
            {
                _logger.Debug("==getAllnews== Request");
                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {
                    var data = entities.APP_News
                        .Select(x => new News
                        {
                            id = x.News_No.ToString(),
                            title = x.News_Title,
                            content = x.News_Content,
                        }).ToList();
                    if (data.Count > 0)
                    {
                        return Ok(new ApiResult<List<News>>(data));
                    }
                    else
                    {
                        return Ok(new ApiResult<object>("500", "資料庫錯誤"));
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("===getAllnews Error===: " + ex.Message);
                _logger.Error("===getAllnews Error===: " + ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// 新增交易紀錄
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/addtransactionRecode")]
        [HttpPost]
        public IHttpActionResult addtransactionRecode([FromBody]NewOrder body)
        {
            string StrBody = string.Empty;
            //int DBSave = 0;
            string otp = string.Empty;
            string SMSResult = string.Empty;
            try
            {
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Debug("==AddTransaction== Request : " + StrBody);
                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {
                    //var data = entities.APP_News
                    //    .Select(x => new News
                    //    {
                    //        id = x.News_No.ToString(),
                    //        title = x.News_Title,
                    //        content = x.News_Content,
                    //    }).ToList();
                    //if (data.Count > 0)
                    //{
                    //    return Ok(new ApiResult<List<News>>(data));
                    //}
                    //else
                    //{
                    //    return Ok(new ApiResult<object>("500", "資料庫錯誤"));
                    //}
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                _logger.Error("===AddTransaction Error===: " + ex.Message);
                _logger.Error("===AddTransaction Error===: " + ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// 取得店家列表
        /// </summary>
        /// <returns></returns>
        [Route("CodecApi/getAllStore")]
        [HttpPost]
        public IHttpActionResult getAllStore()
        {
            string StrBody = string.Empty;
            //int DBSave = 0;
            string otp = string.Empty;
            string SMSResult = string.Empty;
            try
            {
                _logger.Debug("==getAllStore== Request");
                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {
                    var data = entities.APP_Customer
                        .Select(x => new AllStore
                        {
                            id = x.ID.ToString(),
                            date = x.Build_Date.ToString(),
                            storename = x.Store_name,
                            address = x.Address,
                            image = x.Pic,
                            phone = x.Telm,
                            fbfans = x.Facebook,
                            linefans = x.Line,
                            type = "0"
                        }).ToList();
                    if (data.Count > 0)
                    {
                        return Ok(new ApiResult<List<AllStore>>(data));
                    }
                    else
                    {
                        return Ok(new ApiResult<object>("500", "資料庫錯誤"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("===AddTransaction Error===: " + ex.Message);
                _logger.Error("===AddTransaction Error===: " + ex.StackTrace);
                throw ex;
            }
        }
    }
}
