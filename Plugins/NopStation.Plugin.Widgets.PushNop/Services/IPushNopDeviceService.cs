using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Services
{
    public interface IPushNopDeviceService
    {
        Task<IPagedList<WebAppDevice>> GetCampaignDevicesAsync(SmartGroupNotification smartGroupNotification, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}