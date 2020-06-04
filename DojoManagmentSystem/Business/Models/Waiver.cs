using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business.Models
{
    public class Waiver : BaseModel
    {
        public string Note { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Signed")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime DateSigned { get; set; }

        [DisplayName("Is Signed")]
    
        public bool IsSigned { get; set; }

        #region Relationships 
        public virtual Member Member { get; set; }

        public int MemberId { get; set; }
        #endregion
    }
}