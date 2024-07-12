using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public class PushNotificationCampaignModelFactory : IPushNotificationCampaignModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPushNotificationCampaignService _pushNotificationCampaignService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public PushNotificationCampaignModelFactory(IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPushNotificationCampaignService pushNotificationCampaignService,
            ILocalizedModelFactory localizedModelFactory,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _pushNotificationCampaignService = pushNotificationCampaignService;
            _localizedModelFactory = localizedModelFactory;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        public virtual WebApiNotificationCampaignSearchModel PreparePushNotificationCampaignSearchModel(WebApiNotificationCampaignSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<WebApiNotificationCampaignListModel> PreparePushNotificationCampaignListModelAsync(WebApiNotificationCampaignSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var searchFrom = !searchModel.SearchSendStartFromDate.HasValue ? (DateTime?)null :
                _dateTimeHelper.ConvertToUtcTime(searchModel.SearchSendStartFromDate.Value);
            var searchTo = !searchModel.SearchSendStartToDate.HasValue ? (DateTime?)null :
                _dateTimeHelper.ConvertToUtcTime(searchModel.SearchSendStartToDate.Value);

            //get WebApiNotificationCampaigns
            var pushNotificationCampaigns = await _pushNotificationCampaignService.GetAllPushNotificationCampaignsAsync(
                keyword: searchModel.SearchKeyword,
                searchFrom: searchFrom,
                searchTo: searchTo,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new WebApiNotificationCampaignListModel().PrepareToGridAsync(searchModel, pushNotificationCampaigns, () =>
            {
                return pushNotificationCampaigns.SelectAwait(async pushNotificationCampaign =>
                {
                    return await PreparePushNotificationCampaignModelAsync(null, pushNotificationCampaign, true);
                });
            });

            return model;
        }

        public virtual async Task<WebApiNotificationCampaignModel> PreparePushNotificationCampaignModelAsync(WebApiNotificationCampaignModel model,
            WebApiNotificationCampaign pushNotificationCampaign, bool excludeProperties = false)
        {
            Func<WebApiNotificationCampaignLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (pushNotificationCampaign != null)
            {
                if (model == null)
                {
                    model = pushNotificationCampaign.ToModel<WebApiNotificationCampaignModel>();
                    model.SendingWillStartOn = await _dateTimeHelper.ConvertToUserTimeAsync(pushNotificationCampaign.SendingWillStartOnUtc, DateTimeKind.Utc);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(pushNotificationCampaign.CreatedOnUtc, DateTimeKind.Utc);

                    if (!string.IsNullOrWhiteSpace(pushNotificationCampaign.CustomerRoles))
                        model.CustomerRoles = pushNotificationCampaign.CustomerRoles
                            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                    if (!string.IsNullOrWhiteSpace(pushNotificationCampaign.DeviceTypes))
                        model.DeviceTypes = pushNotificationCampaign.DeviceTypes
                            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                    if (pushNotificationCampaign.AddedToQueueOnUtc.HasValue)
                        model.AddedToQueueOn = await _dateTimeHelper.ConvertToUserTimeAsync(pushNotificationCampaign.AddedToQueueOnUtc.Value, DateTimeKind.Utc);

                    model.CopyPushNotificationCampaignModel.Id = pushNotificationCampaign.Id;
                    model.CopyPushNotificationCampaignModel.Name = $"{pushNotificationCampaign.Name} - Copy";

                    if (!excludeProperties)
                    {
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.Title = await _localizationService.GetLocalizedAsync(pushNotificationCampaign, entity => entity.Title, languageId, false, false);
                            locale.Body = await _localizationService.GetLocalizedAsync(pushNotificationCampaign, entity => entity.Body, languageId, false, false);
                        };
                    }
                }
            }

            if (!excludeProperties)
            {
                var allowedTokens = string.Join(", ", _pushNotificationTokenProvider.GetListOfAllowedTokens(new[] { WebApiNotificationTokenGroupNames.StoreTokens, WebApiNotificationTokenGroupNames.CustomerTokens }));
                model.AllowedTokens = $"{allowedTokens}{Environment.NewLine}{Environment.NewLine}" +
                    $"{await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement")}{Environment.NewLine}";

                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);
                await _baseAdminModelFactory.PrepareCustomerRolesAsync(model.AvailableCustomerRoles, false);
                model.AvailableDeviceTypes = (await DeviceType.Android.ToSelectListAsync()).ToList();
                model.AvailableActionTypes = (await NotificationActionType.None.ToSelectListAsync()).ToList();
            }

            return model;
        }

        #endregion
    }
}
