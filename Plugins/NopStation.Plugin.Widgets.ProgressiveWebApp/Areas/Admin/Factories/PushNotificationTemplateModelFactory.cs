using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public class PushNotificationTemplateModelFactory : IPushNotificationTemplateModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public PushNotificationTemplateModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPushNotificationTemplateService pushNotificationTemplateService,
            ILocalizedModelFactory localizedModelFactory,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _localizedModelFactory = localizedModelFactory;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare pushNotificationTemplate search model
        /// </summary>
        /// <param name="searchModel">PushNotificationTemplate search model</param>
        /// <returns>PushNotificationTemplate search model</returns>
        public virtual PushNotificationTemplateSearchModel PreparePushNotificationTemplateSearchModel(PushNotificationTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged pushNotificationTemplate list model
        /// </summary>
        /// <param name="searchModel">PushNotificationTemplate search model</param>
        /// <returns>PushNotificationTemplate list model</returns>
        public virtual async Task<PushNotificationTemplateListModel> PreparePushNotificationTemplateListModelAsync(PushNotificationTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get pushNotificationTemplates
            var pushNotificationTemplates = (await _pushNotificationTemplateService.GetAllPushNotificationTemplatesAsync(0)).ToPagedList(searchModel);

            //prepare list model
            var model = await new PushNotificationTemplateListModel().PrepareToGridAsync(searchModel, pushNotificationTemplates, () =>
            {
                return pushNotificationTemplates.SelectAwait(async pushNotificationTemplate =>
                {
                    //fill in model values from the entity
                    return await PreparePushNotificationTemplateModelAsync(null, pushNotificationTemplate, true);
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare pushNotificationTemplate model
        /// </summary>
        /// <param name="model">PushNotificationTemplate model</param>
        /// <param name="pushNotificationTemplate">PushNotificationTemplate</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>PushNotificationTemplate model</returns>
        public virtual async Task<PushNotificationTemplateModel> PreparePushNotificationTemplateModelAsync(PushNotificationTemplateModel model,
            PushNotificationTemplate pushNotificationTemplate, bool excludeProperties = false)
        {
            Func<PushNotificationTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (pushNotificationTemplate != null)
            {
                //fill in model values from the entity
                model = model ?? pushNotificationTemplate.ToModel<PushNotificationTemplateModel>();
                model.Name = pushNotificationTemplate.Name;

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Title = await _localizationService.GetLocalizedAsync(pushNotificationTemplate, entity => entity.Title, languageId, false, false);
                    locale.Body = await _localizationService.GetLocalizedAsync(pushNotificationTemplate, entity => entity.Body, languageId, false, false);
                };
            }

            if (!excludeProperties)
            {
                var allowedTokens = string.Join(", ", _pushNotificationTokenProvider.GetListOfAllowedTokens(_pushNotificationTokenProvider.GetTokenGroups(pushNotificationTemplate)));
                model.AllowedTokens = $"{allowedTokens}{Environment.NewLine}{Environment.NewLine}" +
                    $"{await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement")}{Environment.NewLine}";

                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            }

            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, pushNotificationTemplate, excludeProperties);

            return model;
        }

        #endregion
    }
}
