namespace AppServiceProvider.Models
{
    /// <summary>
    /// 新增店家
    /// </summary>
    public class AddStore
    {
        /// <summary>
        /// 商店名稱
        /// </summary>
        public string Storename { get; set; } = "";
        /// <summary>
        /// 商店地址
        /// </summary>
        public string Address { get; set; } = "";
        /// <summary>
        /// 圖像資料
        /// </summary>
        public string Imagedata { get; set; } = "";        
        /// <summary>
        /// 店家電話
        /// </summary>
        public string Phone { get; set; } = "";
        /// <summary>
        /// FB粉絲
        /// </summary>
        public string Fbfans { get; set; } = "";
        /// <summary>
        /// Line粉絲
        /// </summary>
        public string Linefans { get; set; } = "";
        /// <summary>
        /// 店家類別 0食1衣2住3行4育5樂
        /// </summary>
        public string Type { get; set; } = "";
    }
}