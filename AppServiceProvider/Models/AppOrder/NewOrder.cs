namespace AppServiceProvider.Models
{
    public class NewOrder
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public string Userid { get; set; }
        /// <summary>
        /// 店家ID
        /// </summary>
        public string Storied { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string Goodsid { get; set; }
        /// <summary>
        /// 數量
        /// </summary>
        public string Amount { get; set; }
    }
}