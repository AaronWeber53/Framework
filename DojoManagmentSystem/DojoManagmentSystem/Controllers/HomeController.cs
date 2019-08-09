using DojoManagmentSystem.DAL;
using DojoManagmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class HomeController : BaseController
    {
        private DojoManagmentContext db = new DojoManagmentContext();

        public ActionResult Index()
        {
            // Gets all of the discipline enrolled members from the database and puts them in a list.
            var memberList = (from mem in db.DisciplineEnrolledMembers
                              where !mem.IsArchived
                              select new
                              {
                                  mem.Member,
                                  mem.Discipline,
                                  mem.Discipline.Name,
                                  mem.Member.FirstName,
                                  mem.Member.LastName,
                                  mem.EndDate,
                                  mem.RemainingCost }).ToList();

            DayOfWeek today = DateTime.Now.DayOfWeek;

            // Gets all of the class sessions from the database and puts them in a list.
            var classSession = (from cls in db.ClassSessions
                                where cls.DayOfWeek == today 
                                && !cls.IsArchived
                                select new
                                {
                                    cls.Id,
                                    cls.Discipline,
                                    cls.Discipline.Name,
                                    cls.DayOfWeek,
                                    cls.StartTime
                                }).ToList();

            // Remove all memberships with an end date prior to the last 2 months.
            memberList.RemoveAll(x => x.EndDate < DateTime.Now.AddMonths(-2));

            // Sorts the members by their end date
            memberList.Sort((x, y) => DateTime.Compare(x.EndDate, y.EndDate));
          
            // Selects the top 5 members from the list.
            if (memberList.Count > 5)
            {
                memberList.RemoveRange(5, memberList.Count - 5);
            }

            // Select top 5 class sessions.
            if (classSession.Count > 5)
            {
                classSession.RemoveRange(5, classSession.Count - 5);
            }

            HomeViewModel viewModel = new HomeViewModel()
            {
                DisciplineEnrolledMembers = new List<DisciplineEnrolledMember>(),
                ClassSessions = new List<ClassSession>()
            };

            // Creates a model and sets lists properties to the model properties.
            foreach (var item in memberList)
            {
                DisciplineEnrolledMember mem = new DisciplineEnrolledMember();

                mem.Member = item.Member;
                mem.Discipline = item.Discipline;
                mem.Discipline.Name = item.Name;
                mem.Member.FirstName = item.FirstName;
                mem.Member.LastName = item.LastName;
                mem.EndDate = item.EndDate;
                mem.RemainingCost = item.RemainingCost;
                viewModel.DisciplineEnrolledMembers.Add(mem);
            }

            // Creates a model and sets lists properties to the class properties.
            foreach (var item in classSession)
            {
                ClassSession session = new ClassSession();
                session.Id = item.Id;
                session.Discipline = item.Discipline;
                session.Discipline.Name = item.Name;
                session.DayOfWeek = item.DayOfWeek;
                session.StartTime = item.StartTime;
                viewModel.ClassSessions.Add(session);
            }

            ViewBag.ClassName = null;
            return View(viewModel);
        }
            
    }
 }
