﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Infastructure.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            var context = HttpContext.Current;
            var isCallbackRequest = false;// callback requests are ajax requests
            if (context != null && context.CurrentHandler != null && context.CurrentHandler is System.Web.UI.Page)
            {
                isCallbackRequest = ((System.Web.UI.Page)context.CurrentHandler).IsCallback;
            }
            return isCallbackRequest || (request["X-Requested-With"] == "XMLHttpRequest") || (request.Headers["X-Requested-With"] == "XMLHttpRequest");
        }
    }
}