using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.DAL;
using DojoManagmentSystem.ViewModels;
using DojoManagmentSystem.Models;

namespace DojoManagmentSystem.Controllers
{
    public class MemberPhoneController : BaseController<MemberPhone>
    {
        private DojoManagmentContext db = new DojoManagmentContext();

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
                db.MemberPhones.Add(memberPhone);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            return PartialView("Create", memberPhone);
        }       

        // GET: Disciplines/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberPhone phone = db.MemberPhones.Find(id);
            if (phone == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", phone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PhoneNumber,ContactID")] MemberPhone phone)
        {
            ViewBag.IsValid = false;

            if (ModelState.IsValid)
            {
                db.Entry(phone).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
                return Json(new JsonReturn { RefreshScreen = true });
            }
            return PartialView("Edit", phone);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberPhone phone = db.MemberPhones.Find(id);
            if (phone == null)
            {
                return HttpNotFound();
            }
            return PartialView(phone);
        }

        // POST: Disciplines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MemberPhone phone = db.MemberPhones.Find(id);
            phone.Delete(db);
            return Json(new JsonReturn { RefreshScreen = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
