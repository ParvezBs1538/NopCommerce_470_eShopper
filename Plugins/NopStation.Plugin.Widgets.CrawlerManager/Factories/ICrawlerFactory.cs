using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Widgets.CrawlerManager.Models;

namespace NopStation.Plugin.Widgets.CrawlerManager.Factories
{
    public interface ICrawlerFactory
    {
        Task<OnlineCustomerSearchModel> PrepareOnlineGuestCustomerSearchModelAsync(OnlineCustomerSearchModel searchModel);
        Task<OnlineCustomerListModel> PrepareOnlineGuestCustomerListModelAsync(OnlineCustomerSearchModel searchModel);
        Task<OnlineCustomerSearchModel> PrepareCrawlersSearchModelAsync(OnlineCustomerSearchModel searchModel);
        Task<CrawlerListModel> PrepareCrawlersListModelAsync(OnlineCustomerSearchModel searchModel);
    }
}
