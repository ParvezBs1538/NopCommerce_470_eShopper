using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/messagetemplate/[action]")]
public partial class MessageTemplateApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IMessageTemplateModelFactory _messageTemplateModelFactory;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IWorkflowMessageService _workflowMessageService;

    #endregion Fields

    #region Ctor

    public MessageTemplateApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IMessageTemplateModelFactory messageTemplateModelFactory,
        IMessageTemplateService messageTemplateService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IWorkflowMessageService workflowMessageService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _messageTemplateModelFactory = messageTemplateModelFactory;
        _messageTemplateService = messageTemplateService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _workflowMessageService = workflowMessageService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(MessageTemplate mt, MessageTemplateModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(mt,
                x => x.BccEmailAddresses,
                localized.BccEmailAddresses,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(mt,
                x => x.Subject,
                localized.Subject,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(mt,
                x => x.Body,
                localized.Body,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(mt,
                x => x.EmailAccountId,
                localized.EmailAccountId,
                localized.LanguageId);
        }
    }

    protected virtual async Task SaveStoreMappingsAsync(MessageTemplate messageTemplate, MessageTemplateModel model)
    {
        messageTemplate.LimitedToStores = model.SelectedStoreIds.Any();
        await _messageTemplateService.UpdateMessageTemplateAsync(messageTemplate);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(messageTemplate);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    await _storeMappingService.InsertStoreMappingAsync(messageTemplate, store.Id);
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

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _messageTemplateModelFactory.PrepareMessageTemplateSearchModelAsync(new MessageTemplateSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<MessageTemplateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _messageTemplateModelFactory.PrepareMessageTemplateListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        //try to get a message template with the specified id
        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(id);
        if (messageTemplate == null)
            return NotFound("No message template found with the specified id");

        //prepare model
        var model = await _messageTemplateModelFactory.PrepareMessageTemplateModelAsync(null, messageTemplate);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<MessageTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a message template with the specified id
        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(model.Id);
        if (messageTemplate == null)
            return NotFound("No message template found with the specified id");

        if (ModelState.IsValid)
        {
            messageTemplate = model.ToEntity(messageTemplate);

            //attached file
            if (!model.HasAttachedDownload)
                messageTemplate.AttachedDownloadId = 0;
            if (model.SendImmediately)
                messageTemplate.DelayBeforeSend = null;
            await _messageTemplateService.UpdateMessageTemplateAsync(messageTemplate);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditMessageTemplate",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditMessageTemplate"), messageTemplate.Id), messageTemplate);

            //stores
            await SaveStoreMappingsAsync(messageTemplate, model);

            //locales
            await UpdateLocalesAsync(messageTemplate, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Updated"));
        }

        //prepare model
        model = await _messageTemplateModelFactory.PrepareMessageTemplateModelAsync(model, messageTemplate, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        //try to get a message template with the specified id
        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(id);
        if (messageTemplate == null)
            return NotFound("No message template found with the specified id");

        await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteMessageTemplate",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteMessageTemplate"), messageTemplate.Id), messageTemplate);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> CopyTemplate([FromBody] BaseQueryModel<MessageTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a message template with the specified id
        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(model.Id);
        if (messageTemplate == null)
            return NotFound("No message template found with the specified id");

        try
        {
            var newMessageTemplate = await _messageTemplateService.CopyMessageTemplateAsync(messageTemplate);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Copied"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpGet("{id}/{languageId}")]
    public virtual async Task<IActionResult> TestTemplate(int id, int languageId = 0)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        //try to get a message template with the specified id
        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(id);
        if (messageTemplate == null)
            return NotFound("No message template found with the specified id");

        //prepare model
        var model = await _messageTemplateModelFactory
            .PrepareTestMessageTemplateModelAsync(new TestMessageTemplateModel(), messageTemplate, languageId);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TestTemplate([FromBody] BaseQueryModel<TestMessageTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get a message template with the specified id
        var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(model.Id);
        if (messageTemplate == null)
            return NotFound("No message template found with the specified id");

        var tokens = new List<Token>();
        foreach (string formKey in form.Keys)
            if (formKey.StartsWith("token_", StringComparison.InvariantCultureIgnoreCase))
            {
                var tokenKey = formKey["token_".Length..].Replace("%", string.Empty);
                var stringValue = form[formKey].ToString();

                //try get non-string value
                object tokenValue;
                if (bool.TryParse(stringValue, out var boolValue))
                    tokenValue = boolValue;
                else if (int.TryParse(stringValue, out var intValue))
                    tokenValue = intValue;
                else if (decimal.TryParse(stringValue, out var decimalValue))
                    tokenValue = decimalValue;
                else
                    tokenValue = stringValue;

                tokens.Add(new Token(tokenKey, tokenValue));
            }

        await _workflowMessageService.SendTestEmailAsync(messageTemplate.Id, model.SendTo, tokens, model.LanguageId);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Test.Success"));
    }

    #endregion
}