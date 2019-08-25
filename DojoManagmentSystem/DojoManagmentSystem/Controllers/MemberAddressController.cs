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
    public class MemberAddressController : BaseController<MemberAddress>
    {
        private DojoManagmentContext db = new DojoManagmentContext();

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
                db.MemberAddresses.Add(memberAddress);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            return PartialView("Create", memberAddress);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberAddress memberAddress = db.MemberAddresses.Find(id);
            if (memberAddress == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", memberAddress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Street,City,ZipCode,State,ContactID")] MemberAddress memberAddress)
        {
            ViewBag.IsValid = false;

            if (ModelState.IsValid)
            {
                db.Entry(memberAddress).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
                return Json(new JsonReturn { RefreshScreen = true });
            }
            return PartialView("Edit", memberAddress);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberAddress memberAddress = db.MemberAddresses.Find(id);
            if (memberAddress == null)
            {
                return HttpNotFound();
            }
            return PartialView(memberAddress);
        }

        // POST: Disciplines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MemberAddress memberAddress = db.MemberAddresses.Find(id);
            memberAddress.Delete(db);
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
