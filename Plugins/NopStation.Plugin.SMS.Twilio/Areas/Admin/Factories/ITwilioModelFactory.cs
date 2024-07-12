using System.Threading.Tasks;
using NopStation.Plugin.SMS.Twilio.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.Twilio.Areas.Admin.Factories
{
    public interface ITwilioModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}