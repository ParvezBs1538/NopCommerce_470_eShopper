using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public interface IWebApiNotificationModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync(ConfigurationModel model);
    }
}