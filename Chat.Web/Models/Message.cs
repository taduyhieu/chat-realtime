using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Chat.Web.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; }

        public string Timestamp { get; set; }

        [ForeignKey("FromUser")]
        public string FromUserId { get; set; }
        public virtual ApplicationUser FromUser { get; set; }
        [ForeignKey("ToUser")]
        public string ToUserId { get; set; }
        public virtual ApplicationUser  ToUser { get; set; }

        public virtual Room ToRoom { get; set; }
        public int Stick { get; set;  }
    }
}