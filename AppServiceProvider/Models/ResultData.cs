using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppServiceProvider.Models
{
    /// <summary>
    /// API呼叫時，傳回的統一物件
    /// </summary>
    public class ApiResult<T>
    {
        /// <summary>
        /// 執行成功與否
        /// </summary>
        public bool Succ { get; set; } = false;
        /// <summary>
        /// 結果代碼(0000=成功，其餘為錯誤代號)
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// 資料時間
        /// </summary>
        public DateTime DataTime { get; set; }
        /// <summary>
        /// 資料本體
        /// </summary>
        public T Data { get; set; }

        
        /// <summary>
        /// 建立成功結果
        /// </summary>
        /// <param name="data"></param>
        public ApiResult()//(T data)
        {
            Code = "0000";
            Succ = true;
            DataTime = DateTime.Now;
        }
        /// <summary>
        /// 建立失敗結果
        /// </summary>
        /// <param name="code"></param>
        /// /// <param name="message"></param>
        public ApiResult(string code, string message)
        {
            Code = code;
            Succ = false;
            this.DataTime = DateTime.Now;
            Message = message;
        }
        /// <summary>
        /// 建立資料查詢結果
        /// </summary>
        /// <param name="data"></param>
        public ApiResult(T data )
        {
            Code = "0000";
            Succ = true;
            this.DataTime = DateTime.Now;
            Data = data;
            //Message = message;
        }
    }
    //public class ApiError : ApiResult//<object>
    //{
    //    /// <summary>
    //    /// 建立失敗結果
    //    /// </summary>
    //    /// <param name="code"></param>
    //    /// <param name="message"></param>
    //    public ApiError(string code, string message)
    //    {
    //        Code = code;
    //        Succ = false;
    //        this.DataTime = DateTime.Now;
    //        Message = message;
    //    }
    //}
    /*
    public class AddUserResult : ApiResult<object>
    {
        public string Otp { get; set; }
        /// <summary>
        /// 建立失敗結果
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public AddUserResult(string code, string message)
        {
            Code = code;
            Succ = false;
            this.DataTime = DateTime.Now;
            Message = message;
        }
    }
    */
}