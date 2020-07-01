using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Web.Infastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static Field GetPropValue(this object obj, String name)
        {
            PropertyInfo property = null;
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { break; }

                Type type = obj.GetType();
                property = type.GetProperty(part);
                if (property == null) { break; }

                obj = property.GetValue(obj, null);
            }
            return new Field() { PropertyInfo = property.PropertyType, Value = obj };
        }
    }
}