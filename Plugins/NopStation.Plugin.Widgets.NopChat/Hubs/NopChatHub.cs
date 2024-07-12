using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nop.Services.Customers;
using Nop.Services.Vendors;
using NopStation.Plugin.Widgets.NopChat.Models;
using NopStation.Plugin.Widgets.NopChat.Services;

namespace NopStation.Plugin.Widgets.NopChat.Hubs
{
    public class NopChatHub : Hub
    {
        #region Fields

        private readonly IHubContext<NopChatHub> _hubContext;
        private readonly INopChatMessageService _chatNopMessageService;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public NopChatHub(IHubContext<NopChatHub> hubContext,
            INopChatMessageService chatNopMessageService,
            ICustomerService customerService,
            IVendorService vendorService)
        {
            _hubContext = hubContext;
            _chatNopMessageService = chatNopMessageService;
            _customerService = customerService;
            _vendorService = vendorService;
        }

        #endregion

        #region Methods

        public async Task SendNewMessage(NopChatMessageModel model)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);

            if (vendor != null && vendor.Name != "")
                model.VendorName = vendor.Name;

            if (model.IsVendorResponse == false)
            {
                await _hubContext.Clients.Group(model.VendorName).SendAsync("NewMessagesHub", new
                {
                    message = model
                });
            }
            else
            {
                var userName = _customerService.GetCustomerByIdAsync(model.CustomerId).Result.Username.ToString();
                await _hubContext.Clients.Groups(userName, model.VendorName).SendAsync("NewMessagesHub", new
                {
                    message = model
                });
            }
        }

        #endregion

        #region Connections

        public override async Task OnConnectedAsync()
        {
            var connectionName = Context.User.Identity.Name;
            var vendorName = _chatNopMessageService.GetVendorNameByCustomerNameIfExxistAsync(connectionName).Result;

            if (vendorName != "")
                connectionName = vendorName.ToString();
            if (connectionName == "")
                connectionName = "Guest";

            await Groups.AddToGroupAsync(Context.ConnectionId, connectionName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await base.OnDisconnectedAsync(ex);
        }

        #endregion
    }
}
