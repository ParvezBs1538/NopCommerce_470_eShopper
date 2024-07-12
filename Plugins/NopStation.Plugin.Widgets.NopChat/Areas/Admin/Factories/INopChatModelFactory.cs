using System.Threading.Tasks;
using NopStation.Plugin.Widgets.NopChat.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.NopChat.Areas.Admin.Factories
{
    public interface INopChatModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}
