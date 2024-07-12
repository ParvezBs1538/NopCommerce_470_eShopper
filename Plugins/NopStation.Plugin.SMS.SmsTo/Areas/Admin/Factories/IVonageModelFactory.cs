using System.Threading.Tasks;
using NopStation.Plugin.SMS.SmsTo.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.SmsTo.Areas.Admin.Factories
{
    public interface ISmsToModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}