using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public interface IWebAppDeviceModelFactory
    {
        WebAppDeviceSearchModel PrepareWebAppDeviceSearchModel(WebAppDeviceSearchModel searchModel);

        Task<WebAppDeviceListModel> PrepareWebAppDeviceListModelAsync(WebAppDeviceSearchModel searchModel);

        Task<WebAppDeviceModel> PrepareWebAppDeviceModelAsync(WebAppDeviceModel model,
            WebAppDevice webAppDevice, bool excludeProperties = false);
    }
}