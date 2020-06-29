using Business;
using DojoManagmentSystem.Infastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
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
        public bool DisplayInRelationships { get; set; } = true;

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

    public class FieldDisplay<T> : FieldDisplay where T : BaseModel
    {
        public FieldDisplay(Expression<Func<T, object>> property)
        {
            MemberExpression me;
            switch (property.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = property.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = property.Body as MemberExpression;
                    break;
            }

            List<string> paths = new List<string>();

            // Loop down list of properties
            while (me != null)
            {
                string propertyName = me.Member.Name;
                Type propertyType = me.Type;

                paths.Add(propertyName);
                me = me.Expression as MemberExpression;
            }

            // Reverse Field Path to correct dotted path
            paths.Reverse();
            FieldName = string.Join(".", paths);
        }
    }
}