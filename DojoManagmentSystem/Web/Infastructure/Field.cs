﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Web.Infastructure
{
    public class Field
    {
        public Type PropertyInfo { get; set; }

        public object Value { get; set; }
    }
}