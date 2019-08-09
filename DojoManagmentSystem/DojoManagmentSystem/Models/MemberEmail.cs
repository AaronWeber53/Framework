﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DojoManagmentSystem.DAL;

namespace DojoManagmentSystem.Models
{
    public class MemberEmail : BaseModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public int ContactID { get; set; }

        public virtual Contact Contact { get; set; }
    }
}