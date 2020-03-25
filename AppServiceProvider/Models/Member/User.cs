using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppServiceProvider.Models
{
    public class User
    {

    }
    public class AddUser
    {
        public string account { get; set; } = "";
        public string Password { get; set; } = "";
        public string name { get; set; } = "";
        public string sex { get; set; } = "";
        public string birthday { get; set; } = "";
        public string email { get; set; } = "";
        public string phone { get; set; } = "";
        public string recommand { get; set; } = "";
    }
}