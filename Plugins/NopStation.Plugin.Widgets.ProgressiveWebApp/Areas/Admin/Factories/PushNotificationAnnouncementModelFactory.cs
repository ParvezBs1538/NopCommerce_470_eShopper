using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public class PushNotificationAnnouncementModelFactory : IPushNotificationAnnouncementModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPushNotificationAnnouncementService _pushNotificationAnnouncementService;

        #endregion

        #region Ctor

        public PushNotificationAnnouncementModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPushNotificationAnnouncementService pushNotificationAnnouncementService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _pushNotificationAnnouncementService = pushNotificationAnnouncementService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare pushNotificationAnnouncement search model
        /// </summary>
        /// <param name="searchModel">PushNotificationAnnouncement search model</param>
        /// <returns>PushNotificationAnnouncement search model</returns>
        public virtual PushNotificationAnnouncementSearchModel PreparePushNotificationAnnouncementSearchModel(PushNotificationAnnouncementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged pushNotificationAnnouncement list model
        /// </summary>
        /// <param name="searchModel">PushNotificationAnnouncement search model</param>
        /// <returns>PushNotificationAnnouncement list model</returns>
        public virtual async Task<PushNotificationAnnouncementListModel> PreparePushNotificationAnnouncementListModelAsync(PushNotificationAnnouncementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get pushNotificationAnnouncements
            var pushNotificationAnnouncements = await _pushNotificationAnnouncementService.GetAllPushNotificationAnnouncementsAsync(searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new PushNotificationAnnouncementListModel().PrepareToGridAsync(searchModel, pushNotificationAnnouncements, () =>
            {
                return pushNotificationAnnouncements.SelectAwait(async pushNotificationAnnouncement =>
                {
                    return await PreparePushNotificationAnnouncementModelAsync(null, pushNotificationAnnouncement, true);
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare pushNotificationAnnouncement model
        /// </summary>
        /// <param name="model">PushNotificationAnnouncement model</param>
        /// <param name="pushNotificationAnnouncement">PushNotificationAnnouncement</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>PushNotificationAnnouncement model</returns>
        public virtual async Task<PushNotificationAnnouncementModel> PreparePushNotificationAnnouncementModelAsync(PushNotificationAnnouncementModel model,
            PushNotificationAnnouncement pushNotificationAnnouncement, bool excludeProperties = false)
        {
            if (pushNotificationAnnouncement != null)
            {
                //fill in model values from the entity
                model = model ?? pushNotificationAnnouncement.ToModel<PushNotificationAnnouncementModel>();

                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(pushNotificationAnnouncement.CreatedOnUtc, DateTimeKind.Utc);
            }

            if (!excludeProperties)
            {

            }
            return model;
        }

        #endregion
    }
}
