using Business;
using DojoManagmentSystem.Infastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class FieldDisplay
    {
        public FieldDisplay() { }
        public FieldDisplay(string fieldName)
        {
            FieldName = fieldName;
        }
        public string HeaderText { get; set; }
        public string FieldName { get; set; }
        public bool AllowSort { get; set; } = true;
        public bool IsSearchField { get; set; } = true;
        public int? FieldWidth { get; set; } = null;

        public object GetValue(BaseModel obj)
        {
            return obj.GetPropValue(FieldName).Value;
        }

        private Type _fieldType;
        public Type FieldType(BaseModel obj)
        {
            if (_fieldType == null)
            {
                _fieldType = obj.GetPropValue(FieldName).PropertyInfo;
            }
            return _fieldType;
        }


        public PropertyInfo GetProperty(Type type)
        {
            PropertyInfo property = null;
            foreach (String part in FieldName.Split('.'))
            {
                property = type.GetProperty(part);
                if (property == null) { return null; }

                type = property.PropertyType;
            }
            return property;
        }
    }
}