using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Factories;
using NopStation.Plugin.Widgets.DMS.Filters;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Controllers.API
{
    [Route("api/DMS")]
    public class DMSApiController : BaseApiController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IShipperService _shipperService;
        private readonly ILocalizationService _localizationService;
        private readonly ICourierShipmentModelFactory _courierShipmentModelFactory;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly IDMSOtpService _dMSOtpService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly INotificationService _notificationService;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOTPRecordService _oTPRecordService;
        private readonly IPictureService _pictureService;
        private readonly IProofOfDeliveryDataService _proofOfDeliveryDataService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CustomerSettings _customerSettings;
        private readonly ILanguageService _languageService;
        private readonly ILogger _logger;
        private readonly IDMSService _dmsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShipmentNoteService _shipmentNoteService;
        private readonly GdprSettings _gdprSettings;
        private readonly IGdprService _gdprService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IShipmentNoteModelFactory _shipmentNoteModelFactory;
        private readonly DMSSettings _dMSSettings;

        #endregion

        #region Ctor

        public DMSApiController(IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            IShipperService shipperService,
            ILocalizationService localizationService,
            ICourierShipmentModelFactory courierShipmentModelFactory,
            ICommonModelFactory commonModelFactory,
            ICourierShipmentService courierShipmentService,
            IDMSOtpService dMSOtpService,
            IShipmentService shipmentService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            INotificationService notificationService,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            IOTPRecordService oTPRecordService,
            IPictureService pictureService,
            IProofOfDeliveryDataService proofOfDeliveryDataService,
            IGenericAttributeService genericAttributeService,
            CustomerSettings customerSettings,
            ILanguageService languageService,
            ILogger logger,
            IDMSService dmsService,
            IDateTimeHelper dateTimeHelper,
            IShipmentNoteService shipmentNoteService,
            GdprSettings gdprSettings,
            IGdprService gdprService,
            StoreInformationSettings storeInformationSettings,
            IShipmentNoteModelFactory shipmentNoteModelFactory,
            DMSSettings dMSSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _shipperService = shipperService;
            _localizationService = localizationService;
            _courierShipmentModelFactory = courierShipmentModelFactory;
            _commonModelFactory = commonModelFactory;
            _courierShipmentService = courierShipmentService;
            _dMSOtpService = dMSOtpService;
            _shipmentService = shipmentService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _notificationService = notificationService;
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _oTPRecordService = oTPRecordService;
            _pictureService = pictureService;
            _proofOfDeliveryDataService = proofOfDeliveryDataService;
            _genericAttributeService = genericAttributeService;
            _customerSettings = customerSettings;
            _languageService = languageService;
            _logger = logger;
            _dmsService = dmsService;
            _dateTimeHelper = dateTimeHelper;
            _shipmentNoteService = shipmentNoteService;
            _gdprSettings = gdprSettings;
            _gdprService = gdprService;
            _storeInformationSettings = storeInformationSettings;
            _shipmentNoteModelFactory = shipmentNoteModelFactory;
            _dMSSettings = dMSSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<Language> EnsureLanguageIsActive(Language language)
        {
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language;
        }

        #endregion

        [HttpGet("applandingsetting")]
        public async Task<IActionResult> AppLandingSetting(bool appStart)
        {
            var response = new GenericResponseModel<AppConfigurationModel>();
            var model = new AppConfigurationModel
            {
                StoreClosed = _storeInformationSettings.StoreClosed,
                AllowCustomersToDeleteAccount = _dMSSettings.AllowCustomersToDeleteAccount,
                EnabledProofOfDelivery = _dMSSettings.EnabledProofOfDelivery,
                ProofOfDeliveryTypeId = _dMSSettings.ProofOfDeliveryTypeId,
                ProofOfDeliveryRequired = _dMSSettings.ProofOfDeliveryRequired,
                OtpLength = _dMSSettings.OtpLength
            };

            model.StringResources = new List<KeyValueApi>();

            var language = await EnsureLanguageIsActive(await _workContext.GetWorkingLanguageAsync());
            model.Rtl = language.Rtl;
            var resources = await _dmsService.LoadLocalizedResourcesAsync(language.Id);
            foreach (var resource in resources)
                model.StringResources.Add(new KeyValueApi() { Key = resource.Key, Value = resource.Value });

            model.LanguageNavSelector = await _commonModelFactory.PrepareLanguageSelectorModelAsync();

            if (_dMSSettings.EnabledProofOfDelivery && _dMSSettings.ProofOfDeliveryTypeId == (int)ProofOfDeliveryTypes.Otp)
                model.ProofOfDeliveryRequired = true;

            response.Data = model;
            return Ok(response);
        }

        [HttpGet("DMSShipments")]
        public async Task<IActionResult> Shipments(CourierShipmentsModel command, ShipmentSearchModel searchModel)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var model = new CourierShipmentsModel();

            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            if (shipper == null || !shipper.Active)
            {
                model.WarningMessage = shipper == null ?
                    await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.InvalidAccount") :
                    await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.InactiveAccount");
                model.InvalidAccount = true;

                return Ok(model.ToGenericResponse());
            }

            model = await _courierShipmentModelFactory.PrepareCourierShipmentsOverviewModelAsync(model, shipper, command, searchModel);

            return Ok(model.ToGenericResponse());
        }

        [HttpGet("DMSShipmentNotes/{shipmentId}")]
        public async Task<IActionResult> ShipmentNotes(int shipmentId)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId); //**
            if (shipment == null)
                return BadRequest();

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            if (courierShipment == null)
                return BadRequest();

            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment.ShipperId != shipper.Id)
                return Unauthorized();

            var response = new GenericResponseModel<DMSShipmentNoteListModel>();
            response.Data = await _shipmentNoteModelFactory.PrepareDMSShipmentNoteListModelByCourierShipmentIdAsync(courierShipment.Id,
                displayToShipper: true);

            return Ok(response);
        }

        [HttpGet("DMSShipmentDetails/{shipmentId}")]
        public async Task<IActionResult> ShipmentDetails(int shipmentId)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId); //**
            if (shipment == null)
                return BadRequest();

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            if (courierShipment == null)
                return BadRequest();

            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Unauthorized();

            var response = new GenericResponseModel<CourierShipmentDetailsModel>();
            response.Data = await _courierShipmentModelFactory.PrepareShipmentDetailsModelAsync(shipment, courierShipment);

            return Ok(response);
        }

        //[HttpPost("DMSMarkAsShipped/{shipmentId:min(0)}")]
        //public async Task<IActionResult> MarkAsShipped(int shipmentId)
        //{
        //    if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
        //        return Unauthorized();

        //    var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
        //    if (shipment == null)
        //        return BadRequest();

        //    var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
        //    var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

        //    if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
        //        return Unauthorized();

        //    try
        //    {
        //        var response = new BaseResponseModel();
        //        await _orderProcessingService.ShipAsync(shipment, true);
        //        response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsShipped");
        //        return Ok(response);
        //    }
        //    catch (Exception exc)
        //    {
        //        //error
        //        return BadRequest(exc.Message);
        //    }
        //}

        [HttpPost("DMSEditShippedDate")]
        public virtual async Task<IActionResult> EditShippedDate(CourierShipmentDetailsModel model)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);
            if (shipment == null)
                return BadRequest();

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Unauthorized();

            try
            {
                if (!model.ShippedDate.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }
                var response = new BaseResponseModel();
                shipment.ShippedDateUtc = model.ShippedDate;
                await _shipmentService.UpdateShipmentAsync(shipment);
                response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsShippedDateUpdate");
                return Ok(response);
            }
            catch (Exception exc)
            {
                //error
                return BadRequest(exc.Message);
            }
        }

        [HttpPost("DMSMarkAsDelivered/{shipmentId:min(0)}")]
        public async Task<IActionResult> MarkAsDelivered(int shipmentId)
        {
            var response = new BaseResponseModel();
            if (_dMSSettings.EnabledProofOfDelivery && (_dMSSettings.ProofOfDeliveryTypeId == (int)ProofOfDeliveryTypes.Otp || _dMSSettings.ProofOfDeliveryRequired))
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.MarkedAsDelivered.VerificationRequired"));
                return BadRequest(response);
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.CustomerNotFound"));

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipmentNotFound"));

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync(customer.Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Unauthorized();

            if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.ReceivedByShipper)
                return BadRequest();

            try
            {
                await _orderProcessingService.DeliverAsync(shipment, true);

                courierShipment.ShipmentStatusType = ShipmentStatusTypes.Delivered;
                courierShipment.UpdatedOnUtc = DateTime.UtcNow;
                await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);

                var note = $"Shipment marked as delivered for shipment Id#{shipmentId}";
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

                response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsDelivered");
                return Ok(response);
            }
            catch (Exception exc)
            {
                //error
                return BadRequest(exc.Message);
            }
        }

        [HttpPost("DMSEditDeliveredDate")]
        public virtual async Task<IActionResult> EditDeliveredDate(CourierShipmentDetailsModel model)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);
            if (shipment == null)
                return BadRequest();

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Unauthorized();

            try
            {
                if (!model.DeliveryDate.HasValue)
                {
                    throw new Exception("Enter Delivery date");
                }
                var response = new BaseResponseModel();
                shipment.DeliveryDateUtc = model.DeliveryDate;
                await _shipmentService.UpdateShipmentAsync(shipment);
                response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsShippedDateUpdate");
                return Ok(response);

            }
            catch (Exception exc)
            {
                //error
                return BadRequest(exc.Message);
            }
        }

        [HttpPost("uploadsignature")]
        public virtual async Task<IActionResult> UploadSignature()
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Account.Avatar.NoFileUploaded"));
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "ssfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
            if (fileBinary.Length > avatarMaxSize)
                return BadRequest(string.Format(await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

            var shipmentSignature = await _pictureService.InsertPictureAsync(fileBinary, contentType, fileName);

            var model = new ShipmentSignatureModel();

            if (shipmentSignature != null)
                model.ShipmentSignatureId = shipmentSignature.Id;

            var response = new GenericResponseModel<ShipmentSignatureModel>();
            response.Data = model;
            return Ok(response);
        }

        [HttpPost("uploadproofofdeliveryphoto")]
        public virtual async Task<IActionResult> UploadProofOfDeliveryPhoto()
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Account.Avatar.NoFileUploaded"));
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "ssfilename";
            var fileName = httpPostedFile.FileName;

            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();

            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            //var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
            var avatarMaxSize = _dMSSettings.ProofOfDeliveryImageMaxSize * 1024 * 1024;

            if (fileBinary.Length > avatarMaxSize)
                return BadRequest(string.Format(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.MaximumUploadedFileSize"), _dMSSettings.ProofOfDeliveryImageMaxSize));

            var shipmentPhoto = await _pictureService.InsertPictureAsync(fileBinary, contentType, fileName);
            if (shipmentPhoto == null)
                return BadRequest();

            var model = new ShipmentProofOfDeliveryPhotoUploadResponseModel();

            var response = new GenericResponseModel<ShipmentProofOfDeliveryPhotoUploadResponseModel>();
            response.Data = model;
            model.ShipmentPictureId = shipmentPhoto.Id;
            //var note = $"Proof of delivery photo uploaded. Photo Id#{shipmentPhoto.Id}";
            //var shipmentNote = new ShipmentNote()
            //{
            //    CourierShipmentId = courierShipment.Id,
            //    NopShipmentId = shipment.Id,
            //    Note = note,
            //    DisplayToShipper = true
            //};
            //await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);

            return Ok(response);
        }

        [HttpGet("getPodData/{shipmentId:min(0)}")]
        public virtual async Task<IActionResult> GetPODData(int shipmentId)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.CustomerNotFound"));

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipmentNotFound"));

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync(customer.Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Unauthorized();

            var shipmentPoD = await _proofOfDeliveryDataService.GetProofOfDeliveryDataByCourierShipmentIdAsync(courierShipment.Id);
            if (shipmentPoD == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipmentPODNotFound"));

            var verifiedByShipperId = (await _shipperService.GetShipperByIdAsync(shipmentPoD.VerifiedByShipperId))?.Id;
            var verifiedByShipperCustomer = await _customerService.GetCustomerByIdAsync(verifiedByShipperId ?? 0);

            var model = shipmentPoD.ToModel<ProofOfDeliveryDataModel>();
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(shipmentPoD.CreatedOnUtc, DateTimeKind.Utc);
            model.VerifiedOn = await _dateTimeHelper.ConvertToUserTimeAsync(shipmentPoD.VerifiedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(shipmentPoD.UpdatedOnUtc, DateTimeKind.Utc);
            model.VerifiedByShipperEmail = verifiedByShipperCustomer?.Email;
            model.ProofOfDeliveryType = shipmentPoD.ProofOfDeliveryType.ToString();
            if (shipmentPoD.ProofOfDeliveryType == ProofOfDeliveryTypes.PhotoOfCourier
                || shipmentPoD.ProofOfDeliveryType == ProofOfDeliveryTypes.CustomersSignature)
            {
                var podPhoto = await _pictureService.GetPictureByIdAsync(shipmentPoD.ProofOfDeliveryReferenceId);
                if (podPhoto != null)
                {
                    model.PODContainPhoto = true;
                    model.PODPhotoUrl = await _pictureService.GetPictureUrlAsync(podPhoto.Id, 300);
                }
            }

            var response = new GenericResponseModel<ProofOfDeliveryDataModel>();
            response.Data = model;

            return Ok(response);
        }

        //[HttpPost("DMSSaveCourierShipment")]
        //public virtual async Task<IActionResult> SaveCourierShipment(CourierShipmentModel model)
        //{
        //    if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
        //        return Unauthorized();

        //    try
        //    {
        //        var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);
        //        if (shipment == null)
        //            return BadRequest("No shipment found with this specific id.");

        //        var shipper = await _shipperService.GetShipperByIdAsync(model.ShipperId);
        //        if (shipper != null && !shipment.DeliveryDateUtc.HasValue)
        //        {
        //            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipment.Id);

        //            if (courierShipment == null)
        //            {
        //                courierShipment = new Domain.CourierShipment
        //                {
        //                    ShipmentId = shipment.Id,
        //                    CreatedOnUtc = DateTime.UtcNow,
        //                    UpdatedOnUtc = DateTime.UtcNow,
        //                    ShipperId = shipper.Id,
        //                    SignaturePictureId = model.SignaturePictureId
        //                };
        //                await _courierShipmentService.InsertCourierShipmentAsync(courierShipment);
        //            }
        //            else
        //            {
        //                courierShipment.UpdatedOnUtc = DateTime.UtcNow;
        //                courierShipment.ShipperId = shipper.Id;
        //                courierShipment.SignaturePictureId = model.SignaturePictureId;

        //                await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);
        //            }

        //            var response = new BaseResponseModel();
        //            response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.SignatureUploaded");
        //            return Ok(response);
        //        }
        //        return BadRequest();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("getstringresources/{languageId?}")]
        [TokenAuthorize(ignore: true)]
        public virtual async Task<IActionResult> GetStringResources(int? languageId)
        {
            var langId = languageId ?? 0;
            var language = await _languageService.GetLanguageByIdAsync(langId);
            if (language == null || !language.Published)
            {
                language = await _workContext.GetWorkingLanguageAsync();
                langId = language.Id;
            }

            var response = new LanguageResourceModel();
            var resources = await _dmsService.LoadLocalizedResourcesAsync(langId);
            foreach (var resource in resources)
                response.StringResources.Add(new KeyValueApi() { Key = resource.Key, Value = resource.Value });

            response.Rtl = language.Rtl;

            return OkWrap(response);
        }

        [HttpGet("getproofofdeliverytype")]
        public virtual async Task<IActionResult> GetProofOfDeliveryType()
        {
            var podTypeId = _dMSSettings.ProofOfDeliveryTypeId;

            //var resources = await _dmsService.LoadLocalizedResourcesAsync(langId);
            var response = new GenericResponseModel<int>();
            response.Data = podTypeId;

            await Task.CompletedTask;

            return Ok(response);
        }

        [HttpPost("uploadproofofdeliveryreferencedata")]
        public virtual async Task<IActionResult> UploadProofOfDeliveryReferenceDataAndMarkAsShipped(ProofOfDeliveryReferenceDataUploadModel model)
        {
            var response = new BaseResponseModel();

            if (!_dMSSettings.EnabledProofOfDelivery)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDeliveryDisabled"));
                return BadRequest(response);
            }

            if (_dMSSettings.ProofOfDeliveryTypeId == (int)ProofOfDeliveryTypes.Otp)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ProofOfDeliveryType.OtpType"));
                return BadRequest(response);
            }

            if (model == null || model.ShipmentId < 1 || model.ProofOfDeliveryReferenceId < 1)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.InvalidData"));
                return BadRequest(response);
            }

            if (model.ProofOfDeliveryTypeId == (int)ProofOfDeliveryTypes.Otp || _dMSSettings.ProofOfDeliveryTypeId != model.ProofOfDeliveryTypeId)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ProofOfDeliveryType.WrongProofOfDeliveryType"));
                return BadRequest(response);
            }

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);

            if (courierShipment == null)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.CourierShipment.NotFound"));
                return BadRequest(response);
            }

            var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);

            if (shipment == null)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ShipmentNotFound"));
                return BadRequest(response);
            }

            if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.Delivered)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.AlreadyMarkedAsDelivered"));
                return BadRequest(response);
            }

            if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.ReceivedByShipper)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.NotPossibleToMark"));
                return BadRequest(response);
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (customer == null || !customer.Active || customer.Deleted)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ShipperNotFound"));
                return BadRequest(response);
            }

            var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

            if (shipper == null)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ShipperNotFound"));
                return BadRequest(response);
            }

            if (customer.Id != shipper.CustomerId)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.WrongShipper"));
                return BadRequest(response);
            }

            var proofOfDeliveryData = new ProofOfDeliveryData()
            {
                ProofOfDeliveryTypeId = model.ProofOfDeliveryTypeId,
                ProofOfDeliveryReferenceId = model.ProofOfDeliveryReferenceId,
                VerifiedByShipperId = shipper.Id,
                VerifiedOnUtc = DateTime.UtcNow,
                NopShipmentId = courierShipment.ShipmentId,
                CourierShipmentId = courierShipment.Id,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            await _proofOfDeliveryDataService.InsertroofOfDeliveryDataAsync(proofOfDeliveryData);
            await _orderProcessingService.DeliverAsync(shipment, true);

            courierShipment.ShipmentStatusType = ShipmentStatusTypes.Delivered;
            courierShipment.UpdatedOnUtc = DateTime.UtcNow;
            await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);



            var note = $"Shipment marked as delivered for shipment Id#{shipment.Id}";
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

            response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsDelivered");

            return Ok(response);
        }

        [HttpPost("verifyshipmentotp")]
        public virtual async Task<IActionResult> VerifyShipmentOtpAndMarkedAsDelivered([FromBody] BaseQueryModel<DMSOtpVerificationRequestModel> queryModel)
        {
            var response = new GenericResponseModel<bool>();

            if (!_dMSSettings.EnabledProofOfDelivery)
            {
                response.Data = new bool();
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDeliveryDisabled"));
                return BadRequest(response);
            }

            if (_dMSSettings.ProofOfDeliveryTypeId != (int)ProofOfDeliveryTypes.Otp)
            {
                response.Data = new bool();
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.NotOtpType"));
                return BadRequest(response);
            }

            if (ModelState.IsValid)
            {
                var model = queryModel.Data;
                var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);

                if (string.IsNullOrEmpty(model.ShipmentOtp))
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.OtpMissing"));

                    return BadRequest(response);
                }

                if (courierShipment == null)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.NotFound"));

                    return BadRequest(response);
                }

                var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);

                if (shipment == null || shipment.DeliveryDateUtc.HasValue)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.ShippingNotPossible"));
                    return BadRequest(response);
                }

                if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.ReceivedByShipper)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.ShippingNotPossible"));
                    return BadRequest(response);
                }

                var customer = await _workContext.GetCurrentCustomerAsync();

                if (customer == null || !customer.Active || customer.Deleted)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ShipperNotFound"));

                    return BadRequest(response);
                }

                var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

                if (shipper == null)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ShipperNotFound"));

                    return BadRequest(response);
                }

                if (customer.Id != shipper.CustomerId)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.WrongShipper"));

                    return BadRequest(response);
                }

                var otpRecord = (await _oTPRecordService.SearchOTPRecordAsync(shipmentId: shipment.Id, deleted: false)).OrderByDescending(q => q.Id).FirstOrDefault();

                if (otpRecord == null)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.OtpNotFound"));

                    //await _dMSOtpService.GenerateCourierVerificationOtpByCourierShipmentAsync(courierShipment);

                    return BadRequest(response);
                }

                if (otpRecord.Verified)
                {
                    response.Data = new bool();
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.AlreadyValidated"));

                    return BadRequest(response);
                }

                if (otpRecord.AuthenticationCode == model.ShipmentOtp)
                {
                    var proofOfDeliveryData = new ProofOfDeliveryData()
                    {
                        ProofOfDeliveryTypeId = (int)ProofOfDeliveryTypes.Otp,
                        ProofOfDeliveryReferenceId = otpRecord.Id,
                        VerifiedByShipperId = shipper.Id,
                        VerifiedOnUtc = DateTime.UtcNow,
                        NopShipmentId = courierShipment.ShipmentId,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow,
                        CourierShipmentId = courierShipment.Id
                    };

                    await _proofOfDeliveryDataService.InsertroofOfDeliveryDataAsync(proofOfDeliveryData);

                    otpRecord.Verified = true;
                    otpRecord.VerifiedByShipperId = shipper.Id;
                    otpRecord.VerifiedOnUtc = DateTime.UtcNow;

                    await _oTPRecordService.UpdateOTPRecordAsync(otpRecord);
                    await _orderProcessingService.DeliverAsync(shipment, true);

                    courierShipment.ShipmentStatusType = ShipmentStatusTypes.Delivered;
                    courierShipment.UpdatedOnUtc = DateTime.UtcNow;
                    await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);


                    var note = $"OTP verified for shipment Id#{shipment.Id}";
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

                    response.Data = true;
                    response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsDelivered");

                    return Ok(response);
                }
                else
                {
                    response.Data = false;
                    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.ValidationFailed"));

                    return BadRequest(response);
                }
            }

            foreach (var modelState in ModelState.Values)
                foreach (var error in modelState.Errors)
                    response.ErrorList.Add(error.ErrorMessage);

            //If we got this far, something failed, redisplay form
            return BadRequest(response);
        }

        [HttpPost("RequestForCourierOtp/{shipmentId:min(0)}")]
        public async Task<IActionResult> RequestForCourierOtp(int shipmentId)
        {
            var response = new BaseResponseModel();
            if (_dMSSettings.EnabledProofOfDelivery && _dMSSettings.ProofOfDeliveryTypeId != (int)ProofOfDeliveryTypes.Otp)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.NotActive"));
                return BadRequest(response);
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            //if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
            //{
            //    response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.NotAuthorized"));
            //    return Unauthorized(response);
            //}

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.NotAuthorized"));
                return Unauthorized(response);
            }

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.ShipmentNotFound") + shipmentId);
                return BadRequest(response);
            }

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.OrderNotFound"));
                return BadRequest(response);
            }

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);

            if (courierShipment == null || courierShipment.ShipmentStatusType != ShipmentStatusTypes.ReceivedByShipper)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.ShipmentNotFound") + shipmentId);
                return BadRequest(response);
            }

            var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

            if (shipper == null || shipper.CustomerId != customer.Id)
            {
                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.NotAuthorized"));
                return Unauthorized(response);
            }

            try
            {
                await _logger.InformationAsync("Opt requested for shipmentId #" + shipmentId);

                var otpRecord = await _dMSOtpService.GenerateCourierVerificationOtpByCourierShipmentAsync(courierShipment, shipper);

                if (otpRecord != null)
                {
                    response.Message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.OtpGenerated");
                    return Ok(response);
                }

                response.ErrorList.Add(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.FailedToGenerateOtp"));

                return BadRequest(response);
            }
            catch (Exception exc)
            {
                //error
                return BadRequest(exc.Message);
            }
        }

        [HttpGet("getdeliverfailedreasons")]
        public virtual async Task<IActionResult> GetDeliverFailedReasons()
        {
            var podTypeId = _dMSSettings.ProofOfDeliveryTypeId;

            var response = new GenericResponseModel<DeliverFailedReasonTypesResponseModel>();

            response.Data.AvailableDeliverFailedReasonTypes = (await DeliverFailedReasonTypes.WrongPhoneNumber.ToSelectListAsync())
                    .Select(item => new SelectListItem(item.Text, item.Value))
                    .ToList();

            await Task.CompletedTask;

            return Ok(response);
        }
    }
}
