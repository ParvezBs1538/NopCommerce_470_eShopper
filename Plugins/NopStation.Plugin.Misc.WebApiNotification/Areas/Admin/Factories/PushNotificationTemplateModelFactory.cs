using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public class PushNotificationTemplateModelFactory : IPushNotificationTemplateModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public PushNotificationTemplateModelFactory(ILocalizationService localizationService,
            IPushNotificationTemplateService pushNotificationTemplateService,
            ILocalizedModelFactory localizedModelFactory,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory)
        {
            _localizationService = localizationService;
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _localizedModelFactory = localizedModelFactory;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Methods

        public virtual async Task<WebApiNotificationTemplateSearchModel> PreparePushNotificationTemplateSearchModelAsync(WebApiNotificationTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            searchModel.AvailableActiveTypes.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
            searchModel.AvailableActiveTypes.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId.Active"),
                Value = "1"
            });
            searchModel.AvailableActiveTypes.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId.Inactive"),
                Value = "2"
            });

            return searchModel;
        }

        public virtual async Task<WebApiNotificationTemplateListModel> PreparePushNotificationTemplateListModelAsync(WebApiNotificationTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            bool? active = null;
            if (searchModel.SearchActiveId == 1)
                active = true;
            if (searchModel.SearchActiveId == 2)
                active = false;

            //get WebApiNotificationTemplates
            var pushNotificationTemplates = await _pushNotificationTemplateService.GetAllPushNotificationTemplatesAsync(searchModel.SearchKeyword,
                active, 0, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new WebApiNotificationTemplateListModel().PrepareToGridAsync(searchModel, pushNotificationTemplates, () =>
            {
                return pushNotificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    //fill in model values from the entity
                    return await PreparePushNotificationTemplateModelAsync(null, pushNotificationTemplate, true);
                });
            });

            return model;
        }

        public virtual async Task<WebApiNotificationTemplateModel> PreparePushNotificationTemplateModelAsync(WebApiNotificationTemplateModel model,
            WebApiNotificationTemplate pushNotificationTemplate, bool excludeProperties = false)
        {
            Func<WebApiNotificationTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (pushNotificationTemplate != null)
            {
                if (model == null)
                {
                    model = pushNotificationTemplate.ToModel<WebApiNotificationTemplateModel>();
                    model.Name = pushNotificationTemplate.Name;

                    if (!excludeProperties)
                    {
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.Title = await _localizationService.GetLocalizedAsync(pushNotificationTemplate, entity => entity.Title, languageId, false, false);
                            locale.Body = await _localizationService.GetLocalizedAsync(pushNotificationTemplate, entity => entity.Body, languageId, false, false);
                        };
                    }
                }
            }

            if (!excludeProperties)
            {
                var allowedTokens = string.Join(", ", _pushNotificationTokenProvider.GetTokenGroups(pushNotificationTemplate));
                model.AllowedTokens = $"{allowedTokens}{Environment.NewLine}{Environment.NewLine}" +
                    $"{await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement")}{Environment.NewLine}";

                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, pushNotificationTemplate, excludeProperties);
                model.AvailableActionTypes = (await NotificationActionType.None.ToSelectListAsync()).ToList();
            }

            return model;
        }

        #endregion
    }
}
