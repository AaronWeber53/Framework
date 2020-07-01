using Web;
using Web.Infastructure.Attributes;
using Web.Infastructure.Extensions;
using Microsoft.Ajax.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Security.Policy;
using System.Collections.Generic;
//using System.Web.Mvc.Html.LabelExtensions;

namespace Web.Infastructure.Extensions
{
    public static class HtmlExtensions
    {
        public static bool IsCurrentAction(this HtmlHelper helper, string actionName, string controllerName)
        {
            string currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
            string currentActionName = (string)helper.ViewContext.RouteData.Values["action"];

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        public static bool CheckHasSecurityPermission(this HtmlHelper helper, string controllerName, string actionName)
        {
            string controller = controllerName + "Controller";
            Assembly asm = typeof(BaseController).Assembly;
            Type typeTest = asm.GetTypes().FirstOrDefault(t => t.Name.ToLower() == controller.ToLower());
            if(typeTest.TryGetAttribute(actionName, true, out PageSecurityAttribute attribute) && attribute.SecurityLevel >= Business.Infastructure.Enums.SecurityLevel.Normal)
            {
                return attribute.CheckUserHasPermission();
            }
            return true;
        }

        public static IHtmlString GenerateLinkWithSecurity(this HtmlHelper helper, string controllerName, string actionName, string linkText, object routeValues = null, object htmlAttributes = null)
        {
            if(helper.CheckHasSecurityPermission(controllerName, actionName))
            {
                return helper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
                //tag = new TagBuilder("a");
                //tag.SetInnerText(linkText);
                //tag.Attributes.Add("href", $"/{controllerName}/{actionName}");
                //if (htmlAttributes != null)
                //{
                //    var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                //    tag.MergeAttributes(attributes);
                //}
            }
            return new HtmlString("");
        }


        public static IHtmlString ActionWithSecurity(this HtmlHelper helper, string controllerName, string actionName, object routeValues = null)
        {
            if(helper.CheckHasSecurityPermission(controllerName, actionName))
            {
                return helper.Action(actionName, controllerName, routeValues);
                //tag = new TagBuilder("a");
                //tag.SetInnerText(linkText);
                //tag.Attributes.Add("href", $"/{controllerName}/{actionName}");
                //if (htmlAttributes != null)
                //{
                //    var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                //    tag.MergeAttributes(attributes);
                //}
            }
            return helper.Action("RedirectToAccessForbidden", "Session");
        }

        public static IHtmlString GetInputByType(this Type type, string placeholder, string value, object htmlAttributes = null)
        {
            //object trueValue = type.ConvertStringToType(value);
            //HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            //var attributes = new StringBuilder();
            //if (htmlAttributes != null)
            //{
            //    foreach (var htmlAttribute in htmlAttributes)
            //    {
            //        attributes.Append(htmlAttribute);
            //    }
            //}

            IHtmlRender part = null;

            if (type.IsEnum)
            {

            }
            else if(type == typeof(DateTime))
            {

            }
            else if (type == typeof(bool))
            {

            }
            else
            {

            }

            return new HtmlString("");
        }

        //public static IHtmlString IsCurrentAction(this HtmlHelper helper, string actionName, string controllerName)
        //{
        //    string currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
        //    string currentActionName = (string)helper.ViewContext.RouteData.Values["action"];

        //    if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase))
        //        return true;
        //    helper.LabelForModel()
        //    return false;
        //}
    }
}