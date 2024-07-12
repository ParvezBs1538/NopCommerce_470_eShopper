using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Infrastructure;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public class FormAttributeParser : IFormAttributeParser
{
    #region Fields

    private readonly IDownloadService _downloadService;
    private readonly IFormAttributeService _formAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Properties

    protected string ChildElementName { get; set; } = "FormAttribute";

    #endregion

    #region Ctor

    public FormAttributeParser(
        IDownloadService downloadService,
        IFormAttributeService formAttributeService,
        ILocalizationService localizationService,
        IWebHelper webHelper)
    {
        _downloadService = downloadService;
        _formAttributeService = formAttributeService;
        _localizationService = localizationService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    #region Form attributes

    public virtual async Task<IList<FormAttributeMapping>> ParseFormAttributeMappingsAsync(string attributesXml)
    {
        var result = new List<FormAttributeMapping>();
        if (string.IsNullOrEmpty(attributesXml))
            return result;

        var ids = ParseAttributeIds(attributesXml);
        foreach (var id in ids)
        {
            var attribute = await _formAttributeService.GetFormAttributeMappingByIdAsync(id);
            if (attribute != null)
                result.Add(attribute);
        }

        return result;
    }

    public virtual async Task<IList<FormAttributeValue>> ParseFormAttributeValuesAsync(string attributesXml, int formAttributeMappingId = 0)
    {
        var values = new List<FormAttributeValue>();
        if (string.IsNullOrEmpty(attributesXml))
            return values;

        var attributes = await ParseFormAttributeMappingsAsync(attributesXml);

        //to load values only for the passed form attribute mapping
        if (formAttributeMappingId > 0)
            attributes = attributes.Where(attribute => attribute.Id == formAttributeMappingId).ToList();

        foreach (var attribute in attributes)
        {
            if (!attribute.ShouldHaveValues())
                continue;

            foreach (var attributeValue in ParseValues(attributesXml, attribute.Id))
            {
                if (string.IsNullOrEmpty(attributeValue) || !int.TryParse(attributeValue, out var attributeValueId))
                    continue;

                var value = await _formAttributeService.GetFormAttributeValueByIdAsync(attributeValueId);
                if (value == null)
                    continue;

                values.Add(value);
            }
        }

        return values;
    }

    public virtual IList<string> ParseValues(string attributesXml, int formAttributeMappingId)
    {
        var selectedValues = new List<string>();
        if (string.IsNullOrEmpty(attributesXml))
            return selectedValues;

        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(attributesXml);

            var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/FormAttribute");
            foreach (XmlNode node1 in nodeList1)
            {
                if (node1.Attributes?["ID"] == null)
                    continue;

                var str1 = node1.Attributes["ID"].InnerText.Trim();
                if (!int.TryParse(str1, out var id))
                    continue;

                if (id != formAttributeMappingId)
                    continue;

                var nodeList2 = node1.SelectNodes(@"FormAttributeValue/Value");
                foreach (XmlNode node2 in nodeList2)
                {
                    var value = node2.InnerText.Trim();
                    selectedValues.Add(value);
                }
            }
        }
        catch (Exception exc)
        {
            Debug.Write(exc.ToString());
        }

        return selectedValues;
    }

    public virtual string AddFormAttribute(string attributesXml, FormAttributeMapping formAttributeMapping, string value)
    {
        var result = string.Empty;
        try
        {
            var xmlDoc = new XmlDocument();
            if (string.IsNullOrEmpty(attributesXml))
            {
                var element1 = xmlDoc.CreateElement("Attributes");
                xmlDoc.AppendChild(element1);
            }
            else
            {
                xmlDoc.LoadXml(attributesXml);
            }

            var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

            XmlElement attributeElement = null;
            //find existing
            var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/FormAttribute");
            foreach (XmlNode node1 in nodeList1)
            {
                if (node1.Attributes?["ID"] == null)
                    continue;

                var str1 = node1.Attributes["ID"].InnerText.Trim();
                if (!int.TryParse(str1, out var id))
                    continue;

                if (id != formAttributeMapping.Id)
                    continue;

                attributeElement = (XmlElement)node1;
                break;
            }

            //create new one if not found
            if (attributeElement == null)
            {
                attributeElement = xmlDoc.CreateElement("FormAttribute");
                attributeElement.SetAttribute("ID", formAttributeMapping.Id.ToString());
                rootElement.AppendChild(attributeElement);
            }

            var attributeValueElement = xmlDoc.CreateElement("FormAttributeValue");
            attributeElement.AppendChild(attributeValueElement);

            var attributeValueValueElement = xmlDoc.CreateElement("Value");
            attributeValueValueElement.InnerText = value;
            attributeValueElement.AppendChild(attributeValueValueElement);

            result = xmlDoc.OuterXml;
        }
        catch (Exception exc)
        {
            Debug.Write(exc.ToString());
        }

        return result;
    }

    public virtual string RemoveFormAttribute(string attributesXml, FormAttributeMapping formAttributeMapping)
    {
        return RemoveAttribute(attributesXml, formAttributeMapping.Id);
    }

    public virtual async Task<bool> AreFormAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes)
    {
        var attributes1 = await ParseFormAttributeMappingsAsync(attributesXml1);
        if (ignoreNonCombinableAttributes)
            attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();

        var attributes2 = await ParseFormAttributeMappingsAsync(attributesXml2);
        if (ignoreNonCombinableAttributes)
            attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();

        if (attributes1.Count != attributes2.Count)
            return false;

        var attributesEqual = true;
        foreach (var a1 in attributes1)
        {
            var hasAttribute = false;
            foreach (var a2 in attributes2)
            {
                if (a1.Id != a2.Id)
                    continue;

                hasAttribute = true;
                var values1Str = ParseValues(attributesXml1, a1.Id);
                var values2Str = ParseValues(attributesXml2, a2.Id);
                if (values1Str.Count == values2Str.Count)
                {
                    foreach (var str1 in values1Str)
                    {
                        var hasValue = false;
                        foreach (var str2 in values2Str)
                        {
                            //case insensitive? 
                            //if (str1.Trim().ToLowerInvariant() == str2.Trim().ToLowerInvariant())
                            if (str1.Trim() != str2.Trim())
                                continue;

                            hasValue = str1.Trim() == str2.Trim();
                            break;
                        }

                        if (hasValue)
                            continue;

                        attributesEqual = false;
                        break;
                    }
                }
                else
                {
                    attributesEqual = false;
                    break;
                }
            }

            if (hasAttribute)
                continue;

            attributesEqual = false;
            break;
        }

        return attributesEqual;
    }

    public virtual async Task<bool?> IsConditionMetAsync(FormAttributeMapping pam, string selectedAttributesXml)
    {
        ArgumentNullException.ThrowIfNull(pam);

        var conditionAttributeXml = pam.ConditionAttributeXml;
        if (string.IsNullOrEmpty(conditionAttributeXml))
            //no condition
            return null;

        //load an attribute this one depends on
        var dependOnAttribute = (await ParseFormAttributeMappingsAsync(conditionAttributeXml)).FirstOrDefault();
        if (dependOnAttribute == null)
            return true;

        var valuesThatShouldBeSelected = ParseValues(conditionAttributeXml, dependOnAttribute.Id)
            //a workaround here:
            //ConditionAttributeXml can contain "empty" values (nothing is selected)
            //but in other cases (like below) we do not store empty values
            //that's why we remove empty values here
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();
        var selectedValues = ParseValues(selectedAttributesXml, dependOnAttribute.Id);
        if (valuesThatShouldBeSelected.Count != selectedValues.Count)
            return false;

        //compare values
        var allFound = true;
        foreach (var t1 in valuesThatShouldBeSelected)
        {
            var found = false;
            foreach (var t2 in selectedValues)
                if (t1 == t2)
                    found = true;
            if (!found)
                allFound = false;
        }

        return allFound;
    }

    public virtual async Task<(string AttributesXml, AttributeMetadata MetaData)> ParseFormAttributesAsync(QuoteForm quoteForm, IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(quoteForm);
        ArgumentNullException.ThrowIfNull(form);

        //form attributes
        var data = await GetFormAttributesXmlAsync(quoteForm, form);

        return data;
    }

    public virtual async Task<IList<string>> GetFormAttributeWarningsAsync(QuoteForm quoteForm, string attributesXml = "",
        bool ignoreNonCombinableAttributes = false, bool ignoreConditionMet = false)
    {
        ArgumentNullException.ThrowIfNull(quoteForm);

        var warnings = new List<string>();

        //ensure it's our attributes
        var attributes1 = await ParseFormAttributeMappingsAsync(attributesXml);
        if (ignoreNonCombinableAttributes)
        {
            attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();
        }

        foreach (var attribute in attributes1)
        {
            if (attribute.QuoteFormId == 0)
            {
                warnings.Add("Attribute error");
                return warnings;
            }

            if (attribute.QuoteFormId != quoteForm.Id)
            {
                warnings.Add("Attribute error");
            }
        }

        //validate required quote attributes (whether they're chosen/selected/entered)
        var attributes2 = await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(quoteForm.Id);
        if (ignoreNonCombinableAttributes)
        {
            attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();
        }

        //validate conditional attributes only (if specified)
        if (!ignoreConditionMet)
        {
            attributes2 = await attributes2.WhereAwait(async x =>
            {
                var conditionMet = await IsConditionMetAsync(x, attributesXml);
                return !conditionMet.HasValue || conditionMet.Value;
            }).ToListAsync();
        }

        foreach (var a2 in attributes2)
        {
            if (a2.IsRequired)
            {
                var found = false;
                //selected form attributes
                foreach (var a1 in attributes1)
                {
                    if (a1.Id != a2.Id)
                        continue;

                    var attributeValuesStr = ParseValues(attributesXml, a1.Id);

                    foreach (var str1 in attributeValuesStr)
                    {
                        if (string.IsNullOrEmpty(str1.Trim()))
                            continue;

                        found = true;
                        break;
                    }
                }

                //if not found
                if (!found)
                {
                    var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(a2.FormAttributeId);

                    var textPrompt = await _localizationService.GetLocalizedAsync(a2, x => x.TextPrompt);
                    var notFoundWarning = !string.IsNullOrEmpty(textPrompt) ?
                        textPrompt :
                        string.Format(await _localizationService.GetResourceAsync("NopStation.QuoteCart.FormAttribute.ValidationError.Required"), await _localizationService.GetLocalizedAsync(formAttribute, a => a.Name));

                    warnings.Add(notFoundWarning);
                }
            }

            if (a2.AttributeControlType != AttributeControlType.ReadonlyCheckboxes)
                continue;

            //customers cannot edit read-only attributes
            var allowedReadOnlyValueIds = (await _formAttributeService.GetFormAttributeValuesAsync(a2.Id))
                .Where(x => x.IsPreSelected)
                .Select(x => x.Id)
                .ToArray();

            var selectedReadOnlyValueIds = (await ParseFormAttributeValuesAsync(attributesXml))
                .Where(x => x.FormAttributeMappingId == a2.Id)
                .Select(x => x.Id)
                .ToArray();

            if (!CommonHelper.ArraysEqual(allowedReadOnlyValueIds, selectedReadOnlyValueIds))
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.QuoteCart.FormAttribute.ValidationError.ReadOnly")));
            }
        }

        //validation rules
        foreach (var pam in attributes2)
        {
            if (!pam.ValidationRulesAllowed())
                continue;

            string enteredText;
            int enteredTextLength;

            var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(pam.FormAttributeId);

            if (pam.AttributeControlType == AttributeControlType.TextBox ||
                pam.AttributeControlType == AttributeControlType.MultilineTextbox)
            {
                //minimum length
                if (pam.ValidationMinLength.HasValue)
                {
                    enteredText = ParseValues(attributesXml, pam.Id).FirstOrDefault();
                    enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                    if (pam.ValidationMinLength.Value > enteredTextLength)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.QuoteCart.FormAttribute.ValidationError.TextboxMinimumLength"), await _localizationService.GetLocalizedAsync(formAttribute, a => a.Name), pam.ValidationMinLength.Value));
                    }
                }

                //maximum length
                if (pam.ValidationMaxLength.HasValue)
                {
                    enteredText = ParseValues(attributesXml, pam.Id).FirstOrDefault();
                    enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                    if (pam.ValidationMaxLength.Value < enteredTextLength)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.QuoteCart.FormAttribute.ValidationError.TextboxMaximumLength"), await _localizationService.GetLocalizedAsync(formAttribute, a => a.Name), pam.ValidationMaxLength.Value));
                    }
                }
            }

            if (pam.AttributeControlType == AttributeControlType.Datepicker)
            {
                var selectedDateStr = ParseValues(attributesXml, pam.Id).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(selectedDateStr) && DateTime.TryParse(selectedDateStr, out var selectedDate))
                {
                    //minimum date
                    if (pam.ValidationMinDate.HasValue && pam.ValidationMinDate.Value > selectedDate)
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.QuoteCart.FormAttribute.ValidationError.DatePickerMinimumDate"), await _localizationService.GetLocalizedAsync(formAttribute, a => a.Name), pam.ValidationMinDate.Value));

                    //maximum date
                    if (pam.ValidationMaxDate.HasValue && pam.ValidationMaxDate.Value < selectedDate)
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.QuoteCart.FormAttribute.ValidationError.DatePickerMaximumDate"), await _localizationService.GetLocalizedAsync(formAttribute, a => a.Name), pam.ValidationMaxDate.Value));
                }
            }
        }

        if (warnings.Count != 0)
            return warnings;

        return warnings;
    }

    #endregion

    #endregion

    #region Utilities

    /// <summary>
    /// Remove an attribute
    /// </summary>
    /// <param name="attributesXml">Attributes in XML format</param>
    /// <param name="attributeValueId">Attribute value id</param>
    /// <returns>Updated result (XML format)</returns>
    protected virtual string RemoveAttribute(string attributesXml, int attributeValueId)
    {
        var result = string.Empty;

        if (string.IsNullOrEmpty(attributesXml))
            return string.Empty;

        try
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(attributesXml);

            var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

            if (rootElement == null)
                return string.Empty;

            XmlElement attributeElement = null;
            //find existing
            var childNodes = xmlDoc.SelectNodes($@"//Attributes/{ChildElementName}");

            if (childNodes == null)
                return string.Empty;

            var count = childNodes.Count;

            foreach (XmlElement childNode in childNodes)
            {
                if (!int.TryParse(childNode.Attributes["ID"]?.InnerText.Trim(), out var id))
                    continue;

                if (id != attributeValueId)
                    continue;

                attributeElement = childNode;
                break;
            }

            //found
            if (attributeElement != null)
            {
                rootElement.RemoveChild(attributeElement);
                count -= 1;
            }

            result = count == 0 ? string.Empty : xmlDoc.OuterXml;
        }
        catch
        {
            //ignore
        }

        return result;
    }

    /// <summary>
    /// Gets selected attribute identifiers
    /// </summary>
    /// <param name="attributesXml">Attributes in XML format</param>
    /// <returns>Selected attribute identifiers</returns>
    protected virtual IList<int> ParseAttributeIds(string attributesXml)
    {
        var ids = new List<int>();
        if (string.IsNullOrEmpty(attributesXml))
            return ids;

        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(attributesXml);

            var elements = xmlDoc.SelectNodes(@$"//Attributes/{ChildElementName}");

            if (elements == null)
                return Array.Empty<int>();

            foreach (XmlNode node in elements)
            {
                if (node.Attributes?["ID"] == null)
                    continue;

                var attributeValue = node.Attributes["ID"].InnerText.Trim();
                if (int.TryParse(attributeValue, out var id))
                    ids.Add(id);
            }
        }
        catch
        {
            //ignore
        }

        return ids;
    }

    protected virtual async Task<(string AttributesXml, AttributeMetadata MetaData)> GetFormAttributesXmlAsync(QuoteForm quoteForm, IFormCollection form)
    {
        var metadata = new AttributeMetadata();
        var attributesXml = string.Empty;
        var formAttributes = await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(quoteForm.Id);
        foreach (var attribute in formAttributes)
        {
            var controlId = $"{QuoteCartDefaults.FormFieldPrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                {
                    var ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (await _formAttributeService.GetFormAttributeValueByIdAsync(selectedAttributeId) is FormAttributeValue value)
                        {
                            metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, value.Name));
                            attributesXml = AddFormAttribute(attributesXml, attribute, selectedAttributeId.ToString());
                        }
                    }
                }
                break;
                case AttributeControlType.Checkboxes:
                {
                    var ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        foreach (var item in ctrlAttributes.ToString()
                            .Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (await _formAttributeService.GetFormAttributeValueByIdAsync(selectedAttributeId) is FormAttributeValue value)
                            {
                                metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, value.Name));
                                attributesXml = AddFormAttribute(attributesXml, attribute, selectedAttributeId.ToString());
                            }
                        }
                    }
                }
                break;
                case AttributeControlType.ReadonlyCheckboxes:
                {
                    //load read-only (already server-side selected) values
                    var attributeValues = await _formAttributeService.GetFormAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttribute in attributeValues.Where(v => v.IsPreSelected))
                    {
                        metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, selectedAttribute.Name));
                        attributesXml = AddFormAttribute(attributesXml,
                            attribute, selectedAttribute.Id.ToString());
                    }
                }
                break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                {
                    var ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, enteredText));
                        attributesXml = AddFormAttribute(attributesXml, attribute, enteredText);
                    }
                }
                break;
                case AttributeControlType.Datepicker:
                {
                    var day = form[controlId + "_day"];
                    var month = form[controlId + "_month"];
                    var year = form[controlId + "_year"];
                    DateTime? selectedDate = null;
                    try
                    {
                        selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                    }
                    catch
                    {
                        // ignored
                    }

                    if (selectedDate.HasValue)
                    {
                        attributesXml = AddFormAttribute(attributesXml, attribute, selectedDate.Value.ToString("D"));
                        metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, selectedDate.Value.ToString("D")));
                    }
                }
                break;
                case AttributeControlType.FileUpload:
                {
                    _ = Guid.TryParse(form[controlId], out var downloadGuid);
                    var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                    if (download != null)
                    {
                        attributesXml = AddFormAttribute(attributesXml, attribute, download.DownloadGuid.ToString());

                        var fileName = $"{download.Filename ?? download.DownloadGuid.ToString()}{download.Extension}";
                        metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, $"<a href=\"{_webHelper.GetStoreLocation()}download/getfileupload/?downloadId={download.DownloadGuid}\" class=\"fileuploadattribute\">{fileName}</a>"));
                    }
                }
                break;
                default:
                    break;
            }
        }
        //validate conditional attributes (if specified)
        foreach (var attribute in formAttributes)
        {
            var conditionMet = await IsConditionMetAsync(attribute, attributesXml);
            if (conditionMet.HasValue && !conditionMet.Value)
            {
                attributesXml = RemoveFormAttribute(attributesXml, attribute);

                if (metadata.Mappings.FirstOrDefault(x => x.AttributeMapping == attribute) is AttributeMetadata.MappingData data)
                    metadata.Mappings.Remove(data);
            }
        }
        return (attributesXml, metadata);
    }

    #endregion
}
