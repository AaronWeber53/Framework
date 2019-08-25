using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Models;
using Rotativa;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class PaymentsController : BaseController<Payment>
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        protected override ListSettings ListSettings => new ListSettings() { ModalOpen = true, AllowDelete = false };
        protected override List<FieldDisplay> ListDisplay => new List<FieldDisplay>
        {
            new FieldDisplay() {FieldName = "Amount" },
            new FieldDisplay() {FieldName = "Date" },
            new FieldDisplay() {FieldName = "PaymentType" },
            new FieldDisplay() {FieldName = "Description" },
        };

        public ActionResult Index()
        {
            return View();
        }

        // GET: Payments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payments/Create
        public ActionResult Create(int id)
        {
            Member member = db.Members.Include("DisciplineEnrolledMembers").Include("DisciplineEnrolledMembers.Discipline").FirstOrDefault(m => m.Id == id);
            if (member != null)
            {
                Payment payment = new Payment() { Member = member, MemberID = id };
                return PartialView(payment);
            }
            return HttpNotFound();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Description,Amount,Date,MemberId,PaymentType,Member")] Payment payment, int membershipId)
        {
            Member member = db.Members.Include("DisciplineEnrolledMembers").Include("DisciplineEnrolledMembers.Discipline").First(m => m.Id == payment.MemberID);
            if (ModelState.IsValid)
            {
                if (membershipId != 0)
                {
                    DisciplineEnrolledMember membership = member.DisciplineEnrolledMembers.First(d => d.Id == membershipId);
                    membership.MakePayment(payment.Amount);
                }
                db.Payments.Add(payment);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }
            payment.Member = member;
            return PartialView("Create", payment);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment =db.Payments.Include("Member").FirstOrDefault(m => m.Id == id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Amount,Date,MemberId,PaymentType")] Payment payment)
        {
            ViewBag.IsValid = false;
            payment.Member = db.Members.Find(payment.MemberID);
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                 db.SaveChanges();
                ViewBag.IsValid = true;
                return Json(new JsonReturn { RefreshScreen = true });
            }
            return PartialView("Edit", payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return PartialView(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment =  db.Payments.Find(id);
            payment.Delete(db);
            return Json(new JsonReturn { RefreshScreen = true });
        }

        // Calls the view to load the pdf.
        public ActionResult PrintPaymentSlip(int id)
        {
            var payment = new Rotativa.PartialViewAsPdf("IndexById", db.Payments.Include("Member").Where(p => p.Id == id).First());
            ViewBag.DateTime = DateTime.Now;
            return payment;
        }

        // The view for individual print slips
        public ActionResult IndexById(int id)
        {
            var payment = db.Payments.Where(p => p.Id == id).First();
            return View(payment);
        }

        // The view for payment history receipts.
        public ActionResult PrintView()
        {
            var payments = db.Payments.ToList();
            return View(payments);
        }

        // Gets the date range form and gets the member id if there is one.
        public ActionResult DateRange(int? id)
        {
            ViewBag.IsValid = false;
            ViewBag.MemberId = id;
            return PartialView("DateRange");
        }

        [HttpPost, ActionName("DateRange")]
        [ValidateAntiForgeryToken]
        public ActionResult DateRange([Bind(Include = "StartDate,EndDate,MemberId")] DateRangeViewModel viewModel)
        {
            var payments = db.Payments.Include("Member").Where(p => !p.IsArchived);

            if (viewModel.StartDate != null)
            {
                payments = payments.Where(p => p.Date >= viewModel.StartDate);
            }
            if (viewModel.EndDate != null)
            {
                payments = payments.Where(p => p.Date <= viewModel.EndDate);
            }
            if (viewModel.MemberId != null)
            {
                payments = payments.Where(p => p.MemberID == viewModel.MemberId);
            }

            ViewBag.Total = payments.Sum(a => a.Amount);
            
            ViewBag.DateTime = DateTime.Now;
            ViewBag.StartDate = string.Format("{0:MM/dd/yyyy}", viewModel.StartDate);
            ViewBag.EndDate = string.Format("{0:MM/dd/yyyy}", viewModel.EndDate);

            return new Rotativa.PartialViewAsPdf("PrintView", payments);
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
