using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Extensions;
using NopStation.Plugin.Widgets.DMS.Filters;
using NopStation.Plugin.Widgets.DMS.Infrastructure;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Models.Shippers;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Controllers.API
{
    [Route("api/shipper")]
    public partial class ShipperApiController : BaseApiController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDeliverFailedRecordService _deliveryFailedRecordService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IProofOfDeliveryDataService _proofOfDeliveryDataService;
        private readonly IShipmentService _shipmentService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IDMSService _dmsService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly INopFileProvider _fileProvider;
        private readonly IShipmentNoteService _shipmentNoteService;
        private readonly IShipperDeviceService _deviceService;
        private readonly IShipperService _shipperService;
        private readonly GdprSettings _gdprSettings;
        private readonly DMSSettings _dMSSettings;
        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public ShipperApiController(CustomerSettings customerSettings,
            IAuthenticationService authenticationService,
            ICourierShipmentService courierShipmentService,
            ICustomerActivityService customerActivityService,
            ICustomerModelFactory customerModelFactory,
            ICommonModelFactory commonModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            IDateTimeHelper dateTimeHelper,
            IDeliverFailedRecordService deliveryFailedRecordService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IProofOfDeliveryDataService proofOfDeliveryDataService,
            IShipmentService shipmentService,
            IShoppingCartService shoppingCartService,
            IDMSService dmsService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            INopFileProvider fileProvider,
            IShipmentNoteService shipmentNoteService,
            IShipperDeviceService deviceService,
            IShipperService shipperService,
            GdprSettings gdprSettings,
            DMSSettings dMSSettings,
            IGdprService gdprService
            )
        {
            _customerSettings = customerSettings;
            _authenticationService = authenticationService;
            _courierShipmentService = courierShipmentService;
            _customerActivityService = customerActivityService;
            _customerModelFactory = customerModelFactory;
            _commonModelFactory = commonModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _dateTimeHelper = dateTimeHelper;
            _deliveryFailedRecordService = deliveryFailedRecordService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _proofOfDeliveryDataService = proofOfDeliveryDataService;
            _shipmentService = shipmentService;
            _shoppingCartService = shoppingCartService;
            _dmsService = dmsService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _fileProvider = fileProvider;
            _shipmentNoteService = shipmentNoteService;
            _deviceService = deviceService;
            _shipperService = shipperService;
            _gdprSettings = gdprSettings;
            _dMSSettings = dMSSettings;
            _gdprService = gdprService;
        }

        #endregion

        #region Utilities

        protected string GetToken(Customer customer, string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>()
                {
                    { DMSDefaults.CustomerId, customer.Id },
                    { "exp", now },
                    { DMSDefaults.DeviceId_Attribute, deviceId }
                };

            return JwtHelper.JwtEncoder.Encode(payload, DMSDefaults.CustomerSecret);
        }

        private async Task<ShipperLoginModel> PrepareShipperLoginModelAsync()
        {
            var loginModel = await _customerModelFactory.PrepareLoginModelAsync(null);
            var model = loginModel.ToShipperLoginModel();
            model.LanguageNavSelector = await _commonModelFactory.PrepareLanguageSelectorModelAsync();
            model.StringResources = new List<KeyValueApi>();

            var language = await _workContext.GetWorkingLanguageAsync();
            model.Rtl = language.Rtl;

            var resources = await _dmsService.LoadLocalizedResourcesAsync(language.Id);
            foreach (var resource in resources)
                model.StringResources.Add(new KeyValueApi() { Key = resource.Key, Value = resource.Value });

            return model;
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
        [TokenAuthorize(ignore: true)]
        public virtual async Task<IActionResult> Login(bool? checkoutAsGuest)
        {
            var model = await PrepareShipperLoginModelAsync();
            return OkWrap(model);
        }

        [HttpPost("login")]
        [TokenAuthorize(ignore: true)]
        public virtual async Task<IActionResult> Login([FromBody] BaseQueryModel<LoginModel> queryModel)
        {
            var model = queryModel.Data;
            var responseData = new LogInResponseModel();

            var deviceId = Request.GetAppDeviceId();
            if (string.IsNullOrEmpty(deviceId))
            {
                ModelState.AddModelError("DeviceId", await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound"));
            }

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

                            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                            {
                                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                                break;
                            }

                            responseData.CustomerInfo = await _customerModelFactory.PrepareCustomerInfoModelAsync(responseData.CustomerInfo, customer, false);
                            responseData.Token = GetToken(customer, deviceId);
                            responseData.ShipperId = (await _shipperService.GetShipperByCustomerIdAsync(customer.Id)).Id;

                            var device = await _deviceService.GetShipperDeviceByCustomerAsync(customer);
                            if (device != null)
                            {
                                device.CustomerId = customer.Id;
                                device.DeviceToken = deviceId;
                                device.UpdatedOnUtc = DateTime.UtcNow;

                                //device.IsRegistered = await _customerService.IsRegisteredAsync(customer);
                                await _deviceService.UpdateShipperDeviceAsync(device);
                            }
                            else
                            {
                                var newDevice = new ShipperDevice
                                {
                                    CustomerId = customer.Id,
                                    DeviceToken = deviceId,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    UpdatedOnUtc = DateTime.UtcNow,
                                    SubscriptionId = "",
                                    DeviceTypeId = 0
                                };

                                await _deviceService.InsertShipperDeviceAsync(newDevice);
                            }

                            //migrate shopping cart
                            await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                            //sign in new customer
                            await _authenticationService.SignInAsync(customer, true);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                            //activity log
                            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                            return OkWrap(responseData);
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

            return BadRequestWrap(responseData, ModelState);
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

        #region Password recovery & change

        [HttpGet("passwordrecovery")]
        [TokenAuthorize(ignore: true)]
        public virtual async Task<IActionResult> PasswordRecovery()
        {
            var response = new GenericResponseModel<PasswordRecoveryModel>();
            var model = new PasswordRecoveryModel();
            response.Data = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

            return Ok(response);
        }

        [HttpPost("passwordrecovery")]
        [TokenAuthorize(ignore: true)]
        public virtual async Task<IActionResult> PasswordRecoverySend([FromBody] BaseQueryModel<PasswordRecoveryModel> queryModel)
        {
            var response = new GenericResponseModel<PasswordRecoveryModel>();
            if (ModelState.IsValid)
            {
                var model = queryModel.Data;
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

                    response.Data = model;
                    response.Message = await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent");
                    return Ok(response);
                }
                else
                {
                    response.Data = model;
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound"));
                    return BadRequest(response);
                }
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            return BadRequest(response);
        }

        [HttpGet("passwordrecoveryconfirm/{token}/{email}")]
        [TokenAuthorize(ignore: true)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirm(string token, string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.DMS.Response.Customer.CustomerNotFound"));

            var response = new GenericResponseModel<PasswordRecoveryConfirmModel>();
            if (string.IsNullOrEmpty(await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute)))
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged"));
                return BadRequest(response);
            }

            //var model = _customerModelFactory.PreparePasswordRecoveryConfirmModel();
            var model = new PasswordRecoveryConfirmModel { ReturnUrl = Url.RouteUrl("Homepage") };

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

            response.Data = model;
            return Ok(response);
        }

        [HttpPost("changepassword")]
        public virtual async Task<IActionResult> ChangePassword([FromBody] BaseQueryModel<ChangePasswordModel> queryModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var model = queryModel.Data;
            if (customer == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.DMS.Response.Customer.CustomerNotFound"));

            var response = new BaseResponseModel();
            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    return Ok();
                }

                response.ErrorList.AddRange(changePasswordResult.Errors);
            }
            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            return BadRequest(response);
        }

        #endregion

        #region GDPR 

        [HttpPost("shipperpermanentdelete")]
        public virtual async Task<IActionResult> ShipperPermanentDelete()
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currentCustomer))
                return Unauthorized();

            if (!_gdprSettings.GdprEnabled)
                return MethodNotAllowed();

            if (!_dMSSettings.AllowCustomersToDeleteAccount)
                return MethodNotAllowed();

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (await _customerService.IsAdminAsync(currentCustomer) && !await SecondAdminAccountExistsAsync(currentCustomer))
                {
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.Account.AdminAccountShouldExists.DeleteAdministrator"));
                }

                //delete
                await _gdprService.PermanentDeleteCustomerAsync(currentCustomer);

                //activity log
                await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), currentCustomer.Id), currentCustomer);

                return Ok(await _localizationService.GetResourceAsync("NopStation.DMS.Account.Deleted"));
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }

        #endregion

        #region Shipment

        [HttpPost("updateshipmentrecievebyshipper")]
        public virtual async Task<IActionResult> UpdateShipmentRecieveByShipper([FromBody] BaseQueryModel<int> queryModel)
        {
            var response = new GenericResponseModel<UpdateShipmentRecieveByShipperResponseModel>();

            if (ModelState.IsValid)
            {
                var model = queryModel.Data;
                var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model);

                if (courierShipment == null)
                {
                    response.Data = new UpdateShipmentRecieveByShipperResponseModel();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.CourierShipment.NotFound"));

                    return BadRequest(response);
                }

                var customer = await _workContext.GetCurrentCustomerAsync();

                if (customer == null || !customer.Active || customer.Deleted)
                {
                    response.Data = new UpdateShipmentRecieveByShipperResponseModel();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound"));

                    return BadRequest(response);
                }

                if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.ReceivedByShipper && courierShipment.ShipperId == customer.Id)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceived"));
                    return BadRequest(response);
                }

                var shipment = await _shipmentService.GetShipmentByIdAsync(model);

                if (shipment == null)
                {
                    response.Data = new UpdateShipmentRecieveByShipperResponseModel();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.Shipment.NotFound"));
                    return BadRequest(response);
                }

                if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.AssignedToShipper)
                {
                    response.Data = new UpdateShipmentRecieveByShipperResponseModel();

                    if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.ReceivedByShipper)
                    {
                        response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceivedByShipper"));
                    }
                    else
                    {
                        response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShippingNotPossible"));
                    }

                    return BadRequest(response);
                }

                var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

                if (shipper == null)
                {
                    response.Data = new UpdateShipmentRecieveByShipperResponseModel();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound"));

                    return BadRequest(response);
                }

                if (customer.Id != shipper.CustomerId)
                {
                    response.Data = new UpdateShipmentRecieveByShipperResponseModel();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.WrongShipper"));

                    return Unauthorized(response);
                }

                response.Data = new UpdateShipmentRecieveByShipperResponseModel()
                {
                    AssignToShippierId = courierShipment.ShipperId,
                    ShipmentId = courierShipment.ShipmentId,
                    ShipmentStatusId = courierShipment.ShipmentStatusId,
                };

                courierShipment.ShipmentStatusType = ShipmentStatusTypes.ReceivedByShipper;

                await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);

                var note = $"Shipment recieved by the shipper for shipment Id#{shipment.Id}";
                var shipmentNote = new ShipmentNote()
                {
                    CourierShipmentId = courierShipment.Id,
                    NopShipmentId = shipment.Id,
                    Note = note,
                    DisplayToShipper = true,
                    DisplayToCustomer = true,
                    UpdatedByCustomerId = shipper.CustomerId,
                };
                await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);

                if (!shipment.ShippedDateUtc.HasValue)
                    await _orderProcessingService.ShipAsync(shipment, true);

                response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.MarkedAsReceived");

                return Ok(response);
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            return BadRequest(response);
        }

        [HttpPost("updateshipmentdeliveryfailed")]
        public virtual async Task<IActionResult> UpdateShipmentDeliveryFailed([FromBody] BaseQueryModel<MarkAsDeliveryFailedRequestModel> queryModel)
        {
            var response = new BaseResponseModel();

            if (ModelState.IsValid)
            {
                var model = queryModel.Data;
                var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);

                if (courierShipment == null)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.CourierShipment.NotFound"));
                    return BadRequest(response);
                }

                var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);

                if (shipment == null)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.Shipment.NotFound"));
                    return BadRequest(response);
                }

                if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.DeliveryFailed)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.AlreadyMarked"));
                    return BadRequest(response);
                }

                if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.ReceivedByShipper)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.ShippingNotPossible"));
                    return BadRequest(response);
                }

                var customer = await _workContext.GetCurrentCustomerAsync();

                if (customer == null || !customer.Active || customer.Deleted)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.ShipperNotFound"));
                    return BadRequest(response);
                }

                var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

                if (shipper == null)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.ShipperNotFound"));
                    return BadRequest(response);
                }

                if (customer.Id != shipper.CustomerId)
                {
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.WrongShipper"));

                    return Unauthorized(response);
                }

                courierShipment.ShipmentStatusType = ShipmentStatusTypes.DeliveryFailed;
                courierShipment.UpdatedOnUtc = DateTime.UtcNow;
                await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);

                var deliverFailedRecord = new DeliverFailedRecord()
                {
                    ShipmentId = courierShipment.ShipmentId,
                    ShipperId = courierShipment.ShipperId,
                    CourierShipmentId = courierShipment.Id,
                    DeliverFailedReasonId = model.DeliverFailedReasonTypeId,
                    Note = model.Note,
                    CreatedOnUtc = DateTime.UtcNow,
                    DeliveryFailedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _deliveryFailedRecordService.InsertDeliverFailedRecordAsync(deliverFailedRecord);

                var note = string.Format(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.DeliveryFailed.CustomerOrderNote"), courierShipment.ShipmentId, DateTime.Now);
                var shipmentNote = new ShipmentNote()
                {
                    CourierShipmentId = courierShipment.Id,
                    NopShipmentId = shipment.Id,
                    Note = note,
                    DisplayToShipper = true,
                    DisplayToCustomer = true,
                    UpdatedByCustomerId = shipper.CustomerId
                };
                await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);

                var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
                if (order != null)
                {
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = note,
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }

                response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsDeliveryFailed");
                return Ok(response);
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            return BadRequest(response);
        }

        #endregion

        #endregion
    }
}