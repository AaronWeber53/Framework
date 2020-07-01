using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using Web.ViewModels;
using Business.Models;

namespace Web.Controllers
{
    public class MemberAddressController : BaseController<MemberAddress>
    {
        public ActionResult Create(int id)
        {
            MemberAddress memberAddress = new MemberAddress();
            memberAddress.ContactID = id;

            return PartialView(memberAddress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Street,City,ZipCode,State,ContactID")] MemberAddress memberAddress)
        {
            if (ModelState.IsValid)
            {
                memberAddress.IsArchived = false;
                db.GetDbSet<MemberAddress>().Add(memberAddress);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            return PartialView("Create", memberAddress);
        }
    }
}
