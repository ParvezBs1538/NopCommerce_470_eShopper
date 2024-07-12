using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record FormAttributeMappingModel : BaseNopEntityModel, ILocalizedModel<FormAttributeMappingLocalizedModel>
{
    #region Ctor

    public FormAttributeMappingModel()
    {
        AvailableFormAttributes = [];
        ConditionModel = new ();
        FormAttributeValueSearchModel = new ();
        Locales = [];
    }

    #endregion

    #region Properties

    public int FormId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.Attribute")]
    public int FormAttributeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.Attribute")]
    public string FormAttribute { get; set; }

    public IList<SelectListItem> AvailableFormAttributes { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.TextPrompt")]
    public string TextPrompt { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.IsRequired")]
    public bool IsRequired { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.AttributeControlType")]
    public int AttributeControlTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.AttributeControlType")]
    public string AttributeControlType { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    //validation fields
    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinLength")]
    [UIHint("Int32Nullable")]
    public int? ValidationMinLength { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxLength")]
    [UIHint("Int32Nullable")]
    public int? ValidationMaxLength { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultDate")]
    [UIHint("DateNullable")]
    public DateTime? DefaultDate { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinDate")]
    [UIHint("DateNullable")]
    public DateTime? ValidationMinDate { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxDate")]
    [UIHint("DateNullable")]
    public DateTime? ValidationMaxDate { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileAllowedExtensions")]
    public string ValidationFileAllowedExtensions { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileMaximumSize")]
    [UIHint("Int32Nullable")]
    public int? ValidationFileMaximumSize { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultValue")]
    public string DefaultValue { get; set; }

    public string ValidationRulesString { get; set; }

    //condition
    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition")]
    public bool ConditionAllowed { get; set; }

    public string ConditionString { get; set; }

    public IList<FormAttributeMappingLocalizedModel> Locales { get; set; }

    public FormAttributeConditionModel ConditionModel { get; set; }

    public FormAttributeValueSearchModel FormAttributeValueSearchModel { get; set; }

    #endregion
}

public partial record FormAttributeMappingLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.TextPrompt")]
    public string TextPrompt { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultValue")]
    public string DefaultValue { get; set; }
}
