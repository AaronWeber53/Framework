using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DojoManagmentSystem.DAL;
using DojoManagmentSystem.ViewModels;
using DojoManagmentSystem.Models;

namespace DojoManagmentSystem.Controllers
{
    public class ContactController : BaseController<Contact>
    {
        private DojoManagmentContext db = new DojoManagmentContext();

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
            Contact contact = db.Contacts.Find(id);
        
            if (contact == null)
            {
                return HttpNotFound();
            }
            ContactsViewModel model = new ContactsViewModel();
            model.Contact = contact;
            model.MemberPhones = new List<MemberPhone>();
            model.MemberEmails = new List<MemberEmail>();
            model.MemberAddresses = new List<MemberAddress>();
            List<MemberPhone> phones = db.MemberPhones.ToList();
            foreach (MemberPhone mp in phones)
            {
                if (mp.ContactID == contact.Id)
                {
                    model.MemberPhones.Add(mp);
                }
            }
            List<MemberEmail> emails = db.MemberEmail.ToList();
            foreach (MemberEmail me in emails)
            {
                if (me.ContactID == contact.Id)
                {
                    model.MemberEmails.Add(me);
                }
            }
            List<MemberAddress> addresses = db.MemberAddresses.ToList();
            foreach (MemberAddress ma in addresses)
            {
                if (ma.ContactID == contact.Id)
                {
                    model.MemberAddresses.Add(ma);
                }
            }
            return View(model);
        }

        // GET: Disciplines/Create
        public ActionResult Create(int id)
        {
            ContactViewModel contact = new ContactViewModel();
            contact.Contact = new Contact();
            contact.MemberPhone = new MemberPhone();
            contact.MemberEmail = new MemberEmail();
            contact.MemberAddress = new MemberAddress();
            bool hasPrimary = db.Members.First(m => m.Id == id).Contact.Any(c => !c.IsArchived);
            contact.Contact.MemberId = id;

            // If the member doesn't have a contact default to primary.
            contact.Contact.IsPrimary = !hasPrimary;

            return PartialView(contact);
        }

        // POST: Disciplines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Contact.IsArchived = false;
                model.MemberPhone.IsArchived = false;
                model.MemberEmail.IsArchived = false;
                model.MemberAddress.IsArchived = false;

                db.Contacts.Add(model.Contact);
                db.SaveChanges();

                model.MemberPhone.ContactID = model.Contact.Id;
                model.MemberEmail.ContactID = model.Contact.Id;
                model.MemberAddress.ContactID = model.Contact.Id;   

                if(model.MemberPhone.PhoneNumber != null) {
                db.MemberPhones.Add(model.MemberPhone);
                }
                if (model.MemberEmail.Email != null) {
                    db.MemberEmail.Add(model.MemberEmail);
                }
                if (model.MemberAddress.Street != null) {
                    db.MemberAddresses.Add(model.MemberAddress);
                }
                db.SaveChanges();

                return Json(new JsonReturn { RefreshScreen = true });
            }

            return PartialView("Create", model);
        }

        // GET: Disciplines/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsValid = false;
            return PartialView("Edit", contact);
        }

        // POST: Disciplines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,RelationShip,IsPrimary,MemberId")] Contact contact)
        {
            ViewBag.IsValid = false;

            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.IsValid = true;
                return PartialView("Edit", contact);
            }
            return View("Edit", contact);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return PartialView(contact);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contact contact = db.Contacts.Find(id);
            List<MemberPhone> phones = db.MemberPhones.ToList();
            foreach(MemberPhone mp in phones)
            {
                if(mp.ContactID == contact.Id)
                {
                    mp.Delete(db);
                }
            }
            List<MemberEmail> emails = db.MemberEmail.ToList();
            foreach (MemberEmail me in emails)
            {
                if (me.ContactID == contact.Id)
                {
                    me.Delete(db);
                }
            }
            List<MemberAddress> addresses = db.MemberAddresses.ToList();
            foreach (MemberAddress ma in addresses)
            {
                if (ma.ContactID == contact.Id)
                {
                    ma.Delete(db);
                }
            }
            contact.Delete(db);
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
