using System.Threading.Tasks;
using NopStation.Plugin.SMS.Vonage.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.Vonage.Areas.Admin.Factories
{
    public interface IVonageModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}