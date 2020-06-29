using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using Business.Models;
using DojoManagmentSystem.ViewModels;
using DojoManagmentSystem.Infastructure.Extensions;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Core.Objects;
using DojoManagmentSystem.Infastructure;
using Business;

namespace DojoManagmentSystem.Controllers
{
    public class AttendanceSheetController : BaseController<AttendanceSheet>
    {
        // GET: AttendanceSheet
        public ActionResult Index()
        {
            var attendanceSheets = db.GetDbSet<AttendanceSheet>().Include(a => a.ClassSession);
            return View(attendanceSheets.ToList());
        }

        public ActionResult List(int id, string sortOrder = null, string searchString = null, int page = 1)
        { 
            ViewBag.SessionId = id;
            ViewBag.DateSortParm = !String.IsNullOrEmpty(sortOrder) && sortOrder == "date_desc" ? "date_asc" : "date_desc";

            // Gets the members from the database
            var sheets = from mem in db.GetDbSet<AttendanceSheet>()
                              where !mem.IsArchived && mem.ClassSessionId == id
                              group mem by DbFunctions.TruncateTime(mem.AttendanceDate)
                              into groups
                              select groups.FirstOrDefault();

            // Order the  members depending on what parameter was passed in.
            switch (sortOrder)
            {
                case "date_desc":
                    sheets = sheets.OrderByDescending(m => m.AttendanceDate);
                    break;
                case "date_asc":
                    sheets = sheets.OrderBy(m => m.AttendanceDate);
                    break;

                default:
                    sheets = sheets.OrderBy(m => m.AttendanceDate);
                    break;
            }

            ListViewModel<AttendanceSheet> model = new ListViewModel<AttendanceSheet>()
            {
                CurrentPage = page,
                CurrentSort = sortOrder,
                CurrentSearch = searchString,
                ObjectList = sheets
            };

            return PartialView("List", model);

        }

        // GET: AttendanceSheet/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendanceSheet attendanceSheet = db.GetDbSet<AttendanceSheet>().Include("ClassSession").Include("ClassSession.Discipline").FirstOrDefault(a => a.Id == id);
            if (attendanceSheet == null)
            {
                return HttpNotFound();
            }

            ViewBag.Name = attendanceSheet.ClassSession.Discipline.Name;
            ViewBag.DayOfWeek = attendanceSheet.ClassSession.DayOfWeek;
            ViewBag.Date = attendanceSheet.AttendanceDate;
            ViewBag.Time = attendanceSheet.ClassSession.StartTime.ToString("h:MM");
            ViewBag.ClassSession = attendanceSheet.ClassSessionId;
            return View(attendanceSheet);
        }

