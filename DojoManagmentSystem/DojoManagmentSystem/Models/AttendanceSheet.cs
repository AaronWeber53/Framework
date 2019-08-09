using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.Models
{
    public class AttendanceSheet : BaseModel
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Attendance Date")]
        public DateTime AttendanceDate { get; set; }

        public int ClassSessionId { get; set; }

        public virtual ClassSession ClassSession { get; set; }

        public int MemberId { get; set; }

        public virtual Member Member { get; set; }
    }
}