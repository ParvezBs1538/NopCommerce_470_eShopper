using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public interface IDeliverySchedulerModelFactory
    {
        Task<SpecialDeliveryOffsetListModel> PrepareOffsetListModelAsync(CategorySearchModel searchModel);

        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}