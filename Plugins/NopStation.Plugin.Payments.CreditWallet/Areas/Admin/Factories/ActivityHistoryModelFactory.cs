using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories
{
    public class ActivityHistoryModelFactory : IActivityHistoryModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IActivityHistoryService _activityHistoryService;
        private readonly ICustomerService _customerService;
        private readonly IWalletService _walletService;

        #endregion

        #region Ctor

        public ActivityHistoryModelFactory(IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IActivityHistoryService activityHistoryService,
            ICustomerService customerService,
            IWalletService walletService)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _activityHistoryService = activityHistoryService;
            _customerService = customerService;
            _walletService = walletService;
        }

        #endregion

        #region Methods

        public virtual async Task<ActivityHistorySearchModel> PrepareActivityHistorySearchModelAsync(ActivityHistorySearchModel searchModel,
            Wallet wallet = null)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SearchWalletCustomerId = wallet?.WalletCustomerId ?? 0;
            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.AvailableActivityTypes = (await ActivityType.OrderPlaced.ToSelectListAsync()).ToList();
            searchModel.AvailableActivityTypes.Insert(0,
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0",
                    Selected = true
                });
            searchModel.SelectedActivityTypeIds.Add(0);

            return searchModel;
        }

        public virtual async Task<ActivityHistoryListModel> PrepareActivityHistoryListModelAsync(ActivityHistorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var atids = !searchModel.SelectedActivityTypeIds.Any() ? new List<int>() :
                searchModel.SelectedActivityTypeIds;
            if (atids.Contains(0))
                atids.Remove(0);

            var activityHistorys = await _activityHistoryService.GetAllActivityHistoryAsync(
                customerId: searchModel.SearchWalletCustomerId,
                email: searchModel.SearchEmail,
                atids: atids.ToArray(),
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new ActivityHistoryListModel().PrepareToGridAsync(searchModel, activityHistorys, () =>
            {
                return activityHistorys.SelectAwait(async activityHistory =>
                {
                    var wallet = await _walletService.GetWalletByCustomerIdAsync(activityHistory.WalletCustomerId);
                    return await PrepareActivityHistoryModelAsync(null, activityHistory, wallet, true);
                });
            });

            return model;
        }

        public virtual async Task<ActivityHistoryModel> PrepareActivityHistoryModelAsync(ActivityHistoryModel model, ActivityHistory activityHistory,
            Wallet wallet, bool excludeProperties = false)
        {
            if (activityHistory != null)
            {
                if (model == null)
                {
                    model = activityHistory.ToModel<ActivityHistoryModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(activityHistory.CreatedOnUtc, DateTimeKind.Utc);
                    model.ActivityTypeStr = await _localizationService.GetLocalizedEnumAsync(activityHistory.ActivityType);
                    model.CreditUsed = activityHistory.CurrentTotalCreditUsed - activityHistory.PreviousTotalCreditUsed;

                    var createdBy = await _customerService.GetCustomerByIdAsync(activityHistory.CreatedByCustomerId);
                    model.CreatedByCustomer = createdBy?.Email;

                    var walletCustomer = await _customerService.GetCustomerByIdAsync(wallet.WalletCustomerId);
                    model.WalletCustomerEmail = walletCustomer.Email;
                    model.WalletCustomerName = await _customerService.GetCustomerFullNameAsync(walletCustomer);

                }
            }

            if (!excludeProperties)
            {

            }
            return model;
        }

        #endregion
    }
}