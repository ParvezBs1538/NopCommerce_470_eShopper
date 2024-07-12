using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Models;
using NopStation.Plugin.Widgets.WidgetPush.Domains;
using NopStation.Plugin.Widgets.WidgetPush.Services;

namespace NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Factories
{
    public class WidgetPushItemModelFactory : IWidgetPushItemModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWidgetPushItemService _widgetPushItemService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public WidgetPushItemModelFactory(IDateTimeHelper dateTimeHelper,
            IWidgetPushItemService widgetPushItemService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory)
        {
            _dateTimeHelper = dateTimeHelper;
            _widgetPushItemService = widgetPushItemService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Methods

        public virtual WidgetPushItemSearchModel PrepareWidgetPushItemSearchModel(WidgetPushItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<WidgetPushItemListModel> PrepareWidgetPushItemListModelAsync(WidgetPushItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var widgetPushItems = await _widgetPushItemService.GetAllWidgetPushItemsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new WidgetPushItemListModel().PrepareToGridAsync(searchModel, widgetPushItems, () =>
            {
                return widgetPushItems.SelectAwait(async widgetPushItem =>
                {
                    return await PrepareWidgetPushItemModelAsync(null, widgetPushItem, true);
                });
            });

            return model;
        }

        public virtual async Task<WidgetPushItemModel> PrepareWidgetPushItemModelAsync(WidgetPushItemModel model, WidgetPushItem widgetPushItem, bool excludeProperties = false)
        {
            Func<WidgetPushItemLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (widgetPushItem != null)
            {
                if (model == null)
                {
                    model = new WidgetPushItemModel()
                    {
                        Name = widgetPushItem.Name,
                        Content = widgetPushItem.Content,
                        DisplayOrder = widgetPushItem.DisplayOrder,
                        Id = widgetPushItem.Id,
                        WidgetZone = widgetPushItem.WidgetZone,
                        Active = widgetPushItem.Active,
                        DisplayEndDate = widgetPushItem.DisplayEndDateUtc.HasValue ?
                            await _dateTimeHelper.ConvertToUserTimeAsync(widgetPushItem.DisplayEndDateUtc.Value) : null,
                        DisplayStartDate = widgetPushItem.DisplayStartDateUtc.HasValue ?
                            await _dateTimeHelper.ConvertToUserTimeAsync(widgetPushItem.DisplayStartDateUtc.Value) : null
                    };
                }

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Content = await _localizationService.GetLocalizedAsync(widgetPushItem, entity => entity.Content, languageId, false, false);
                };
            }

            if (!excludeProperties)
            {
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, widgetPushItem, excludeProperties);
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                var widgetZones = await _widgetPushItemService.GetAllWidgetZonesAsync();
                var nopWidgetZones = typeof(PublicWidgetZones).GetProperties()
                    .Where(x => x.PropertyType == typeof(string))
                    .Select(x => x.GetValue(typeof(PublicWidgetZones)).ToString()).ToList();
                widgetZones = widgetZones.Concat(nopWidgetZones).Distinct().OrderBy(x => x).ToList();
                var sb = new StringBuilder("var existingWidgetZones = [ \"");
                sb.Append(string.Join("\", \"", widgetZones));
                sb.Append("\" ];");
                model.FormattedExistingWidgetZones = sb.ToString();
            }

            return model;
        }

        #endregion
    }
}