        #region QuickAttendance
        public ActionResult QuickAttendance()
        {
            // Check if page is locked.
            ViewBag.IsLocked = ApplicationContext.CurrentApplicationContext.CurrentSession.AttendanceLock;
            QuickAttendanceViewModel model = new QuickAttendanceViewModel();

            int dow = (int)DateTime.Now.DayOfWeek;

            // Get list of displines today.
             var disciplines = from dis in db.GetDbSet<Discipline>()
                              where dis.ClassSessions.Any(dt => (int)dt.DayOfWeek == dow)
                              group dis by dis.Id
                              into groups
                              select groups.FirstOrDefault();
            model.Disciplines = disciplines.ToList();

            // Get list of members
            model.Members = db.GetDbSet<Member>().Where(d => !d.IsArchived).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult QuickAttendanceRecord(int memberId, int disciplineId)
        {
            ViewBag.IsLocked = ApplicationContext.CurrentApplicationContext.CurrentSession.AttendanceLock;

            // Get selected member
            Member member = db.GetDbSet<Member>().Where(m => m.Id == memberId).FirstOrDefault();

            // Find the class session going on right now for given discipline.
            DateTime dtNow = DateTime.Now;
            int dow = (int)dtNow.DayOfWeek;
            var classSessionQuery = from cs in db.GetDbSet<ClassSession>().Include("Discipline")
                                    where cs.DisciplineId == disciplineId &&
                               cs.StartTime <= dtNow && cs.EndTime >= dtNow
                               && (int)cs.DayOfWeek == dow
                                    select cs;

            ClassSession classSession = classSessionQuery.FirstOrDefault();

            // If class session is null...
            if (classSession == null)
            {
                // Find the closest class session to now from the given disipline.
                classSessionQuery = from cs in db.GetDbSet<ClassSession>().Include("Discipline")
                                    where cs.DisciplineId == disciplineId
                                    && (int)cs.DayOfWeek == dow
                                    orderby EntityFunctions.DiffSeconds(cs.StartTime, dtNow), 
                                    EntityFunctions.DiffSeconds(cs.EndTime, dtNow)
                                    select cs;

                classSession = classSessionQuery.FirstOrDefault();
            }

            // If class session or member is null..
            if (classSession == null || member == null)
            {
                // Return an error.
                return Json(new { Error = true });
            }
            else
            {
                // Check if member is already checked in.
                bool alreadyCheckedIn = db.GetDbSet<AttendanceSheet>().Any(a => a.MemberId == member.Id
                && a.ClassSessionId == classSession.Id
                && DbFunctions.TruncateTime(a.AttendanceDate) == DbFunctions.TruncateTime(dtNow));   

                // If the member is not currently checked in...
                if (!alreadyCheckedIn)
                {
                    // Create attendance record for member.
                    AttendanceSheet sheet = new AttendanceSheet()
                    {
                        AttendanceDate = dtNow,
                        MemberId = member.Id,
                        ClassSessionId = classSession.Id
                    };
                    db.GetDbSet<AttendanceSheet>().Add(sheet);
                    db.SaveChanges();
                }

                // Return that member has been checked in and class session checked in for.
                return Json(new { Error = false, checkedIn = alreadyCheckedIn, classSession = $"{classSession.Discipline.Name} {classSession.StartTime.ToString("h:mm tt")} - {classSession.EndTime.ToString("h:mm tt")}", member = member.FullName });
            }


        }
        #endregion

        #region Create
        // GET: AttendanceSheet/Create
        public ActionResult Create(int id)
        {
            string DropIn = Request.Form["DropInList"];
            ClassSession classSession = db.GetDbSet<ClassSession>().Include("Discipline").Include("Discipline.EnrolledMembers").Include("Discipline.EnrolledMembers.Member").FirstOrDefault(c => c.Id == id);
            AttendanceSheet attendanceSheet = new AttendanceSheet { ClassSession = classSession };

            ViewBag.OutputDropDown = this.OutputFilter(id);

            // Get closest day of the week and set as default.
            attendanceSheet.AttendanceDate = DateTime.Now.StartOfWeek(classSession?.DayOfWeek ?? DateTime.Now.DayOfWeek);
            ViewBag.Discipline = classSession.Discipline.Name;
            ViewBag.DayOfWeek = classSession.DayOfWeek;
            ViewBag.StartTime = classSession.StartTime.ToString("h:mm tt");
            ViewBag.ClassSessionId = id;
            return PartialView(attendanceSheet);
        }

        // POST: AttendanceSheet/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AttendanceDate,ClassSessionId")] AttendanceSheet attendanceSheet)
        {
            string DropIn = Request.Form["DropInList"];
            ClassSession classSession = db.GetDbSet<ClassSession>().Include("Discipline").Include("Discipline.EnrolledMembers").Include("Discipline.EnrolledMembers.Member").FirstOrDefault(c => c.Id == attendanceSheet.ClassSessionId);

            attendanceSheet.ClassSession = classSession;
            string[] ids = Request.Form.GetValues("present");
            ViewBag.ClassSessionId = attendanceSheet.ClassSessionId;
            ViewBag.Discipline = classSession.Discipline.Name;
            ViewBag.DayOfWeek = classSession.DayOfWeek;
            ViewBag.StartTime = classSession.StartTime.ToString("h:mm tt");
            if (attendanceSheet.AttendanceDate.DayOfWeek == classSession.DayOfWeek)
            {
                if (attendanceSheet.ClassSession == null)
                {
                    return PartialView("Create", attendanceSheet);
                }
                if (ModelState.IsValid)
                {
                    List<AttendanceSheet> attendance = new List<AttendanceSheet>();
                    if (ids != null)
                    {
                        foreach (string id in ids)
                        {
                            AttendanceSheet attendee = new AttendanceSheet { ClassSessionId = classSession.Id, AttendanceDate = attendanceSheet.AttendanceDate,  MemberId = int.Parse(id) };
                            attendance.Add(attendee);
                        }
                    }
                    db.GetDbSet<AttendanceSheet>().AddRange(attendance);
                    db.SaveChanges();

                    return Json(new JsonReturn { RefreshScreen = true });
                }
            }
            attendanceSheet.AttendanceDate = DateTime.Now.StartOfWeek(classSession.DayOfWeek);

            ViewBag.Invalid = true;

            List<string> presentIds = ids.ToList();
            ViewBag.ids = presentIds;
            List<Member> dropIns = this.GetDropIns(ids, attendanceSheet.ClassSessionId);
            List<Member> output = this.OutputFilter(attendanceSheet.ClassSessionId);

            ViewBag.OutputDropDown = output.Except(dropIns);
            ViewBag.DropIns = dropIns;

            return PartialView(attendanceSheet);
        }
        #endregion

        #region Edit
        // GET: AttendanceSheet/Edit/5
        public override ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendanceSheet attendanceSheet = db.GetDbSet<AttendanceSheet>().Include("ClassSession").Include("ClassSession.Discipline").FirstOrDefault(a => a.Id == id);
            if (attendanceSheet == null)
            {
                return HttpNotFound();
            }
            List<AttendanceViewModel> modelList = GetAttendance(attendanceSheet);
            List<AttendanceSheet> sheets = db.GetDbSet<AttendanceSheet>().Where(x => !x.IsArchived).ToList();
            foreach(AttendanceSheet A in sheets.Where(a => a.ClassSessionId == attendanceSheet.ClassSessionId && a.AttendanceDate == attendanceSheet.AttendanceDate))
            {
                bool dropIn = true;

                foreach(AttendanceViewModel B in modelList)
                {
                    if(A.MemberId == B.MemberId)
                    {
                        dropIn = false;
                    }
                }

                if(dropIn == true){
                    modelList.Add(new AttendanceViewModel { FullName = A.Member.FullName, ClassSessionId = attendanceSheet.ClassSessionId, MemberId = A.MemberId, Present = true});
                }
            }

            ViewBag.Name = attendanceSheet.ClassSession.Discipline.Name;
            ViewBag.DayOfWeek = attendanceSheet.ClassSession.DayOfWeek;
            ViewBag.AttendanceDate = attendanceSheet.AttendanceDate;
            ViewBag.DisplayDate = attendanceSheet.AttendanceDate.ToString("MM/dd/yyyy");
            ViewBag.Time = attendanceSheet.ClassSession.StartTime.ToString("h:MM");
            ViewBag.ClassSessionId = attendanceSheet.ClassSessionId;
            ViewBag.Date = attendanceSheet.AttendanceDate;
            ViewBag.IsValid = false;
            return PartialView(modelList);
        }

