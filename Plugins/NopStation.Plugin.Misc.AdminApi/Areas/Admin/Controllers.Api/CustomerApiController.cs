using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/customer/[action]")]
public partial class CustomerApiController : BaseAdminApiController
{
    #region Fields

    private readonly CustomerSettings _customerSettings;
    private readonly DateTimeSettings _dateTimeSettings;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly ForumSettings _forumSettings;
    private readonly GdprSettings _gdprSettings;
    private readonly IAddressService _addressService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly ICustomerRegistrationService _customerRegistrationService;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IExportManager _exportManager;
    private readonly IForumService _forumService;
    private readonly IGdprService _gdprService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    private readonly IPermissionService _permissionService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IRewardPointService _rewardPointService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly ITaxService _taxService;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly TaxSettings _taxSettings;

    #endregion

    #region Ctor

    public CustomerApiController(CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        EmailAccountSettings emailAccountSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IForumService forumService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        IPermissionService permissionService,
        IQueuedEmailService queuedEmailService,
        IRewardPointService rewardPointService,
        IStoreContext storeContext,
        IStoreService storeService,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        TaxSettings taxSettings)
    {
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _emailAccountSettings = emailAccountSettings;
        _forumSettings = forumSettings;
        _gdprSettings = gdprSettings;
        _addressAttributeParser = addressAttributeParser;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _addressAttributeService = addressAttributeService;
        _addressService = addressService;
        _customerActivityService = customerActivityService;
        _addressAttributeParser = addressAttributeParser;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _customerModelFactory = customerModelFactory;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _emailAccountService = emailAccountService;
        _eventPublisher = eventPublisher;
        _exportManager = exportManager;
        _forumService = forumService;
        _gdprService = gdprService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _permissionService = permissionService;
        _queuedEmailService = queuedEmailService;
        _rewardPointService = rewardPointService;
        _storeContext = storeContext;
        _storeService = storeService;
        _taxService = taxService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _taxSettings = taxSettings;
    }

    #endregion

    #region Utilities

    protected virtual async Task<string> ValidateCustomerRolesAsync(IList<CustomerRole> customerRoles, IList<CustomerRole> existingCustomerRoles)
    {
        ArgumentNullException.ThrowIfNull(customerRoles);

        ArgumentNullException.ThrowIfNull(existingCustomerRoles);

        //check ACL permission to manage customer roles
        var rolesToAdd = customerRoles.Except(existingCustomerRoles, new CustomerRoleComparerByName());
        var rolesToDelete = existingCustomerRoles.Except(customerRoles, new CustomerRoleComparerByName());
        if (rolesToAdd.Any(role => role.SystemName != NopCustomerDefaults.RegisteredRoleName) || rolesToDelete.Any())
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return await _localizationService.GetResourceAsync("Admin.Customers.Customers.CustomerRolesManagingError");
        }

