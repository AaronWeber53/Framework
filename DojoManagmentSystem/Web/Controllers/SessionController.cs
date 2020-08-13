using Business.DAL;
using Web.Infastructure;
using Business.Models;
using Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Infastructure;
using Business;

namespace Web.Controllers
{
    public class SessionController : BaseController
    {
        private DatabaseContext db = new DatabaseContext();

        // GET: Session
        public ActionResult Index()
        {
            return RedirectToAction("SignIn");
        }

        // Get: SignIn
        public ActionResult SignIn(bool timeout = false)
        {
            // Check if there is a current session...
            if (ApplicationContext.CurrentApplicationContext?.CurrentSession.UserId != null)
            {
                // If there is a session redirect to home.
                return RedirectToAction("Index", "Home", null);
            }

            if (timeout)
            {
                ViewBag.ErrorMessage = "You were logged out due to inactivity";
            }

            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string email, string password, bool rememberMe = false)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                List<User> users = db.GetDbSet<User>().Where(u => !u.IsArchived).ToList();

                if (email != null && password != null)
                {
                    // Hashes password input to compare to stored password.
                    string hashPassword = EncryptionHelper.EncryptText(password);

                    foreach (User u in users)
                    {
                        // Check if username is in database and if password matches.
                        if (u.Username.ToLower() == email.ToLower() && u.Password == hashPassword)
                        {
                            string hash = ApplicationContext.CurrentApplicationContext.CurrentSession?.SessionHash;
                            Session newSession = null;

                            if (hash == null)
                            {
                                newSession = GetCurrentSession(db);
                            }
                            else
                            {
                                newSession = db.GetDbSet<Session>().FirstOrDefault(s => s.SessionHash == hash);
                            }
                            // Create a session for this user.
                            newSession.UserId = u.Id;
                            // Add the session to the database.
                            newSession.Save(db);
                            // Redirect to home.
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            ViewBag.ErrorMessage = "Username and/or password was incorrect";
            return View();
        }

        public ActionResult SignOut()
        {
            // Kill the cookie that holds the session.
            //Response.Cookies["SessionGuid"].Expires = DateTime.Now.AddDays(-1);

            string hash = ApplicationContext.CurrentApplicationContext.CurrentSession.SessionHash;
            Session newSession = db.GetDbSet<Session>().FirstOrDefault(s => s.SessionHash == hash);
            newSession.UserId = null;
            // Add the session to the database.
            newSession.Save(db);

            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public ActionResult LockSession()
        {
            Session curSession = GetCurrentSession(db);

            // Update the current session to be locked to the attendance screen
            curSession.AttendanceLock = true;
            db.Entry(curSession).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("QuickAttendance", "AttendanceSheet", null);
        }

        /// <summary>
        /// Get a  modal for inputting password and if the password is correct unlock the session
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public ActionResult UnlockSession()
        {
            Session curSession = GetCurrentSession(db);
            UnlockViewModel model = new UnlockViewModel() { Username = curSession.User.Username };
            return PartialView("UnlockSession", model);
        }

        /// <summary>
        /// Get a  modal for inputting password and if the password is correct unlock the session
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnlockSession([Bind(Include = "Password,Username")] UnlockViewModel model)
        {
            Session curSession = GetCurrentSession(db);

            // If the given password is the users password. 
            if (curSession.User.Password == EncryptionHelper.EncryptText(model.Password))
            {
                curSession.AttendanceLock = false;
                db.Entry(curSession).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { RefreshScreen = true });
            }

            ModelState.AddModelError("Password", "Password is incorrect");

            return PartialView("UnlockSession", model);
        }

        private Session GetCurrentSession(DatabaseContext db)
        {
            HttpCookie sessionCookie = Request.Cookies["SessionGuid"];

            // Get the hash value from the cookies.
            string hash = sessionCookie.Value;

            // Based on the hash look for the corresponding session.
            return db.GetDbSet<Session>().Include("User").Include("User.Member").FirstOrDefault(s => s.SessionHash == hash);
        }
    }
}