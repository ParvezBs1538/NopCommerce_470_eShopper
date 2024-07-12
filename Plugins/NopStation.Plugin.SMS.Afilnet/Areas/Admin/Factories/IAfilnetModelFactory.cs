using System.Threading.Tasks;
using NopStation.Plugin.SMS.Afilnet.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.Afilnet.Areas.Admin.Factories
{
    public interface IAfilnetModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}