using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nop.Core;

namespace NopStation.Plugin.Misc.AlgoliaSearch
{
    public class ProductUploadHub : Hub
    {
        private readonly IWorkContext _workContext;
        private readonly IHubContext<ProductUploadHub> _hubContext;

        public ProductUploadHub(IWorkContext workContext,
            IHubContext<ProductUploadHub> hubContext)
        {
            _hubContext = hubContext;
            _workContext = workContext;
        }

        public async Task UploadProductsAsync(int pageNumber, int totalPages, int currentPageProducts, int totalProducts,
            int binding, int failed, int uploaded, int status, string message = "")
        {
            var customerGuid = (await _workContext.GetCurrentCustomerAsync()).CustomerGuid;
            await _hubContext.Clients.All.SendAsync("dataSent", new
            {
                TotalProducts = totalProducts,
                UploadedProducts = uploaded,
                CurrentPageProducts = currentPageProducts,
                Binding = binding,
                CurrentPage = pageNumber + 1,
                TotalPages = totalPages,
                Failed = failed,
                Status = status,
                Message = message,
                CustomerGuid = customerGuid
            });
        }
    }
}
