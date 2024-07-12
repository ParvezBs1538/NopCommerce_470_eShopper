using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.ProductRequests.Domains;
using NopStation.Plugin.Widgets.ProductRequests.Factories;
using NopStation.Plugin.Widgets.ProductRequests.Models;
using NopStation.Plugin.Widgets.ProductRequests.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;

namespace NopStation.Plugin.Widgets.ProductRequests.Controllers
{
    public class ProductRequestController : NopStationPublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IProductRequestService _productRequestService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly ProductRequestSettings _productRequestSettings;
        private readonly IStoreContext _storeContext;
        private readonly IProductRequestModelFactory _productRequestModelFactory;
        private readonly IWebHelper _webHelper;

        public ProductRequestController(IWorkContext workContext,
            IProductRequestService productRequestService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IProductRequestModelFactory productRequestModelFactory,
            ProductRequestSettings productRequestSettings,
            IStoreContext storeContext,
            IWebHelper webHelper)
        {
            _workContext = workContext;
            _productRequestService = productRequestService;
            _localizationService = localizationService;
            _customerService = customerService;
            _productRequestSettings = productRequestSettings;
            _storeContext = storeContext;
            _productRequestModelFactory = productRequestModelFactory;
            _webHelper = webHelper;
        }

        #region Utilities

        protected virtual async Task<bool> IsMinimumProductRequestCreateIntervalValidAsync(Customer customer)
        {
            if (_productRequestSettings.MinimumProductRequestCreateInterval == 0)
                return true;

            var lastProductRequest = (await _productRequestService.GetAllProductRequestsAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: customer.Id, pageSize: 1))
                .FirstOrDefault();
            if (lastProductRequest == null)
                return true;

            var interval = DateTime.UtcNow - lastProductRequest.CreatedOnUtc;
            return interval.TotalSeconds > _productRequestSettings.MinimumProductRequestCreateInterval;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> History()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (!(await _workContext.GetCurrentCustomerAsync()).HasAccessToProductRequest(_productRequestSettings))
                return RedirectToRoute("HomePage");

            var model = await _productRequestModelFactory.PrepareProductRequestListModelAsync();

            return View(model);
        }

        public async Task<IActionResult> AddNew()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (!(await _workContext.GetCurrentCustomerAsync()).HasAccessToProductRequest(_productRequestSettings))
                return RedirectToRoute("HomePage");

            var model = _productRequestModelFactory.PrepareAddNewProductRequestModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(ProductRequestModel model)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (!(await _workContext.GetCurrentCustomerAsync()).HasAccessToProductRequest(_productRequestSettings))
                return RedirectToRoute("HomePage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await IsMinimumProductRequestCreateIntervalValidAsync(customer))
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.ProductRequests.ProductRequests.MinProductRequestCreateInterval"));

            if (ModelState.IsValid)
            {
                var productRequest = new ProductRequest()
                {
                    Description = model.Description,
                    Link = model.Link,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Name = model.Name,
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id
                };

                await _productRequestService.InsertProductRequestAsync(productRequest);

                return RedirectToRoute("ProductRequestDetails", new { id = productRequest.Id });
            }

            model = _productRequestModelFactory.PrepareAddNewProductRequestModel();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (!(await _workContext.GetCurrentCustomerAsync()).HasAccessToProductRequest(_productRequestSettings))
                return RedirectToRoute("HomePage");

            var productRequest = await _productRequestService.GetProductRequestByIdAsync(id);
            if (productRequest == null || productRequest.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return NotFound();

            var model = await _productRequestModelFactory.PrepareProductRequestModelAsync(productRequest);
            return View(model);
        }

        #endregion
    }
}
