using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.ViewModels;
using DojoManagmentSystem.Infastructure.Extensions;
using Business.DAL;
using Business.Models;

namespace DojoManagmentSystem.Controllers
{
    public class ClassSessionController : BaseController<ClassSession>
    {
        private DatabaseContext db = new DatabaseContext();

        protected override List<FieldDisplay> ListDisplay => new List<FieldDisplay>
                {
                    new FieldDisplay() {FieldName = "StartTime" },
                    new FieldDisplay() {FieldName = "EndTime" },
                    new FieldDisplay() {FieldName = "DayOfWeek" },
                };
        public ActionResult Attendance(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            IQueryable<AttendanceSheet> sheets = null;
            using (db)
            {
                // Gets the members from the database
                sheets = from mem in db.GetDbSet<AttendanceSheet>()
                             group mem by DbFunctions.TruncateTime(mem.AttendanceDate)
                                  into groups
                             select groups.FirstOrDefault();

            }

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
            using (db)
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
            }
            return PartialView(classSession);
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
