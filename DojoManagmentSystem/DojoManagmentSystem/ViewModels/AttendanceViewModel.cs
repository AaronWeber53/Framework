using Business.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class AttendanceViewModel
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        public bool Present { get; set; }

        public string FullName { get; set; }

        public int MemberId { get; set; }

        public int ClassSessionId { get; set; }

        public virtual ClassSession ClassSession { get; set; }
    }
}