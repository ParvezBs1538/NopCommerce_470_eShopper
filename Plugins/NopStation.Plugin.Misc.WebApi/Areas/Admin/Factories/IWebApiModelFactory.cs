using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public interface IWebApiModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<LocaleResourceListModel> PrepareLocaleResourceListModelAsync(LocaleResourceSearchModel searchModel, Language language);
    }
}