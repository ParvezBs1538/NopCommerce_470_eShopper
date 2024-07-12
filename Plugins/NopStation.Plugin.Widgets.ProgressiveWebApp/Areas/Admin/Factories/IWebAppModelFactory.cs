using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public interface IWebAppModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}