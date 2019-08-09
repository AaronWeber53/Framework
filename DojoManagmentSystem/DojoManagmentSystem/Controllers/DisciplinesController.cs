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
    public class DisciplinesController : BaseController
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        // GET: Disciplines
        public ActionResult Index()
        {
            return View(db.Disciplines.ToList());
        }

        public ActionResult List(string sortOrder = null, string searchString = null, int page = 1)
        {
            ViewBag.NameSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "name_desc" ? "name_asc" : "name_desc";

            // Gets the members from the database
            var disciplines = from mem in db.Disciplines
                          where !mem.IsArchived
                          select mem;

            // Order the  members depending on what parameter was passed in.
            switch (sortOrder)
            {
                case "name_desc":
                    disciplines = disciplines.OrderByDescending(m => m.Name);
                    break;
                case "name_asc":
                    disciplines = disciplines.OrderBy(m => m.Name);
                    break;

                default:
                    disciplines = disciplines.OrderBy(m => m.Name);
                    break;
            }
            int totalPages = GetTotalPages(disciplines.Count());
            disciplines = disciplines.Skip(ItemsPerPage * (page - 1)).Take(ItemsPerPage);

            ListViewModel<Discipline> model = new ListViewModel<Discipline>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                NumberOfPages = totalPages,
                ObjectList = disciplines.ToList(),
                FieldsToDisplay = new List<FieldDisplay>
                {
                    new FieldDisplay(){ FieldName = "Name" },
                    new FieldDisplay(){ FieldName = "Description" },
                }
            };

            return PartialView("List", model);
        }

        // GET: Disciplines/Details/5
        public ActionResult Details(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discipline discipline = db.Disciplines.Find(id);
        
            if (discipline == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassName = null;
            return View(discipline);
        }

        // GET: Disciplines/Create
        public ActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: Disciplines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Discipline discipline)
        {
            if (ModelState.IsValid)
            {
                discipline.IsArchived = false;
                db.Disciplines.Add(discipline);
                db.SaveChanges();
                return Json(new JsonReturn
                {
                    RedirectLink = Url.Action("Details", new { id = discipline.Id })
                });
            }

            return PartialView("Create", discipline);
        }

        // GET: Disciplines/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discipline discipline = db.Disciplines.Find(id);
            if (discipline == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", discipline);
        }

        // POST: Disciplines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description")] Discipline discipline)
        {
            ViewBag.IsValid = false;

            if (ModelState.IsValid)
            {
                db.Entry(discipline).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
                return PartialView("Edit", discipline);
            }
            return View("Edit", discipline);
        }

        // GET: Disciplines/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discipline discipline = db.Disciplines.Find(id);
            if (discipline == null)
            {
                return HttpNotFound();
            }
            return PartialView(discipline);
        }

        // POST: Disciplines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Discipline discipline = db.Disciplines.Find(id);
            discipline.Delete(db);
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