        // POST: AttendanceSheet/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AttendanceDate,MemberId,Present,ClassSessionId")] AttendanceSheet attendanceSheet)
        {
            ClassSession classSession = db.GetDbSet<ClassSession>().Include("Discipline").FirstOrDefault(c => c.Id == attendanceSheet.ClassSessionId);
            attendanceSheet.ClassSession = classSession;
            List<AttendanceViewModel> modelList = GetAttendance(attendanceSheet);

            ViewBag.Name = attendanceSheet.ClassSession.Discipline.Name;
            ViewBag.DayOfWeek = attendanceSheet.ClassSession.DayOfWeek;
            ViewBag.AttendanceDate = attendanceSheet.AttendanceDate;
            ViewBag.DisplayDate = attendanceSheet.AttendanceDate.ToString("MM/dd/yyyy");
            ViewBag.Time = attendanceSheet.ClassSession.StartTime.ToString("h:MM");
            ViewBag.ClassSessionId = attendanceSheet.ClassSessionId;

            if (ModelState.IsValid)
            {
                // Gets the list of selected members from the form
                List<string> ids = Request.Form.GetValues("item.Present").ToList();

                // Creates a list of members that where not previously marked present
                List<AttendanceViewModel> newAttendees = modelList.Where(a => a.Present == false).ToList();
                List<AttendanceSheet> addAttendance = new List<AttendanceSheet>();

                //Creates a list of members that have been selected
                newAttendees.ForEach(a => { if (ids.IndexOf(a.MemberId.ToString()) >= 0) addAttendance.Add(new AttendanceSheet
                {
                    MemberId = a.MemberId,
                    ClassSessionId = a.ClassSessionId,
                    AttendanceDate = attendanceSheet.AttendanceDate
                });});

                // Creates a list of members that where already marked present for the selected session
                List<AttendanceSheet> oldAttendance = (from att in db.GetDbSet<AttendanceSheet>()
                                                       where att.ClassSessionId == attendanceSheet.ClassSessionId
                                                       && DbFunctions.TruncateTime(att.AttendanceDate) == DbFunctions.TruncateTime(attendanceSheet.AttendanceDate)
                                                       select att).ToList();
                List<AttendanceSheet> removeAttendance = new List<AttendanceSheet>();

                // Creates a list of members that have been unselected
                oldAttendance.ForEach(a => { if (ids.IndexOf(a.MemberId.ToString()) < 0) removeAttendance.Add(a); });
                
                // Updates database with selected and unselected members
                db.GetDbSet<AttendanceSheet>().AddRange(addAttendance);
                db.GetDbSet<AttendanceSheet>().RemoveRange(removeAttendance);
                db.SaveChanges();

                modelList = GetAttendance(attendanceSheet);
                List<AttendanceSheet> sheets = db.GetDbSet<AttendanceSheet>().Where(x => !x.IsArchived).ToList();
                foreach (AttendanceSheet A in sheets.Where(a => a.ClassSessionId == attendanceSheet.ClassSessionId && a.AttendanceDate == attendanceSheet.AttendanceDate))
                {
                    bool dropIn = true;

                    foreach (AttendanceViewModel B in modelList)
                    {
                        if (A.MemberId == B.MemberId)
                        {
                            dropIn = false;
                        }
                    }

                    if (dropIn == true)
                    {
                        modelList.Add(new AttendanceViewModel { FullName = A.Member.FullName, ClassSessionId = attendanceSheet.ClassSessionId, MemberId = A.MemberId, Present = true});
                    }
                }
                ViewBag.IsValid = true;
                return PartialView("Edit", modelList);
            }
            ViewBag.ClassSessionId = attendanceSheet.ClassSessionId;
            ViewBag.IsValid = false;
            return PartialView(modelList);
        }
        #endregion

