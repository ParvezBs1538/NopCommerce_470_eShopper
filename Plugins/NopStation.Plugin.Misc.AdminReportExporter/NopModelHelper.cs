using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdminReportExporter
{
    public class NopModelHelper
    {
        public static string PropertyLabel<T>(Expression<Func<T, object>> func)
        {
            if (func.Body is not MemberExpression member || member.Member is not PropertyInfo propertyInfo)
                return string.Empty;

            return PropertyLabel<T>(propertyInfo.Name);
        }

        public static string PropertyLabel<T>(string propertyName)
        {
            var provider = EngineContext.Current.Resolve<IModelMetadataProvider>();

            var metadata = provider.GetMetadataForProperty(typeof(T), propertyName);
            return (metadata.AdditionalValues.TryGetValue("NopResourceDisplayNameAttribute", out var value)
                && value is NopResourceDisplayNameAttribute resourceDisplayName
                && !string.IsNullOrEmpty(resourceDisplayName.DisplayName)) ? resourceDisplayName.DisplayName : propertyName;
        }
    }
}
