using System;
using System.Collections.Generic;

namespace WebApplication1.Models.DB
{
    public partial class User
    {
        public User()
        {
            OnlineUsers = new HashSet<OnlineUsers>();
        }

        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsVerified { get; set; }
        public string ActivationCode { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string Role { get; set; }

        public virtual ICollection<OnlineUsers> OnlineUsers { get; set; }
    }
}
