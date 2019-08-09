using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DojoManagmentSystem.DAL;

namespace DojoManagmentSystem.Models
{
    public class Discipline : BaseModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<ClassSession> ClassSessions { get; set; }

        public virtual ICollection<DisciplineEnrolledMember> EnrolledMembers { get; set; }

        public override void Delete(DojoManagmentContext db)
        {
            ClassSessions.ToList().ForEach(a => a.Delete(db));
            EnrolledMembers.ToList().ForEach(a => a.IsArchived = true);
            base.Delete(db);
        }
    }
}