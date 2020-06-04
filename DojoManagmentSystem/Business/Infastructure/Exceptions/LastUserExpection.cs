using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business.Infastructure.Exceptions
{
    public class LastUserExpection : Exception
    {
        public LastUserExpection(string message) : base(message) { }
    }
}