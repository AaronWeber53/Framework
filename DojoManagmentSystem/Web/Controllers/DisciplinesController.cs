using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using Business.Models;
using Web.ViewModels;

namespace Web.Controllers
{
    public class DisciplineController : BaseController<Discipline>
    {
        protected override ListSettings ListSettings => new ListSettings() 
        { 
            AllowDelete = false,
            Links = new List<Link>()
            {
                new Link() { URL = "/Discipline/Create", Text = "Add Discipline" }
            }
        };

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
                ObjectList = db.GetDbSet<ClassSession>(),
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
                ObjectList = db.GetDbSet<DisciplineEnrolledMember>(),
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
                db.GetDbSet<Discipline>().Add(discipline);
                db.SaveChanges();
                return Json(new JsonReturn
                {
                    RedirectLink = Url.Action($"Details/{discipline.Id}")
                });
            }

            return PartialView("Create", discipline);
        }
    }
}
