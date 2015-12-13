using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

public static class RouteValuesHelper
{
    public static bool IsController(this HtmlHelper helper, string controller)
    {
        return helper.ViewContext.RouteData.Values.Any(p => p.Key == "controller" && p.Value as string == controller);
    }

    public static bool IsAction(this HtmlHelper helper, params string[] action)
    {
        return helper.ViewContext.RouteData.Values.Any(p => p.Key == "action" && action.Contains(p.Value as string));
    }
}

