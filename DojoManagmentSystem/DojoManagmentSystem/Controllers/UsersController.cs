using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using DojoManagmentSystem.Infastructure; 
using Business.Models;
using Business.Infastructure.Exceptions;
using Business.Infastructure;

namespace DojoManagmentSystem.Controllers
{
    public class UsersController : BaseController<User>
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        protected override List<string> EditRelationships => new List<string>()
        {
            "Member"
        };

        // GET: Users
        public ActionResult Index()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.GetDbSet<User>().Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create(int id)
        {
            if (db.GetDbSet<Member>().ToList().Exists(m => m.Id == id))
            {
                CreateUserModel obj = new CreateUserModel() { MemberId = id };
                return PartialView(obj);
            }
            return HttpNotFound();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberId,Username,Password,ConfirmPassword")] CreateUserModel user)
        {
            if (db.GetDbSet<User>().Where(a => !a.IsArchived).Any(u => u.Username.ToLower() == user.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "A user with this Username already exists");
            }

            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords must match!");
            }

            if (ModelState.IsValid)
            {
                // Hashes user password before storing in the database
                User newUser = new User() { Username = user.Username, HashPassword = user.Password };

                // Sets the users member
                var member = db.GetDbSet<Member>().First(m => m.Id == user.MemberId);

                newUser.Member = member;
                member.User = newUser;

                db.GetDbSet<User>().Add(newUser);
                db.SaveChanges();
                return Json(new JsonReturn
                {
                    RefreshScreen = true
                });
            }

            return PartialView(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Username,Password")] User user)
        {
            if (db.GetDbSet<User>().ToList().Exists(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Email", "A user with this email already exists");
            }

            if (ModelState.IsValid)
            {
                user.HashPassword = user.Password;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public override ActionResult DeleteConfirmed(long id)
        {
            try
            {
                User user = GetObj(id);
                user.Delete(db);
                user.Member.User = null;
                db.Entry(user.Member).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }
            catch (LastUserExpection ex)
            {
                return Json(new JsonReturn
                {
                    ErrorMessage = ex.Message
                });
            }
        }

        public ActionResult ChangePassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.GetDbSet<User>().Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ChangePasswordModel obj = new ChangePasswordModel() { Id = id ?? 0};
            return PartialView("ChangePassword", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "Id,CurrentPassword,NewPassword,ConfirmPassword")] ChangePasswordModel obj)
        {
            User user = db.GetDbSet<User>().Find(obj.Id);

            if (!string.IsNullOrEmpty(obj.CurrentPassword) && EncryptionHelper.EncryptText(obj.CurrentPassword) != user.Password)
            {
                ModelState.AddModelError("CurrentPassword", "Incorrect password");
            }

            if (obj.NewPassword != obj.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords must match");
            }

            if (ModelState.IsValid)
            {
                user.HashPassword = obj.NewPassword;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }
            return PartialView("ChangePassword", obj);
        }

        #region Temporary View Models
        /// <summary>
        /// This model is used to store/display data for changing a password.
        /// </summary>
        public class ChangePasswordModel
        {
            public int Id { get; set; }

            [Required]
            [DisplayName("Current Password")]
            public string CurrentPassword { get; set; }

            [Required]
            [DisplayName("New Password")]
            public string NewPassword { get; set; }

            [Required]
            [DisplayName("Confirm Password")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// This model is used to store/display data for creating a user.
        /// </summary>
        public class CreateUserModel
        {
            [Required]
            public int MemberId { get; set; }

            [Required]
            public string Username { get; set; }

            [Required]
            [DisplayName("Password")]
            public string Password { get; set; }

            [Required]
            [DisplayName("Confirm Password")]
            public string ConfirmPassword { get; set; }

        }
        #endregion
    }
}
