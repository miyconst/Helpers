using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Helpers
{
    public class Formatter
    {
        public string Clear { get; set; }
        public string Format { get; set; }
        public string Template { get; set; }
        //public string Transform { get; set; }
        //public List<string> Errors { get; set; }

        //public Delegate Compiled { get; set; }
        //public LambdaExpression TransformExpression { get; set; }

        public Formatter(string Clear = "", string Format = ".*", string Template = "$&")//, string Transform = "")
        {
            this.Clear = Clear ?? "";
            this.Format = Format ?? ".*";
            this.Template = Template ?? "$&";
            //this.Transform = Transform ?? "";
            //Errors = new List<string>();
        }

        public string Apply(string value)
        {
            if (value.IsNullOrEmpty())
            {
                value = "";
            }

            value = this.ApplyClear(value);
            value = this.ApplyFormat(value);
            //value = this.ApplyTransform(value);

            return value;
        }

        //private string ApplyTransform(string value)
        //{
        //    if (Transform.IsNotNullOrEmpty())
        //    {
        //        try
        //        {
        //            if (Compiled == null)
        //            {
        //                CompileTransform();
        //            }

        //            if (Compiled != null)
        //            {
        //                value = Compiled.DynamicInvoke(value).ToString();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            if (this.Errors == null)
        //            {
        //                this.Errors = new List<string>();
        //            }

        //            this.Errors.Add(ex.Message);
        //        }
        //    }
        //    return value;
        //}

        private string ApplyFormat(string value)
        {
            value = value.ReplaceRegex(this.Format, this.Template);
            return value;
        }

        private string ApplyClear(string value)
        {
            value = value.ReplaceRegex(this.Clear, "");
            return value;
        }
    }
}