        //ensure a customer is not added to both 'Guests' and 'Registered' customer roles
        //ensure that a customer is in at least one required role ('Guests' and 'Registered')
        var isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.GuestsRoleName) is not null;
        var isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.RegisteredRoleName) is not null;
        if (isInGuestsRole && isInRegisteredRole)
            return await _localizationService.GetResourceAsync("Admin.Customers.Customers.GuestsAndRegisteredRolesError");
        if (!isInGuestsRole && !isInRegisteredRole)
            return await _localizationService.GetResourceAsync("Admin.Customers.Customers.AddCustomerToGuestsOrRegisteredRoleError");

        //no errors
        return string.Empty;
    }

    protected virtual async Task<string> ParseCustomCustomerAttributesAsync(NameValueCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = string.Empty;
        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in customerAttributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            StringValues ctrlAttributes;

            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (selectedAttributeId > 0)
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    var cblAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.ToString()
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
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

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, enteredText);
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

    private async Task<bool> SecondAdminAccountExistsAsync(Customer customer)
    {
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: [(await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id]);

        return customers.Any(c => c.Active && c.Id != customer.Id);
    }

    #endregion

    #region Customers

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerList([FromBody] BaseQueryModel<CustomerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _customerModelFactory.PrepareCustomerListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerModelAsync(new CustomerModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();

        if (!string.IsNullOrWhiteSpace(model.Email) && await _customerService.GetCustomerByEmailAsync(model.Email) != null)
            ModelState.AddModelError(string.Empty, "Email is already registered");

        if (!string.IsNullOrWhiteSpace(model.Username) && _customerSettings.UsernamesEnabled &&
            await _customerService.GetCustomerByUsernameAsync(model.Username) != null)
        {
            ModelState.AddModelError(string.Empty, "Username is already registered");
        }

        //validate customer roles
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = new List<CustomerRole>();
        foreach (var customerRole in allCustomerRoles)
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                newCustomerRoles.Add(customerRole);
        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, new List<CustomerRole>());
        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
        }

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
            !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        var errorList = new List<string>();
        if (ModelState.IsValid)
        {
            //fill entity from model
            var customer = model.ToEntity<Customer>();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            customer.CustomerGuid = Guid.NewGuid();
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;
            customer.RegisteredInStoreId = currentStore.Id;

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                customer.TimeZoneId = model.TimeZoneId;
            if (_customerSettings.GenderEnabled)
                customer.Gender = model.Gender;
            if (_customerSettings.FirstNameEnabled)
                customer.FirstName = model.FirstName;
            if (_customerSettings.LastNameEnabled)
                customer.LastName = model.LastName;
            if (_customerSettings.DateOfBirthEnabled)
                customer.DateOfBirth = model.DateOfBirth;
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

            await _customerService.InsertCustomerAsync(customer);

            //newsletter subscriptions
            if (!string.IsNullOrEmpty(customer.Email))
            {
                var allStores = await _storeService.GetAllStoresAsync();
                foreach (var store in allStores)
                {
                    var newsletterSubscription = await _newsLetterSubscriptionService
                        .GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                    if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                        model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    {
                        //subscribed
                        if (newsletterSubscription == null)
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
                    else
                    {
                        //not subscribed
                        if (newsletterSubscription != null)
                        {
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletterSubscription);
                        }
                    }
                }
            }

            //password
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        errorList.Add(changePassError);
                }
            }

            //customer roles
            foreach (var customerRole in newCustomerRoles)
            {
                //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
                if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                    continue;

                await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
            }

            await _customerService.UpdateCustomerAsync(customer);

            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't have access to other functionality in admin area
            if (await _customerService.IsAdminAsync(customer) && customer.VendorId > 0)
            {
                customer.VendorId = 0;
                await _customerService.UpdateCustomerAsync(customer);

                errorList.Add(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotBeVendor"));
            }

            //ensure that a customer in the Vendors role has a vendor account associated.
            //otherwise, he will have access to ALL products
            if (await _customerService.IsVendorAsync(customer) && customer.VendorId == 0)
            {
                var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
                await _customerService.RemoveCustomerRoleMappingAsync(customer, vendorRole);

                errorList.Add(await _localizationService.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomer"), customer.Id), customer);

            return Created(customer.Id, await _localizationService.GetResourceAsync("Admin.Customers.Customers.Added"), errorList);
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, errorList);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null || customer.Deleted)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerModelAsync(null, customer);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null || customer.Deleted)
            return NotFound("No customer found with the specified id");

        //validate customer roles
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = new List<CustomerRole>();
        foreach (var customerRole in allCustomerRoles)
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                newCustomerRoles.Add(customerRole);

        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, await _customerService.GetCustomerRolesAsync(customer));

        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
        }

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
            !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        var errorList = new List<string>();
        if (ModelState.IsValid)
        {
            try
            {
                customer.AdminComment = model.AdminComment;
                customer.IsTaxExempt = model.IsTaxExempt;

                //prevent deactivation of the last active administrator
                if (!await _customerService.IsAdminAsync(customer) || model.Active || await SecondAdminAccountExistsAsync(customer))
                    customer.Active = model.Active;
                else
                    errorList.Add(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.Deactivate"));

                //email
                if (!string.IsNullOrWhiteSpace(model.Email))
                    await _customerRegistrationService.SetEmailAsync(customer, model.Email, false);
                else
                    customer.Email = model.Email;

                //username
                if (_customerSettings.UsernamesEnabled)
                {
                    if (!string.IsNullOrWhiteSpace(model.Username))
                        await _customerRegistrationService.SetUsernameAsync(customer, model.Username);
                    else
                        customer.Username = model.Username;
                }

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber = customer.VatNumber;

                    customer.VatNumber = model.VatNumber;
                    //set VAT number status
                    if (!string.IsNullOrEmpty(model.VatNumber))
                    {
                        if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                        {
                            customer.VatNumberStatusId = (int)(await _taxService.GetVatNumberStatusAsync(model.VatNumber)).vatNumberStatus;
                        }
                    }
                    else
                    {
                        customer.VatNumberStatusId = (int)VatNumberStatus.Empty;
                    }
                }

                //vendor
                customer.VendorId = model.VendorId;

                //form fields
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.DateOfBirth;
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

                //custom customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;
                //newsletter subscriptions
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    var allStores = await _storeService.GetAllStoresAsync();
                    foreach (var store in allStores)
                    {
                        var newsletterSubscription = await _newsLetterSubscriptionService
                            .GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                        if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                            model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                        {
                            //subscribed
                            if (newsletterSubscription == null)
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
                        else
                        {
                            //not subscribed
                            if (newsletterSubscription != null)
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletterSubscription);
                            }
                        }
                    }
                }

                var currentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer, true);

                //customer roles
                foreach (var customerRole in allCustomerRoles)
                {
                    //ensure that the current customer cannot add/remove to/from "Administrators" system role
                    //if he's not an admin himself
                    if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName &&
                        !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                        continue;

                    if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    {
                        //new role
                        if (currentCustomerRoleIds.All(roleId => roleId != customerRole.Id))
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                    }
                    else
                    {
                        //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                        if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await SecondAdminAccountExistsAsync(customer))
                        {
                            errorList.Add(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteRole"));
                            continue;
                        }

                        //remove role
                        if (currentCustomerRoleIds.Any(roleId => roleId == customerRole.Id))
                            await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
                    }
                }

                await _customerService.UpdateCustomerAsync(customer);

                //ensure that a customer with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to the other functionality in admin area
                if (await _customerService.IsAdminAsync(customer) && customer.VendorId > 0)
                {
                    customer.VendorId = 0;
                    await _customerService.UpdateCustomerAsync(customer);
                    errorList.Add(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotBeVendor"));
                }

                //ensure that a customer in the Vendors role has a vendor account associated.
                //otherwise, he will have access to ALL products
                if (await _customerService.IsVendorAsync(customer) && customer.VendorId == 0)
                {
                    var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, vendorRole);

                    errorList.Add(await _localizationService.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditCustomer",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomer"), customer.Id), customer);

                return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Updated"), errorList);
            }
            catch (Exception exc)
            {
                errorList.Add(exc.Message);
            }
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerModelAsync(model, customer, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, errorList);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ChangePassword([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //ensure that the current customer cannot change passwords of "Administrators" if he's not an admin himself
        if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanChangePassword"));
        }

        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var changePassRequest = new ChangePasswordRequest(model.Email,
            false, _customerSettings.DefaultPasswordFormat, model.Password);
        var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
        if (changePassResult.Success)
            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.PasswordChanged"));
        else
            return BadRequest(null, changePassResult.Errors);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MarkVatNumberAsValid([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MarkVatNumberAsInvalid([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        customer.VatNumberStatusId = (int)VatNumberStatus.Invalid;
        await _customerService.UpdateCustomerAsync(customer);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RemoveAffiliate([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        customer.AffiliateId = 0;
        await _customerService.UpdateCustomerAsync(customer);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RemoveBindMFA(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute, string.Empty);

        //raise event       
        await _eventPublisher.PublishAsync(new CustomerChangeMultiFactorAuthenticationProviderEvent(customer));

        return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.UnbindMFAProvider"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        try
        {
            //prevent attempts to delete the user, if it is the last active administrator
            if (await _customerService.IsAdminAsync(customer) && !await SecondAdminAccountExistsAsync(customer))
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
            }

            //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
            if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
            }

            //delete
            await _customerService.DeleteCustomerAsync(customer);

            //remove newsletter subscription (if exists)
            foreach (var store in await _storeService.GetAllStoresAsync())
            {
                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                if (subscription != null)
                    await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), customer.Id), customer);

            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Deleted"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> SendWelcomeMessage([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

        return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendWelcomeMessage.Success"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReSendActivationMessage([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //email validation message
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
        await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

        return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.ReSendActivationMessage.Success"));
    }

    public virtual async Task<IActionResult> SendEmail([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        try
        {
            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new NopException("Customer email is empty");
            if (!CommonHelper.IsValidEmail(customer.Email))
                throw new NopException("Customer email is not valid");
            if (string.IsNullOrWhiteSpace(model.SendEmail.Subject))
                throw new NopException("Email subject is empty");
            if (string.IsNullOrWhiteSpace(model.SendEmail.Body))
                throw new NopException("Email body is empty");

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null)
                throw new NopException("Email account can't be loaded");
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                EmailAccountId = emailAccount.Id,
                FromName = emailAccount.DisplayName,
                From = emailAccount.Email,
                ToName = await _customerService.GetCustomerFullNameAsync(customer),
                To = customer.Email,
                Subject = model.SendEmail.Subject,
                Body = model.SendEmail.Body,
                CreatedOnUtc = DateTime.UtcNow,
                DontSendBeforeDateUtc = model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
            };
            await _queuedEmailService.InsertQueuedEmailAsync(email);

            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendEmail.Queued"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    public virtual async Task<IActionResult> SendPm([FromBody] BaseQueryModel<CustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        try
        {
            if (!_forumSettings.AllowPrivateMessages)
                throw new NopException("Private messages are disabled");
            if (await _customerService.IsGuestAsync(customer))
                throw new NopException("Customer should be registered");
            if (string.IsNullOrWhiteSpace(model.SendPm.Subject))
                throw new NopException(await _localizationService.GetResourceAsync("PrivateMessages.SubjectCannotBeEmpty"));
            if (string.IsNullOrWhiteSpace(model.SendPm.Message))
                throw new NopException(await _localizationService.GetResourceAsync("PrivateMessages.MessageCannotBeEmpty"));

            var store = await _storeContext.GetCurrentStoreAsync();

            var privateMessage = new PrivateMessage
            {
                StoreId = store.Id,
                ToCustomerId = customer.Id,
                FromCustomerId = customer.Id,
                Subject = model.SendPm.Subject,
                Text = model.SendPm.Message,
                IsDeletedByAuthor = false,
                IsDeletedByRecipient = false,
                IsRead = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _forumService.InsertPrivateMessageAsync(privateMessage);

            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendPM.Sent"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    #endregion

    #region Reward points history

    [HttpPost]
    public virtual async Task<IActionResult> RewardPointsHistorySelect([FromBody] BaseQueryModel<CustomerRewardPointsSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareRewardPointsListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RewardPointsHistoryAdd([FromBody] BaseQueryModel<AddRewardPointsToCustomerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //prevent adding a new row with zero value
        if (model.Points == 0)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.RewardPoints.AddingZeroValueNotAllowed"));

        if (model.Points < 0 && model.PointsValidity.HasValue)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.RewardPoints.Fields.AddNegativePointsValidity"));

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //check whether delay is set
        DateTime? activatingDate = null;
        if (!model.ActivatePointsImmediately && model.ActivationDelay > 0)
        {
            var delayPeriod = (RewardPointsActivatingDelayPeriod)model.ActivationDelayPeriodId;
            var delayInHours = delayPeriod.ToHours(model.ActivationDelay);
            activatingDate = DateTime.UtcNow.AddHours(delayInHours);
        }

        //whether points validity is set
        DateTime? endDate = null;
        if (model.PointsValidity > 0)
            endDate = (activatingDate ?? DateTime.UtcNow).AddDays(model.PointsValidity.Value);

        //add reward points
        await _rewardPointService.AddRewardPointsHistoryEntryAsync(customer, model.Points, model.StoreId, model.Message,
            activatingDate: activatingDate, endDate: endDate);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Addresses

    [HttpPost]
    public virtual async Task<IActionResult> AddressesSelect([FromBody] BaseQueryModel<CustomerAddressSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    [HttpPost("{id}/{customerId}")]
    public virtual async Task<IActionResult> AddressDelete(int id, int customerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //try to get an address with the specified id
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, id);

        if (address == null)
            return NotFound("No address found with the specified id");

        await _customerService.RemoveCustomerAddressAsync(customer, address);
        await _customerService.UpdateCustomerAsync(customer);

        //now delete the address record
        await _addressService.DeleteAddressAsync(address);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{customerId}")]
    public virtual async Task<IActionResult> AddressCreate(int customerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerAddressModelAsync(new CustomerAddressModel(), customer, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddressCreate([FromBody] BaseQueryModel<CustomerAddressModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //custom address attributes
        var customAttributes = await form.ParseCustomAddressAttributesAsync(_addressAttributeParser, _addressAttributeService);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (ModelState.IsValid)
        {
            var address = model.Address.ToEntity<Address>();
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.InsertAddressAsync(address);

            await _customerService.InsertCustomerAddressAsync(customer, address);

            return Created(address.Id, await _localizationService.GetResourceAsync("Admin.Customers.Customers.Addresses.Added"));
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerAddressModelAsync(model, customer, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}/{customerId}")]
    public virtual async Task<IActionResult> AddressEdit(int id, int customerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //try to get an address with the specified id
        var address = await _addressService.GetAddressByIdAsync(id);
        if (address == null)
            return NotFound();

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerAddressModelAsync(null, customer, address);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddressEdit([FromBody] BaseQueryModel<CustomerAddressModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //try to get an address with the specified id
        var address = await _addressService.GetAddressByIdAsync(model.Address.Id);
        if (address == null)
            return NotFound("No address found with the specified id");

        //custom address attributes
        var customAttributes = await form.ParseCustomAddressAttributesAsync(_addressAttributeParser, _addressAttributeService);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (ModelState.IsValid)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            await _addressService.UpdateAddressAsync(address);

            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Addresses.Updated"));
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerAddressModelAsync(model, customer, address, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    #endregion

    #region Orders

    [HttpPost]
    public virtual async Task<IActionResult> OrderList([FromBody] BaseQueryModel<CustomerOrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerOrderListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    #endregion

    #region Customer

    [HttpGet("{period}")]
    public virtual async Task<IActionResult> LoadCustomerStatistics(string period)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var result = new List<object>();

        var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
        var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        var searchCustomerRoleIds = new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)).Id };

        var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

        switch (period)
        {
            case "year":
                //year statistics
                var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                for (var i = 0; i <= 12; i++)
                {
                    result.Add(new
                    {
                        date = searchYearDateUser.Date.ToString("Y", culture),
                        value = (await _customerService.GetAllCustomersAsync(
                            createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
                            createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                            customerRoleIds: searchCustomerRoleIds,
                            pageIndex: 0,
                            pageSize: 1, getOnlyTotalCount: true)).TotalCount.ToString()
                    });

                    searchYearDateUser = searchYearDateUser.AddMonths(1);
                }

                break;
            case "month":
                //month statistics
                var monthAgoDt = nowDt.AddDays(-30);
                var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                for (var i = 0; i <= 30; i++)
                {
                    result.Add(new
                    {
                        date = searchMonthDateUser.Date.ToString("M", culture),
                        value = (await _customerService.GetAllCustomersAsync(
                            createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
                            createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                            customerRoleIds: searchCustomerRoleIds,
                            pageIndex: 0,
                            pageSize: 1, getOnlyTotalCount: true)).TotalCount.ToString()
                    });

                    searchMonthDateUser = searchMonthDateUser.AddDays(1);
                }

                break;
            case "week":
            default:
                //week statistics
                var weekAgoDt = nowDt.AddDays(-7);
                var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                for (var i = 0; i <= 7; i++)
                {
                    result.Add(new
                    {
                        date = searchWeekDateUser.Date.ToString("d dddd", culture),
                        value = (await _customerService.GetAllCustomersAsync(
                            createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
                            createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                            customerRoleIds: searchCustomerRoleIds,
                            pageIndex: 0,
                            pageSize: 1, getOnlyTotalCount: true)).TotalCount.ToString()
                    });

                    searchWeekDateUser = searchWeekDateUser.AddDays(1);
                }

                break;
        }

        var response = new GenericResponseModel<object>
        {
            Data = result
        };

        return Ok(response);
    }

    #endregion

    #region Current shopping cart/ wishlist

    [HttpPost]
    public virtual async Task<IActionResult> GetCartList([FromBody] BaseQueryModel<CustomerShoppingCartSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerShoppingCartListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    #endregion

    #region Activity log

    [HttpPost]
    public virtual async Task<IActionResult> ListActivityLog([FromBody] BaseQueryModel<CustomerActivityLogSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerActivityLogListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    #endregion

    #region Back in stock subscriptions

    [HttpPost]
    public virtual async Task<IActionResult> BackInStockSubscriptionList([FromBody] BaseQueryModel<CustomerBackInStockSubscriptionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerBackInStockSubscriptionListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    #endregion

    #region GDPR

    public virtual async Task<IActionResult> GdprLog()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerModelFactory.PrepareGdprLogSearchModelAsync(new GdprLogSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GdprLogList([FromBody] BaseQueryModel<GdprLogSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _customerModelFactory.PrepareGdprLogListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> GdprDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        if (!_gdprSettings.GdprEnabled)
            return BadRequest();

        try
        {
            //prevent attempts to delete the user, if it is the last active administrator
            if (await _customerService.IsAdminAsync(customer) && !await SecondAdminAccountExistsAsync(customer))
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
            }

            //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
            if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
            }

            //delete
            await _gdprService.PermanentDeleteCustomerAsync(customer);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), customer.Id), customer);

            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Deleted"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GdprExport(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        try
        {
            //log
            //_gdprService.InsertLog(customer, 0, GdprRequestType.ExportData, await _localizationService.GetResource("Gdpr.Exported"));
            //export
            //export
            var store = await _storeContext.GetCurrentStoreAsync();
            var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(customer, store.Id);

            return File(bytes, MimeTypes.TextXlsx, $"customerdata-{customer.Id}.xlsx");
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }
    #endregion

    #region Export / Import

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelAll([FromBody] BaseQueryModel<CustomerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: model.SelectedCustomerRoleIds.ToArray(),
            email: model.SearchEmail,
            username: model.SearchUsername,
            firstName: model.SearchFirstName,
            lastName: model.SearchLastName,
            dayOfBirth: int.TryParse(model.SearchDayOfBirth, out var dayOfBirth) ? dayOfBirth : 0,
            monthOfBirth: int.TryParse(model.SearchMonthOfBirth, out var monthOfBirth) ? monthOfBirth : 0,
            company: model.SearchCompany,
            phone: model.SearchPhone,
            zipPostalCode: model.SearchZipPostalCode);

        try
        {
            var bytes = await _exportManager.ExportCustomersToXlsxAsync(customers);
            return File(bytes, MimeTypes.TextXlsx, "customers.xlsx");
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    public virtual async Task<IActionResult> ExportExcelSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        var customers = new List<Customer>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .ToArray();

            customers.AddRange(await _customerService.GetCustomersByIdsAsync(ids));
        }

        try
        {
            var bytes = await _exportManager.ExportCustomersToXlsxAsync(customers);
            return File(bytes, MimeTypes.TextXlsx, "customers.xlsx");
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportXmlAll([FromBody] BaseQueryModel<CustomerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: model.SelectedCustomerRoleIds.ToArray(),
            email: model.SearchEmail,
            username: model.SearchUsername,
            firstName: model.SearchFirstName,
            lastName: model.SearchLastName,
            dayOfBirth: int.TryParse(model.SearchDayOfBirth, out var dayOfBirth) ? dayOfBirth : 0,
            monthOfBirth: int.TryParse(model.SearchMonthOfBirth, out var monthOfBirth) ? monthOfBirth : 0,
            company: model.SearchCompany,
            phone: model.SearchPhone,
            zipPostalCode: model.SearchZipPostalCode);

        try
        {
            var xml = await _exportManager.ExportCustomersToXmlAsync(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    public virtual async Task<IActionResult> ExportXmlSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        var customers = new List<Customer>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .ToArray();

            customers.AddRange(await _customerService.GetCustomersByIdsAsync(ids));
        }

        try
        {
            var xml = await _exportManager.ExportCustomersToXmlAsync(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    #endregion
}