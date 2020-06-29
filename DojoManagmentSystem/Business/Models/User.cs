﻿using Business.DAL;
using Business.Infastructure;
using Business.Infastructure.Enums;
using Business.Infastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Business.Models
{
    public class User : BaseModel
    {
        [MaxLength(100)]
        [Index("UsernameIndex")]
        public string Username { get; set; }

        public string Password { get; set; }

        [DisplayName("Security Level")]
        public SecurityLevel SecurityLevel { get; set; } = SecurityLevel.User;

        public virtual Member Member { get; set; }

        public virtual ICollection<Session> Sesssion { get; set; }

        public string HashPassword
        {
            set
            {
                Password = EncryptionHelper.EncryptText(value);
            }
        }

        public override void Delete(DatabaseContext db)
        {
            int userCount = db.GetDbSet<User>().Count(u => !u.IsArchived);
            if (userCount <= 1)
            {
                throw new LastUserExpection("The last user in the system can not be deleted");
            }
            else
            {
                base.Delete(db);
            }
        }
    }
}