using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.MegaMenu.Services;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Factories;

public class CategoryIconModelFactory : ICategoryIconModelFactory
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly ICategoryIconService _categoryIconService;
    private readonly ICategoryService _categoryService;
    private readonly IPictureService _pictureService;
    private readonly MegaMenuSettings _megaMenuSettings;

    #endregion

    #region Ctor

    public CategoryIconModelFactory(CatalogSettings catalogSettings,
        IBaseAdminModelFactory baseAdminModelFactory,
        IDateTimeHelper dateTimeHelper,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ICategoryIconService categoryIconService,
        ICategoryService categoryService,
        IPictureService pictureService,
        MegaMenuSettings megaMenuSettings)
    {
        _catalogSettings = catalogSettings;
        _baseAdminModelFactory = baseAdminModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _languageService = languageService;
        _localizationService = localizationService;
        _categoryIconService = categoryIconService;
        _categoryService = categoryService;
        _pictureService = pictureService;
        _megaMenuSettings = megaMenuSettings;
    }

    #endregion

    #region Utilities

    protected async Task PrepareAvailableCategoriesAsync(IList<SelectListItem> items, bool excludeDefaultItem = false)
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        foreach (var category in categories)
        {
            items.Add(new SelectListItem()
            {
                Text = await _categoryService.GetFormattedBreadCrumbAsync(category, categories),
                Value = category.Id.ToString()
            });
        }

        if (!excludeDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare categoryIcon search model
    /// </summary>
    /// <param name="searchModel">CategoryIcon search model</param>
    /// <returns>CategoryIcon search model</returns>
    public virtual async Task<CategoryIconSearchModel> PrepareCategoryIconSearchModelAsync(CategoryIconSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged categoryIcon list model
    /// </summary>
    /// <param name="searchModel">CategoryIcon search model</param>
    /// <returns>CategoryIcon list model</returns>
    public virtual async Task<CategoryIconListModel> PrepareCategoryIconListModelAsync(CategoryIconSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get categoryIcons
        var categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
            showHidden: true,
            storeId: searchModel.SearchStoreId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var categoryIcons = _categoryIconService.GetAllCategoryIconsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new CategoryIconListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                //fill in model values from the entity
                return await PrepareCategoryIconModelAsync(null, category);
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare categoryIcon model
    /// </summary>
    /// <param name="model">CategoryIcon model</param>
    /// <param name="categoryIcon">CategoryIcon</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>CategoryIcon model</returns>
    public virtual async Task<CategoryIconModel> PrepareCategoryIconModelAsync(CategoryIconModel model, Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        if (model == null)
        {
            model = new CategoryIconModel();
            var categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(category.Id);
            Picture picture = null;
            if (categoryIcon != null)
            {
                model = categoryIcon.ToModel<CategoryIconModel>();
                picture = await _pictureService.GetPictureByIdAsync(categoryIcon.PictureId);
            }
            if (picture == null)
                picture = await _pictureService.GetPictureByIdAsync(_megaMenuSettings.DefaultCategoryIconId);
            model.CategoryId = category.Id;
            model.CategoryName = await _categoryService.GetFormattedBreadCrumbAsync(category);
            model.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture, 25)).Url;
        }

        return model;
    }

    #endregion
}
