using DojoManagmentSystem;
using DojoManagmentSystem.Infastructure.Attributes;
using DojoManagmentSystem.Infastructure.Extensions;
using Microsoft.Ajax.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
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