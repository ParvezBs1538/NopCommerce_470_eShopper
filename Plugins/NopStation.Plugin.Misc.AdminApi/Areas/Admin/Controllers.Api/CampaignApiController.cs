using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/campaign/[action]")]
public partial class CampaignApiController : BaseAdminApiController
{
    #region Fields

    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly ICampaignModelFactory _campaignModelFactory;
    private readonly ICampaignService _campaignService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IEmailAccountService _emailAccountService;
    private readonly ILocalizationService _localizationService;
    private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public CampaignApiController(EmailAccountSettings emailAccountSettings,
        ICampaignModelFactory campaignModelFactory,
        ICampaignService campaignService,
        ICustomerActivityService customerActivityService,
        IDateTimeHelper dateTimeHelper,
        IEmailAccountService emailAccountService,
        ILocalizationService localizationService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        IPermissionService permissionService,
        IStoreContext storeContext,
        IStoreService storeService,
        ILogger logger)
    {
        _emailAccountSettings = emailAccountSettings;
        _campaignModelFactory = campaignModelFactory;
        _campaignService = campaignService;
        _customerActivityService = customerActivityService;
        _dateTimeHelper = dateTimeHelper;
        _emailAccountService = emailAccountService;
        _localizationService = localizationService;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _permissionService = permissionService;
        _storeContext = storeContext;
        _storeService = storeService;
        _logger = logger;
    }

    #endregion

    #region Utilities

    protected virtual async Task<EmailAccount> GetEmailAccountAsync(int emailAccountId)
    {
        return await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId)
            ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)
            ?? throw new NopException("Email account could not be loaded");
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _campaignModelFactory.PrepareCampaignSearchModelAsync(new CampaignSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<CampaignSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _campaignModelFactory.PrepareCampaignListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _campaignModelFactory.PrepareCampaignModelAsync(new CampaignModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CampaignModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var campaign = model.ToEntity<Campaign>();

            campaign.CreatedOnUtc = DateTime.UtcNow;
            campaign.DontSendBeforeDateUtc = model.DontSendBeforeDate.HasValue ?
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value) : null;

            await _campaignService.InsertCampaignAsync(campaign);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCampaign",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCampaign"), campaign.Id), campaign);

            return Created(campaign.Id, await _localizationService.GetResourceAsync("Admin.Promotions.Campaigns.Added"));
        }

        //prepare model
        model = await _campaignModelFactory.PrepareCampaignModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        //try to get a campaign with the specified id
        var campaign = await _campaignService.GetCampaignByIdAsync(id);
        if (campaign == null)
            return NotFound("No campaign found with the specified id");

        //prepare model
        var model = await _campaignModelFactory.PrepareCampaignModelAsync(null, campaign);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CampaignModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a campaign with the specified id
        var campaign = await _campaignService.GetCampaignByIdAsync(model.Id);
        if (campaign == null)
            return NotFound("No campaign found with the specified id");

        if (ModelState.IsValid)
        {
            campaign = model.ToEntity(campaign);

            campaign.DontSendBeforeDateUtc = model.DontSendBeforeDate.HasValue ?
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value) : null;

            await _campaignService.UpdateCampaignAsync(campaign);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditCampaign",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCampaign"), campaign.Id), campaign);

            return Ok(await _localizationService.GetResourceAsync("Admin.Promotions.Campaigns.Updated"));
        }

        //prepare model
        model = await _campaignModelFactory.PrepareCampaignModelAsync(model, campaign, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SendTestEmail([FromBody] BaseQueryModel<CampaignModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a campaign with the specified id
        var campaign = await _campaignService.GetCampaignByIdAsync(model.Id);
        if (campaign == null)
            return NotFound("No campaign found with the specified id");

        //prepare model
        model = await _campaignModelFactory.PrepareCampaignModelAsync(model, campaign);

        //ensure that the entered email is valid
        if (!CommonHelper.IsValidEmail(model.TestEmail))
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.WrongEmail"));
        }

        try
        {
            var emailAccount = await GetEmailAccountAsync(model.EmailAccountId);
            var store = await _storeContext.GetCurrentStoreAsync();
            var subscription = await _newsLetterSubscriptionService
                .GetNewsLetterSubscriptionByEmailAndStoreIdAsync(model.TestEmail, store.Id);
            if (subscription != null)
            {
                //there's a subscription. let's use it
                await _campaignService.SendCampaignAsync(campaign, emailAccount, new List<NewsLetterSubscription> { subscription });
            }
            else
            {
                //no subscription found
                await _campaignService.SendCampaignAsync(campaign, emailAccount, model.TestEmail);
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Promotions.Campaigns.TestEmailSentToCustomers"));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);

            //prepare model
            model = await _campaignModelFactory.PrepareCampaignModelAsync(model, campaign, true);

            //if we got this far, something failed, redisplay form
            return BadRequestWrap(model, null, new List<string>() { exc.Message });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> SendMassEmail([FromBody] BaseQueryModel<CampaignModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a campaign with the specified id
        var campaign = await _campaignService.GetCampaignByIdAsync(model.Id);
        if (campaign == null)
            return NotFound("No campaign found with the specified id");

        //prepare model
        model = await _campaignModelFactory.PrepareCampaignModelAsync(model, campaign);

        try
        {
            var emailAccount = await GetEmailAccountAsync(model.EmailAccountId);

            //subscribers of certain store?
            var storeId = (await _storeService.GetStoreByIdAsync(campaign.StoreId))?.Id ?? 0;
            var subscriptions = await _newsLetterSubscriptionService.GetAllNewsLetterSubscriptionsAsync(storeId: storeId,
                customerRoleId: model.CustomerRoleId,
                isActive: true);
            var totalEmailsSent = await _campaignService.SendCampaignAsync(campaign, emailAccount, subscriptions);

            return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.Promotions.Campaigns.MassEmailSentToCustomers"), totalEmailsSent));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);

            //prepare model
            model = await _campaignModelFactory.PrepareCampaignModelAsync(model, campaign, true);

            //if we got this far, something failed, redisplay form
            return BadRequestWrap(model, null, new List<string>() { exc.Message });
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCampaigns))
            return AdminApiAccessDenied();

        //try to get a campaign with the specified id
        var campaign = await _campaignService.GetCampaignByIdAsync(id);
        if (campaign == null)
            return NotFound("No campaign found with the specified id");

        await _campaignService.DeleteCampaignAsync(campaign);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteCampaign",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCampaign"), campaign.Id), campaign);

        return Ok(await _localizationService.GetResourceAsync("Admin.Promotions.Campaigns.Deleted"));
    }

    #endregion
}