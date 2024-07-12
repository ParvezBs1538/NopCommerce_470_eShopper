using System.Threading.Tasks;
using NopStation.Plugin.SMS.TeleSign.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.TeleSign.Areas.Admin.Factories
{
    public interface ITeleSignModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}