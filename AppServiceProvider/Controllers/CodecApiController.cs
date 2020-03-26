using System;
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
        public CreditCardResult CreditCard([FromBody]CreditCard body)
        {
            CreditCardResult Result = new CreditCardResult();
            string StrBody = string.Empty;
            try
            {
                //logger.Debug("APP Request Body : " + body);
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Debug("App Request Body : " + StrBody);
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
                CreditCardResult dataSet = JsonConvert.DeserializeObject<CreditCardResult>(tempResult);
                //return tempResult;
                return dataSet;
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
            return  body; //Json(new ApiResult<byte[]>);
        }



        #endregion
        
        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("CodecApi/AddUser")]
        [HttpPost]
        public ApiResult Register([FromBody]AddUser body)
        {
            //CreditCardResult Result = new CreditCardResult();
            string StrBody = string.Empty;
            int DBSave = 0;
            string otp = string.Empty;
            try
            {
                //logger.Debug("APP Request Body : " + body);
                StrBody = JsonConvert.SerializeObject(body);
                _logger.Debug("AddUser Request Body : " + StrBody);
                
                using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                {
                    //驗手機 身分證字號 重複
                    if (entities.APP_User.FirstOrDefault(x => x.User_Account == body.phone) != null)
                    {// Message 手機號碼重複
                        return new ApiResult("500", "手機號碼重複");
                    }
                    if (entities.APP_User.FirstOrDefault(x => x.Identifier == body.Id) != null)
                    { // Message 身分證字號重複
                        return new ApiResult("500", "身分證字號重複");
                    }

                    //取APP_System_ID
                    APP_User APPUser = new APP_User()
                    {
                        System_ID = body.Id,
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
                    if (DBSave > 0)
                    {
                        otp = _MemberService.GetCacheData(body.Id);
                        //SendSMS
                        //_MemberService.SendSMS(_MemberService.GetSMSString(body));
                    }
                    //return entities.APP_Order.ToList();
                }
                //var opt = @"{opt=1234 }";
                ApiResult dataSet = new ApiResult();
                //string tempResult = _OrderService.CreditCardOrder(StrBody);

                //ApiResult dataSet = JsonConvert.DeserializeObject<ApiResult>();
                //return tempResult;
                return dataSet;
            }
            catch (Exception ex)
            {
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
        public ApiResult AddUserAuth([FromBody]CheckOTP body)
        {   
            //ApiResult Result = new ApiResult();
            int DBSave = 0;
            string otp = string.Empty;
            try
            {
                if (_MemberService.CheckCacheData(body.ID, body.otp))
                {//OTP Check OK
                    using (Gomypay_AppEntities entities = new Gomypay_AppEntities())
                    {
                        APP_User UpdateAppUser = entities.APP_User.FirstOrDefault(c => c.Identifier == body.ID);
                        if (UpdateAppUser != null)
                        {
                            UpdateAppUser.OtpCheck = "1";
                        }

                        //APP_User 新增一個帳號狀態欄位???
                        DBSave = entities.SaveChanges();

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
                    return new ApiResult();
                }
                else 
                {
                    return new ApiResult("500","驗證碼錯誤");
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
        public ApiResult ResendSMS([FromBody]ResendSMS body)
        {
            //ApiResult Result = new ApiResult();
            int DBSave = 0;
            string otp = string.Empty;
            try
            {
                 _MemberService.GetCacheData(body.ID);
                    return new ApiResult();
                
                
                    return new ApiResult("500", "驗證碼錯誤");
                    //return otp wrong
                

                //return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }















        /*
        /// <summary>
        /// 新增使用者
        /// </summary>
        /// <param name="encKey"></param>
        /// <param name="rawText"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string AddUser(string encKey, string rawText)
        {
            logger.Trace("我是追蹤:Trace");
            logger.Debug("我是偵錯:Debug");
            logger.Info("我是資訊:Info");

            //SecurityManager.Authorize(Request);
            return Json("AddUser").ToString(); //Json(new ApiResult<byte[]>);
        }
        /// <summary>
        /// 取得所有使用者列表
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string ListAllUser()
        {
            //SecurityManager.Authorize(Request);
            return Json("ListAllUser").ToString(); //Json(new ApiResult<byte[]>);
        }
        /// <summary>
        /// 3.	刪除使用者
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string DeleteUser()
        {
            //SecurityManager.Authorize(Request);
            return Json("DeleteUser").ToString(); //Json(new ApiResult<byte[]>);
        }
        /// <summary>
        /// 4.	新增使用者信用卡資訊
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string addCreditCard()
        {
            //SecurityManager.Authorize(Request);
            return Json("addCreditCard").ToString(); //Json(new ApiResult<byte[]>);
        }
        /// <summary>
        /// 5.	取得使用者信用卡資訊
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string getCreditCardInfo()
        {
            //SecurityManager.Authorize(Request);
            return Json("getCreditCardInfo").ToString(); //Json(new ApiResult<byte[]>);
        }

        /// <summary>
        /// 6.	刪除信用卡資訊
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string DeleteCreditCard()
        {
            //SecurityManager.Authorize(Request);
            return Json("DeleteCreditCard").ToString(); //Json(new ApiResult<byte[]>);
        }

        /// <summary>
        /// 7.	新增使用者任務
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string addMyMission()
        {
            //SecurityManager.Authorize(Request);
            return Json("addMyMission").ToString(); //Json(new ApiResult<byte[]>);
        }

        /// <summary>
        /// 8.	更新使用者任務狀態
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string UpdateMyMission()
        {
            //SecurityManager.Authorize(Request);
            return Json("UpdateMyMission").ToString(); //Json(new ApiResult<byte[]>);
        }

        /// <summary>
        /// 9.	取得使用者所有任務
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string getAllMyMissionInfo()
        {
            //SecurityManager.Authorize(Request);
            return Json("getAllMyMissionInfo").ToString(); //Json(new ApiResult<byte[]>);
        }

        /// <summary>
        /// 10.	增加使用者票卷
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public string AddTicket()
        {
            //SecurityManager.Authorize(Request);
            return Json("AddTicket").ToString(); //Json(new ApiResult<byte[]>);
        }
        */
    }
}
