using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business.Models
{
    public class Payment : BaseModel
    {
        [MaxLength(100)]
        public string Description { get; set; }

        [DataType(DataType.Currency)]       
        [Range(1, int.MaxValue, ErrorMessage = "Only positive amounts allowed")]
        [Required]
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [EnumDataType(typeof(PaymentType))]
        [DisplayName("Payment Type")]   
        public PaymentType PaymentType { get; set; }

        #region Relationships
        public virtual Member Member { get; set; }

        public long MemberID { get; set; }
        #endregion
    }
}