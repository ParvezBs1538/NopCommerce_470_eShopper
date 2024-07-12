using System.Threading.Tasks;
using NopStation.Plugin.SMS.BulkSms.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.BulkSms.Areas.Admin.Factories
{
    public interface IBulkSmsModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}