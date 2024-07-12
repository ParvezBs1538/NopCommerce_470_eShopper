using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Services.Attributes;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial class SurveyAttributeParser : ISurveyAttributeParser
    {
        #region Fields

        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IAttributeParser<SurveyAttribute, SurveyAttributeValue> _surveyAttributeParser;

        #endregion

        #region Ctor

        public SurveyAttributeParser(IDownloadService downloadService,
            ILocalizationService localizationService,
            ISurveyAttributeService surveyAttributeService,
            IWebHelper webHelper,
            IAttributeParser<SurveyAttribute, SurveyAttributeValue> surveyAttributeParser)
        {
            _downloadService = downloadService;
            _surveyAttributeService = surveyAttributeService;
            _webHelper = webHelper;
            _surveyAttributeParser = surveyAttributeParser;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        protected virtual async Task<(string AttributesXml, AttributeMetadata MetaData)> GetSurveyAttributesXmlAsync(Survey survey, IFormCollection form)
        {
            var metadata = new AttributeMetadata();
            var attributesXml = string.Empty;
            var surveyAttributes = await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id);
            foreach (var attribute in surveyAttributes)
            {
                var controlId = $"{DynamicSurveyDefaults.SurveyAttributePrefix}{attribute.Id}";
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
                                if (await _surveyAttributeService.GetSurveyAttributeValueByIdAsync(selectedAttributeId) is SurveyAttributeValue value)
                                {
                                    metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, value.Name));
                                    attributesXml = AddSurveyAttribute(attributesXml, attribute, selectedAttributeId.ToString());
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
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (await _surveyAttributeService.GetSurveyAttributeValueByIdAsync(selectedAttributeId) is SurveyAttributeValue value)
                                    {
                                        metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, value.Name));
                                        attributesXml = AddSurveyAttribute(attributesXml, attribute, selectedAttributeId.ToString());
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _surveyAttributeService.GetSurveyAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttribute in attributeValues.Where(v => v.IsPreSelected))
                            {
                                metadata.Mappings.Add(new AttributeMetadata.MappingData(attribute, selectedAttribute.Name));
                                attributesXml = AddSurveyAttribute(attributesXml,
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
                                attributesXml = AddSurveyAttribute(attributesXml, attribute, enteredText);
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
                                attributesXml = AddSurveyAttribute(attributesXml, attribute, selectedDate.Value.ToString("D"));
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
                                attributesXml = AddSurveyAttribute(attributesXml, attribute, download.DownloadGuid.ToString());

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
            foreach (var attribute in surveyAttributes)
            {
                var conditionMet = await IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = RemoveSurveyAttribute(attributesXml, attribute);

                    if (metadata.Mappings.FirstOrDefault(x => x.AttributeMapping == attribute) is AttributeMetadata.MappingData data)
                        metadata.Mappings.Remove(data);
                }
            }
            return (attributesXml, metadata);
        }

        protected virtual async Task<string> GetFieldNameAsync(SurveyAttributeMapping surveyAttributeMapping, SurveyAttribute surveyAttribute)
        {
            var textPrompt = await _localizationService.GetLocalizedAsync(surveyAttributeMapping, x => x.TextPrompt);
            if (!string.IsNullOrEmpty(textPrompt))
                return textPrompt;

            if (surveyAttribute == null)
                return string.Empty;

            textPrompt = await _localizationService.GetLocalizedAsync(surveyAttribute, x => x.Name);

            return textPrompt;
        }

        #endregion

        #region Survey attributes

        public virtual async Task<IList<SurveyAttributeMapping>> ParseSurveyAttributeMappingsAsync(string attributesXml)
        {
            var result = new List<SurveyAttributeMapping>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = _surveyAttributeParser.ParseAttributeIds(attributesXml);
            foreach (var id in ids)
            {
                var attribute = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(id);
                if (attribute != null)
                    result.Add(attribute);
            }

            return result;
        }

        public virtual async Task<IList<SurveyAttributeValue>> ParseSurveyAttributeValuesAsync(string attributesXml, int surveyAttributeMappingId = 0)
        {
            var values = new List<SurveyAttributeValue>();
            if (string.IsNullOrEmpty(attributesXml))
                return values;

            var attributes = await ParseSurveyAttributeMappingsAsync(attributesXml);

            //to load values only for the passed survey attribute mapping
            if (surveyAttributeMappingId > 0)
                attributes = attributes.Where(attribute => attribute.Id == surveyAttributeMappingId).ToList();

            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                foreach (var attributeValue in ParseValues(attributesXml, attribute.Id))
                {
                    if (string.IsNullOrEmpty(attributeValue) || !int.TryParse(attributeValue, out var attributeValueId))
                        continue;

                    var value = await _surveyAttributeService.GetSurveyAttributeValueByIdAsync(attributeValueId);
                    if (value == null)
                        continue;

                    values.Add(value);
                }
            }

            return values;
        }

        public virtual IList<string> ParseValues(string attributesXml, int surveyAttributeMappingId)
        {
            var selectedValues = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/SurveyAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != surveyAttributeMappingId)
                        continue;

                    var nodeList2 = node1.SelectNodes(@"SurveyAttributeValue/Value");
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

        public virtual string AddSurveyAttribute(string attributesXml, SurveyAttributeMapping surveyAttributeMapping, string value)
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
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/SurveyAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != surveyAttributeMapping.Id)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                //create new one if not found
                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("SurveyAttribute");
                    attributeElement.SetAttribute("ID", surveyAttributeMapping.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("SurveyAttributeValue");
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

        public virtual string RemoveSurveyAttribute(string attributesXml, SurveyAttributeMapping surveyAttributeMapping)
        {
            return _surveyAttributeParser.RemoveAttribute(attributesXml, surveyAttributeMapping.Id);
        }

        public virtual async Task<bool> AreSurveyAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes)
        {
            var attributes1 = await ParseSurveyAttributeMappingsAsync(attributesXml1);
            if (ignoreNonCombinableAttributes)
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();

            var attributes2 = await ParseSurveyAttributeMappingsAsync(attributesXml2);
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

        public virtual async Task<bool?> IsConditionMetAsync(SurveyAttributeMapping pam, string selectedAttributesXml)
        {
            if (pam == null)
                throw new ArgumentNullException(nameof(pam));

            var conditionAttributeXml = pam.ConditionAttributeXml;
            if (string.IsNullOrEmpty(conditionAttributeXml))
                //no condition
                return null;

            //load an attribute this one depends on
            var dependOnAttribute = (await ParseSurveyAttributeMappingsAsync(conditionAttributeXml)).FirstOrDefault();
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

        public virtual async Task<(string AttributesXml, AttributeMetadata MetaData)> ParseSurveyAttributesAsync(Survey survey, IFormCollection form)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //survey attributes
            var data = await GetSurveyAttributesXmlAsync(survey, form);

            return data;
        }

        public virtual async Task<IList<string>> GetSurveyAttributeWarningsAsync(Survey survey, string attributesXml = "",
            bool ignoreNonCombinableAttributes = false, bool ignoreConditionMet = false)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            var warnings = new List<string>();

            //ensure it's our attributes
            var attributes1 = await ParseSurveyAttributeMappingsAsync(attributesXml);
            if (ignoreNonCombinableAttributes)
            {
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();
            }

            foreach (var attribute in attributes1)
            {
                if (attribute.SurveyId == 0)
                {
                    warnings.Add("Attribute error");
                    return warnings;
                }

                if (attribute.SurveyId != survey.Id)
                {
                    warnings.Add("Attribute error");
                }
            }

            //validate required survey attributes (whether they're chosen/selected/entered)
            var attributes2 = await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id);
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
                    //selected survey attributes
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
                        var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(a2.SurveyAttributeId);

                        var textPrompt = await _localizationService.GetLocalizedAsync(a2, x => x.TextPrompt);
                        var notFoundWarning = string.Format(await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.SelectAttribute"), await GetFieldNameAsync(a2, surveyAttribute));

                        warnings.Add(notFoundWarning);
                    }
                }

                if (a2.AttributeControlType != AttributeControlType.ReadonlyCheckboxes)
                    continue;

                //customers cannot edit read-only attributes
                var allowedReadOnlyValueIds = (await _surveyAttributeService.GetSurveyAttributeValuesAsync(a2.Id))
                    .Where(x => x.IsPreSelected)
                    .Select(x => x.Id)
                    .ToArray();

                var selectedReadOnlyValueIds = (await ParseSurveyAttributeValuesAsync(attributesXml))
                    .Where(x => x.SurveyAttributeMappingId == a2.Id)
                    .Select(x => x.Id)
                    .ToArray();

                if (!CommonHelper.ArraysEqual(allowedReadOnlyValueIds, selectedReadOnlyValueIds))
                {
                    warnings.Add("You cannot change read-only values");
                }
            }

            //validation rules
            foreach (var pam in attributes2)
            {
                if (!pam.ValidationRulesAllowed())
                    continue;

                string enteredText;
                int enteredTextLength;

                var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(pam.SurveyAttributeId);

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
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.TextboxMinimumLength"), await GetFieldNameAsync(pam, surveyAttribute), pam.ValidationMinLength.Value));
                        }
                    }

                    //maximum length
                    if (pam.ValidationMaxLength.HasValue)
                    {
                        enteredText = ParseValues(attributesXml, pam.Id).FirstOrDefault();
                        enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (pam.ValidationMaxLength.Value < enteredTextLength)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.TextboxMaximumLength"), await GetFieldNameAsync(pam, surveyAttribute), pam.ValidationMaxLength.Value));
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
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.DatepicketMinimumDate"), await GetFieldNameAsync(pam, surveyAttribute), pam.ValidationMinDate.Value));

                        //maximum date
                        if (pam.ValidationMaxDate.HasValue && pam.ValidationMaxDate.Value < selectedDate)
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.DatepicketMaximumDate"), await GetFieldNameAsync(pam, surveyAttribute), pam.ValidationMaxDate.Value));
                    }
                }
            }

            if (warnings.Any())
                return warnings;

            return warnings;
        }

        #endregion
    }
}