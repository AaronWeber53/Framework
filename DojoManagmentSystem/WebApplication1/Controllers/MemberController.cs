using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using Business.Infastructure.Enums;
using Business.Models;
using Web.ViewModels;

namespace Web.Controllers
{
    public class MemberController : BaseController<Member>
    {
        protected override ListSettings ListSettings => new ListSettings()
        {
            AllowSearch = true,
            Links = new List<Link>()
            {
                new Link()
                {
                    URL = "/Member/Create",
                    Text = "Add Member"
                }
            }
        };

        protected override List<FieldDisplay> ListDisplay => new List<FieldDisplay>()
        {
            new FieldDisplay("HasUser") { AllowSort = false},
            new FieldDisplay("User.SecurityLevel"),
            new FieldDisplay<Member>(m => m.FirstName),
            new FieldDisplay("LastName"),
            new FieldDisplay("IsInstructor"),
        };

        protected override List<Expression<Func<Member, object>>> EditRelationships => new List<Expression<Func<Member, object>>>()
        {
            m => m.User,
            m => m.DisciplineEnrolledMembers,
            m => m.Payments,
            m => m.Waivers,
            m => m.Contacts
        };

        #region Relation Lists
        //// GET: Payments
        public ActionResult Payments(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<Payment> test = RelationshipList<Payment>(id, filter, sortOrder, searchString, page);
            test.ListSettings = new ListSettings()
            {
                ModalOpen = true,
                Links = new List<Link>()
                    {
                        new Link()
                        {
                            URL = $"/Payment/DateRange/{id}",
                            Text = "Print Payments",
                            ButtonColor = Link.Color.Blue,
                            Icon = Link.Icons.Print
                        },
                        new Link()
                        {
                            URL = $"/Payment/Create/{id}",
                            Text = "Add Payment"
                        }
                    }
            };

            return ListView(test);
        }

        //// GET: Payments
        public ActionResult Contacts(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<Contact> model = new ListViewModel<Contact>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ObjectList = db.GetDbSet<Contact>(),
                FieldsToDisplay = new List<FieldDisplay>
            {
                new FieldDisplay() {FieldName = "Name" },
                new FieldDisplay() {FieldName = "RelationShip" },
                new FieldDisplay() {FieldName = "IsPrimary" },
            }
            };

            return ListView(model);
        }

        //// GET: Payments
        public ActionResult Waivers(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<Waiver> model = new ListViewModel<Waiver>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ObjectList = db.GetDbSet<Waiver>(),
                ListSettings = new ListSettings() { ModalOpen = true },
                FieldsToDisplay = new List<FieldDisplay>
                {
                    new FieldDisplay() {FieldName = "IsSigned" },
                    new FieldDisplay() {FieldName = "DateSigned" },
                    new FieldDisplay() {FieldName = "Note" },
                }
            };

            return ListView(model);
        }

        public ActionResult Attendance(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<AttendanceSheet> model = new ListViewModel<AttendanceSheet>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ListSettings = new ListSettings() { ItemsPerPage = 3, AllowOpen = false },
                ObjectList = db.GetDbSet<AttendanceSheet>(),
                FieldsToDisplay = new List<FieldDisplay>
                {
                    new FieldDisplay() {FieldName = "AttendanceDate" },
                }
            };

            return ListView(model);
        }

        public ActionResult Disciplines(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
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
                    new FieldDisplay() {FieldName = "StartDate" },
                    new FieldDisplay() {FieldName = "EndDate" },
                    new FieldDisplay() {FieldName = "RemainingCost" },
                    new FieldDisplay() {FieldName = "Cost" },
                }
            };

            return ListView(model);
        }
        #endregion

        public ActionResult TestOwner()
        {
            return HttpNotFound();
        }

        public ActionResult TestAdmin()
        {
            return HttpNotFound();
        }

        public ActionResult TestUser()
        {
            return HttpNotFound();
        }

        public ActionResult TestNormal()
        {
            return HttpNotFound();
        }

        [NonAction]
        public override ActionResult Details(long? id) { return null; }

        public ActionResult Details(long id, string tab)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           Member member = db.GetDbSet<Member>().Include("User").FirstOrDefault(m => m.Id == id);

            if (member == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassName = null;
            ViewBag.TabName = tab ?? "general";
            return View(member);
        }

        protected override void SetAdditionalEditValues(Member obj)
        {
            base.SetAdditionalEditValues(obj);
            ViewBag.DisabledSecurityChange = obj.User.SecurityLevel < Business.Infastructure.Enums.SecurityLevel.Admin;
        }

        // GET: Member/Create
        public ActionResult Create()
        {
            ViewBag.Disciplines = db.GetDbSet<Discipline>().ToList();

            return PartialView("Create");
        }

        // POST: Member/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,IsInstructor")] Member member)
        {
            ViewBag.Disciplines = db.GetDbSet<Discipline>().ToList();
            string disciplineId = Request.Form["Dropdown"];
            if (ModelState.IsValid)
            {
                member.IsArchived = false;
                db.GetDbSet<Member>().Add(member);
                db.SaveChanges();
                if (disciplineId == "None")
                {
                    return Json(new JsonReturn
                    {
                        RedirectLink = Url.Action("List"),
                    });
                }
                else
                {
                    DisciplineEnrolledMember enrolledMember = new DisciplineEnrolledMember();
                    enrolledMember.MemberId = member.Id;
                    enrolledMember.Member = db.GetDbSet<Member>().Find(enrolledMember.MemberId);
                    enrolledMember.DisciplineId = int.Parse(disciplineId);
                    enrolledMember.Discipline = db.GetDbSet<Discipline>().Find(enrolledMember.DisciplineId);

                    return PartialView("Enroll", enrolledMember);
                }
            }

            return PartialView("Create", member);
        }

        public override ActionResult EditValidation(long? id)
        {
            return base.EditValidation(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(DisciplineEnrolledMember enrolledMember)
        {
            if (ModelState.IsValid)
            {
                enrolledMember.RemainingCost = enrolledMember.Cost;
                enrolledMember.EndDate = enrolledMember.StartDate.AddMonths(enrolledMember.MembershipLength);
                db.GetDbSet<DisciplineEnrolledMember>().Add(enrolledMember);
                db.SaveChanges();
                return Json(new JsonReturn
                {
                    RedirectLink = Url.Action("Details", new { id = enrolledMember.MemberId }),
                });
            }

            ViewBag.DisciplineId = new SelectList(db.GetDbSet<Discipline>(), "Id", "Name", enrolledMember.DisciplineId);
            return PartialView(enrolledMember);
        }

    }
}
