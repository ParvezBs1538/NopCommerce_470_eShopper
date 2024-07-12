using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public interface IFormAttributeParser
{
    string AddFormAttribute(string attributesXml, FormAttributeMapping formAttributeMapping, string value);

    Task<bool> AreFormAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes);

    Task<IList<string>> GetFormAttributeWarningsAsync(QuoteForm quoteForm, string attributesXml = "", bool ignoreNonCombinableAttributes = false, bool ignoreConditionMet = false);

    Task<bool?> IsConditionMetAsync(FormAttributeMapping pam, string selectedAttributesXml);

    Task<IList<FormAttributeMapping>> ParseFormAttributeMappingsAsync(string attributesXml);

    Task<(string AttributesXml, AttributeMetadata MetaData)> ParseFormAttributesAsync(QuoteForm quoteForm, IFormCollection form);

    Task<IList<FormAttributeValue>> ParseFormAttributeValuesAsync(string attributesXml, int formAttributeMappingId = 0);

    IList<string> ParseValues(string attributesXml, int formAttributeMappingId);

    string RemoveFormAttribute(string attributesXml, FormAttributeMapping formAttributeMapping);
}