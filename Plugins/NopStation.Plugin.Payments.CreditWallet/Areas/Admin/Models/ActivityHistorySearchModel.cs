using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record ActivityHistorySearchModel : BaseSearchModel
    {
        public ActivityHistorySearchModel()
        {
            SelectedActivityTypeIds = new List<int>();
            AvailableActivityTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.List.SearchWalletCustomer")]
        public int SearchWalletCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.List.SelectedActivityTypes")]
        public IList<int> SelectedActivityTypeIds { get; set; }

        public IList<SelectListItem> AvailableActivityTypes { get; set; }
    }
}