        #region DropIn
        public ActionResult DropIn(int id, DateTime date)
        {
            ViewBag.ClassSessionId = id;

            ViewBag.Date = date;

            ClassSession classSession = db.GetDbSet<ClassSession>().Find(id);

            var members = (from mem in db.GetDbSet<Member>().Include("DisciplineEnrolledMembers")
                                 where !mem.IsArchived && 
                                 !mem.DisciplineEnrolledMembers.Any(d => d.DisciplineId == classSession.DisciplineId)
                                 select mem).ToList();

            // Get list of currentSheets
            var curSheets = (from sheet in db.GetDbSet<AttendanceSheet>()
                            where sheet.ClassSessionId == id &&
                            DbFunctions.TruncateTime(sheet.AttendanceDate) == DbFunctions.TruncateTime(date)
                            select new
                            {
                                sheet.MemberId
                            }).ToList();

            members.RemoveAll(a => curSheets.Any(s => s.MemberId == a.Id));

            return PartialView(members);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DropIn()
        {
            string ClassSessionId = Request.Form["ClassSessionId"];
            string DropInId = Request.Form["Dropdown"];
            string date = Request.Form["Date"];
            DateTime attendanceDate = DateTime.Parse(date);

            AttendanceSheet newAttendanceSheet = new AttendanceSheet { ClassSessionId = int.Parse(ClassSessionId), MemberId = int.Parse(DropInId), Member= db.GetDbSet<Member>().Find(int.Parse(DropInId)), AttendanceDate = attendanceDate };
                       
            db.GetDbSet<AttendanceSheet>().Add(newAttendanceSheet);
            db.SaveChanges();

            return Json(new JsonReturn { RefreshScreen = true });
        }
        #endregion

        // GET: AttendanceSheet/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendanceSheet attendanceSheet = db.GetDbSet<AttendanceSheet>().Find(id);
            if (attendanceSheet == null)
            {
                return HttpNotFound();
            }
            return PartialView(attendanceSheet);
        }

