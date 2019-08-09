using DojoManagmentSystem.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.Models
{
    public class Member : BaseModel
    {
        [DisplayName("First Name")]
        [Required]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Required]
        public string LastName { get; set; }

        [DisplayName("Is Instructor")]
        public bool IsInstructor { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        [DisplayName("Full Name")]  
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        [DisplayName("Has User")]
        public bool HasUser
        {
            get
            {
                return User != null;
            }
        }

        #region Relationships
        public virtual User User { get; set; }

        public virtual ICollection<Contact> Contact { get; set; } = new List<Contact>();

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public virtual ICollection<Waiver> Waivers { get; set; } = new List<Waiver>();

        public virtual ICollection<DisciplineEnrolledMember> DisciplineEnrolledMembers { get; set; } = new List<DisciplineEnrolledMember>();

        public virtual ICollection<AttendanceSheet> AttendanceSheets { get; set; } = new List<AttendanceSheet>();

        #endregion

        /// <summary>
        /// Overriding the delete to make sure that all of the attached items are deleted as well from the member.
        /// </summary>
        /// <param name="db">Database being used.</param>
        public override void Delete(DojoManagmentContext db)
        {
            DisciplineEnrolledMembers.ToList().ForEach(a => a.IsArchived = true);
            Payments.ToList().ForEach(a => a.IsArchived = true);
            Waivers.ToList().ForEach(a => a.IsArchived = true);
            Contact.ToList().ForEach(a => a.IsArchived = true);
            AttendanceSheets.ToList().ForEach(a => a.IsArchived = true);
            User?.Delete(db);
            base.Delete(db);
        }
    }
}