using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Localization;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Localization;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/language/[action]")]
public partial class LanguageApiController : BaseAdminApiController
{
    #region Const

    private const string FLAGS_PATH = @"images\flags";

    #endregion

    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILanguageModelFactory _languageModelFactory;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly INopFileProvider _fileProvider;
    private readonly IPermissionService _permissionService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;

    #endregion

    #region Ctor

    public LanguageApiController(ICustomerActivityService customerActivityService,
        ILanguageModelFactory languageModelFactory,
        ILanguageService languageService,
        ILocalizationService localizationService,
        INopFileProvider fileProvider,
        IPermissionService permissionService,
        IStoreMappingService storeMappingService,
        IStoreService storeService)
    {
        _customerActivityService = customerActivityService;
        _languageModelFactory = languageModelFactory;
        _languageService = languageService;
        _localizationService = localizationService;
        _fileProvider = fileProvider;
        _permissionService = permissionService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(Language language, LanguageModel model)
    {
        language.LimitedToStores = model.SelectedStoreIds.Any();
        await _languageService.UpdateLanguageAsync(language);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(language);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(language, store.Id);
            }
            else
            {
                //remove store
                var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                if (storeMappingToDelete != null)
                    await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
            }
        }
    }

    #endregion

    #region Languages

    public virtual async Task<IActionResult> Cultures()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.SpecificCultures)
                .OrderBy(x => x.EnglishName);

        var response = new GenericResponseModel<List<KeyValuePair<string, string>>>
        {
            Data = cultures.Select(x => new KeyValuePair<string, string>($"{x.EnglishName}. {x.IetfLanguageTag}", x.IetfLanguageTag)).ToList()
        };

        return Ok(response);
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _languageModelFactory.PrepareLanguageSearchModelAsync(new LanguageSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<LanguageSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _languageModelFactory.PrepareLanguageListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _languageModelFactory.PrepareLanguageModelAsync(new LanguageModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<LanguageModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var language = model.ToEntity<Language>();
            await _languageService.InsertLanguageAsync(language);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewLanguage",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewLanguage"), language.Id), language);

            //Stores
            await SaveStoreMappingsAsync(language, model);

            return Created(language.Id, await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Added"));
        }

        //prepare model
        model = await _languageModelFactory.PrepareLanguageModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //try to get a language with the specified id
        var language = await _languageService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound("No language found with the specified id");

        //prepare model
        var model = await _languageModelFactory.PrepareLanguageModelAsync(null, language);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<LanguageModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a language with the specified id
        var language = await _languageService.GetLanguageByIdAsync(model.Id);
        if (language == null)
            return NotFound("No language found with the specified id");

        if (ModelState.IsValid)
        {
            //ensure we have at least one published language
            var allLanguages = await _languageService.GetAllLanguagesAsync();
            if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id && !model.Published)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.PublishedLanguageRequired"));
            }

            //update
            language = model.ToEntity(language);
            await _languageService.UpdateLanguageAsync(language);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditLanguage",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditLanguage"), language.Id), language);

            //Stores
            await SaveStoreMappingsAsync(language, model);

            //notification
            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Updated"));
        }

        //prepare model
        model = await _languageModelFactory.PrepareLanguageModelAsync(model, language, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //try to get a language with the specified id
        var language = await _languageService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound("No language found with the specified id");

        //ensure we have at least one published language
        var allLanguages = await _languageService.GetAllLanguagesAsync();
        if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.PublishedLanguageRequired"));
        }

        //delete
        await _languageService.DeleteLanguageAsync(language);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteLanguage",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteLanguage"), language.Id), language);

        //notification
        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetAvailableFlagFileNames()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var flagNames = _fileProvider
            .EnumerateFiles(_fileProvider.GetAbsolutePath(FLAGS_PATH), "*.png")
            .Select(_fileProvider.GetFileName)
            .ToList();

        var availableFlagFileNames = flagNames.Select(flagName => new SelectListItem
        {
            Text = flagName,
            Value = flagName
        }).ToList();

        return Ok(new GenericResponseModel<object>() { Data = availableFlagFileNames });
    }

    #endregion

    #region Resources

    [HttpPost]
    public virtual async Task<IActionResult> Resources([FromBody] BaseQueryModel<LocaleResourceSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a language with the specified id
        var language = await _languageService.GetLanguageByIdAsync(searchModel.LanguageId);
        if (language == null)
            return NotFound("No language found with the specified id");

        //prepare model
        var model = await _languageModelFactory.PrepareLocaleResourceListModelAsync(searchModel, language);

        return OkWrap(model);
    }

    //ValidateAttribute is used to force model validation
    [HttpPost]
    public virtual async Task<IActionResult> ResourceUpdate([FromBody] BaseQueryModel<LocaleResourceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (model.ResourceName != null)
            model.ResourceName = model.ResourceName.Trim();
        if (model.ResourceValue != null)
            model.ResourceValue = model.ResourceValue.Trim();

        if (!ModelState.IsValid)
        {
            return BadRequestWrap(model, ModelState);
        }

        var resource = await _localizationService.GetLocaleStringResourceByIdAsync(model.Id);
        // if the resourceName changed, ensure it isn't being used by another resource
        if (!resource.ResourceName.Equals(model.ResourceName, StringComparison.InvariantCultureIgnoreCase))
        {
            var res = await _localizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, model.LanguageId, false);
            if (res != null && res.Id != resource.Id)
            {
                return BadRequest(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Resources.NameAlreadyExists"), res.ResourceName));
            }
        }

        //fill entity from model
        resource = model.ToEntity(resource);

        await _localizationService.UpdateLocaleStringResourceAsync(resource);

        return Ok(defaultMessage: true);
    }

    //ValidateAttribute is used to force model validation
    [HttpPost("{languageId}")]
    public virtual async Task<IActionResult> ResourceAdd(int languageId, [FromBody] BaseQueryModel<LocaleResourceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (model.ResourceName != null)
            model.ResourceName = model.ResourceName.Trim();
        if (model.ResourceValue != null)
            model.ResourceValue = model.ResourceValue.Trim();

        if (!ModelState.IsValid)
        {
            return BadRequestWrap(model, ModelState);
        }

        var res = await _localizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, model.LanguageId, false);
        if (res == null)
        {
            //fill entity from model
            var resource = model.ToEntity<LocaleStringResource>();

            resource.LanguageId = languageId;

            await _localizationService.InsertLocaleStringResourceAsync(resource);
        }
        else
        {
            return BadRequest(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Resources.NameAlreadyExists"), model.ResourceName));
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ResourceDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //try to get a locale resource with the specified id
        var resource = await _localizationService.GetLocaleStringResourceByIdAsync(id);
        if (resource == null)
            return NotFound("No resource found with the specified id");

        await _localizationService.DeleteLocaleStringResourceAsync(resource);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Export / Import

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ExportXml(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //try to get a language with the specified id
        var language = await _languageService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound("No language found with the specified id");

        try
        {
            var xml = await _localizationService.ExportResourcesToXmlAsync(language);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "language_pack.xml");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ImportXml(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
            return AdminApiAccessDenied();

        //try to get a language with the specified id
        var language = await _languageService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound("No language found with the specified id");

        try
        {
            var importxmlfile = await Request.GetFirstOrDefaultFileAsync();
            if (importxmlfile != null && importxmlfile.Length > 0)
            {
                using var sr = new StreamReader(importxmlfile.OpenReadStream(), Encoding.UTF8);
                await _localizationService.ImportResourcesFromXmlAsync(language, sr);
            }
            else
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Languages.Imported"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion
}