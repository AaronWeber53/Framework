using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DojoManagmentSystem.Infastructure.Extensions
{
    public static class MarkerAttributeExtensions
    {
        public static bool HasMarkerAttribute<T>(this AuthorizationContext that)
        {
            return that.Controller.HasMarkerAttribute<T>()
                || that.ActionDescriptor.HasMarkerAttribute<T>();
        }

        public static bool HasMarkerAttribute<T>(this ActionExecutingContext that)
        {
            return that.Controller.HasMarkerAttribute<T>()
                || that.ActionDescriptor.HasMarkerAttribute<T>();
        }

        public static bool HasMarkerAttribute<T>(this ControllerBase that)
        {
            return that.GetType().HasMarkerAttribute<T>();
        }

        public static bool HasMarkerAttribute<T>(this Type that)
        {
            return that.IsDefined(typeof(T), true);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type that, bool checkBaseController)
        {
            return that.GetCustomAttributes(typeof(T), checkBaseController).Cast<T>();
        }

        public static bool HasMarkerAttribute<T>(this ActionDescriptor that)
        {
            return that.IsDefined(typeof(T), false);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ActionDescriptor that, bool checkBaseController)
        {
            return that.GetCustomAttributes(typeof(T), checkBaseController).Cast<T>();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ControllerDescriptor that, bool checkBaseController)
        {
            return that.GetCustomAttributes(typeof(T), checkBaseController).Cast<T>();
        }

        public static bool TryGetAttribute<T>(this ActionExecutingContext that, bool inherit, out T test)
        {
            test = default;
            if (that.HasMarkerAttribute<T>())
            {
                test = that.ActionDescriptor.GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();  
                if (test == null)
                {
                    test = that.Controller.GetType().GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();
                }
            }
            return test != null;
        }

        public static bool TryGetAttribute<T>(this Type controller, string action, bool inherit, out T test)
        {
            test = default;
            if (controller.HasMarkerAttribute<T>())
            {
                test = controller.GetMethod(action).GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();  
                if (test == null)
                {
                    test = controller.GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();
                }
            }
            return test != null;
        }
    }
}