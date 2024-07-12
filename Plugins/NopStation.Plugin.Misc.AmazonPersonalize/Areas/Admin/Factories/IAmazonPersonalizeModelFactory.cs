using System.Threading.Tasks;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Factories
{
    public interface IAmazonPersonalizeModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}