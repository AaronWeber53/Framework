using System;
using System.Web.Mvc;
//using System.Web.Mvc.Html.LabelExtensions;

namespace DojoManagmentSystem.Infastructure.Extensions
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