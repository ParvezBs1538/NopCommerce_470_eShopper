using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public interface IFormAttributeFormatter
{
    Task<string> FormatAttributesAsync(QuoteForm quoteForm, string attributesXml);

    Task<string> FormatAttributesAsync(QuoteForm quoteForm, string attributesXml, Customer customer, string separator = "<br />", bool htmlEncode = true, bool renderFormAttributes = true, bool allowHyperlinks = true);
}