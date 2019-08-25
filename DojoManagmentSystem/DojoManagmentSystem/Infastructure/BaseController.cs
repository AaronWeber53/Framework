using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Infastructure;
using DojoManagmentSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DojoManagmentSystem
{
    public abstract class BaseController : Controller 
    {
        protected DojoManagmentContext db = new DojoManagmentContext();

        protected string ListView = "~/Views/Shared/List.cshtml";

        /// <summary>
        /// Whenever a action is executed this function will be called first.
        /// </summary>
        /// <param name="filterContext">Context of the action.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {           
            // Check if the SesssionGuid cookie is set.
            if (Request.Cookies["SessionGuid"] != null)
            {
                // Get the session cookie.
                HttpCookie sessionCookie = Request.Cookies["SessionGuid"];

                // Get the hash value from the cookies.
                string hash = sessionCookie.Value;

                // Based on the hash look for the corresponding session.
                Models.Session curSession = db.Sessions.Include("User").Include("User.Member").FirstOrDefault(s => s.SessionHash == hash);

                // Set variables for layout data.
                bool isValidSession = true;
                //Check if the current session isn't null and the session isn't expired.
                if (curSession != null)
                {
                    ApplicationContext.CurrentApplicationContext.CurrentSession = curSession;
                    curSession.Expires = !curSession.RememberMe ? DateTime.Now.AddMinutes(10) : DateTime.Now.AddDays(30);
                    ViewBag.UserId = curSession.User.Member.Id;
                    ViewBag.FullName = curSession.User.Member.FullName;

                    // Set the current session to the found session.
                    if (curSession.AttendanceLock)
                    {
                        // Make sure that the cookie session will not expire while in attendance mode.
                        sessionCookie.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Set(sessionCookie);

                        // If you are not currently going to quick attendance redirect route.
                        if (filterContext.ActionDescriptor.ActionName != "QuickAttendance" && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName != "AttendanceSheet")
                        {
                            // Always redirect to quick attendance if somebody tries to get out.
                            filterContext.Result = new RedirectResult("/AttendanceSheet/QuickAttendance");
                        }
                    }
                    else if (curSession.Expires > DateTime.Now)
                    {

                        // Reset the session expiration and the cookie expiration.
                        curSession.Expires = !curSession.RememberMe ? DateTime.Now.AddMinutes(10) : DateTime.Now.AddDays(30);
                        sessionCookie.Expires = curSession.Expires;
                        Response.Cookies.Set(sessionCookie);

                        ApplicationContext.CurrentApplicationContext.CurrentSession = curSession;
                    }
                    else
                    {
                        isValidSession = false;
                    }
                }
                else
                {
                    isValidSession = false;
                }

                // If session is valid...
                if (isValidSession)
                {
                    // Update the session information in the database,
                    db.Entry(curSession).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    // Remove the cookie and redirect to login.
                    sessionCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Set(sessionCookie);
                    RedirectToSignIn(filterContext, true);
                }
            }
            else
            {
                // Redirect to login.
                RedirectToSignIn(filterContext);
            }

            base.OnActionExecuting(filterContext);
        }

        private void RedirectToSignIn(ActionExecutingContext filterContext, bool timeout = false)
        {
            // Remove invalid cookie and redirect to login.
            filterContext.Result = new RedirectResult($"/Session/SignIn?{(timeout ? "Timeout=true": "")}");

        }

        protected int ItemsPerPage = 5;

        protected int GetTotalPages(int numberOfItems)
        {
            return (int)Math.Ceiling((decimal)numberOfItems / (decimal)ItemsPerPage);
        }

        public class JsonReturn
        {
            // Refreshing the screen on return.
            public bool RefreshScreen { get; set; }

            // Redirects to the given link.
            public string RedirectLink { get; set; } = "";

            // If the redirect link is a redirect to a modal.
            public bool ModalRedirect { get; set; }

            // Set the a redirect link and say it is a modal link.
            public string ModalRedirectLink
            {
                set
                {
                    ModalRedirect = true;
                    RedirectLink = value;
                }
            }

            // Set an error.
            public string ErrorMessage { get; set; } = "";
        }
    }

    public abstract class BaseController<T> : BaseController where T : BaseModel
    {
        protected virtual ListSettings ListSettings { get; } = new ListSettings();
        protected virtual List<FieldDisplay> ListDisplay { get; } = new List<FieldDisplay>();

        public ActionResult List(string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<T> model = new ListViewModel<T>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ListSettings = ListSettings,
                ObjectList = db.GetDBList<T>(),
                FieldsToDisplay = ListDisplay
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView(ListView, model);
            }
            return View(ListView, model);
        }

    }
}