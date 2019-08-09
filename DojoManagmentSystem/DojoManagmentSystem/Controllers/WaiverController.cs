using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Models;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class WaiverController : BaseController
    {
        private DojoManagmentContext db = new DojoManagmentContext();
        
        // GET: Waiver/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waiver waiver = db.Waivers.Find(id);
            if (waiver == null)
            {
                return HttpNotFound();
            }
            return View(waiver);
        }

        public ActionResult List(int id, string sortOrder = null, string searchString = null, int page = 1)
        {
            ItemsPerPage = 3;

            // Gets the waivers from the database
            var waivers = from wav in db.Waivers
                              where !wav.IsArchived && wav.MemberId == id
                              select wav;

            waivers = waivers.OrderByDescending(w => w.DateSigned);
            int totalPages = GetTotalPages(waivers.Count());
            waivers = waivers.Skip(ItemsPerPage * (page - 1)).Take(ItemsPerPage);

            ListViewModel<Waiver> model = new ListViewModel<Waiver>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                NumberOfPages = totalPages,
                ObjectList = waivers.ToList()
            };

            ViewBag.MemberId = id;
            return PartialView("List", model);
        }

        // GET: Waiver/Create
        public ActionResult Create(int id)
        {
            ViewBag.LastUserIdModifiedBy = new SelectList(db.Users, "Id", "Username");

            Waiver waiver = new Waiver { DateSigned = DateTime.Today };

            ViewBag.MemberId = id;
            return PartialView(waiver);
        }

        // POST: Waiver/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Note,DateSigned,IsSigned,MemberId")] Waiver waiver)
        {
            if (ModelState.IsValid)
            {
                db.Waivers.Add(waiver);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }
            
            ViewBag.MemberId = waiver.MemberId;
            return PartialView(waiver);
        }

        // GET: Waiver/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waiver waiver = db.Waivers.Find(id);
            if (waiver == null)
            {
                return HttpNotFound();
            }
            ViewBag.MemberId = waiver.MemberId;
            return PartialView("Edit", waiver);
        }

        // POST: Waiver/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Note,DateSigned,IsSigned,MemberId")] Waiver waiver)
        {
            if (ModelState.IsValid)
            {
                db.Entry(waiver).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            ViewBag.MemberId = waiver.MemberId;
            return PartialView("Edit", waiver);
        }

        // GET: Waiver/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waiver waiver = db.Waivers.Find(id);
            if (waiver == null)
            {
                return HttpNotFound();
            }
            return PartialView(waiver);
        }

        // POST: Waiver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Waiver waiver = db.Waivers.Find(id);
            waiver.Delete(db);
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
