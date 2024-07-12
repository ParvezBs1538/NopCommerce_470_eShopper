using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/email/[action]")]
public partial class EmailAccountApiController : BaseAdminApiController
{
    #region Fields

    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEmailAccountModelFactory _emailAccountModelFactory;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEmailSender _emailSender;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public EmailAccountApiController(EmailAccountSettings emailAccountSettings,
        ICustomerActivityService customerActivityService,
        IEmailAccountModelFactory emailAccountModelFactory,
        IEmailAccountService emailAccountService,
        IEmailSender emailSender,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _emailAccountSettings = emailAccountSettings;
        _customerActivityService = customerActivityService;
        _emailAccountModelFactory = emailAccountModelFactory;
        _emailAccountService = emailAccountService;
        _emailSender = emailSender;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List(bool showtour = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _emailAccountModelFactory.PrepareEmailAccountSearchModelAsync(new EmailAccountSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<EmailAccountSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _emailAccountModelFactory.PrepareEmailAccountListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> MarkAsDefaultEmail(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        var defaultEmailAccount = await _emailAccountService.GetEmailAccountByIdAsync(id);
        if (defaultEmailAccount == null)
            return NotFound("No email account found with the specified id");

        _emailAccountSettings.DefaultEmailAccountId = defaultEmailAccount.Id;
        await _settingService.SaveSettingAsync(_emailAccountSettings);

        return Ok(defaultMessage: true);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _emailAccountModelFactory.PrepareEmailAccountModelAsync(new EmailAccountModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<EmailAccountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var emailAccount = model.ToEntity<EmailAccount>();

            //set password manually
            emailAccount.Password = model.Password;
            await _emailAccountService.InsertEmailAccountAsync(emailAccount);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewEmailAccount",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewEmailAccount"), emailAccount.Id), emailAccount);

            return Created(emailAccount.Id, await _localizationService.GetResourceAsync("Admin.Configuration.EmailAccounts.Added"));
        }

        //prepare model
        model = await _emailAccountModelFactory.PrepareEmailAccountModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        //try to get an email account with the specified id
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(id);
        if (emailAccount == null)
            return NotFound("No email account found with the specified id");

        //prepare model
        var model = await _emailAccountModelFactory.PrepareEmailAccountModelAsync(null, emailAccount);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<EmailAccountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an email account with the specified id
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(model.Id);
        if (emailAccount == null)
            return NotFound("No email account found with the specified id");

        if (ModelState.IsValid)
        {
            emailAccount = model.ToEntity(emailAccount);
            await _emailAccountService.UpdateEmailAccountAsync(emailAccount);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditEmailAccount",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditEmailAccount"), emailAccount.Id), emailAccount);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.EmailAccounts.Updated"));
        }

        //prepare model
        model = await _emailAccountModelFactory.PrepareEmailAccountModelAsync(model, emailAccount, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ChangePassword([FromBody] BaseQueryModel<EmailAccountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an email account with the specified id
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(model.Id);
        if (emailAccount == null)
            return NotFound("No email account found with the specified id");

        //do not validate model
        emailAccount.Password = model.Password;
        await _emailAccountService.UpdateEmailAccountAsync(emailAccount);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.EmailAccounts.Fields.Password.PasswordChanged"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> SendTestEmail([FromBody] BaseQueryModel<EmailAccountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an email account with the specified id
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(model.Id);
        if (emailAccount == null)
            return NotFound("No email account found with the specified id");

        if (!CommonHelper.IsValidEmail(model.SendTestEmailTo))
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.WrongEmail"));
        }

        try
        {
            if (string.IsNullOrWhiteSpace(model.SendTestEmailTo))
                return BadRequest("Enter test email address");

            var store = await _storeContext.GetCurrentStoreAsync();
            var subject = store.Name + ". Testing email functionality.";
            var body = "Email works fine.";
            await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, model.SendTestEmailTo, null);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.EmailAccounts.SendTestEmail.Success"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AdminApiAccessDenied();

        //try to get an email account with the specified id
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(id);
        if (emailAccount == null)
            return NotFound("No email account found with the specified id");

        try
        {
            await _emailAccountService.DeleteEmailAccountAsync(emailAccount);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteEmailAccount",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteEmailAccount"), emailAccount.Id), emailAccount);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.EmailAccounts.Deleted"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion
}