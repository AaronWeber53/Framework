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
using DojoManagmentSystem.Infastructure.Exceptions;
using DojoManagmentSystem.Models;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class MemberController : BaseController<Member>
    {
        protected override ListSettings ListSettings => new ListSettings()
        {
            AllowSearch = true
        };

        protected override List<FieldDisplay> ListDisplay => new List<FieldDisplay>()
        {
            new FieldDisplay("HasUser") { AllowSort = false},
            new FieldDisplay("FirstName"),
            new FieldDisplay("LastName"),
            new FieldDisplay("IsInstructor"),
        };

        #region Relation Lists
        //// GET: Payments
        public ActionResult Payments(int id, string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<Payment> model = new ListViewModel<Payment>()
            {
                RelationID = id,
                Action = MethodBase.GetCurrentMethod().Name,
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ObjectList = db.Payments,
                ListSettings = new ListSettings()
                {
                    ModalOpen = true,
                    Links = new List<Link>()
                    {
                        new Link()
                        {
                            URL = $"/Payments/DateRange/{id}",
                            Text = "Print Payments",
                            ButtonColor = Link.Color.Blue,
                            Icon = Link.Icons.Print
                        },
                        new Link()
                        {
                            URL = $"/Payments/Create/{id}",
                            Text = "Add Payment"
                        }
                    }
                },
                FieldsToDisplay = new List<FieldDisplay>
            {
                new FieldDisplay() {FieldName = "Amount" },
                new FieldDisplay() {FieldName = "Date" },
                new FieldDisplay() {FieldName = "PaymentType" },
                new FieldDisplay() {FieldName = "Description" },
            }
            };

            return ListView(model);
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
                ObjectList = db.Contacts,
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
                ObjectList = db.Waivers,
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
                ObjectList = db.AttendanceSheets,
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
                ObjectList = db.DisciplineEnrolledMembers,
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

        public ActionResult Index()
        {
            return View();
        }

        // GET: Member/Details/5
        public ActionResult Details(int? id, string tab)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Include("User").FirstOrDefault(m => m.Id == id);

            if (member == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassName = null;
            ViewBag.TabName = tab ?? "general";
            return View(member);
        }

        // GET: Member/Create
        public ActionResult Create()
        {
            ViewBag.Disciplines = db.Disciplines.ToList();

            return PartialView("Create");
        }

        // POST: Member/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,IsInstructor")] Member member)
        {
            ViewBag.Disciplines = db.Disciplines.ToList();
            string disciplineId = Request.Form["Dropdown"];

            if (ModelState.IsValid)
            {
                member.IsArchived = false;
                db.Members.Add(member);
                db.SaveChanges();
                if (disciplineId == "None")
                {
                    return Json(new JsonReturn
                    {
                        RedirectLink = Url.Action("Index"),
                    });
                }
                else
                {
                    DisciplineEnrolledMember enrolledMember = new DisciplineEnrolledMember();
                    enrolledMember.MemberId = member.Id;
                    enrolledMember.Member = db.Members.Find(enrolledMember.MemberId);
                    enrolledMember.DisciplineId = int.Parse(disciplineId);
                    enrolledMember.Discipline = db.Disciplines.Find(enrolledMember.DisciplineId);

                    return PartialView("Enroll", enrolledMember);
                }
            }

            return PartialView("Create", member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(DisciplineEnrolledMember enrolledMember)
        {
            if (ModelState.IsValid)
            {
                enrolledMember.RemainingCost = enrolledMember.Cost;
                enrolledMember.EndDate = enrolledMember.StartDate.AddMonths(enrolledMember.MembershipLength);
                db.DisciplineEnrolledMembers.Add(enrolledMember);
                db.SaveChanges();
                return Json(new JsonReturn
                {
                    RedirectLink = Url.Action("Details", new { id = enrolledMember.MemberId }),
                });
            }

            ViewBag.DisciplineId = new SelectList(db.Disciplines, "Id", "Name", enrolledMember.DisciplineId);
            return PartialView(enrolledMember);
        }

        // GET: Member/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Include("User").FirstOrDefault(m => m.Id == id);
            if (member == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", member);
        }

        // POST: Member/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Note,IsInstructor")] Member member)
        {
            ViewBag.IsValid = false;
            if (ModelState.IsValid)
            {
                db.Entry(member).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
            }
            member.User = db.Users.Include("Member").FirstOrDefault(u => u.Member.Id == member.Id && !u.IsArchived);
            return PartialView("Edit", member);
        }

        // GET: Member/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return PartialView(member);
        }

        // POST: Member/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Member member = db.Members.Include("DisciplineEnrolledMembers").Include("Payments")
                    .Include("Waivers").Include("Contact")
                    .Include("User").FirstOrDefault(m => m.Id == id);
                member.Delete(db);
                return Json(new JsonReturn { RefreshScreen = true });
            }
            catch (LastUserExpection ex)
            {
                return Json(new JsonReturn { ErrorMessage = ex.Message });
            }
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
