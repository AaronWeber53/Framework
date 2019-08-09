using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DojoManagmentSystem.DAL;

namespace DojoManagmentSystem.Models
{
    public class ClassSession : BaseModel
    {
        [DataType(DataType.Time)]
        [DisplayName("Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime StartTime { get; set; }        

        [DataType(DataType.Time)]
        [DisplayName("End Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime EndTime { get; set; }

        [DisplayName("Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }

        public int DisciplineId { get; set; }

        public virtual Discipline Discipline { get; set; }

        public virtual ICollection<AttendanceSheet> AttendanceSheets { get; set; }

        public override void Delete(DojoManagmentContext db)
        {
            AttendanceSheets.ToList().ForEach(a => a.IsArchived = true);
            base.Delete(db);
        }
    }
}