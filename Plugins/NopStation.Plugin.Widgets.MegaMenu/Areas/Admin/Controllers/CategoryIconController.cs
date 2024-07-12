using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.MegaMenu.Domains;
using NopStation.Plugin.Widgets.MegaMenu.Services;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Controllers;

public class CategoryIconController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ICategoryIconModelFactory _categoryIconModelFactory;
    private readonly ICategoryIconService _categoryIconService;
    private readonly ICategoryService _categoryService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public CategoryIconController(ILocalizationService localizationService,
        INotificationService notificationService,
        ICategoryIconModelFactory categoryIconModelFactory,
        ICategoryIconService categoryIconService,
        ICategoryService categoryService,
        IPermissionService permissionService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _categoryIconModelFactory = categoryIconModelFactory;
        _categoryIconService = categoryIconService;
        _categoryService = categoryService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods        

    public virtual async Task<IActionResult> Index()
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var searchModel = await _categoryIconModelFactory.PrepareCategoryIconSearchModelAsync(new CategoryIconSearchModel());
        return View(searchModel);
    }

    public virtual async Task<IActionResult> GetList(CategoryIconSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var model = await _categoryIconModelFactory.PrepareCategoryIconListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> Create(int categoryId)
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        var oldCategoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(categoryId);
        if (oldCategoryIcon != null)
            return RedirectToAction("Edit", new { categoryId = categoryId });

        var model = await _categoryIconModelFactory.PrepareCategoryIconModelAsync(null, category);
        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(CategoryIconModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        var oldCategoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(model.CategoryId);
        if (oldCategoryIcon != null)
            return RedirectToAction("Edit", new { categoryId = model.CategoryId });

        if (ModelState.IsValid)
        {
            var categoryIcon = model.ToEntity<CategoryIcon>();

            await _categoryIconService.InsertCategoryIconAsync(categoryIcon);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons.Created"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { categoryId = categoryIcon.CategoryId });
        }

        model = await _categoryIconModelFactory.PrepareCategoryIconModelAsync(model, category);

        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int categoryId)
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        var categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(categoryId);
        if (categoryIcon == null)
            return RedirectToAction("Create", new { categoryId = categoryId });

        var model = await _categoryIconModelFactory.PrepareCategoryIconModelAsync(null, category);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(CategoryIconModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        var categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(model.CategoryId);
        if (categoryIcon == null)
            return RedirectToAction("Create", new { categoryId = model.CategoryId });

        if (ModelState.IsValid)
        {
            categoryIcon = model.ToEntity(categoryIcon);
            await _categoryIconService.UpdateCategoryIconAsync(categoryIcon);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { categoryId = categoryIcon.CategoryId });
        }
        model = await _categoryIconModelFactory.PrepareCategoryIconModelAsync(model, category);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
            return AccessDeniedView();

        var categoryIcon = await _categoryIconService.GetCategoryIconByIdAsync(id);
        if (categoryIcon == null)
            throw new ArgumentNullException(nameof(categoryIcon));

        await _categoryIconService.DeleteCategoryIconAsync(categoryIcon);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons.Deleted"));

        return RedirectToAction("List");
    }

    #endregion
}
