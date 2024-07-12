using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories
{
    public class SmartGroupNotificationModelFactory : ISmartGroupNotificationModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ISmartGroupNotificationService _smartGroupNotificationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly IPushNopDeviceService _webAppDeviceService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ISmartGroupService _smartGroupService;

        #endregion

        #region Ctor

        public SmartGroupNotificationModelFactory(IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ISmartGroupNotificationService smartGroupNotificationService,
            ILocalizedModelFactory localizedModelFactory,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IPushNopDeviceService webAppDeviceService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ISmartGroupService smartGroupService)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _smartGroupNotificationService = smartGroupNotificationService;
            _localizedModelFactory = localizedModelFactory;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _webAppDeviceService = webAppDeviceService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _smartGroupService = smartGroupService;
        }

        #endregion

        #region Methods

        public virtual Task<GroupNotificationSearchModel> PrepareSmartGroupNotificationSearchModelAsync(GroupNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<GroupNotificationListModel> PrepareSmartGroupNotificationListModelAsync(GroupNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get pushNotificationCampaigns
            var smartGroupNotifications = (await _smartGroupNotificationService.GetAllSmartGroupNotificationsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize)).ToPagedList(searchModel);

            //prepare list model
            var model = await new GroupNotificationListModel().PrepareToGridAsync(searchModel, smartGroupNotifications, () =>
            {
                return smartGroupNotifications.SelectAwait(async smartGroupNotification =>
                {
                    return await PrepareSmartGroupNotificationModelAsync(null, smartGroupNotification, true);
                });
            });

            return model;
        }

        public virtual async Task<GroupNotificationModel> PrepareSmartGroupNotificationModelAsync(GroupNotificationModel model,
            SmartGroupNotification smartGroupNotification, bool excludeProperties = false)
        {
            Func<GroupNotificationLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (smartGroupNotification != null)
            {
                if (model == null)
                {
                    model = smartGroupNotification.ToModel<GroupNotificationModel>();
                    model.SendingWillStartOn = await _dateTimeHelper.ConvertToUserTimeAsync(smartGroupNotification.SendingWillStartOnUtc, DateTimeKind.Utc);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(smartGroupNotification.CreatedOnUtc, DateTimeKind.Utc);

                    if (smartGroupNotification.AddedToQueueOnUtc.HasValue)
                        model.AddedToQueueOn = await _dateTimeHelper.ConvertToUserTimeAsync(smartGroupNotification.AddedToQueueOnUtc.Value, DateTimeKind.Utc);
                    if (!model.SendToAll)
                    {
                        var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(smartGroupNotification.SmartGroupId.Value);
                        model.SmartGroupName = smartGroup?.Name;
                    }

                    model.Subscriptions = (await _webAppDeviceService.GetCampaignDevicesAsync(smartGroupNotification, 0, 1)).TotalCount;
                    model.CopySmartGroupNotificationModel.Id = smartGroupNotification.Id;
                    model.CopySmartGroupNotificationModel.Name = $"{smartGroupNotification.Name} - Copy";

                    if (!excludeProperties)
                    {
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.Title = await _localizationService.GetLocalizedAsync(smartGroupNotification, entity => entity.Title, languageId, false, false);
                            locale.Body = await _localizationService.GetLocalizedAsync(smartGroupNotification, entity => entity.Body, languageId, false, false);
                        };
                    }
                }
            }

            if (!excludeProperties)
            {
                var allowedTokens = string.Join(", ", _pushNotificationTokenProvider.GetListOfAllowedTokens(new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens }));
                model.AllowedTokens = $"{allowedTokens}{Environment.NewLine}{Environment.NewLine}" +
                    $"{await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement")}{Environment.NewLine}";

                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);

                var smartGroups = await _smartGroupService.GetAllSmartGroupsAsync();
                foreach (var smartGroup in smartGroups)
                {
                    model.AvailableSmartGroups.Add(new SelectListItem
                    {
                        Text = smartGroup.Name,
                        Value = smartGroup.Id.ToString()
                    });
                }
                model.AvailableSmartGroups.Insert(0, new SelectListItem()
                {
                    Text = "----",
                    Value = ""
                });
            }

            return model;
        }

        #endregion
    }
}
