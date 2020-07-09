using Business;
using Business.DAL;
using Business.Infastructure.Enums;
using Business.Infastructure.Exceptions;
using Business.Models;
using Web.Infastructure.Attributes;
using Web.Infastructure.Extensions;
using Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.Routing;

namespace Web
{
    [PageSecurity(SecurityLevel.User)]
    public abstract class BaseController : Controller
    {
        protected DatabaseContext db = new DatabaseContext();

        protected string ListViewPath = "~/Views/Shared/List.cshtml";

        /// <summary>
        /// Whenever a action is executed this function will be called first.
        /// </summary>
        /// <param name="filterContext">Context of the action.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                // Set variables for layout data.
                Session curSession = null;
                HttpCookie sessionCookie = null;
                if (TryGetCookie("SessionGuid", out sessionCookie))
                {
                    // Get the hash value from the cookies.
                    string hash = sessionCookie.Value;

                    // Based on the hash look for the corresponding session.
                    curSession = db.GetDbSet<Session>().Include("User").Include("User.Member").FirstOrDefault(s => s.SessionHash == hash);
                }

                //Check if the current session isn't null and the session isn't expired.
                if (curSession != null)
                {
                    curSession.Expires = !curSession.RememberMe ? DateTime.Now.AddMinutes(10) : DateTime.Now.AddDays(30);
                    ViewBag.UserId = curSession.User?.Member?.Id;
                    ViewBag.FullName = curSession.User?.Member?.FullName;

                    // Set the current session to the found session.
                    if (curSession.AttendanceLock)
                    {
                        // Make sure that the cookie session will not expire while in attendance mode.
                        AddOrUpdateCookie(sessionCookie, new TimeSpan(30, 0, 0, 0));

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
                        TimeSpan timespan = !curSession.RememberMe ? new TimeSpan(0, 10, 0) : new TimeSpan(30, 0, 0, 0);
                        AddOrUpdateCookie(sessionCookie, timespan);

                        ApplicationContext.CurrentApplicationContext.CurrentSession = curSession;
                    }
                }
                else
                {
                    curSession = new Session(null);
                    sessionCookie = GetOrCreateCookie("SessionGuid", curSession.SessionHash);
                }
                ApplicationContext.CurrentApplicationContext.CurrentSession = curSession;

                
                if (filterContext.TryGetAttribute(true, out PageSecurityAttribute attribute) && attribute.SecurityLevel >= SecurityLevel.User)
                {
                    // Check if the SesssionGuid cookie is set.
                    bool test = attribute.CheckUserHasPermission();

                    // If session is valid...
                    if (!test)
                    {
                        AccessForbidden(filterContext);
                    }
                }

                if (curSession != null)
                {
                    // Update the session information in the database,
                    //db.Entry(curSession).State = EntityState.Modified;
                    curSession.Save(db);
                    sessionCookie.Value = curSession.SessionHash;
                    AddOrUpdateCookie(sessionCookie);
                }
                else if (sessionCookie != null)
                {
                    // Remove the cookie and redirect to login.
                    AddOrUpdateCookie(sessionCookie, new TimeSpan(-1, 0, 0, 0));
                    //RedirectToSignIn(filterContext, true);
                }
            }
            base.OnActionExecuting(filterContext);
        }

        protected HttpCookie GetOrCreateCookie(string name, string hash, TimeSpan? expiration = null)
        {
            if (!TryGetCookie(name, out HttpCookie cookie))
            {
                cookie = new HttpCookie(name, hash);
            }
            return cookie;
        }

        protected void CreateOrUpdateCookie(string name, string hash, TimeSpan? expiration = null)
        {
            if (TryGetCookie(name, out HttpCookie cookie))
            {
                cookie = new HttpCookie(name, hash);
            }

            AddOrUpdateCookie(cookie, expiration);
        }

        protected void AddOrUpdateCookie(HttpCookie cookie, TimeSpan? expiration = null)
        {
            if (!string.IsNullOrEmpty(cookie.Value))
            {
                if (expiration == null)
                {
                    expiration = new TimeSpan(1, 10, 0);
                }
                cookie.Expires = DateTime.Now.Add(expiration.Value);

                if (Request.Cookies.AllKeys.Contains(cookie.Name))
                {
                    Response.Cookies.Set(cookie);
                }
                else
                {
                    Response.Cookies.Add(cookie);
                }

            }
        }

        protected bool TryGetCookie(string name, out HttpCookie cookie)
        {
            cookie = Request.Cookies[name];
            return cookie != null && !string.IsNullOrEmpty(cookie.Value);
        }

        private void RedirectToSignIn(ActionExecutingContext filterContext, bool timeout = false)
        {
            // Remove invalid cookie and redirect to login.
            filterContext.Result = new RedirectResult($"/Session/SignIn?{(timeout ? "Timeout=true" : "")}");

        }

        private void AccessForbidden(ActionExecutingContext filterContext)
        {
            if (Request.IsAjaxRequest() || ControllerContext.IsChildAction)
            {
            }
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult RedirectToAccessForbidden()
        {
            if (Request.IsAjaxRequest() || ControllerContext.IsChildAction)
            {
                ViewBag.Layout = "_layout";
            }

            return View("~/Views/Shared/AccessForbidden.cshtml", "");
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
        protected virtual List<Expression<Func<T, object>>> EditRelationships { get; } = new List<Expression<Func<T, object>>>();

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult Details(long? id)
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
            return View(obj);
        }

        [PageSecurity(SecurityLevel.User)]
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
                T obj = GetObj(id);
                if (obj == null)
                {
                    return HttpNotFound();
                }

                if (TryUpdateModel(obj))
                {
                    obj.Save(db);
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

        protected void AddErrorMessage(string fieldName, string message)
        {
            ModelState.AddModelError(fieldName, message);
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
            IQueryable<T> list = db.GetDbSet<T>();
            list = IncludeRelationshipsInSearch(list);
            return list.FirstOrDefault (o => o.Id == id);
        }

        private IQueryable<T> IncludeRelationshipsInSearch(IQueryable<T> query)
        {
            foreach (var s in EditRelationships)
            {
                query = query.Include(s);
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