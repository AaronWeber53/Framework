using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.ViewModels
{
    public class ClassSessionViewModel
    {
        public ClassSession ClassSession { get; set; }

        public virtual List<AttendanceSheet> AttendanceSheets { get; set; }
    }
}