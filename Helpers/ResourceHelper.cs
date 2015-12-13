using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

/// <summary>
/// Check mvc version before use
/// </summary>
public static class ResourceHelper
{
    public static string LocalResource(this WebViewPage page, string key)
    {
        bool found;
        return LocalResource(page, key, out found);
    }

    public static string LocalResource(this WebViewPage page, string key, out bool found)
    {
        var test = page.ViewContext.HttpContext.GetLocalResourceObject(page.VirtualPath, key);
        var value = test as string;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            value = string.Format("*{0}*", key.ToLowerInvariant());
            found = false;
        }
        else
        {
            found = true;
        }

        return value;
    }

    public static string _(this WebViewPage page, string text)
    {
        var value = page.ViewContext.HttpContext.GetLocalResourceObject(page.VirtualPath, text) as string;
        if (string.IsNullOrWhiteSpace(value))
            value = text;
        return value;
    }

    public static string Content(this WebViewPage Page, string Path)
    {
        string result;
        string v = string.Empty;
        PropertyInfo pi;

        result = Page.Url.Content(Path);
        v = Page.ViewBag.Version;
        if (v.IsNullOrEmpty())
        {
            pi = Page.Context.ApplicationInstance.GetType().GetProperties().FirstOrDefault(val => val.Name == "Version");
            if (pi != null)
            {
                v = pi.GetValue(Page.Context.ApplicationInstance, null).ToString();
            }
        }
        if (!string.IsNullOrEmpty(v))
        {   
            if (result.IndexOf("?") >= 0)
            {
                result += "&";
            }
            else
            {
                result += "?";
            }
            result += "v=" + v;
        }
        
        return result;
    }
}
