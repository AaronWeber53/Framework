using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using DojoManagmentSystem.ViewModels;
using Business.Models;

namespace DojoManagmentSystem.Controllers
{
    public class MemberEmailController : BaseController<MemberEmail>
    {
        public ActionResult Create(int id)
        {
            MemberEmail memberEmail = new MemberEmail();
            memberEmail.ContactID = id;

            return PartialView(memberEmail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Email,ContactID")] MemberEmail memberEmail)
        {
            if (ModelState.IsValid)
            {
                memberEmail.IsArchived = false;
                db.GetDbSet<MemberEmail>().Add(memberEmail);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            return PartialView("Create", memberEmail);
        }
    }
}
