using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public interface IHelpdeskModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}