using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormFieldModel : BaseNopEntityModel, ILocalizedModel<FormFieldLocalizedModel>
{
    public FormFieldModel()
    {
        AvailableControlTypes = [];
        Locales = [];
    }

    public int FormId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.TextPrompt")]
    public string TextPrompt { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.IsRequired")]
    public bool IsRequired { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.FieldControlType")]
    public int FieldControlTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.FieldControlType")]
    public string FieldControlType { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.DefaultValue")]
    public string DefaultValue { get; set; }

    //validation fields

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.ValidationRules.MinLength")]
    public int? ValidationMinLength { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.ValidationRules.MaxLength")]
    public int? ValidationMaxLength { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.ValidationRules.FileAllowedExtensions")]
    public string ValidationFileAllowedExtensions { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.ValidationRules.FileMaximumSize")]
    public int? ValidationFileMaximumSize { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.Condition")]
    public string ConditionAttributeXml { get; set; }

    public IList<FormFieldLocalizedModel> Locales { get; set; }

    public IList<SelectListItem> AvailableControlTypes { get; set; }

    public FormFieldValueSearchModel FieldValueSearchModel { get; set; }
}

public partial record FormFieldLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.TextPrompt")]
    public string TextPrompt { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormField.Fields.DefaultValue")]
    public string DefaultValue { get; set; }
}
