using Business.DAL;
using Business.Infastructure;
using Business.Infastructure.Enums;
using Business.Infastructure.Exceptions;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Business.Models
{
    [Table("OldTable")]
    public class User : BaseModel
    {
        [MaxLength(100)]
        [Index("UsernameIndex")]
        public string Username { get; set; }

        public string Password { get; set; }

        [DisplayName("Security Level")]
        public SecurityLevel SecurityLevel { get; set; } = SecurityLevel.User;

        public DateTime? LastModified { get; set; }

        [Index("ModifiedById")]
        public long? LastUserIdModifiedBy { get; set; }

        public bool IsArchived { get; set; } = false;

        public virtual Member Member { get; set; }

        public virtual ICollection<Session> Sesssion { get; set; }

        public string HashPassword
        {
            set
            {
                Password = EncryptionHelper.EncryptText(value);
            }
        }


        //public void Delete(DatabaseContext db)
        //{
        //    int userCount = db.GetDbSet<User>().Count(u => !u.IsArchived);
        //    if (userCount <= 1)
        //    {
        //        throw new LastUserExpection("The last user in the system can not be deleted");
        //    }
        //    else
        //    {
        //        base.Delete(db);
        //    }
        //}

        public void Save(DatabaseContext db)
        {
            throw new NotImplementedException();
        }
    }
}