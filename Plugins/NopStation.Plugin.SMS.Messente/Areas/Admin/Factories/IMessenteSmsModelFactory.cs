using System.Threading.Tasks;
using NopStation.Plugin.SMS.Messente.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.Messente.Areas.Admin.Factories
{
    public interface IMessenteSmsModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}