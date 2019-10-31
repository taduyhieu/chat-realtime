using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Models.ViewModels
{
    public class UserRoomViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }

        public int Role { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
    }
}