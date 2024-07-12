using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.TagHelpers;

[HtmlTargetElement("nop-div", Attributes = FOR_ATTRIBUTE_NAME, TagStructure = TagStructure.WithoutEndTag)]
public class NopDivTagHelper : TagHelper
{
    #region Constants

    private const string FOR_ATTRIBUTE_NAME = "asp-for";
    private const string DISPLAY_CHECK_ATTRIBUTE_NAME = "asp-check-access";
    private const string DISPLAY_VALUE_ATTRIBUTE_NAME = "asp-value";

    #endregion

    #region Properties

    protected IHtmlGenerator Generator { get; set; }

    /// <summary>
    /// An expression to be evaluated against the current model
    /// </summary>
    [HtmlAttributeName(FOR_ATTRIBUTE_NAME)]
    public ModelExpression For { get; set; }

    [HtmlAttributeName(DISPLAY_CHECK_ATTRIBUTE_NAME)]
    public bool CheckAccess { get; set; }

    [HtmlAttributeName(DISPLAY_VALUE_ATTRIBUTE_NAME)]
    public string Value { get; set; }

    /// <summary>
    /// ViewContext
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    #endregion

    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public NopDivTagHelper(IHtmlGenerator generator,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IPermissionService permissionService)
    {
        Generator = generator;
        _localizationService = localizationService;
        _workContext = workContext;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Asynchronously executes the tag helper with the given context and output
    /// </summary>
    /// <param name="context">Contains information associated with the current HTML tag</param>
    /// <param name="output">A stateful HTML element used to generate an HTML tag</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (output == null)
            throw new ArgumentNullException(nameof(output));

        //create a label wrapper
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        //merge classes
        var classValue = output.Attributes.ContainsName("class")
            ? $"{output.Attributes["class"].Value} form-text-row"
            : "form-text-row";
        output.Attributes.SetAttribute("class", classValue);

        var value = string.IsNullOrWhiteSpace(Value) ? For.Model : Value;

        //add hint
        if (CheckAccess && !await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageNopStationFeatures))
            value = "<i>hidden text...</i>";

        output.Content.AppendHtml(value?.ToString() ?? "");
    }

    #endregion
}