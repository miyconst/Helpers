using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;
using System.Web.Caching;

namespace Helpers
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString RadioListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList)
        {
            return RadioListFor<TModel, TProperty>(htmlHelper, expression, selectList, new { });
        }
        public static MvcHtmlString RadioListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            System.Text.StringBuilder sb;
            MvcHtmlString result;
            string id, name;
            Dictionary<string, object> newProperties = new Dictionary<string, object>();
            PropertyInfo[] properties = htmlAttributes.GetType().GetProperties();

            name = (expression.Body as MemberExpression).Member.Name;
            foreach (PropertyInfo pi in properties)
            {
                newProperties.Add(pi.Name.ToLower(), pi.GetValue(htmlAttributes, null));
            }
            if (newProperties.ContainsKey("id"))
            {
                id = newProperties["id"].ToString();
            }
            else
            {
                id = "rbt" + name;
                newProperties.Add("id", id);
            }

            sb = new System.Text.StringBuilder();
            foreach (SelectListItem item in selectList)
            {
                newProperties["id"] = id + item.Value;
                sb.Append(htmlHelper.RadioButtonFor<TModel, TProperty>(expression, item.Value, newProperties).ToString());
                //sb.Append(htmlHelper.RadioButton(name, htmlAttributes, item.Selected));
                sb.Append("<label for='").Append(id).Append(item.Value).Append("'>").Append(item.Text).Append("</label>");
            }
            result = new MvcHtmlString(sb.ToString());
            return result;
        }

        public static MvcHtmlString RadioList(this HtmlHelper htmlHelper, string Name, IEnumerable<SelectListItem> selectList, object htmlAttributes, bool Separate = false)
        {
            System.Text.StringBuilder sb;
            MvcHtmlString result;
            string id;
            Dictionary<string, object> newProperties = new Dictionary<string, object>();
            PropertyInfo[] properties = htmlAttributes.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
                newProperties.Add(pi.Name.ToLower(), pi.GetValue(htmlAttributes, null));
            }
            if (newProperties.ContainsKey("id"))
            {
                id = newProperties["id"].ToString();
            }
            else
            {
                id = "rbt" + Name;
                newProperties.Add("id", id);
            }

            sb = new System.Text.StringBuilder();
            foreach (SelectListItem item in selectList)
            {
                newProperties["id"] = id + item.Value;
                sb.Append(htmlHelper.RadioButton(Name, item.Value, item.Selected, newProperties));
                sb.Append("<label for='").Append(id).Append(item.Value).Append("'>").Append(item.Text).Append("</label>");
                if (Separate)
                {
                    sb.Append("<br/>");
                }
            }
            result = new MvcHtmlString(sb.ToString());
            return result;
        }

        public static MvcHtmlString RadioYesNoFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string trueLabel, string falseLabel, object htmlAttributes, bool Separate = false)
        {
            Dictionary<bool, string> values = new Dictionary<bool, string>();
            string name = (expression.Body as MemberExpression).Member.Name;
            object value = expression.Compile()(htmlHelper.ViewData.Model);
            if (value is bool? && value != null)
            {
                value = (value as bool?).Value;
            }
            values.Add(true, trueLabel);
            values.Add(false, falseLabel);
            return htmlHelper.RadioList(name, new SelectList(values, "Key", "Value", value), htmlAttributes, Separate);
        }
        public static MvcHtmlString RadioYesNoFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return htmlHelper.RadioYesNoFor(expression, bool.FalseString, bool.TrueString, htmlAttributes);
        }

        public static MvcHtmlString Script(this HtmlHelper htmlHelper, string[] Links, string Directory = null, bool Defer = false)
        {
            return new MvcHtmlString(htmlHelper.StyleOrScript(Links, true, Directory, Defer));
        }
        public static string ScriptUrl(this HtmlHelper htmlHelper, string[] Links, string Directory = null, bool Defer = false)
        {
            return htmlHelper.StyleOrScript(Links, true, Directory, Defer, true);
        }
        public static MvcHtmlString Style(this HtmlHelper htmlHelper, string[] Links, string Directory = null)
        {
            return new MvcHtmlString(htmlHelper.StyleOrScript(Links, false, Directory));
        }
        public static string StyleUrl(this HtmlHelper htmlHelper, string[] Links, string Directory = null)
        {
            return htmlHelper.StyleOrScript(Links, false, Directory, true);
        }

        /// <summary>
        /// Captchas the text box.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static MvcHtmlString CaptchaTextBox(this HtmlHelper htmlHelper, string name, string htmlAttributes)
        {
            return new MvcHtmlString(String.Format(@"<input type=""text"" id=""{0}"" name=""{0}"" value="""" maxlength=""{1}"" autocomplete=""off"" {2}/>",
                name, Helpers.CaptchaImage.TextLength, htmlAttributes
                ));

        }

        /// <summary>
        /// Generates the captcha image.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns>
        /// Returns the <see cref="Uri"/> for the generated <see cref="CaptchaImage"/>.
        /// </returns>
        public static MvcHtmlString CaptchaImage(this HtmlHelper htmlHelper, string imgUrl, int height, int width)
        {
            Helpers.CaptchaImage image = new Helpers.CaptchaImage
            {
                Height = height,
                Width = width,
            };

            HttpRuntime.Cache.Add(
                image.UniqueId,
                image,
                null,
                DateTime.Now.AddSeconds(Helpers.CaptchaImage.CacheTimeOut),
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                null);

            StringBuilder stringBuilder = new StringBuilder(256);
            stringBuilder.Append("<input type=\"hidden\" name=\"captcha-guid\" value=\"");
            stringBuilder.Append(image.UniqueId);
            stringBuilder.Append("\" />");
            stringBuilder.AppendLine();
            stringBuilder.Append("<img src=\"");

            //!!!
            stringBuilder.Append(imgUrl + "?id=" + image.UniqueId);
            stringBuilder.Append("\" alt=\"CAPTCHA\" width=\"");
            stringBuilder.Append(width);
            stringBuilder.Append("\" height=\"");
            stringBuilder.Append(height);
            stringBuilder.Append("\" />");

            return new MvcHtmlString(stringBuilder.ToString());
        }

        private static string StyleOrScript(this HtmlHelper htmlHelper, string[] Links, bool Script, string Directory = null, bool Defer = false, bool UrlOnly = false)
        {
            UrlHelper Url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            string link;
            string result;
            string template, url;
            FileInfo fi;
            string version = HttpContext.Current.Application["Version"] as string;
            string extension = Script ? ".js" : ".css";
            TextReader tr;
            TextWriter resultWriter;
            StringBuilder sb;

            if (!Links.Any())
            {
                return string.Empty;
            }

            if (version.IsNullOrEmpty())
            {
                version = "v0.0.0.1";
            }

            if (Script)
            {
                if (UrlOnly)
                {
                    template = "{0}";
                }
                else if (Defer)
                {
                    template = "<script type='text/javascript' src='{0}' defer='defer'></script>";
                }
                else
                {
                    template = "<script type='text/javascript' src='{0}'></script>";
                }
                url = Directory.IsNullOrEmpty() ? "/Compress/Script/" : Directory;
            }
            else
            {
                if (UrlOnly)
                {
                    template = "{0}";
                }
                {
                    template = "<link type='text/css' rel='stylesheet' href='{0}' />";
                }
                url = Directory.IsNullOrEmpty() ? "/Compress/Style/" : Directory;
            }

            if (Debug)
            {
                sb = new StringBuilder();

                foreach (string s in Links)
                {
                    sb.Append(string.Format(template, Url.Content(s))).Append(Environment.NewLine);
                }

                result = sb.ToString();

                return result;
            }

            link = string.Format("{0}{1}-{2}{3}", url, string.Join(";", Links).GetHashCode(), version, extension);
            fi = new FileInfo(HttpRuntime.AppDomainAppPath + link);

            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            if (!fi.Exists)
            {
                resultWriter = new StreamWriter(fi.FullName, false, Encoding.UTF8);

                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                foreach (string s in Links)
                {
                    string path = s.Replace("~", string.Empty);

                    resultWriter.Write("/*-- Begin ");
                    resultWriter.Write(s);
                    resultWriter.Write(" --*/");
                    resultWriter.Write(Environment.NewLine);

                    tr = new StreamReader(HttpRuntime.AppDomainAppPath + path);

                    tr.CopyStream(resultWriter);
                    tr.Dispose();

                    resultWriter.Write(Environment.NewLine);
                    resultWriter.Write("/*-- End ");
                    resultWriter.Write(s);
                    resultWriter.Write(" --*/");
                    resultWriter.Write(Environment.NewLine);
                }

                resultWriter.Dispose();
            }

            result = string.Format(template, Url.Content("~" + link));

            return result;
        }

        public static bool Debug
        {
            get
            {
                bool result = false;

                object o = WebConfigurationManager.GetWebApplicationSection("system.web/compilation");
                System.Web.Configuration.CompilationSection compilation = o as System.Web.Configuration.CompilationSection;
                result = compilation.Debug;

                return result;
            }
        }
    }
}