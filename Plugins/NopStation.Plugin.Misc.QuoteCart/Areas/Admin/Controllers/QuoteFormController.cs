using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Controllers;

public class QuoteFormController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IAclService _aclService;
    private readonly ICustomerService _customerService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IFormAttributeService _formAttributeService;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly ILanguageService _languageService;
    private readonly ISettingService _settingService;
    private readonly IQuoteFormModelFactory _quoteFormModelFactory;
    private readonly IQuoteFormService _quoteFormService;

    #endregion

    #region Ctor

    public QuoteFormController(
        ILocalizationService localizationService,
        IAclService aclService,
        ICustomerService customerService,
        ILocalizedEntityService localizedEntityService,
        IFormAttributeService formAttributeService,
        IFormAttributeParser formAttributeParser,
        INotificationService notificationService,
        IPermissionService permissionService,
        IStoreContext storeContext,
        ILanguageService languageService,
        ISettingService settingService,
        IQuoteFormModelFactory quoteFormModelFactory,
        IQuoteFormService quoteFormService)
    {
        _localizationService = localizationService;
        _aclService = aclService;
        _customerService = customerService;
        _localizedEntityService = localizedEntityService;
        _formAttributeService = formAttributeService;
        _formAttributeParser = formAttributeParser;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _storeContext = storeContext;
        _languageService = languageService;
        _settingService = settingService;
        _quoteFormModelFactory = quoteFormModelFactory;
        _quoteFormService = quoteFormService;
    }

    #endregion

    #region Methods

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        var model = await _quoteFormModelFactory.PrepareFormSearchModelAsync(new QuoteFormSearchModel());

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> List(QuoteFormSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return await AccessDeniedDataTablesJson();

        var model = await _quoteFormModelFactory.PrepareFormListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Create/Update/Delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        var model = await _quoteFormModelFactory.PrepareQuoteFormModelAsync(new QuoteFormModel(), null);
        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(QuoteFormModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var form = model.ToEntity<QuoteForm>();

            await _quoteFormService.InsertQuoteFormAsync(form);

            await _quoteFormModelFactory.UpdateLocalesAsync(form, model);

            await _quoteFormModelFactory.SaveStoreMappingsAsync(form, model);

            await _quoteFormService.UpdateQuoteFormAsync(form);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Created"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = form.Id })
                : RedirectToAction("List");
        }
        model = await _quoteFormModelFactory.PrepareQuoteFormModelAsync(model, null);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        var form = await _quoteFormService.GetFormByIdAsync(id);
        if (form == null)
            return RedirectToAction("List");

        var model = await _quoteFormModelFactory.PrepareQuoteFormModelAsync(null, form);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(QuoteFormModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        var form = await _quoteFormService.GetFormByIdAsync(model.Id);

        if (form == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            form = model.ToEntity(form);

            await _quoteFormService.UpdateQuoteFormAsync(form);

            await _quoteFormModelFactory.UpdateLocalesAsync(form, model);

            await _quoteFormModelFactory.SaveStoreMappingsAsync(form, model);

            await _quoteFormService.UpdateQuoteFormAsync(form);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Updated"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = model.Id })
                : RedirectToAction("List");
        }

        model = await _quoteFormModelFactory.PrepareQuoteFormModelAsync(model, form);
        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Delete(int id, bool gridView = false)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        var form = await _quoteFormService.GetFormByIdAsync(id);

        if (form == null)
            return RedirectToAction("List");

        await _quoteFormService.DeleteQuoteFormAsync(form);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Deleted"));

        if (!gridView)
        {
            return RedirectToAction("List");
        }

        return new NullJsonResult();
    }

    #endregion

    #region Form attributes

    [HttpPost]
    public async Task<IActionResult> FormAttributeMappingList(FormAttributeMappingSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return await AccessDeniedDataTablesJson();

        //try to get a form with the specified id
        var form = await _quoteFormService.GetFormByIdAsync(searchModel.FormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //prepare model
        var model = await _quoteFormModelFactory.PrepareFormAttributeMappingListModelAsync(searchModel, form);

        return Json(model);
    }

    public virtual async Task<IActionResult> FormAttributeMappingCreate(int formId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form with the specified id
        var form = await _quoteFormService.GetFormByIdAsync(formId)
            ?? throw new ArgumentException("No form found with the specified id");

        //prepare model
        var model = await _quoteFormModelFactory.PrepareFormAttributeMappingModelAsync(new FormAttributeMappingModel(), form, null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> FormAttributeMappingCreate(FormAttributeMappingModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form with the specified id
        var form = await _quoteFormService.GetFormByIdAsync(model.FormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //ensure this attribute is not mapped yet
        if ((await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(form.Id))
            .Any(x => x.FormAttributeId == model.FormAttributeId))
        {
            //redisplay form
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.AlreadyExists"));

            model = await _quoteFormModelFactory.PrepareFormAttributeMappingModelAsync(model, form, null, true);

            return View(model);
        }

        //insert mapping
        var formAttributeMapping = model.ToEntity<FormAttributeMapping>();
        formAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;

        await _formAttributeService.InsertFormAttributeMappingAsync(formAttributeMapping);
        await UpdateLocalesAsync(formAttributeMapping, model);

        //predefined values
        var predefinedValues = await _formAttributeService.GetPredefinedFormAttributeValuesAsync(model.FormAttributeId);
        foreach (var predefinedValue in predefinedValues)
        {
            var pav = new FormAttributeValue
            {
                FormAttributeMappingId = formAttributeMapping.Id,
                Name = predefinedValue.Name,
                IsPreSelected = predefinedValue.IsPreSelected,
                DisplayOrder = predefinedValue.DisplayOrder
            };
            await _formAttributeService.InsertFormAttributeValueAsync(pav);

            //locales
            var languages = await _languageService.GetAllLanguagesAsync(true);

            //localization
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
            }
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Added"));

        if (!continueEditing)
        {
            //select an appropriate card
            SaveSelectedCardName("form-form-attributes");
            return RedirectToAction("Edit", new { id = form.Id });
        }

        return RedirectToAction("FormAttributeMappingEdit", new { id = formAttributeMapping.Id });
    }

    public virtual async Task<IActionResult> FormAttributeMappingEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(id)
            ?? throw new ArgumentException("No form attribute mapping found with the specified id");

        //try to get a form with the specified id
        var form = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //prepare model
        var model = await _quoteFormModelFactory.PrepareFormAttributeMappingModelAsync(null, form, formAttributeMapping);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> FormAttributeMappingEdit(FormAttributeMappingModel model, bool continueEditing, IFormCollection formData)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No form attribute mapping found with the specified id");

        //try to get a form with the specified id
        var form = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //ensure this attribute is not mapped yet
        if ((await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(form.Id))
            .Any(x => x.FormAttributeId == model.FormAttributeId && x.Id != formAttributeMapping.Id))
        {
            //redisplay form
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.AlreadyExists"));

            model = await _quoteFormModelFactory.PrepareFormAttributeMappingModelAsync(model, form, formAttributeMapping, true);

            return View(model);
        }

        //fill entity from model
        formAttributeMapping = model.ToEntity(formAttributeMapping);
        formAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
        await _formAttributeService.UpdateFormAttributeMappingAsync(formAttributeMapping);

        await UpdateLocalesAsync(formAttributeMapping, model);

        await SaveConditionAttributesAsync(formAttributeMapping, model.ConditionModel, formData);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Updated"));

        if (!continueEditing)
        {
            //select an appropriate card
            SaveSelectedCardName("form-form-attributes");
            return RedirectToAction("Edit", new { id = form.Id });
        }

        return RedirectToAction("FormAttributeMappingEdit", new { id = formAttributeMapping.Id });
    }

    [HttpPost]
    public virtual async Task<IActionResult> FormAttributeMappingDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(id)
            ?? throw new ArgumentException("No form attribute mapping found with the specified id");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        await _formAttributeService.DeleteFormAttributeMappingAsync(formAttributeMapping);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Deleted"));

        //select an appropriate card
        SaveSelectedCardName("form-form-attributes");
        return RedirectToAction("Edit", new { id = formAttributeMapping.QuoteFormId });
    }

    [HttpPost]
    public virtual async Task<IActionResult> FormAttributeValueList(FormAttributeValueSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return await AccessDeniedDataTablesJson();

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(searchModel.FormAttributeMappingId)
            ?? throw new ArgumentException("No form attribute mapping found with the specified id");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //prepare model
        var model = await _quoteFormModelFactory.PrepareFormAttributeValueListModelAsync(searchModel, formAttributeMapping);

        return Json(model);
    }

    public virtual async Task<IActionResult> FormAttributeValueCreatePopup(int formAttributeMappingId)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(formAttributeMappingId)
            ?? throw new ArgumentException("No form attribute mapping found with the specified id");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //prepare model
        var model = await _quoteFormModelFactory.PrepareFormAttributeValueModelAsync(new FormAttributeValueModel(), formAttributeMapping, null);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> FormAttributeValueCreatePopup(FormAttributeValueModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(model.FormAttributeMappingId);
        if (formAttributeMapping == null)
            return RedirectToAction("List", "QuoteForm");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        if (formAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");
            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        //ensure a picture is uploaded
        if (formAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
        {
            ModelState.AddModelError(string.Empty, "Image is required");
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            var pav = model.ToEntity<FormAttributeValue>();

            await _formAttributeService.InsertFormAttributeValueAsync(pav);
            await UpdateLocalesAsync(pav, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        //prepare model
        model = await _quoteFormModelFactory.PrepareFormAttributeValueModelAsync(model, formAttributeMapping, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> FormAttributeValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute value with the specified id
        var formAttributeValue = await _formAttributeService.GetFormAttributeValueByIdAsync(id);
        if (formAttributeValue == null)
            return RedirectToAction("List", "QuoteForm");

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(formAttributeValue.FormAttributeMappingId);
        if (formAttributeMapping == null)
            return RedirectToAction("List", "QuoteForm");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        //prepare model
        var model = await _quoteFormModelFactory.PrepareFormAttributeValueModelAsync(null, formAttributeMapping, formAttributeValue);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> FormAttributeValueEditPopup(FormAttributeValueModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute value with the specified id
        var formAttributeValue = await _formAttributeService.GetFormAttributeValueByIdAsync(model.Id);
        if (formAttributeValue == null)
            return RedirectToAction("List", "QuoteForm");

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(formAttributeValue.FormAttributeMappingId);
        if (formAttributeMapping == null)
            return RedirectToAction("List", "QuoteForm");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        if (formAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");
            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        //ensure a picture is uploaded
        if (formAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
        {
            ModelState.AddModelError(string.Empty, "Image is required");
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            formAttributeValue = model.ToEntity(formAttributeValue);
            await _formAttributeService.UpdateFormAttributeValueAsync(formAttributeValue);

            await UpdateLocalesAsync(formAttributeValue, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        //prepare model
        model = await _quoteFormModelFactory.PrepareFormAttributeValueModelAsync(model, formAttributeMapping, formAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> FormAttributeValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm))
            return AccessDeniedView();

        //try to get a form attribute value with the specified id
        var formAttributeValue = await _formAttributeService.GetFormAttributeValueByIdAsync(id)
            ?? throw new ArgumentException("No form attribute value found with the specified id");

        //try to get a form attribute mapping with the specified id
        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingByIdAsync(formAttributeValue.FormAttributeMappingId)
            ?? throw new ArgumentException("No form attribute mapping found with the specified id");

        //try to get a form with the specified id
        _ = await _quoteFormService.GetFormByIdAsync(formAttributeMapping.QuoteFormId)
            ?? throw new ArgumentException("No form found with the specified id");

        await _formAttributeService.DeleteFormAttributeValueAsync(formAttributeValue);

        return new NullJsonResult();
    }

    #endregion

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(QuoteForm form, QuoteFormModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(form,
                x => x.Title,
                localized.Title,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(form,
                x => x.Info,
                localized.Info,
                localized.LanguageId);
        }
    }

    protected virtual async Task SaveFormAclAsync(QuoteForm form, QuoteFormModel model)
    {
        form.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _quoteFormService.UpdateQuoteFormAsync(form);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(form);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(form, customerRole.Id);
            }
            else
            {
                //remove role
                var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                if (aclRecordToDelete != null)
                    await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
            }
        }
    }

    protected virtual async Task UpdateLocalesAsync(FormAttributeMapping sam, FormAttributeMappingModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(sam,
                x => x.TextPrompt,
                localized.TextPrompt,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(sam,
                x => x.DefaultValue,
                localized.DefaultValue,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(FormAttributeValue pav, FormAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(pav,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task SaveConditionAttributesAsync(FormAttributeMapping formAttributeMapping,
        FormAttributeConditionModel model, IFormCollection form)
    {
        string attributesXml = null;
        if (model.EnableCondition)
        {
            var attribute = await _formAttributeService.GetFormAttributeMappingByIdAsync(model.SelectedFormAttributeId);
            if (attribute != null)
            {
                var controlId = $"{QuoteCartDefaults.FormFieldPrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            //for conditions we should empty values save even when nothing is selected
                            //otherwise "attributesXml" will be empty
                            //hence we won't be able to find a selected attribute
                            attributesXml = _formAttributeParser.AddFormAttribute(null, attribute,
                                selectedAttributeId > 0 ? selectedAttributeId.ToString() : string.Empty);
                        }
                        else
                        {
                            //for conditions we should empty values save even when nothing is selected
                            //otherwise "attributesXml" will be empty
                            //hence we won't be able to find a selected attribute
                            attributesXml = _formAttributeParser.AddFormAttribute(null,
                                attribute, string.Empty);
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            var anyValueSelected = false;
                            foreach (var item in cblAttributes.ToString()
                                .Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId <= 0)
                                    continue;

                                attributesXml = _formAttributeParser.AddFormAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                                anyValueSelected = true;
                            }

                            if (!anyValueSelected)
                            {
                                //for conditions we should save empty values even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _formAttributeParser.AddFormAttribute(null,
                                    attribute, string.Empty);
                            }
                        }
                        else
                        {
                            //for conditions we should save empty values even when nothing is selected
                            //otherwise "attributesXml" will be empty
                            //hence we won't be able to find a selected attribute
                            attributesXml = _formAttributeParser.AddFormAttribute(null,
                                attribute, string.Empty);
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //these attribute types are supported as conditions
                        break;
                }
            }
        }

        formAttributeMapping.ConditionAttributeXml = attributesXml;
        await _formAttributeService.UpdateFormAttributeMappingAsync(formAttributeMapping);
    }

    #endregion
}
