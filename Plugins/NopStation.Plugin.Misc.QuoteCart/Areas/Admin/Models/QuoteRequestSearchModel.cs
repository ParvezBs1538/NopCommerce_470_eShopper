using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record QuoteRequestSearchModel : BaseSearchModel
{
    #region Ctor

    public QuoteRequestSearchModel()
    {
        SearchRequestStatusIds = [];
        AvailableRequestStatuses = [];
        AvailableStores = [];
        AvailableForms = [];
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.StartDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchStartDate { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchEndDate { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestStatus")]
    public List<int> SearchRequestStatusIds { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Store")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.CustomerEmail")]
    public string SearchCustomerEmail { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.FormId")]
    public int SearchFormId { get; set; }

    public IList<SelectListItem> AvailableRequestStatuses { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableForms { get; set; }

    public bool HideStoresList { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.GoDirectlyToCustomRequestNumber")]
    public string GoDirectlyToCustomRequestNumber { get; set; }

    #endregion
}
