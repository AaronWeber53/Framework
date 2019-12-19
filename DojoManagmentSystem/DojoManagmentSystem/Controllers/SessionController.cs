using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Infastructure;
using DojoManagmentSystem.Models;
using DojoManagmentSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DojoManagmentSystem.Controllers
{
    public class SessionController : Controller
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        // GET: Session
        public ActionResult Index()
        {
            return RedirectToAction("SignIn");
        }

        // Get: SignIn
        public ActionResult SignIn(bool timeout = false)
        {
            // Check if there is a current session...
            if (Request.Cookies["SessionGuid"] != null)
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
                        // Create a session for this user.
                        Session newSession = new Session(u.Id, rememberMe);

                        // Create a cookie holding the sessions hash and add it.
                        HttpCookie cookie = new HttpCookie("SessionGuid", newSession.SessionHash);
                        cookie.Expires = newSession.Expires;
                        Response.Cookies.Add(cookie);

                        // Add the session to the database.
                        db.GetDbSet<Session>().Add(newSession);
                        db.SaveChanges();

                        // Redirect to home.
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.ErrorMessage = "Username and/or password was incorrect";
            return View();
        }

        public ActionResult SignOut()
        {
            // Kill the cookie that holds the session.
            Response.Cookies["SessionGuid"].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public ActionResult LockSession()
        {
            Session curSession = GetCurrentSession();

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
            Session curSession = GetCurrentSession();
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
            Session curSession = GetCurrentSession();

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

        private Session GetCurrentSession()
        {
            HttpCookie sessionCookie = Request.Cookies["SessionGuid"];

            // Get the hash value from the cookies.
            string hash = sessionCookie.Value;

            // Based on the hash look for the corresponding session.
            return db.GetDbSet<Session>().Include("User").Include("User.Member").FirstOrDefault(s => s.SessionHash == hash);
        }
    }
}