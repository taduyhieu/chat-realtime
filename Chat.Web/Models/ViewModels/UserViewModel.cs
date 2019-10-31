using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public int CurrentRoomId { get; set; }
        public string CurrentRoomName { get; set; }
        public string Device { get; set; }
    }
}