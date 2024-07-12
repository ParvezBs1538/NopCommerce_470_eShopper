using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public class SliderModelFactory : ISliderModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IApiSliderService _sliderService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public SliderModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IApiSliderService sliderService,
            IPictureService pictureService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _sliderService = sliderService;
            _pictureService = pictureService;
        }

        #endregion

        #region Utilities

        protected async Task PrepareSliderTypesAsync(IList<SelectListItem> items, bool excludeDefaultItem = false, string label = "")
        {
            var selectList = await SliderType.Product.ToSelectListAsync(false);
            foreach (var item in selectList)
                items.Add(item);

            if (!excludeDefaultItem)
            {
                label = string.IsNullOrWhiteSpace(label) ? await _localizationService.GetResourceAsync("Admin.Common.All") : label;
                items.Insert(0, new SelectListItem()
                {
                    Text = label,
                    Value = "0"
                });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare slider search model
        /// </summary>
        /// <param name="searchModel">Slider search model</param>
        /// <returns>Slider search model</returns>
        public virtual async Task<SliderSearchModel> PrepareSliderSearchModelAsync(SliderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareSliderTypesAsync(searchModel.AvailableSliderTypes);
            searchModel.SelectedSliderTypes = new List<int> { 0 };
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged slider list model
        /// </summary>
        /// <param name="searchModel">Slider search model</param>
        /// <returns>Slider list model</returns>
        public virtual async Task<SliderListModel> PrepareSliderListModelAsync(SliderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var selectedTypes = (searchModel.SelectedSliderTypes?.Contains(0) ?? true) ? null : searchModel.SelectedSliderTypes.ToList();

            //get sliders
            var sliders = await _sliderService.GetAllApiSlidersAsync(selectedTypes, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new SliderListModel().PrepareToGridAsync(searchModel, sliders, () =>
            {
                return sliders.SelectAwait(async slider =>
                {
                    return await PrepareSliderModelAsync(null, slider, true);
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare slider model
        /// </summary>
        /// <param name="model">Slider model</param>
        /// <param name="slider">Slider</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Slider model</returns>
        public virtual async Task<SliderModel> PrepareSliderModelAsync(SliderModel model, ApiSlider slider, bool excludeProperties = false)
        {
            if (slider != null)
            {
                if (model == null)
                {
                    model = slider.ToModel<SliderModel>();
                    if (slider.ActiveStartDateUtc.HasValue)
                        model.ActiveStartDate = await _dateTimeHelper.ConvertToUserTimeAsync(slider.ActiveStartDateUtc.Value, DateTimeKind.Utc);
                    if (slider.ActiveEndDateUtc.HasValue)
                        model.ActiveEndDate = await _dateTimeHelper.ConvertToUserTimeAsync(slider.ActiveEndDateUtc.Value, DateTimeKind.Utc);

                    model.PictureUrl = await _pictureService.GetPictureUrlAsync(slider.PictureId, 120);
                    model.SliderTypeStr = await _localizationService.GetLocalizedEnumAsync(slider.SliderType);
                }

                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.CreatedOnUtc, DateTimeKind.Utc);
            }

            if (!excludeProperties)
            {
                await PrepareSliderTypesAsync(model.AvailableSliderTypes, true);
            }

            return model;
        }

        #endregion
    }
}
