using DojoManagmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class QuickAttendanceViewModel
    {
        public List<Discipline> Disciplines { get; set; }

        public List<Member> Members { get; set; }
    }
}