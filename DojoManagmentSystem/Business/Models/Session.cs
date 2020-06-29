using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;

namespace Business.Models
{
    public class Session : BaseModel
    {
        public Session() { }
        public Session(long? userId, bool rememberMe = false)
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

        public long? UserId { get; set; }

        public bool AttendanceLock { get; set; }

        public virtual User User { get; set; }

        // Update this to user login expires, session should always hold imformation about this user while cookie is active
        [DataType(DataType.DateTime)]
        public DateTime Expires { get; set; } = DateTime.Now.Add(new TimeSpan(1, 10, 0));
    }
}