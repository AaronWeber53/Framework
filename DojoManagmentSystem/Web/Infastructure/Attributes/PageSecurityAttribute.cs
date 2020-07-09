using Business;
using Business.DAL;
using Business.Infastructure.Enums;
using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Infastructure.Attributes
{
    [AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Method,
                       AllowMultiple = false)]
    public class PageSecurityAttribute : Attribute
    {

        public SecurityLevel SecurityLevel { get; set; } 

        public PageSecurityAttribute(SecurityLevel securityLevel = SecurityLevel.Normal)
        {
            SecurityLevel = securityLevel;
        }

        public bool CheckUserHasPermission()
        {
            if (SecurityLevel == SecurityLevel.Normal)
            {
                return true;
            }

            using(DatabaseContext db = new DatabaseContext())
            {
                long? currentUserID = ApplicationContext.CurrentApplicationContext?.CurrentSession?.UserId;
                User user = db.GetDbSet<User>().FirstOrDefault(u => u.Id == currentUserID);

                if (user != null)
                {
                    return user.SecurityLevel >= SecurityLevel;
                }
            }
            return false;
        }
    }
}