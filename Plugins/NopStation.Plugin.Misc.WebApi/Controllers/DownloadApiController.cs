using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Models.Common;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/download")]
    public partial class DownloadApiController : BaseApiController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public DownloadApiController(CustomerSettings customerSettings,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext)
        {
            _customerSettings = customerSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _orderService = orderService;
            _productService = productService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpGet("sample/{productId}")]
        public virtual async Task<IActionResult> Sample(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            if (!product.HasSampleDownload)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.NoSampleDownload"));

            var download = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
            if (download == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.SampleDownloadNotAvailable"));

            if (download.UseDownloadUrl)
            {
                var response = new GenericResponseModel<DownloadModel>();
                response.Data.Redirect = true;
                response.Data.DownloadUrl = download.DownloadUrl;
                return Ok(response);
            }

            if (download.DownloadBinary == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadDataNotAvailable"));

            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        [HttpGet("getdownload/{orderItemId}/{agree?}")]
        public virtual async Task<IActionResult> GetDownload(Guid orderItemId, bool agree = false)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemId);
            if (orderItem == null)
                return NotFound();

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (!await _orderService.IsDownloadAllowedAsync(orderItem))
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.NotAllowed"));

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (await _workContext.GetCurrentCustomerAsync() == null)
                    return Unauthorized();

                if (order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.NotYourOrder"));
            }

            var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
            if (download == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadNotAvailable"));

            if (product.HasUserAgreement && !agree)
            {
                var response = new GenericResponseModel<DownloadModel>();
                response.Data.HasUserAgreement = true;
                response.Data.OrderItemId = orderItemId;
                return Ok(response);
            }

            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return BadRequest(string.Format(await _localizationService.GetResourceAsync("DownloadableProducts.ReachedMaximumNumber"), product.MaxNumberOfDownloads));

            if (download.UseDownloadUrl)
            {
                //increase download
                orderItem.DownloadCount++;
                await _orderService.UpdateOrderAsync(order);

                var response = new GenericResponseModel<DownloadModel>();
                response.Data.Redirect = true;
                response.Data.DownloadUrl = download.DownloadUrl;
                return Ok(response);
            }

            //binary download
            if (download.DownloadBinary == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadDataNotAvailable"));

            //increase download
            orderItem.DownloadCount++;
            await _orderService.UpdateOrderAsync(order);

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        [HttpGet("getlicense/{orderItemId}")]
        public virtual async Task<IActionResult> GetLicense(Guid orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemId);
            if (orderItem == null)
                return NotFound();

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (!await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.NotAllowed"));

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (await _workContext.GetCurrentCustomerAsync() == null || order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return Unauthorized();
            }

            var download = await _downloadService.GetDownloadByIdAsync(orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0);
            if (download == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadNotAvailable"));

            if (download.UseDownloadUrl)
            {
                var response = new GenericResponseModel<DownloadModel>();
                response.Data.Redirect = true;
                response.Data.DownloadUrl = download.DownloadUrl;
                return Ok(response);
            }

            //binary download
            if (download.DownloadBinary == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadDataNotAvailable"));

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        [HttpGet("getfileupload/{downloadId}")]
        public virtual async Task<IActionResult> GetFileUpload(Guid downloadId)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(downloadId);
            if (download == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadNotAvailable"));

            if (download.UseDownloadUrl)
            {
                var response = new GenericResponseModel<DownloadModel>();
                response.Data.Redirect = true;
                response.Data.DownloadUrl = download.DownloadUrl;
                return Ok(response);
            }

            //binary download
            if (download.DownloadBinary == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadDataNotAvailable"));

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        [HttpGet("ordernotefile/{orderNoteId}")]
        public virtual async Task<IActionResult> GetOrderNoteFile(int orderNoteId)
        {
            var orderNote = await _orderService.GetOrderNoteByIdAsync(orderNoteId);
            if (orderNote == null)
                return NotFound();

            var order = await _orderService.GetOrderByIdAsync(orderNote.OrderId);

            if (await _workContext.GetCurrentCustomerAsync() == null || order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return Unauthorized();

            var download = await _downloadService.GetDownloadByIdAsync(orderNote.DownloadId);
            if (download == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadNotAvailable"));

            if (download.UseDownloadUrl)
            {
                var response = new GenericResponseModel<DownloadModel>();
                response.Data.Redirect = true;
                response.Data.DownloadUrl = download.DownloadUrl;
                return Ok(response);
            }

            //binary download
            if (download.DownloadBinary == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Download.DownloadDataNotAvailable"));

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderNote.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        #endregion
    }
}