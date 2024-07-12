using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record QuoteFormModel : BaseNopEntityModel, ILocalizedModel<QuoteFormLocalizedModel>, IStoreMappingSupportedModel
{
    public QuoteFormModel()
    {
        SelectedStoreIds = [];
        Locales = [];
        AvailableEmailAccounts = [];
        SelectedCustomerRoleIds = [];
    }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.SelectedStoreIds")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<QuoteFormLocalizedModel> Locales { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Info")]
    public string Info { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.IsActive")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SubmitButtonText")]
    public string SubmitButtonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SendEmailToCustomer")]
    public bool SendEmailToCustomer { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SendEmailToStoreOwner")]
    public bool SendEmailToStoreOwner { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.StoreOwnerEmail")]
    public string StoreOwnerEmailAddress { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.ShowTermsAndConditionsCheckbox")]
    public bool ShowTermsAndConditionsCheckbox { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.TermsAndConditions")]
    public string TermsAndConditions { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.DefaultEmailAccountId")]
    public int DefaultEmailAccountId { get; set; }

    public IList<SelectListItem> AvailableEmailAccounts { get; set; }

    public FormAttributeMappingSearchModel FormAttributeMappingSearchModel { get; set; }

    public IList<int> SelectedCustomerRoleIds { get; set; }
}

public class QuoteFormLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Info")]
    public string Info { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SubmitButtonText")]
    public string SubmitButtonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.ShowTermsAndConditionsCheckbox")]
    public bool ShowTermsAndConditionsCheckbox { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.TermsAndConditions")]
    public string TermsAndConditions { get; set; }
}
