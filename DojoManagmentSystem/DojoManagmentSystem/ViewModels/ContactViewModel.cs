using DojoManagmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class ContactViewModel
    {
        public Contact Contact { get; set; }

        public MemberPhone MemberPhone { get; set; }

        public MemberEmail MemberEmail { get; set; }

        public MemberAddress MemberAddress { get; set; }
    }
}