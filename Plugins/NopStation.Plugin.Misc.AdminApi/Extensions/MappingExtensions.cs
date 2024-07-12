using Nop.Plugin.NopStation.Core.Models.Api;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Nop.Plugin.NopStation.AdminApi.Extensions
{
    public static class MappingExtensions
    {
        public static NameValueCollection ToNameValueCollection(this List<KeyValueApi> formValues)
        {
            var form = new NameValueCollection();
            foreach (var values in formValues)
            {
                form.Add(values.Key, values.Value);
            }
            return form;
        }
    }
}
