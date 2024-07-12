using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Factories
{
    public interface IProgressiveWebAppModelFactory
    {
        Task<FooterComponentModel> PrepareFooterComponentModelAsync();
    }
}