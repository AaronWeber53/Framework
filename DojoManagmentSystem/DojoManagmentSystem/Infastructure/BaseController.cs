using Business;
using Business.DAL;
using Business.Infastructure.Exceptions;
using Business.Models;
using DojoManagmentSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace DojoManagmentSystem
{
    public abstract class BaseController : Controller 
    {
        protected DojoManagmentContext db = new DojoManagmentContext();

        protected string ListViewPath = "~/Views/Shared/List.cshtml";

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
                Session curSession = db.GetDbSet<Session>().Include("User").Include("User.Member").FirstOrDefault(s => s.SessionHash == hash);

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
        protected virtual List<string> EditRelationships { get; } = new List<string>();

        public virtual ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.IsValid = false;
            T obj = GetObj(id);
            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("Edit", obj);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditValidation(long? id)
        {
            using (db)
            {
                bool isValid = false;
                T obj = db.GetDbSet<T>().FirstOrDefault(o => o.Id == id);
                if(obj == null)
                {
                    return HttpNotFound();
                }

                if (TryUpdateModel(obj))
                {
                    db.SaveChanges();
                    isValid = true;
                }
                SetAdditionalEditValues(obj);
                ViewBag.IsValid = isValid;
                return PartialView("Edit", obj);
            }
        }

        protected virtual void SetAdditionalEditValues(T obj)
        {

        }


        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            T obj = GetObj(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView(obj);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteConfirmed(long id)
        {
            try
            {
                T obj = GetObj(id);
                obj.Delete(db);
                return Json(new JsonReturn { RefreshScreen = true });
            }
            catch (LastUserExpection ex)
            {
                return Json(new JsonReturn { ErrorMessage = ex.Message });
            }
        }
        
        protected T GetObj(long? id)
        {
            IQueryable<T> list = db.GetDbSet<T>().Where(a => a.Id == id);
            list = IncludeRelationshipsInSearch(list);
            return list.FirstOrDefault();
        }

        private IQueryable<T> IncludeRelationshipsInSearch(IQueryable<T> query)
        {
            foreach(string s in EditRelationships)
            {
                query.Include(s);
            }
            return query;
        }

        protected ListViewModel<TRel> RelationshipList<TRel>(long? id, string filter, string sortOrder, string searchString, int page = 1) where TRel : BaseModel
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            BaseController<TRel> baseController = asm.GetTypes()
                .Where(type => typeof(BaseController<TRel>).IsAssignableFrom(type))
                .Select(t => (BaseController<TRel>)Activator.CreateInstance(t)).FirstOrDefault();

            ListViewModel<TRel> model = new ListViewModel<TRel>()
            {
                RelationID = id,
                Action = this.ControllerContext.RouteData.Values["action"].ToString(),
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ListSettings = baseController.ListSettings,
                ObjectList = db.GetDbSet<TRel>(),
                FieldsToDisplay = baseController.ListDisplay.Where(f => f.DisplayInRelationships)
            };

            return model;
        }

        public ActionResult List(string filter = null, string sortOrder = null, string searchString = null, int page = 1)
        {
            ListViewModel<T> model = new ListViewModel<T>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                FilterField = filter,
                ListSettings = ListSettings,
                ObjectList = db.GetDbSet<T>(),
                FieldsToDisplay = ListDisplay
            };

            return ListView(model);
        }

        protected ActionResult ListView(ListViewModel model)
        {
            if (Request.IsAjaxRequest() || ControllerContext.IsChildAction)
            {
                return PartialView(ListViewPath, model);
            }
            return View(ListViewPath, model);
        }
    }
}