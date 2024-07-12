using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Infrastructure;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public class FormAttributeFormatter : IFormAttributeFormatter
{
    #region Fields

    private readonly IDownloadService _downloadService;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IFormAttributeService _formAttributeService;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public FormAttributeFormatter(
        IDownloadService downloadService,
        IFormAttributeParser formAttributeParser,
        IFormAttributeService formAttributeService,
        IHtmlFormatter htmlFormatter,
        ILocalizationService localizationService,
        IWebHelper webHelper,
        IWorkContext workContext)
    {
        _downloadService = downloadService;
        _formAttributeParser = formAttributeParser;
        _formAttributeService = formAttributeService;
        _htmlFormatter = htmlFormatter;
        _localizationService = localizationService;
        _webHelper = webHelper;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public virtual async Task<string> FormatAttributesAsync(QuoteForm quoteForm, string attributesXml)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return await FormatAttributesAsync(quoteForm, attributesXml, customer);
    }

    public virtual async Task<string> FormatAttributesAsync(QuoteForm quoteForm, string attributesXml,
        Customer customer, string separator = "<br />", bool htmlEncode = true,
        bool renderFormAttributes = true, bool allowHyperlinks = true)
    {
        var result = new StringBuilder();
        var currentLanguage = await _workContext.GetWorkingLanguageAsync();
        //attributes
        if (renderFormAttributes)
        {
            foreach (var attribute in await _formAttributeParser.ParseFormAttributeMappingsAsync(attributesXml))
            {
                var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(attribute.FormAttributeId);
                var attributeName = await _localizationService.GetLocalizedAsync(formAttribute, a => a.Name, currentLanguage.Id);

                //attributes without values
                if (!attribute.ShouldHaveValues())
                {
                    foreach (var value in _formAttributeParser.ParseValues(attributesXml, attribute.Id))
                    {
                        var formattedAttribute = string.Empty;
                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);

                            //we never encode multiline textbox input
                            formattedAttribute = $"{attributeName}: {_htmlFormatter.FormatText(value, false, true, false, false, false, false)}";
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

                                formattedAttribute = $"{attributeName}: {attributeText}";
                            }
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = $"{attributeName}: {value}";

                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }

                        if (string.IsNullOrEmpty(formattedAttribute))
                            continue;

                        if (result.Length > 0)
                            result.Append(separator);
                        result.Append(formattedAttribute);
                    }
                }
                //form attribute values
                else
                {
                    foreach (var attributeValue in await _formAttributeParser.ParseFormAttributeValuesAsync(attributesXml, attribute.Id))
                    {
                        var formattedAttribute = $"{attributeName}: {await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, currentLanguage.Id)}";

                        //encode (if required)
                        if (htmlEncode)
                            formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);

                        if (string.IsNullOrEmpty(formattedAttribute))
                            continue;

                        if (result.Length > 0)
                            result.Append(separator);
                        result.Append(formattedAttribute);
                    }
                }
            }
        }

        return result.ToString();
    }

    #endregion
}