        // POST: AttendanceSheet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AttendanceSheet attendanceSheet = db.GetDbSet<AttendanceSheet>().Find(id);

            List<AttendanceSheet> attendanceSheets = (from att in db.GetDbSet<AttendanceSheet>()
                                    where att.ClassSessionId == attendanceSheet.ClassSessionId
                                    && DbFunctions.TruncateTime(att.AttendanceDate) == DbFunctions.TruncateTime(attendanceSheet.AttendanceDate)
                                    select att).ToList();

            db.GetDbSet<AttendanceSheet>().RemoveRange(attendanceSheets);
            db.SaveChanges();
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

        private List<AttendanceViewModel> GetAttendance(AttendanceSheet attendanceSheet)
        {
            // Gets a list of members from the database for the selected discipline
            var mems = (from mem in db.GetDbSet<Member>().Include("DisciplineEnrolledMembers").Include("AttendanceSheets")
                        where (!mem.IsArchived &&
                        mem.DisciplineEnrolledMembers.Any(d => d.DisciplineId == attendanceSheet.ClassSession.DisciplineId))
                        || (mem.AttendanceSheets.Any(a => 
                        a.ClassSessionId == attendanceSheet.ClassSessionId
                        && DbFunctions.TruncateTime(a.AttendanceDate) == DbFunctions.TruncateTime(attendanceSheet.AttendanceDate)))
                           select new
                           {
                               mem.Id,
                               Member = mem
                           }).ToList();

            // Gets a list of attendance records from the database for the selected discipline
            var attendanceSheets = (from att in db.GetDbSet<AttendanceSheet>()
                                    where !att.IsArchived &&
                                    att.ClassSessionId == attendanceSheet.ClassSessionId
                                    && DbFunctions.TruncateTime(att.AttendanceDate) == DbFunctions.TruncateTime(attendanceSheet.AttendanceDate)
                                    select new
                                    {
                                        att.MemberId
                                    });

            List<AttendanceViewModel> modelList = new List<AttendanceViewModel>();

            // Creates a list of view models for the currently enrolled members
            mems.ForEach(m => modelList.Add(new AttendanceViewModel
            {
                ClassSessionId = attendanceSheet.ClassSessionId,
                MemberId = m.Id,
                FullName = m.Member.FullName
            }));

            // Marks members as present if an attendance record exists for that member
            modelList.ForEach(m => { if (attendanceSheets.Where(a => a.MemberId == m.MemberId).Count() > 0) m.Present = true; });
             
            return modelList;
        }

        private List<Member> OutputFilter(long id)
        {
            ClassSession classSession = db.GetDbSet<ClassSession>().Find(id);
            var membersList = (from mem in db.GetDbSet<Member>().Include("DisciplineEnrolledMembers")
                               where !mem.IsArchived &&
                               !mem.DisciplineEnrolledMembers.Any(d => d.DisciplineId == classSession.DisciplineId)
                               select mem);
            return membersList.ToList();
        }

        private List<Member> GetDropIns(string[] ids, long id)
        {
            ClassSession classSession = db.GetDbSet<ClassSession>().Find(id);
            var membersList = (from mem in db.GetDbSet<Member>().Include("DisciplineEnrolledMembers")
                               where !mem.IsArchived &&
                               !mem.DisciplineEnrolledMembers.Any(d => d.DisciplineId == classSession.DisciplineId) &&
                               ids.Any(i => i == mem.Id.ToString())
                               select mem);
            return membersList.ToList();
        }
    }
}
