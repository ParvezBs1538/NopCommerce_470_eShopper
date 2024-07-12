using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public interface IAffiliateStationModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}