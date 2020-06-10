using System;
using System.Collections.Generic;
using Business.Models;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class HomeViewModel
    {
        public int Id { get; set; }

        public virtual ICollection<DisciplineEnrolledMember> DisciplineEnrolledMembers { get; set; }

        public virtual ICollection<ClassSession> ClassSessions { get; set; }
    }
}