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
using DojoManagmentSystem.Infastructure.Extensions;

namespace DojoManagmentSystem.Controllers
{
    public class ClassSessionController : BaseController<ClassSession>
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        // GET: ClassSession
        public ActionResult Index()
        {
            var disciplines = db.GetDbSet<Discipline>().Include(d => d.ClassSessions);
            return PartialView(disciplines.ToList());
        }

        public ActionResult Attendance(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            // Gets the members from the database
            var sheets = from mem in db.GetDbSet<AttendanceSheet>()
                         group mem by DbFunctions.TruncateTime(mem.AttendanceDate)
                              into groups
                         select groups.FirstOrDefault();

            ListViewModel<AttendanceSheet> model = new ListViewModel<AttendanceSheet>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ListSettings = new ListSettings() { ModalOpen = true },
                ObjectList = sheets,
                FieldsToDisplay = new List<FieldDisplay>
                {
                    new FieldDisplay() {FieldName = "AttendanceDate" },
                }
            };

            return ListView(model);
        }

        // GET: ClassSession/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassSession model = db.GetDbSet<ClassSession>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        // GET: ClassSession/Create
        public ActionResult Create(int id)
        {
            ViewBag.DisciplineId = id;
            return PartialView();
        }

        // POST: ClassSession/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StartTime,EndTime,DayOfWeek,DisciplineId")] ClassSession classSession)
        {
            if (ModelState.IsValid)
            {
                db.GetDbSet<ClassSession>().Add(classSession);
                db.SaveChanges();
                return Json(new JsonReturn
                {
                    RedirectLink = Url.Action("Details", new { id = classSession.Id })
                });
            }

            ViewBag.DisciplineId = new SelectList(db.GetDbSet<Discipline>(), "Id", "Name", classSession.DisciplineId);
            return PartialView(classSession);
        }

        // GET: ClassSession/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassSession classSession = db.GetDbSet<ClassSession>().Find(id);
            ClassSessionEditViewModel classSessionOutput = new ClassSessionEditViewModel { Id = classSession.Id, DayOfWeek = classSession.DayOfWeek, EndTime = classSession.EndTime, StartTime = classSession.StartTime, DisciplineId = classSession.DisciplineId };
            if (classSession == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", classSessionOutput);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StartTime,EndTime,DayOfWeek,DisciplineId")] ClassSession classSession)
        {
            ClassSessionEditViewModel classSessionOutput = new ClassSessionEditViewModel { Id = classSession.Id, DayOfWeek = classSession.DayOfWeek, EndTime = classSession.EndTime, StartTime = classSession.StartTime, DisciplineId = classSession.DisciplineId };
            ViewBag.IsValid = false;

            if (ModelState.IsValid)
            {
                db.Entry(classSession).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
                return PartialView("Edit", classSessionOutput);
            }
            return View("Edit", classSessionOutput);
        }

        // GET: ClassSession/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassSession classSession = db.GetDbSet<ClassSession>().Find(id);
            if (classSession == null)
            {
                return HttpNotFound();
            }
            return PartialView(classSession);
        }

        // POST: ClassSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ClassSession classSession = db.GetDbSet<ClassSession>().Include("AttendanceSheets").FirstOrDefault(a => a.Id == id);
            classSession.Delete(db);
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
