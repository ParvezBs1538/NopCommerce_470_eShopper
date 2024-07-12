using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Controllers;

public class FormAttributeController : NopStationAdminController
{
    #region Fields

    private readonly IFormAttributeModelFactory _formAttributeModelFactory;
    private readonly IFormAttributeService _formAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public FormAttributeController(
        IFormAttributeModelFactory formAttributeModelFactory,
        IFormAttributeService formAttributeService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IPermissionService permissionService)
    {
        _formAttributeModelFactory = formAttributeModelFactory;
        _formAttributeService = formAttributeService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _notificationService = notificationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    #region list/create/edit/delete

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        var model = await _formAttributeModelFactory.PrepareFormAttributeSearchModelAsync(new());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(FormAttributeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _formAttributeModelFactory.PrepareFormAttributeListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //prepare model
        var model = await _formAttributeModelFactory.PrepareFormAttributeModelAsync(new FormAttributeModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(FormAttributeModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var formAttribute = model.ToEntity<FormAttribute>();
            await _formAttributeService.InsertFormAttributeAsync(formAttribute);
            await UpdateLocalesAsync(formAttribute, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = formAttribute.Id });
        }

        //prepare model
        model = await _formAttributeModelFactory.PrepareFormAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute with the specified id

        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(id);

        if (formAttribute == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _formAttributeModelFactory.PrepareFormAttributeModelAsync(null, formAttribute);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(FormAttributeModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(model.Id);

        if (formAttribute == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            formAttribute = model.ToEntity(formAttribute);
            await _formAttributeService.UpdateFormAttributeAsync(formAttribute);

            await UpdateLocalesAsync(formAttribute, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = formAttribute.Id });
        }

        //prepare model
        model = await _formAttributeModelFactory.PrepareFormAttributeModelAsync(model, formAttribute, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(id);
        if (formAttribute == null)
            return RedirectToAction(nameof(List));

        await _formAttributeService.DeleteFormAttributeAsync(formAttribute);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Deleted"));

        return RedirectToAction("List");
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        if (selectedIds != null)
        {
            await _formAttributeService.DeleteFormAttributesAsync(await _formAttributeService.GetFormAttributesByIdsAsync(selectedIds.ToArray()));
        }
        return Json(new { Result = true });
    }

    #endregion

    #region Used by forms

    [HttpPost]
    public virtual async Task<IActionResult> UsedByForms(FormAttributeFormSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return await AccessDeniedDataTablesJson();

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(searchModel.FormAttributeId)
            ?? throw new ArgumentException("No form attribute found with the specified id");

        //prepare model
        var model = await _formAttributeModelFactory.PrepareFormAttributeFormListModelAsync(searchModel, formAttribute);

        return Json(model);
    }

    #endregion

    #region Predefined values

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedFormAttributeValueList(PredefinedFormAttributeValueSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return await AccessDeniedDataTablesJson();

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(searchModel.FormAttributeId)
            ?? throw new ArgumentException("No form attribute found with the specified id");

        //prepare model
        var model = await _formAttributeModelFactory.PreparePredefinedFormAttributeValueListModelAsync(searchModel, formAttribute);

        return Json(model);
    }

    public virtual async Task<IActionResult> PredefinedFormAttributeValueCreatePopup(int formAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(formAttributeId)
            ?? throw new ArgumentException("No form attribute found with the specified id", nameof(formAttributeId));

        //prepare model
        var model = await _formAttributeModelFactory
            .PreparePredefinedFormAttributeValueModelAsync(new(), formAttribute, null);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedFormAttributeValueCreatePopup(PredefinedFormAttributeValueModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(model.FormAttributeId)
            ?? throw new ArgumentException("No form attribute found with the specified id");

        if (ModelState.IsValid)
        {
            //fill entity from model
            var psav = model.ToEntity<PredefinedFormAttributeValue>();

            await _formAttributeService.InsertPredefinedFormAttributeValueAsync(psav);
            await UpdateLocalesAsync(psav, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        //prepare model
        model = await _formAttributeModelFactory.PreparePredefinedFormAttributeValueModelAsync(model, formAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> PredefinedFormAttributeValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a predefined form attribute value with the specified id
        var formAttributeValue = await _formAttributeService.GetPredefinedFormAttributeValueByIdAsync(id)
            ?? throw new ArgumentException("No predefined form attribute value found with the specified id");

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(formAttributeValue.FormAttributeId)
            ?? throw new ArgumentException("No form attribute found with the specified id");

        //prepare model
        var model = await _formAttributeModelFactory.PreparePredefinedFormAttributeValueModelAsync(null, formAttribute, formAttributeValue);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedFormAttributeValueEditPopup(PredefinedFormAttributeValueModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a predefined form attribute value with the specified id
        var formAttributeValue = await _formAttributeService.GetPredefinedFormAttributeValueByIdAsync(model.Id)
            ?? throw new ArgumentException("No predefined form attribute value found with the specified id");

        //try to get a form attribute with the specified id
        var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(formAttributeValue.FormAttributeId)
            ?? throw new ArgumentException("No form attribute found with the specified id");

        if (ModelState.IsValid)
        {
            formAttributeValue = model.ToEntity(formAttributeValue);
            await _formAttributeService.UpdatePredefinedFormAttributeValueAsync(formAttributeValue);

            await UpdateLocalesAsync(formAttributeValue, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        //prepare model
        model = await _formAttributeModelFactory.PreparePredefinedFormAttributeValueModelAsync(model, formAttribute, formAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedFormAttributeValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a predefined form attribute value with the specified id
        var formAttributeValue = await _formAttributeService.GetPredefinedFormAttributeValueByIdAsync(id)
            ?? throw new ArgumentException("No predefined form attribute value found with the specified id", nameof(id));

        await _formAttributeService.DeletePredefinedFormAttributeValueAsync(formAttributeValue);

        return new NullJsonResult();
    }

    #endregion

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(FormAttribute formAttribute, FormAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(formAttribute,
                entity => entity.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(formAttribute,
                entity => entity.Description,
                localized.Description,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(PredefinedFormAttributeValue predefinedFormAttributeValue, PredefinedFormAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(predefinedFormAttributeValue,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion
}
