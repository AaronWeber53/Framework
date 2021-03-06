﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Business.DAL;

namespace Business.Models
{
    public class MemberPhone : BaseModel
    {
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        public long ContactID { get; set; }

        public virtual Contact Contact { get; set; }
    }
}