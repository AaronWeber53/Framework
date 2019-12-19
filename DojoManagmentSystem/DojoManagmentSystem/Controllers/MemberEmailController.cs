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

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberEmail email = db.GetDbSet<MemberEmail>().Find(id);
            if (email == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", email);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,ContactID")] MemberEmail memberEmail)
        {
            ViewBag.IsValid = false;

            if (ModelState.IsValid)
            {
                db.Entry(memberEmail).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
                return Json(new JsonReturn { RefreshScreen = true });
            }
            return PartialView("Edit", memberEmail);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberEmail email = db.GetDbSet<MemberEmail>().Find(id);
            if (email == null)
            {
                return HttpNotFound();
            }
            return PartialView(email);
        }

        // POST: Disciplines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MemberEmail email = db.GetDbSet<MemberEmail>().Find(id);
            email.Delete(db);
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
