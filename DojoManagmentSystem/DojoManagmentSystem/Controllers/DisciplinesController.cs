using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Models;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class DisciplineController : BaseController<Discipline>
    {
        protected override ListSettings ListSettings => new ListSettings() { AllowDelete = false };

        protected override List<FieldDisplay> ListDisplay
        {
            get
            {
                return new List<FieldDisplay>()
                {
                    new FieldDisplay(){ FieldName = "Name" },
                    new FieldDisplay(){ FieldName = "Description" },
                };
            }
        }
        public ActionResult ClassSessions(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<ClassSession> model = new ListViewModel<ClassSession>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ObjectList = db.ClassSessions,
                FieldsToDisplay = new List<FieldDisplay>
                {
                    new FieldDisplay() {FieldName = "StartTime" },
                    new FieldDisplay() {FieldName = "EndTime" },
                    new FieldDisplay() {FieldName = "DayOfWeek" },
                }
            };

            return ListView(model);
        }

        public ActionResult Members(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<DisciplineEnrolledMember> model = new ListViewModel<DisciplineEnrolledMember>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ListSettings = new ListSettings() { ModalOpen = true },
                ObjectList = db.DisciplineEnrolledMembers,
                FieldsToDisplay = new List<FieldDisplay>
                {
                    new FieldDisplay() {FieldName = "Member.FirstName" },
                    new FieldDisplay() {FieldName = "Member.LastName" },
                    new FieldDisplay() {FieldName = "StartDate" },
                    new FieldDisplay() {FieldName = "EndDate" },
                    new FieldDisplay() {FieldName = "RemainingCost" },
                    new FieldDisplay() {FieldName = "Cost" },
                }
            };

            return ListView(model);
        }

        // GET: Disciplines
        public ActionResult Index()
        {
            return View(db.Disciplines.ToList());
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
