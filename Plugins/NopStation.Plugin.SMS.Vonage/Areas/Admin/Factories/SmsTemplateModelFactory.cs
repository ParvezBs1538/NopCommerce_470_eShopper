using NopStation.Plugin.SMS.Vonage.Areas.Admin.Models;
using NopStation.Plugin.SMS.Vonage.Domains;
using NopStation.Plugin.SMS.Vonage.Services;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Vonage.Areas.Admin.Factories
{
    public class SmsTemplateModelFactory : ISmsTemplateModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISmsTokenProvider _vonagesmsTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;

        #endregion

        #region Ctor

        public SmsTemplateModelFactory(ILocalizationService localizationService,
            ISmsTemplateService smsTemplateService,
            ILocalizedModelFactory localizedModelFactory,
            ISmsTokenProvider vonagesmsTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IAclSupportedModelFactory aclSupportedModelFactory)
        {
            _localizationService = localizationService;
            _smsTemplateService = smsTemplateService;
            _localizedModelFactory = localizedModelFactory;
            _vonagesmsTokenProvider = vonagesmsTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _aclSupportedModelFactory = aclSupportedModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare SmsTemplate search model
        /// </summary>
        /// <param name="searchModel">SmsTemplate search model</param>
        /// <returns>SmsTemplate search model</returns>
        public virtual SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged SmsTemplate list model
        /// </summary>
        /// <param name="searchModel">SmsTemplate search model</param>
        /// <returns>SmsTemplate list model</returns>
        public virtual async Task<SmsTemplateListModel> PrepareSmsTemplateListModelAsync(SmsTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get SmsTemplates
            var smsTemplates = (await _smsTemplateService.GetAllSmsTemplatesAsync(0)).ToPagedList(searchModel);

            //prepare list model
            var model = new SmsTemplateListModel().PrepareToGridAsync(searchModel, smsTemplates, () =>
            {
                return smsTemplates.SelectAwait(async smsTemplate =>
                {
                    //fill in model values from the entity
                    return await PrepareSmsTemplateModelAsync(null, smsTemplate, true);
                });
            });

            return await model;
        }

        /// <summary>
        /// Prepare SmsTemplate model
        /// </summary>
        /// <param name="model">SmsTemplate model</param>
        /// <param name="smsTemplate">SmsTemplate</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>SmsTemplate model</returns>
        public virtual async Task<SmsTemplateModel> PrepareSmsTemplateModelAsync(SmsTemplateModel model, 
            SmsTemplate smsTemplate, bool excludeProperties = false)
        {
            Func<SmsTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (smsTemplate != null)
            {
                //fill in model values from the entity
                model = model ?? smsTemplate.ToModel<SmsTemplateModel>();
                model.Name = smsTemplate.Name;

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Body = await _localizationService.GetLocalizedAsync(smsTemplate, entity => entity.Body, languageId, false, false);
                };
            }

            if (!excludeProperties)
            {
                var allowedTokens = string.Join(", ", _vonagesmsTokenProvider.GetListOfAllowedTokens(_vonagesmsTokenProvider.GetTokenGroups(smsTemplate)));
                model.AllowedTokens = $"{allowedTokens}{Environment.NewLine}{Environment.NewLine}" +
                    $"{await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement")}{Environment.NewLine}";
                
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            }

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, smsTemplate, excludeProperties);

            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, smsTemplate, excludeProperties);

            return model;
        }

        #endregion
    }
}
