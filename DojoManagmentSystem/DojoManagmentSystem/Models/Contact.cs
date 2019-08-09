using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DojoManagmentSystem.DAL;

namespace DojoManagmentSystem.Models
{
    public class Contact : BaseModel
    {
        //The name of the contact
        [Required]
        public string Name { get; set; }

        [DisplayName("Relationship")]
        public string RelationShip { get; set; }

        [DisplayName("Is Primary")]
        public bool IsPrimary { get; set; }

        public int MemberId { get; set; }

        public virtual Member Member { get; set; }

        #region Relationships
        public virtual User User { get; set; }

        public virtual ICollection<MemberAddress> MemberAddresses { get; set; } = new List<MemberAddress>();

        public virtual ICollection<MemberPhone> MemberPhones { get; set; } = new List<MemberPhone>();

        public virtual ICollection<MemberEmail> MemberEmails { get; set; } = new List<MemberEmail>();

        #endregion

    }
}