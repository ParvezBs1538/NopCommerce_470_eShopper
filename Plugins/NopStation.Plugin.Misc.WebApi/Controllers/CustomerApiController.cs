using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Models.Common;
using NopStation.Plugin.Misc.WebApi.Models.Customers;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/customer")]
    public partial class CustomerApiController : BaseApiController
    {
        #region Fields

        protected readonly AddressSettings _addressSettings;
        protected readonly CaptchaSettings _captchaSettings;
        protected readonly CustomerSettings _customerSettings;
        protected readonly DateTimeSettings _dateTimeSettings;
        protected readonly ForumSettings _forumSettings;
        protected readonly GdprSettings _gdprSettings;
        protected readonly HtmlEncoder _htmlEncoder;
        protected readonly IAddressModelFactory _addressModelFactory;
        protected readonly IAddressService _addressService;
        private readonly IApiDeviceService _deviceService;
        protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
        protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
        protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
        protected readonly IAuthenticationService _authenticationService;
        protected readonly ICountryService _countryService;
        protected readonly ICurrencyService _currencyService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ICustomerModelFactory _customerModelFactory;
        protected readonly ICustomerRegistrationService _customerRegistrationService;
        protected readonly ICustomerService _customerService;
        protected readonly IDownloadService _downloadService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly IExportManager _exportManager;
        protected readonly IExternalAuthenticationService _externalAuthenticationService;
        protected readonly IGdprService _gdprService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IGiftCardService _giftCardService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILogger _logger;
        protected readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        protected readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INopFileProvider _fileProvider;
        protected readonly INotificationService _notificationService;
        protected readonly IOrderService _orderService;
        protected readonly IPermissionService _permissionService;
        protected readonly IPictureService _pictureService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly ISettingService _settingService;
        protected readonly IStateProvinceService _stateProvinceService;
        protected readonly IStoreContext _storeContext;
        protected readonly ITaxService _taxService;
        protected readonly IWorkContext _workContext;
        protected readonly IWorkflowMessageService _workflowMessageService;
        protected readonly LocalizationSettings _localizationSettings;
        protected readonly MediaSettings _mediaSettings;
        protected readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
        private readonly OrderSettings _orderSettings;
        protected readonly StoreInformationSettings _storeInformationSettings;
        protected readonly TaxSettings _taxSettings;
        private readonly WebApiSettings _webApiSettings;
        private readonly IShoppingCartService _shoppingCartService;

        private static readonly char[] _separator = [','];

        #endregion

        #region Ctor

        public CustomerApiController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            HtmlEncoder htmlEncoder,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IApiDeviceService deviceService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
            IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
            IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IReturnRequestService returnRequestService,
            ISettingService settingService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
            OrderSettings orderSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings,
            WebApiSettings webApiSettings,
            IShoppingCartService shoppingCartService)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _htmlEncoder = htmlEncoder;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _deviceService = deviceService;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _downloadService = downloadService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
            _orderSettings = orderSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
            _webApiSettings = webApiSettings;
            _shoppingCartService = shoppingCartService;
        }

        #endregion

        #region Utilities

        protected async Task<string> GetToken(Customer customer)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>()
                {
                    { WebApiCustomerDefaults.CustomerId, customer.Id },
                    { "exp", now }
                };

            if (string.IsNullOrWhiteSpace(_webApiSettings.SecretKey))
            {
                var secretKey = HelperExtension.RandomString(48);
                await _settingService.SaveSettingAsync(new WebApiSettings
                {
                    SecretKey = secretKey
                });
                return JwtHelper.JwtEncoder.Encode(payload, secretKey);
            }

            return JwtHelper.JwtEncoder.Encode(payload, _webApiSettings.SecretKey);
        }

        protected string GetDeviceIdFromHeader()
        {
            _ = Request.Headers.TryGetValue(WebApiCustomerDefaults.DeviceId, out var headerValues);
            if (headerValues.Count > 0)
            {
                var device = headerValues.FirstOrDefault();
                if (device != null)
                    return device;
            }
            return string.Empty;
        }

        protected virtual async Task<string> ParseCustomCustomerAttributes(NameValueCollection form)
        {
            ArgumentNullException.ThrowIfNull(form);

            var attributesXml = "";
            var attributes = await _customerAttributeService.GetAllAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                         .Where(v => v.IsPreSelected)
                                         .Select(v => v.Id)
                                         .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, NameValueCollection form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                {
                    ModelState.AddModelError("", consent.RequiredMessage);
                }
            }
        }

        protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
            CustomerInfoModel newCustomerInfoModel, NameValueCollection form)
        {
            try
            {
                //consents
                var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                    if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth().ToString()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
                {
                    var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
                {
                    var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync(exception.Message, exception, customer);
            }
        }

        private async Task<bool> SecondAdminAccountExistsAsync(Customer customer)
        {
            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });

            return customers.Any(c => c.Active && c.Id != customer.Id);
        }

        #endregion

        #region Methods

        #region Login / logout

        [HttpGet("login")]
        public virtual async Task<IActionResult> Login(bool? checkoutAsGuest)
        {
            var model = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);
            return OkWrap(model);
        }

        [HttpPost("login")]
        public virtual async Task<IActionResult> Login([FromBody] BaseQueryModel<LoginModel> queryModel)
        {
            var model = queryModel.Data;
            var response = new GenericResponseModel<LogInResponseModel>();
            var responseData = new LogInResponseModel();

            if (ModelState.IsValid)
            {
                var customerUserName = model.Username;
                var customerEmail = model.Email;
                var userNameOrEmail = _customerSettings.UsernamesEnabled ? customerUserName : customerEmail;

                var loginResult = await _customerRegistrationService.ValidateCustomerAsync(userNameOrEmail, model.Password);

                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? await _customerService.GetCustomerByUsernameAsync(customerUserName)
                                : await _customerService.GetCustomerByEmailAsync(customerEmail);

                            responseData.CustomerInfo = await _customerModelFactory.PrepareCustomerInfoModelAsync(responseData.CustomerInfo, customer, false);
                            responseData.Token = await GetToken(customer);

                            //migrate shopping cart
                            await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                            //sign in new customer
                            await _authenticationService.SignInAsync(customer, true);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                            //activity log
                            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                            var deviceId = GetDeviceIdFromHeader();
                            var device = await _deviceService.GetApiDeviceByDeviceIdAsync(deviceId, (await _storeContext.GetCurrentStoreAsync()).Id);
                            if (device != null)
                            {
                                device.CustomerId = customer.Id;
                                device.IsRegistered = await _customerService.IsRegisteredAsync(customer);
                                await _deviceService.UpdateApiDeviceAsync(device);
                            }

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

        [HttpGet("logout")]
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

        #region Password recovery

        [HttpGet("passwordrecovery")]
        public virtual async Task<IActionResult> PasswordRecovery()
        {
            var response = new GenericResponseModel<PasswordRecoveryModel>();
            var model = new PasswordRecoveryModel();
            response.Data = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

            return Ok(response);
        }

        [HttpPost("passwordrecovery")]
        public virtual async Task<IActionResult> PasswordRecoverySend([FromBody] BaseQueryModel<PasswordRecoveryModel> queryModel)
        {
            var model = queryModel.Data;

            if (ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                    //send email
                    await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id);

                    return OkWrap(model, await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent"));
                }
                else
                {
                    return BadRequestWrap(model, errors: new List<string>() {
                        await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound")
                    });
                }
            }

            return BadRequestWrap(model, ModelState);
        }

        [HttpGet("passwordrecoveryconfirm/{token}/{email}")]
        public virtual async Task<IActionResult> PasswordRecoveryConfirm(string token, string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.CustomerNotFound"));

            var model = new PasswordRecoveryConfirmModel { ReturnUrl = Url.RouteUrl("Homepage") };

            if (string.IsNullOrEmpty(await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute)))
            {
                return BadRequestWrap(model, errors: new List<string>() {
                        await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged")
                    });
            }

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                return BadRequestWrap(model, errors: new List<string>() {
                        await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken")
                    });
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                return BadRequestWrap(model, errors: new List<string>() {
                        await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired")
                    });
            }

            return OkWrap(model);
        }

        [HttpPost("passwordrecoveryconfirm/{token}/{email}")]
        public virtual async Task<IActionResult> PasswordRecoveryConfirmPOST(string token, string email, [FromBody] BaseQueryModel<PasswordRecoveryConfirmModel> queryModel)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return RedirectToRoute("Homepage");

            var model = queryModel.Data;
            model.ReturnUrl = Url.RouteUrl("Homepage");
            var response = new GenericResponseModel<PasswordRecoveryConfirmModel>();

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                response.Data = model;
                response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken"));
                return BadRequest(response);
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                response.Data = model;
                response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired"));
                return BadRequest(response);
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
                        response.ErrorList.Add(error.ErrorMessage);
                return BadRequest(response);
            }

            var responseChangePassword = await _customerRegistrationService
                .ChangePasswordAsync(new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
            if (!responseChangePassword.Success)
            {
                foreach (var error in responseChangePassword.Errors)
                    response.ErrorList.Add(error);
                return BadRequest(response);
            }

            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute, "");

            //authenticate customer after changing password
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            model.DisablePasswordChanging = true;
            response.Data = model;
            return Ok(response);
        }

        #endregion

        #region Register

        [HttpGet("register")]
        public virtual async Task<IActionResult> Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return BadRequest();

            var response = new GenericResponseModel<RegisterModel>();
            var model = new RegisterModel();
            response.Data = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);
            return Ok(response);
        }


        [HttpPost("register")]
        public virtual async Task<IActionResult> Register([FromBody] BaseQueryModel<RegisterModel> queryModel)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return BadRequest();

            var response = new GenericResponseModel<RegisterModel>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var model = queryModel.Data;

            if (await _customerService.IsRegisteredAsync(customer))
            {
                //Already registered customer. 
                await _authenticationService.SignOutAsync();

                //raise logged out event       
                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

                //Save a new record
                await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            customer.RegisteredInStoreId = store.Id;

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    store.Id,
                    isApproved);
                var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;

                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        customer.VatNumber = model.VatNumber;

                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    //save customer attributes
                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                        //save newsletter value
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(model.Email, store.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = isNewsletterActive;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                                }
                            }
                            //else
                            //{
                            //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                            //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            //}
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = isNewsletterActive,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                                }
                            }
                        }
                    }

                    if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    {
                        //privacy policy is required
                        //GDPR
                        if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        {
                            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            {
                                //agree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                            }
                            else
                            {
                                //disagree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                            }
                        }
                    }

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        Company = customer.Company,
                        CountryId = customer.CountryId > 0
                            ? (int?)customer.CountryId
                            : null,
                        StateProvinceId = customer.StateProvinceId > 0
                            ? (int?)customer.StateProvinceId
                            : null,
                        County = customer.County,
                        City = customer.City,
                        Address1 = customer.StreetAddress,
                        Address2 = customer.StreetAddress2,
                        ZipPostalCode = customer.ZipPostalCode,
                        PhoneNumber = customer.Phone,
                        FaxNumber = customer.Fax,
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (await _addressService.IsAddressValidAsync(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);

                        await _addressService.InsertAddressAsync(defaultAddress);

                        await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                        customer.BillingAddressId = defaultAddress.Id;
                        customer.ShippingAddressId = defaultAddress.Id;

                        await _customerService.UpdateCustomerAsync(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //raise event       
                    await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                    var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            //email validation message
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                            await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);

                            response.Message = await _localizationService.GetResourceAsync("Account.Register.Result.EmailValidation");
                            return Ok(response);

                        case UserRegistrationType.AdminApproval:
                            response.Message = await _localizationService.GetResourceAsync("Account.Register.Result.AdminApproval");
                            return Ok(response);

                        case UserRegistrationType.Standard:
                            //send customer welcome message
                            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                            response.Message = await _localizationService.GetResourceAsync("Account.Register.Result.Standard");
                            return Ok(response);

                        default:
                            return BadRequest();
                    }
                }
                //errors
                response.ErrorList.AddRange(registrationResult.Errors);
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            response.Data = await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);
            return BadRequest(response);
        }

        [HttpPost("checkusernameavailability")]
        public virtual async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

            if (!UsernamePropertyValidator<string, string>.IsValid(username, _customerSettings))
            {
                statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
            }
            else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (currentCustomer != null &&
                    currentCustomer.Username != null &&
                    currentCustomer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var customer = await _customerService.GetCustomerByUsernameAsync(username);
                    if (customer == null)
                    {
                        statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            var response = new GenericResponseModel<bool>
            {
                Data = usernameAvailable,
                Message = statusText
            };
            return Ok(response);
        }

        [HttpGet("activation/{token}/{email}")]
        public virtual async Task<IActionResult> AccountActivation(string token, string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.CustomerNotFound"));

            var response = new GenericResponseModel<AccountActivationModel>();
            var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.AccountActivation.AlreadyActivated"));
                return BadRequest(response);
            }

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest();

            //activate user account
            customer.Active = true;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, "");
            //send welcome message
            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

            //activating newsletter if need
            var store = await _storeContext.GetCurrentStoreAsync();
            var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
            if (newsletter != null && !newsletter.Active)
            {
                newsletter.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
            }

            response.Message = await _localizationService.GetResourceAsync("Account.AccountActivation.Activated");
            return Ok(response);
        }

        #endregion

        #region My account / Info

        [HttpGet("info")]
        public virtual async Task<IActionResult> Info()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var response = new GenericResponseModel<CustomerInfoModel>();
            var model = new CustomerInfoModel();
            response.Data = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, await _workContext.GetCurrentCustomerAsync(), false);
            return Ok(response);
        }


        [HttpPost("info")]
        public virtual async Task<IActionResult> Info([FromBody] BaseQueryModel<CustomerInfoModel> queryModel)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var model = queryModel.Data;
            var response = new GenericResponseModel<CustomerInfoModel>();
            var oldCustomerModel = new CustomerInfoModel();

            var customer = await _workContext.GetCurrentCustomerAsync();

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        var userName = model.Username.Trim();
                        if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            await _customerRegistrationService.SetUsernameAsync(customer, userName);

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }
                    //email
                    var email = model.Email.Trim();
                    if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.VatNumber;
                        customer.VatNumber = model.VatNumber;

                        if (prevVatNumber != model.VatNumber)
                        {
                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            customer.VatNumberStatusId = (int)vatNumberStatus;

                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                            }
                            else
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        await LogGdprAsync(customer, oldCustomerModel, model, form);

                    response.Data = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, await _workContext.GetCurrentCustomerAsync(), false);
                    return Ok(response);
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            response.Data = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);
            return BadRequest(response);
        }

        [HttpPost("removeexternalassociation/{id}")]
        public virtual async Task<IActionResult> RemoveExternalAssociation(int id)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            //ensure it's our record
            var ear = await _externalAuthenticationService.GetExternalAuthenticationRecordByIdAsync(id);
            if (ear != null)
                await _externalAuthenticationService.DeleteExternalAuthenticationRecordAsync(ear);

            return Ok();
        }

        [HttpGet("revalidateemail/{token}/{email}")]
        public virtual async Task<IActionResult> EmailRevalidation(string token, string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.CustomerNotFound"));

            var response = new GenericResponseModel<EmailRevalidationModel>();
            var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.EmailRevalidation.AlreadyChanged"));
                return BadRequest(response);
            }

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest();

            if (string.IsNullOrEmpty(customer.EmailToRevalidate))
                return BadRequest();

            if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return BadRequest();

            //change email
            try
            {
                await _customerRegistrationService.SetEmailAsync(customer, customer.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                return InternalServerError(exc.Message);
            }

            customer.EmailToRevalidate = null;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute, "");

            //re-authenticate (if usernames are disabled)
            if (!_customerSettings.UsernamesEnabled)
            {
                await _authenticationService.SignInAsync(customer, true);
            }

            response.Message = await _localizationService.GetResourceAsync("Account.EmailRevalidation.Changed");
            return Ok(response);
        }

        [HttpGet("menuvisibilitysettings")]
        public virtual async Task<IActionResult> MenuVisibilitySettings(bool appStart)
        {
            var response = new GenericResponseModel<MenuVisibilityModel>();

            var model = new MenuVisibilityModel
            {
                HasReturnRequests = _orderSettings.ReturnRequestsEnabled &&
                    (await _returnRequestService.SearchReturnRequestsAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                    (await _workContext.GetCurrentCustomerAsync()).Id, pageIndex: 0, pageSize: 1)).Any(),
                HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab
            };

            response.Data = model;

            return Ok(response);
        }

        #endregion

        #region My account / Addresses

        [HttpGet("addresses")]
        public virtual async Task<IActionResult> Addresses()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var response = new GenericResponseModel<CustomerAddressListModel>();
            response.Data = await _customerModelFactory.PrepareCustomerAddressListModelAsync();
            return Ok(response);
        }

        [HttpPost("addressdelete/{addressId:min(0)}")]
        public virtual async Task<IActionResult> AddressDelete(int addressId)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var customer = await _workContext.GetCurrentCustomerAsync();

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                //now delete the address record
                await _addressService.DeleteAddressAsync(address);
            }

            return Ok(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.AddressDeleted"));
        }

        [HttpGet("addressadd")]
        public virtual async Task<IActionResult> AddressAdd()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var response = new GenericResponseModel<CustomerAddressEditModel>();
            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            response.Data = model;
            return Ok(response);
        }

        [HttpPost("addressadd")]
        public virtual async Task<IActionResult> AddressAdd([FromBody] BaseQueryModel<CustomerAddressEditModel> queryModel)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var model = queryModel.Data;

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            //custom address attributes
            var customAttributes = await form.ParseCustomAddressAttributesAsync(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);

                return Created(address.Id, await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.AddressUpdated"));
            }

            var response = new BaseResponseModel();
            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            return BadRequest(response);
        }

        [HttpGet("addressedit/{addressId:min(0)}")]
        public virtual async Task<IActionResult> AddressEdit(int addressId)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var response = new GenericResponseModel<CustomerAddressEditModel>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.AddressNotFound"));

            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            response.Data = model;

            return Ok(response);
        }

        [HttpPost("addressedit/{addressId:min(0)}")]
        public virtual async Task<IActionResult> AddressEdit([FromBody] BaseQueryModel<CustomerAddressEditModel> queryModel, int addressId)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var model = queryModel.Data;
            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.AddressNotFound"));

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            //custom address attributes
            var customAttributes = await form.ParseCustomAddressAttributesAsync(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                await _addressService.UpdateAddressAsync(address);

                return Ok(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.AddressUpdated"));
            }

            var response = new BaseResponseModel();
            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            return BadRequest(response);
        }

        #endregion

        #region My account / Downloadable products

        [HttpGet("downloadableproducts")]
        public virtual async Task<IActionResult> DownloadableProducts()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (_customerSettings.HideDownloadableProductsTab)
                return BadRequest();

            var response = new GenericResponseModel<CustomerDownloadableProductsModel>();
            response.Data = await _customerModelFactory.PrepareCustomerDownloadableProductsModelAsync();
            return Ok(response);
        }

        [HttpGet("useragreement/{orderItemId:guid}")]
        public virtual async Task<IActionResult> UserAgreement(Guid orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemId);
            if (orderItem == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.OrderItemNotFound"));

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            if (product == null || !product.HasUserAgreement)
                return BadRequest();

            var response = new GenericResponseModel<UserAgreementModel>();
            response.Data = await _customerModelFactory.PrepareUserAgreementModelAsync(orderItem, product);
            return Ok(response);
        }

        #endregion

        #region My account / Change password

        [HttpGet("changepassword")]
        public virtual async Task<IActionResult> ChangePassword()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var response = new GenericResponseModel<ChangePasswordModel>();
            response.Data = await _customerModelFactory.PrepareChangePasswordModelAsync();

            //display the cause of the change password 
            if (await _customerService.IsPasswordExpiredAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.ChangePassword.PasswordIsExpired"));
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpPost("changepassword")]
        public virtual async Task<IActionResult> ChangePassword([FromBody] BaseQueryModel<ChangePasswordModel> queryModel)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var model = queryModel.Data;
            var response = new BaseResponseModel();
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
                if (changePasswordResult.Success)
                    return Ok(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));

                //errors
                response.ErrorList.AddRange(changePasswordResult.Errors);
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            return BadRequest(response);
        }

        #endregion

        #region My account / Avatar

        [HttpGet("avatar")]
        public virtual async Task<IActionResult> Avatar()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return MethodNotAllowed();

            var response = new GenericResponseModel<CustomerAvatarModel>();
            response.Data = await _customerModelFactory.PrepareCustomerAvatarModelAsync(new CustomerAvatarModel());
            return Ok(response);
        }

        [HttpPost("uploadavatar")]
        public virtual async Task<IActionResult> UploadAvatar()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return MethodNotAllowed();

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Account.Avatar.NoFileUploaded"));
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "cafilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType.ToLowerInvariant();
            if (!contentType.Equals("image/jpeg") && !contentType.Equals("image/gif"))
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Avatar.UploadRules"));

            var response = new GenericResponseModel<CustomerAvatarModel>();

            if (ModelState.IsValid)
            {
                try
                {
                    var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                    if (fileBinary.Length > avatarMaxSize)
                        return BadRequest(string.Format(await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                    var customer = await _workContext.GetCurrentCustomerAsync();

                    var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
                    if (customerAvatar != null)
                        customerAvatar = await _pictureService.UpdatePictureAsync(customerAvatar.Id, fileBinary, contentType, null);
                    else
                        customerAvatar = await _pictureService.InsertPictureAsync(fileBinary, contentType, null);

                    var customerAvatarId = 0;
                    if (customerAvatar != null)
                        customerAvatarId = customerAvatar.Id;

                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, customerAvatarId);

                    var model = new CustomerAvatarModel();
                    model.AvatarUrl = await _pictureService.GetPictureUrlAsync(
                        await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                        _mediaSettings.AvatarPictureSize,
                        false);

                    response.Data = model;
                    return Ok(response);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }

                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
                        response.ErrorList.Add(error.ErrorMessage);
            }

            //If we got this far, something failed, redisplay form
            response.Data = await _customerModelFactory.PrepareCustomerAvatarModelAsync(new CustomerAvatarModel());
            return BadRequest(response);
        }

        [HttpPost("removeavatar")]
        public virtual async Task<IActionResult> RemoveAvatar()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return MethodNotAllowed();

            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
            if (customerAvatar != null)
                await _pictureService.DeletePictureAsync(customerAvatar);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, 0);

            return Ok(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Customer.AvatarRemoved"));
        }

        #endregion

        #region GDPR tools

        [HttpGet("gdpr")]
        public virtual async Task<IActionResult> GdprTools()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_gdprSettings.GdprEnabled)
                return MethodNotAllowed();

            var response = new GenericResponseModel<GdprToolsModel>();
            response.Data = await _customerModelFactory.PrepareGdprToolsModelAsync();
            return Ok(response);
        }


        [HttpPost("gdprexport")]
        public virtual async Task<IActionResult> GdprToolsExport()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!_gdprSettings.GdprEnabled)
                return RedirectToRoute("CustomerInfo");

            //log
            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ExportData, await _localizationService.GetResourceAsync("Gdpr.Exported"));

            var store = await _storeContext.GetCurrentStoreAsync();

            //export
            var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(customer, store.Id);

            return File(bytes, MimeTypes.TextXlsx, "customerdata.xlsx");
        }


        [HttpPost("gdprdelete")]
        public virtual async Task<IActionResult> GdprToolsDelete()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_gdprSettings.GdprEnabled)
                return MethodNotAllowed();

            //log
            await _gdprService.InsertLogAsync(await _workContext.GetCurrentCustomerAsync(), 0, GdprRequestType.DeleteCustomer, await _localizationService.GetResourceAsync("Gdpr.DeleteRequested"));

            var response = new GenericResponseModel<GdprToolsModel>();
            response.Data = await _customerModelFactory.PrepareGdprToolsModelAsync();
            response.Message = await _localizationService.GetResourceAsync("Gdpr.DeleteRequested.Success");
            return Ok(response);
        }

        [HttpPost("permanentdelete")]
        public virtual async Task<IActionResult> GdprDelete()
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currentCustomer))
                return Unauthorized();

            if (!_webApiSettings.AllowCustomersToDeleteAccount)
                return MethodNotAllowed();

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (await _customerService.IsAdminAsync(currentCustomer) && !await SecondAdminAccountExistsAsync(currentCustomer))
                {
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Customers.AdminAccountShouldExists.DeleteAdministrator"));
                }

                //delete
                await _gdprService.PermanentDeleteCustomerAsync(currentCustomer);

                //activity log
                await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), currentCustomer.Id), currentCustomer);

                return Ok(await _localizationService.GetResourceAsync("NopStation.WebApi.Customers.Deleted"));
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }

        #endregion

        #region Check gift card balance

        [HttpGet("checkgiftcardbalance")]
        public virtual async Task<IActionResult> CheckGiftCardBalance()
        {
            if (!(_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance))
                return MethodNotAllowed();

            var response = new GenericResponseModel<CheckGiftCardBalanceModel>();
            response.Data = await _customerModelFactory.PrepareCheckGiftCardBalanceModelAsync();
            return Ok(response);
        }


        [HttpPost("checkgiftcardbalance")]
        public virtual async Task<IActionResult> CheckBalance([FromBody] BaseQueryModel<CheckGiftCardBalanceModel> queryModel)
        {
            var model = queryModel.Data;
            var response = new GenericResponseModel<CheckGiftCardBalanceModel>();

            if (ModelState.IsValid)
            {
                var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: model.GiftCardCode)).FirstOrDefault();
                if (giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard))
                {
                    var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard), await _workContext.GetWorkingCurrencyAsync());
                    model.Result = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);
                    response.Data = model;

                    return Ok(response);
                }
                else
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("CheckGiftCardBalance.GiftCardCouponCode.Invalid"));
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            return BadRequest(response);
        }

        #endregion

        #endregion
    }
}