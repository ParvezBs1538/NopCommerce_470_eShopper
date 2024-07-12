using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.AdminApi.Extensions;
using NopStation.Plugin.Misc.AdminApi.Filters;
using NopStation.Plugin.Misc.AdminApi.Infrastructure;
using NopStation.Plugin.Misc.AdminApi.Models.Customers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Controllers;

[Route("api/admincustomer/[action]")]
[NstAuthorize]
public class AdminCustomerApiController : NopStationApiController
{
    #region Fields

    private readonly CustomerSettings _customerSettings;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly ICustomerRegistrationService _customerRegistrationService;
    private readonly ICustomerService _customerService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILocalizationService _localizationService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IWorkContext _workContext;
    private readonly ICommonModelFactory _commonModelFactory;
    private readonly ILanguageService _languageService;
    private readonly INopFileProvider _fileProvider;
    private readonly AdminApiSettings _adminApiSettings;
    private readonly ISettingService _settingService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public AdminCustomerApiController(CustomerSettings customerSettings,
        IAuthenticationService authenticationService,
        ICustomerActivityService customerActivityService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        ICommonModelFactory commonModelFactory,
        ILanguageService languageService,
        INopFileProvider fileProvider,
        AdminApiSettings adminApiSettings,
        ISettingService settingService,
        IPermissionService permissionService)
    {
        _customerSettings = customerSettings;
        _authenticationService = authenticationService;
        _customerActivityService = customerActivityService;
        _customerModelFactory = customerModelFactory;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _commonModelFactory = commonModelFactory;
        _languageService = languageService;
        _fileProvider = fileProvider;
        _adminApiSettings = adminApiSettings;
        _settingService = settingService;
        _permissionService = permissionService;
    }

    #endregion

    #region Utilities

    protected async Task<string> GetTokenAsync(Customer customer)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);

        var payload = new Dictionary<string, object>()
            {
                { AdminApiCustomerDefaults.CustomerId, customer.Id },
                { "exp", now }
            };

        if (string.IsNullOrWhiteSpace(_adminApiSettings.SecretKey))
        {
            var secretKey = JwtHelper.RandomString(48);
            await _settingService.SaveSettingAsync(new AdminApiSettings
            {
                SecretKey = secretKey
            });
            return JwtHelper.JwtEncoder.Encode(payload, secretKey);
        }

        return JwtHelper.JwtEncoder.Encode(payload, _adminApiSettings.SecretKey);
    }

    private async Task<AdminLoginModel> PrepareAdminLoginModelAsync()
    {
        var loginModel = await _customerModelFactory.PrepareLoginModelAsync(null);
        var model = loginModel.ToAdminLoginModel();
        model.LanguageNavSelector = await _commonModelFactory.PrepareLanguageSelectorModelAsync();
        model.StringResources = [];
        var resourceFilePath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Misc.AdminApi/");

        var filePath = _fileProvider.Combine(resourceFilePath, "loginStringResources.json");
        var text = await _fileProvider.ReadAllTextAsync(filePath, Encoding.UTF8);
        var resources = JsonConvert.DeserializeObject<List<string>>(text);

        var language = await _workContext.GetWorkingLanguageAsync();
        model.Rtl = language.Rtl;

        foreach (var resource in resources)
            model.StringResources.Add(new (resource,
                await _localizationService.GetResourceAsync(resource, languageId: language.Id, defaultValue: resource)));

        return model;
    }

    #endregion

    #region Login / logout

    public virtual async Task<IActionResult> Login()
    {
        var model = await PrepareAdminLoginModelAsync();
        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Login([FromBody] BaseQueryModel<LoginModel> queryModel)
    {
        var model = queryModel.Data;
        var response = new GenericResponseModel<LogInResponseModel>();
        var responseData = new LogInResponseModel();

        if (ModelState.IsValid)
        {
            if (_customerSettings.UsernamesEnabled && model.Username != null)
            {
                model.Username = model.Username.Trim();
            }

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);

            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = _customerSettings.UsernamesEnabled
                            ? await _customerService.GetCustomerByUsernameAsync(model.Username)
                            : await _customerService.GetCustomerByEmailAsync(model.Email);

                        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel, customer))
                        {
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                            break;
                        }

                        responseData.CustomerInfo = await _customerModelFactory.PrepareCustomerInfoModelAsync(responseData.CustomerInfo, customer, false);
                        responseData.Token = await GetTokenAsync(customer);

                        //migrate shopping cart
                        await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                        //sign in new customer
                        await _authenticationService.SignInAsync(customer, true);

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                        //activity log
                        await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                        response.Data = responseData;
                        return Ok(response);
                    }
                case CustomerLoginResults.CustomerNotExist:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                    break;
                case CustomerLoginResults.Deleted:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                    break;
                case CustomerLoginResults.NotActive:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                    break;
                case CustomerLoginResults.NotRegistered:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                    break;
                case CustomerLoginResults.LockedOut:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                    break;
                case CustomerLoginResults.WrongPassword:
                default:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                    break;
            }
        }

        foreach (var modelState in ModelState.Values)
            foreach (var error in modelState.Errors)
                response.ErrorList.Add(error.ErrorMessage);

        return BadRequest(response);
    }

    public virtual async Task<IActionResult> Logout()
    {
        //activity log
        await _customerActivityService.InsertActivityAsync(await _workContext.GetCurrentCustomerAsync(), "PublicStore.Logout",
            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"), await _workContext.GetCurrentCustomerAsync());

        //standard logout 
        await _authenticationService.SignOutAsync();

        //raise logged out event       
        await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(await _workContext.GetCurrentCustomerAsync()));

        return Ok();
    }

    #endregion

    #region Language

    public virtual async Task<IActionResult> SetLanguage(int langId)
    {
        
        var language = await _languageService.GetLanguageByIdAsync(langId);
        if (!language?.Published ?? false || langId == 0)
            language = await _workContext.GetWorkingLanguageAsync();

        await _workContext.SetWorkingLanguageAsync(language);

        var model = await PrepareAdminLoginModelAsync();
        return OkWrap(model);
    }

    #endregion
}
