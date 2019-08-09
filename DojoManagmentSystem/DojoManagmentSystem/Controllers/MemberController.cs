using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Infastructure.Exceptions;
using DojoManagmentSystem.Models;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class MemberController : BaseController
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        public ActionResult List(string sortOrder = null, string searchString = null, int page = 1)
        {
            ViewBag.ClassName = null;
            ViewBag.FirstNameSortParm = String.IsNullOrEmpty(sortOrder) ? "firstname_desc" : "";
            ViewBag.LastNameSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "lastname_desc" ? "lastname_asc" : "lastname_desc";


            // Gets the members from the database
            var members = from mem in db.Members.Include("DisciplineEnrolledMembers").Include("User")
                          where !mem.IsArchived                          
                          select mem;

            // If the search is not empty or null, get the members who match the search.
            if (!String.IsNullOrEmpty(searchString))
            {
                members = members.Where(m => m.FirstName.Contains(searchString) || m.LastName.Contains(searchString));
            }

            // Order the  members depending on what parameter was passed in.
            switch (sortOrder)
            {
                case "firstname_desc":
                    members = members.OrderByDescending(m => m.FirstName);
                    break;
                case "lastname_desc":
                    members = members.OrderBy(m => m.LastName);
                    break;
                case "lastname_asc":
                    members = members.OrderByDescending(m => m.LastName);
                    break;
                default:
                    members = members.OrderBy(m => m.FirstName);
                    break;
            }
            int totalPages = GetTotalPages(members.Count());
            members = members.Skip(ItemsPerPage * (page - 1)).Take(ItemsPerPage);

            ListViewModel<Member> model = new ListViewModel<Member>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                NumberOfPages = totalPages,
                ObjectList = members.ToList()
            };

            return PartialView("Members", model);
        }

        public ActionResult AttendanceList(int id, string sortOrder = null, int page = 1)
        {
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.DisciplineSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "discipline_desc" ? "discipline_asc" : "discipline_desc";

            ItemsPerPage = 3;

            // Gets the payments from the database
            var sheets = from p in db.AttendanceSheets.Include("ClassSession.Discipline")
                         where p.Member.Id == id && !p.IsArchived
                         select p;

            // Order the  payments depending on what parameter was passed in. 
            switch (sortOrder)
            {
                case "discipline_desc":
                    sheets = sheets.OrderBy(p => p.ClassSession.Discipline.Name);
                    break;
                case "discipline_asc":
                    sheets = sheets.OrderByDescending(p => p.ClassSession.Discipline.Name);
                    break;
                case "date_desc":
                    sheets = sheets.OrderBy(p => p.AttendanceDate);
                    break;
                default:
                    sheets = sheets.OrderByDescending(p => p.AttendanceDate);
                    break;
            }
            int totalPages = GetTotalPages(sheets.Count());
            sheets = sheets.Skip(ItemsPerPage * (page - 1)).Take(ItemsPerPage);

            ViewBag.MemberId = id;

            ListViewModel<AttendanceSheet> model = new ListViewModel<AttendanceSheet>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                NumberOfPages = totalPages,
                ObjectList = sheets.ToList()
            };

            return PartialView("AttendanceHistory", model);
        }

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
