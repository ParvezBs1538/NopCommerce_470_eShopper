using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteFormModel : BaseNopEntityModel
{

    public QuoteFormModel()
    {
        QuoteFormAttributes = [];
    }

    public string Title { get; set; }

    public string Info { get; set; }

    public bool Active { get; set; }

    public bool LimitedToStores { get; set; }

    public bool SendEmailToCustomer { get; set; }

    public bool SendEmailToStoreOwner { get; set; }

    public string StoreOwnerEmailAddress { get; set; }

    public string SubmitButtonText { get; set; }

    public bool ShowTermsAndConditionsCheckbox { get; set; }

    public string TermsAndConditions { get; set; }

    public int DefaultEmailAccountId { get; set; }

    public int DisplayOrder { get; set; }

    public bool ShowGuestEmailField { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.QuoteCart.QuoteForm.Fields.GuestEmail")]
    public string GuestEmail { get; set; }

    public IList<QuoteFormAttributeModel> QuoteFormAttributes { get; set; }
}


public partial record QuoteFormAttributeModel : BaseNopEntityModel
{
    public QuoteFormAttributeModel()
    {
        AllowedFileExtensions = [];
        Values = [];
    }

    public int QuoteFormId { get; set; }

    public int QuoteFormAttributeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string TextPrompt { get; set; }

    public bool IsRequired { get; set; }

    public string DefaultValue { get; set; }

    public int? SelectedDay { get; set; }

    public int? SelectedMonth { get; set; }

    public int? SelectedYear { get; set; }

    public DateTime? ValidationMinDate { get; set; }

    public DateTime? ValidationMaxDate { get; set; }

    public bool HasCondition { get; set; }

    public IList<string> AllowedFileExtensions { get; set; }

    public int AttributeControlTypeId { get; set; }

    public AttributeControlType AttributeControlType
    {
        get => (AttributeControlType)AttributeControlTypeId;
        set => AttributeControlTypeId = (int)value;
    }


    public IList<QuoteFormAttributeValueModel> Values { get; set; }
}

public partial record QuoteFormAttributeValueModel : BaseNopEntityModel
{
    public QuoteFormAttributeValueModel()
    {
        ImageSquaresPictureModel = new ();
    }

    public string Name { get; set; }

    public PictureModel ImageSquaresPictureModel { get; set; }

    public string ColorSquaresRgb { get; set; }

    public bool IsPreSelected { get; set; }
}
