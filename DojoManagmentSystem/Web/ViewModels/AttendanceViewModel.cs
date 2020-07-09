using Business.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.ViewModels
{
    public class AttendanceViewModel
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        public bool Present { get; set; }

        public string FullName { get; set; }

        public long MemberId { get; set; }

        public long ClassSessionId { get; set; }

        public virtual ClassSession ClassSession { get; set; }
    }
}