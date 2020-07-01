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
    public class MemberPhoneController : BaseController<MemberPhone>
    {
        // GET: Disciplines/Create
        public ActionResult Create(int id)
        {
            MemberPhone memberPhone = new MemberPhone();
            memberPhone.ContactID = id;

            return PartialView(memberPhone);
        }

        // POST: Disciplines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PhoneNumber,ContactID")] MemberPhone memberPhone)
        {
            if (ModelState.IsValid)
            {
                memberPhone.IsArchived = false;
                db.GetDbSet<MemberPhone>().Add(memberPhone);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            return PartialView("Create", memberPhone);
        }       
    }
}
