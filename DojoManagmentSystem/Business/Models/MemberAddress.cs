using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Business.DAL;

namespace Business.Models
{
    public class MemberAddress : BaseModel
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        [DisplayName("Zip Code")]
        [DataType(DataType.PostalCode)]
        public int ZipCode { get; set; }

        [DisplayName("Address")]
        public string FullAddress
        {
            get
            {
                return $"{Street} {City}, {State} {ZipCode}";
            }
        }

        public int ContactID { get; set; }

        public virtual Contact Contact { get; set; }
    }
}