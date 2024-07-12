using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial interface ISurveyAttributeFormatter
    {
        Task<string> FormatAttributesAsync(Survey survey, string attributesXml);

        Task<string> FormatAttributesAsync(Survey survey, string attributesXml,
            Customer customer, string separator = "<br />", bool htmlEncode = true,
            bool renderSurveyAttributes = true, bool allowHyperlinks = true);

        Task<IDictionary<string, List<string>>> GetAttributeValuesAsync(string attributesXml, bool htmlEncode = true);
    }
}
