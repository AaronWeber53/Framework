using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class ContactsViewModel
    {
        public Contact Contact { get; set; }

        public List<MemberPhone> MemberPhones { get; set; }

        public List<MemberEmail> MemberEmails { get; set; }

        public List<MemberAddress> MemberAddresses { get; set; }
    }
}