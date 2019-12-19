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
    public class DisciplineEnrolledMemberController : BaseController<DisciplineEnrolledMember>
    {
        public ActionResult Index(int id)
        {
            ViewBag.DisciplineId = id;

            return PartialView(db.GetDbSet<Member>().ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DisciplineEnrolledMember enrolledMember = db.GetDbSet<DisciplineEnrolledMember>().Find(id);
            if (enrolledMember == null)
            {
                return HttpNotFound();
            }
            return View(enrolledMember);
        }

        public ActionResult DisciplineList(int id, string sortOrder = null, string searchString = null, int page = 1)
        {
            ItemsPerPage = 3;
            ViewBag.MemberId = id;
            ViewBag.NameSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "lastname_desc" ? "lastname_asc" : "lastname_desc";
            ViewBag.PaymentRemainingSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "remainingcost_desc" ? "remainingcost_asc" : "remainingcost_desc";
            ViewBag.EndDateSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "enddate_desc" ? "enddate_asc" : "enddate_desc";

            var members = db.GetDbSet<DisciplineEnrolledMember>().Include("Discipline").Where(m => !m.IsArchived && m.MemberId == id);

            // Order the  members depending on what parameter was passed in.
            switch (sortOrder)
            {
                case "name_desc":
                    members = members.OrderBy(m => m.Member.LastName);
                    break;
                case "name_asc":
                    members = members.OrderByDescending(m => m.Member.LastName);
                    break;
                case "remainingcost_asc":
                    members = members.OrderBy(m => m.RemainingCost);
                    break;
                case "remainingcost_desc":
                    members = members.OrderByDescending(m => m.RemainingCost);
                    break;
                case "enddate_asc":
                    members = members.OrderBy(m => m.EndDate);
                    break;
                case "enddate_desc":
                    members = members.OrderByDescending(m => m.EndDate);
                    break;
                default:
                    members = members.OrderBy(m => m.EndDate);
                    break;
            }

            ListViewModel<DisciplineEnrolledMember> model = new ListViewModel<DisciplineEnrolledMember>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                ObjectList = members
            };
            return PartialView("DisciplineList", model);
        }

        public ActionResult Create(int id)
        {            
            DisciplineEnrolledMember enrolledMember = new DisciplineEnrolledMember();
            enrolledMember.DisciplineId = id;
            enrolledMember.StartDate = DateTime.Today;
            ViewBag.Members = db.GetDbSet<Member>().Where(a => !a.IsArchived).ToList();

            return PartialView(enrolledMember);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DisciplineId,Cost,MembershipLength,StartDate")] DisciplineEnrolledMember enrolledMember)
        {
            if (Request.Form["students"] == "None")
            {
                ViewBag.Unselected = true;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    enrolledMember.MemberId = int.Parse(Request.Form["students"]);
                    enrolledMember.Member = db.GetDbSet<Member>().Find(enrolledMember.MemberId);
                    enrolledMember.RemainingCost = enrolledMember.Cost;
                    enrolledMember.EndDate = enrolledMember.StartDate.AddMonths(enrolledMember.MembershipLength);
                    db.GetDbSet<DisciplineEnrolledMember>().Add(enrolledMember);
                    db.SaveChanges();
                    return Json(new JsonReturn { RefreshScreen = true });
                }
            }

            ViewBag.Members = db.GetDbSet<Member>().ToList();
            ViewBag.DisciplineId = new SelectList(db.GetDbSet<Discipline>(), "Id", "Name", enrolledMember.DisciplineId);
            return PartialView(enrolledMember);
        }
        

        public ActionResult Edit(int? id)
        {
            DisciplineEnrolledMember enrolledMember = db.GetDbSet<DisciplineEnrolledMember>().Find(id);

            // If opened from the member page, generates a link to the discipline page
            //if (origin == "member")
            //{
            //    ViewBag.LinkValue = "Go to Discipline";
            //    ViewBag.LinkUrl = $"/Disciplines/Detail/{enrolledMember.DisciplineId}";
            //}
            //// If opened from the discipline page, generates a link to the member page
            //else if (origin == "discipline")
            //{
            //    ViewBag.LinkValue = "Go to Member";
            //    ViewBag.LinkUrl = $"/Member/Details/{enrolledMember.MemberId}";
            //}

            if (enrolledMember == null)
            {
                return HttpNotFound();
            }

            //ViewBag.Origin = origin;
            return PartialView(enrolledMember);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DisciplineEnrolledMember enrolledMember)
        {

            if (ModelState.IsValid)
            {
                db.Entry(enrolledMember).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }

            string[] origin = Request.Form.GetValues("origin");

            // If opened from the member page, generates a link to the discipline page
            if (origin[0] == "member")
            {
                ViewBag.LinkValue = "Go to Discipline";
                ViewBag.LinkUrl = $"/Discipline/Details/{enrolledMember.DisciplineId}";
            }
            // If opened from the discipline page, generates a link to the member page
            else if (origin[0] == "discipline")
            {
                ViewBag.LinkValue = "Go to Member";
                ViewBag.LinkUrl = $"/Member/Details/{enrolledMember.MemberId}";
            }

            return PartialView(enrolledMember);
        }
        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DisciplineEnrolledMember enrolledMember = db.GetDbSet<DisciplineEnrolledMember>().Include("Member").FirstOrDefault(m => m.Id == id);
            if (enrolledMember == null)
            {
                return HttpNotFound();
            }
            return PartialView(enrolledMember);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DisciplineEnrolledMember enrolledMember = db.GetDbSet<DisciplineEnrolledMember>().Find(id);
            enrolledMember.Delete(db);
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
