using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Helpers
{
    public class DecimalModelBinder : DefaultModelBinder
    {
        #region Implementation of IModelBinder

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == null || 
                string.IsNullOrEmpty(valueProviderResult.AttemptedValue) ||
                valueProviderResult.AttemptedValue.Equals("N.aN") ||
                valueProviderResult.AttemptedValue.Equals("NaN") ||
                valueProviderResult.AttemptedValue.Equals("Infini.ty") ||
                valueProviderResult.AttemptedValue.Equals("Infinity"))
            {
                return 0m;
            }

            return valueProviderResult == null ? base.BindModel(controllerContext, bindingContext) : valueProviderResult.AttemptedValue.ToDecimal();
        }

        #endregion
    }
}
