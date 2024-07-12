using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Order;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Models.Order;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/returnrequest")]
    public class ReturnRequestApiController : BaseApiController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IReturnRequestModelFactory _returnRequestModelFactory;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public ReturnRequestApiController(ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IReturnRequestService returnRequestService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings)
        {
            _customerService = customerService;
            _customNumberFormatter = customNumberFormatter;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _returnRequestModelFactory = returnRequestModelFactory;
            _returnRequestService = returnRequestService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Methods

        [HttpGet("history")]
        public virtual async Task<IActionResult> CustomerReturnRequests()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var response = await _returnRequestModelFactory.PrepareCustomerReturnRequestsModelAsync();
            return OkWrap(response);
        }

        [HttpGet("returnrequest/{orderId}")]
        public virtual async Task<IActionResult> ReturnRequest(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return BadRequest();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return BadRequest();

            var response = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(new SubmitReturnRequestModel(), order);
            return OkWrap(response);
        }

        [HttpPost("returnrequest/{orderId}")]
        public virtual async Task<IActionResult> ReturnRequest(int orderId, [FromBody] BaseQueryModel<SubmitReturnRequestModel> queryModel)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return BadRequest();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return BadRequest();

            var count = 0;

            var downloadId = 0;
            if (_orderSettings.ReturnRequestsAllowFiles)
            {
                var download = await _downloadService.GetDownloadByGuidAsync(queryModel.Data.UploadedFileGuid);
                if (download != null)
                    downloadId = download.Id;
            }

            //returnable products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isNotReturnable: false);
            foreach (var orderItem in orderItems)
            {
                var quantity = 0; //parse quantity
                foreach (var formKey in queryModel.FormValues)
                    if (formKey.Key.Equals($"quantity{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(formKey.Value, out quantity);
                        break;
                    }
                if (quantity > 0)
                {
                    var rrr = await _returnRequestService.GetReturnRequestReasonByIdAsync(queryModel.Data.ReturnRequestReasonId);
                    var rra = await _returnRequestService.GetReturnRequestActionByIdAsync(queryModel.Data.ReturnRequestActionId);

                    var rr = new ReturnRequest
                    {
                        CustomNumber = "",
                        StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                        OrderItemId = orderItem.Id,
                        Quantity = quantity,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                        ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                        RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                        CustomerComments = queryModel.Data.Comments,
                        UploadedFileId = downloadId,
                        StaffNotes = string.Empty,
                        ReturnRequestStatus = ReturnRequestStatus.Pending,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    await _returnRequestService.InsertReturnRequestAsync(rr);

                    //set return request custom number
                    rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                    await _returnRequestService.UpdateReturnRequestAsync(rr);

                    //notify store owner
                    await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                    //notify customer
                    await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                    count++;
                }
            }

            queryModel.Data = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(queryModel.Data, order);
            if (count > 0)
                queryModel.Data.Result = await _localizationService.GetResourceAsync("ReturnRequests.Submitted");
            else
                queryModel.Data.Result = await _localizationService.GetResourceAsync("ReturnRequests.NoItemsSubmitted");

            return OkWrap(queryModel.Data);
        }

        [HttpPost("uploadfile")]
        public virtual async Task<IActionResult> UploadFileReturnRequest()
        {
            if (!_orderSettings.ReturnRequestsEnabled || !_orderSettings.ReturnRequestsAllowFiles)
                return BadRequest();

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.ReturnRequest.NoFileUploaded"));

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var validationFileMaximumSize = _orderSettings.ReturnRequestsFileMaximumSize;
            if (validationFileMaximumSize > 0)
            {
                //compare in bytes
                var maxFileSizeBytes = validationFileMaximumSize * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                    return BadRequest(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), validationFileMaximumSize));
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            var response = new GenericResponseModel<UploadFileReturnRequestModel>();
            response.Data = new UploadFileReturnRequestModel()
            {
                DownloadGuid = download.DownloadGuid
            };

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Ok(response);
        }

        #endregion
    }
}
