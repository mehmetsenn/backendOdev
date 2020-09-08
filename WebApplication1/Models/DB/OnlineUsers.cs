using System;
using System.Collections.Generic;

namespace WebApplication1.Models.DB
{
    public partial class OnlineUsers
    {
        public int LoginId { get; set; }
        public int? FkUserId { get; set; }
        public DateTime? LoginAttemp { get; set; }
        public DateTime? LoginTime { get; set; }

        public virtual User FkUser { get; set; }
    }
}
