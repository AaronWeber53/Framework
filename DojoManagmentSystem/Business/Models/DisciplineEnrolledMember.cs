using Business.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business.Models
{
    public class DisciplineEnrolledMember : BaseModel
    {
        private DatabaseContext db = new DatabaseContext();

        [DataType(DataType.Currency)]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive costs allowed")]
        [Required]
        public decimal Cost { get; set; }

        [DataType(DataType.Currency)]
        [DisplayName("Still Owes")]
    
        public decimal RemainingCost { get; set; }

        [DisplayName("Membership Length (Months)")]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive lengths allowed")]
        public int MembershipLength { get; set; }

        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; } = DateTime.Now;

        public bool Expired
        {
            get
            {
                if (this.EndDate < DateTime.Now)
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }
        }

        public bool ExpiringSoon
        {
            get
            {
                // If the membership is expiring in the next 14 days...
                if (this.EndDate < DateTime.Now.AddDays(14) && this.EndDate > DateTime.Now)
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }
        }

        public int MonthCount
        {
            get
            {
                var attendanceSheets = db.GetDbSet<AttendanceSheet>().Where(x => x.MemberId == this.MemberId && x.ClassSession.DisciplineId == this.DisciplineId && x.AttendanceDate.Month == DateTime.Now.Month && x.AttendanceDate.Year == DateTime.Now.Year);

                return attendanceSheets.Count();
            }
        }

        public int TotalCount
        {
            get
            {
                var attendanceSheets = db.GetDbSet<AttendanceSheet>().Where(x => x.MemberId == this.MemberId && x.ClassSession.DisciplineId == this.DisciplineId);

                return attendanceSheets.Count();
            }
        }

        public void MakePayment(decimal amount)
        {
            RemainingCost -= amount;
            if (RemainingCost <= 0)
            {
                EndDate = EndDate.AddMonths(MembershipLength);
                if (RemainingCost < 0)
                {
                    decimal remainder = Math.Abs(RemainingCost);
                    RemainingCost = Cost;
                    MakePayment(remainder);
                }
                else
                {
                    RemainingCost = Cost;
                }
            }
        }

        #region Relationships
        public long DisciplineId { get; set; }

        public virtual Discipline Discipline { get; set; }

        public long MemberId { get; set; }

        public virtual Member Member { get; set; }

        #endregion
    }
}