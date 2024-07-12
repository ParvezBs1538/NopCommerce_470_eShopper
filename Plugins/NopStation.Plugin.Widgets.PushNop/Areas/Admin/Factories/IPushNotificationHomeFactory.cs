using System.Threading.Tasks;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories
{
    public interface IPushNotificationHomeFactory
    {
        Task<PushNopDashbordModel> PreparePushNotificationDashboardModelAsync(PushNopDashbordModel model);
    }
}
