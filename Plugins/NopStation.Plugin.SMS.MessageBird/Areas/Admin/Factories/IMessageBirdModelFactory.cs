using System.Threading.Tasks;
using NopStation.Plugin.SMS.MessageBird.Areas.Admin.Models;

namespace NopStation.Plugin.SMS.MessageBird.Areas.Admin.Factories
{
    public interface IMessageBirdModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}