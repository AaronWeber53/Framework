using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DojoManagmentSystem.Models
{
    public class Session : BaseModel
    {
        public Session() { }
        public Session(int userId, bool rememberMe = false)
        {
            SessionHash = Guid.NewGuid().ToString();
            UserId = userId;
            RememberMe = rememberMe;
            if (RememberMe)
            {
                Expires = DateTime.Now.AddDays(30);
            }
        }

        public string SessionHash { get; set; }

        public bool RememberMe { get; set; }

        public int UserId { get; set; }

        public bool AttendanceLock { get; set; }

        public virtual User User { get; set; }

        public DateTime Expires { get; set; } = DateTime.Now.Add(new TimeSpan(1, 10, 0));
    }
}