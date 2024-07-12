using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProductRequests.Domains;
using NopStation.Plugin.Widgets.ProductRequests.Models;
using NopStation.Plugin.Widgets.ProductRequests.Services;
using Nop.Services.Helpers;

namespace NopStation.Plugin.Widgets.ProductRequests.Factories
{
    public class ProductRequestModelFactory : IProductRequestModelFactory
    {
        private readonly IWorkContext _workContext;
        private readonly ProductRequestSettings _productRequestSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductRequestService _productRequestService;

        public ProductRequestModelFactory(IWorkContext workContext,
            ProductRequestSettings productRequestSettings, 
            IDateTimeHelper dateTimeHelper,
            IProductRequestService productRequestService)
        {
            _workContext = workContext;
            _productRequestSettings  = productRequestSettings;
            _dateTimeHelper = dateTimeHelper;
            _productRequestService = productRequestService;
        }

        public async Task<ProductRequestListModel> PrepareProductRequestListModelAsync()
        {
            var requests = await _productRequestService.GetAllProductRequestsAsync(customerId: (await _workContext.GetCurrentCustomerAsync()).Id);
            var model = new ProductRequestListModel();
            model.Requests = await requests.SelectAwait(async x => await PrepareProductRequestModelAsync(x)).ToListAsync();

            return model;
        }

        public ProductRequestModel PrepareAddNewProductRequestModel()
        {
            var model = new ProductRequestModel
            {
                DescriptionRequired = _productRequestSettings.DescriptionRequired,
                LinkRequired = _productRequestSettings.LinkRequired
            };

            return model;
        }

        public async Task<ProductRequestModel> PrepareProductRequestModelAsync(ProductRequest productRequest)
        {
            var model = new ProductRequestModel
            {
                DescriptionRequired = _productRequestSettings.DescriptionRequired,
                LinkRequired = _productRequestSettings.LinkRequired,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(productRequest.CreatedOnUtc, DateTimeKind.Utc),
                AdminComment = productRequest.AdminComment,
                Description = productRequest.Description,
                Id = productRequest.Id,
                Link = productRequest.Link,
                Name = productRequest.Name
            };

            return model;
        }
    }
}
