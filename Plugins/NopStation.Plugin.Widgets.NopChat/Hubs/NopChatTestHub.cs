using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NopStation.Plugin.Widgets.NopChat.Hubs
{
    public class NopChatTestHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await base.OnDisconnectedAsync(ex);
        }
    }
}
