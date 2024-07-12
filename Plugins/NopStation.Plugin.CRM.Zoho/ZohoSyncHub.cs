using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nop.Core;

namespace NopStation.Plugin.CRM.Zoho
{
    public class ZohoSyncHub : Hub
    {
        private readonly IWorkContext _workContext;
        private readonly IHubContext<ZohoSyncHub> _hubContext;

        public ZohoSyncHub(IWorkContext workContext,
            IHubContext<ZohoSyncHub> hubContext)
        {
            _hubContext = hubContext;
            _workContext = workContext;
        }

        public async Task SyncZohoItemsAsync(string currentStep, IList<string> selectedTables, IList<string> completedTables, string message = "")
        {
            var customerGuid = (await _workContext.GetCurrentCustomerAsync()).CustomerGuid;
            await _hubContext.Clients.All.SendAsync("zohoSynced", new
            {
                CurrentStep = currentStep,
                SelectedTables = selectedTables,
                CompletedTables = completedTables,
                Message = message,
                CustomerGuid = customerGuid
            });
        }
    }
}
