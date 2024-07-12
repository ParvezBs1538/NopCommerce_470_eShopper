using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nop.Core;

namespace NopStation.Plugin.Misc.AmazonPersonalize
{
    public class InteractionsUploadHub : Hub
    {
        private readonly IWorkContext _workContext;
        private readonly IHubContext<InteractionsUploadHub> _hubContext;

        public InteractionsUploadHub(IWorkContext workContext,
            IHubContext<InteractionsUploadHub> hubContext)
        {
            _workContext = workContext;
            _hubContext = hubContext;
        }

        public async Task UploadInteractionsAsync(int pageNumber, int totalPages, int currentPageProducts, int totalProducts,
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