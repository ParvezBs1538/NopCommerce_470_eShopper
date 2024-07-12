using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial class SurveyAttributeFormatter : ISurveyAttributeFormatter
    {
        #region Fields

        private readonly IDownloadService _downloadService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SurveyAttributeFormatter(IDownloadService downloadService,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            ISurveyAttributeParser surveyAttributeParser,
            ISurveyAttributeService surveyAttributeService,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _downloadService = downloadService;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _surveyAttributeParser = surveyAttributeParser;
            _surveyAttributeService = surveyAttributeService;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual async Task<string> FormatAttributesAsync(Survey survey, string attributesXml)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return await FormatAttributesAsync(survey, attributesXml, customer);
        }

        public virtual async Task<string> FormatAttributesAsync(Survey survey, string attributesXml,
            Customer customer, string separator = "<br />", bool htmlEncode = true,
            bool renderSurveyAttributes = true, bool allowHyperlinks = true)
        {
            var result = new StringBuilder();
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            //attributes
            if (renderSurveyAttributes)
            {
                foreach (var attribute in await _surveyAttributeParser.ParseSurveyAttributeMappingsAsync(attributesXml))
                {
                    var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(attribute.SurveyAttributeId);
                    var attributeName = await _localizationService.GetLocalizedAsync(attribute, a => a.TextPrompt, currentLanguage.Id);
                    if (string.IsNullOrEmpty(attributeName))
                        attributeName = await _localizationService.GetLocalizedAsync(surveyAttribute, a => a.Name, currentLanguage.Id);

                    //attributes without values
                    if (!attribute.ShouldHaveValues())
                    {
                        foreach (var value in _surveyAttributeParser.ParseValues(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = string.Empty;

                            var formattedValues = new List<string>();

                            if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = WebUtility.HtmlEncode(attributeName);

                                //we never encode multiline textbox input
                                formattedValues.Add(_htmlFormatter.FormatText(value, false, true, false, false, false, false));
                            }
                            else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                //file upload
                                _ = Guid.TryParse(value, out var downloadGuid);
                                var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                if (download != null)
                                {
                                    var fileName = $"{download.Filename ?? download.DownloadGuid.ToString()}{download.Extension}";

                                    //encode (if required)
                                    if (htmlEncode)
                                        fileName = WebUtility.HtmlEncode(fileName);

                                    var attributeText = allowHyperlinks ? $"<a href=\"{_webHelper.GetStoreLocation()}download/getfileupload/?downloadId={download.DownloadGuid}\" class=\"fileuploadattribute\">{fileName}</a>"
                                        : fileName;

                                    //encode (if required)
                                    if (htmlEncode)
                                        attributeName = WebUtility.HtmlEncode(attributeName);

                                    formattedValues.Add(attributeText);
                                }
                            }
                            else
                            {
                                //other attributes (textbox, datepicker)
                                formattedAttribute = value;

                                //encode (if required)
                                if (htmlEncode)
                                    formattedValues.Add(WebUtility.HtmlEncode(formattedAttribute));
                            }

                            if (formattedValues.Count <= 0)
                                continue;

                            formattedAttribute = $"{attributeName}: {string.Join(", ", formattedValues)}";

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                    //survey attribute values
                    else
                    {
                        var formattedValues = new List<string>();

                        foreach (var attributeValue in await _surveyAttributeParser.ParseSurveyAttributeValuesAsync(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = $"{await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, currentLanguage.Id)}";

                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            formattedValues.Add(formattedAttribute);
                        }

                        if (result.Length > 0)
                            result.Append(separator);
                        result.Append($"{attributeName}: {string.Join(", ", formattedValues)}");
                    }
                }
            }

            return result.ToString();
        }

        public virtual async Task<IDictionary<string, List<string>>> GetAttributeValuesAsync(string attributesXml, bool htmlEncode = true)
        {
            var result = new Dictionary<string, List<string>>();
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            //attributes

            foreach (var attribute in await _surveyAttributeParser.ParseSurveyAttributeMappingsAsync(attributesXml))
            {
                var surveyAttribute = await _surveyAttributeService.GetSurveyAttributeByIdAsync(attribute.SurveyAttributeId);
                var attributeName = await _localizationService.GetLocalizedAsync(surveyAttribute, a => a.Name, currentLanguage.Id);

                //attributes without values
                if (!attribute.ShouldHaveValues())
                {
                    foreach (var value in _surveyAttributeParser.ParseValues(attributesXml, attribute.Id))
                    {
                        var formattedAttribute = string.Empty;

                        var formattedValues = new List<string>();

                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);

                            //we never encode multiline textbox input
                            formattedValues.Add(_htmlFormatter.FormatText(value, false, true, false, false, false, false));
                        }
                        else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            _ = Guid.TryParse(value, out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                            {
                                var fileName = $"{download.Filename ?? download.DownloadGuid.ToString()}{download.Extension}";

                                //encode (if required)
                                if (htmlEncode)
                                    fileName = WebUtility.HtmlEncode(fileName);

                                var attributeText = fileName;

                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = WebUtility.HtmlEncode(attributeName);

                                formattedValues.Add(attributeText);
                            }
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = value;

                            //encode (if required)
                            if (htmlEncode)
                                formattedValues.Add(WebUtility.HtmlEncode(formattedAttribute));
                        }

                        if (formattedValues.Count <= 0)
                            continue;

                        result[attributeName] = formattedValues;
                    }
                }
                //survey attribute values
                else
                {
                    var formattedValues = new List<string>();

                    foreach (var attributeValue in await _surveyAttributeParser.ParseSurveyAttributeValuesAsync(attributesXml, attribute.Id))
                    {
                        var formattedAttribute = $"{await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, currentLanguage.Id)}";

                        //encode (if required)
                        if (htmlEncode)
                            formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);

                        if (string.IsNullOrEmpty(formattedAttribute))
                            continue;

                        formattedValues.Add(formattedAttribute);

                        result[attributeName] = formattedValues;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}