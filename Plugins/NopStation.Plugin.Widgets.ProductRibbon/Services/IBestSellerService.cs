using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Widgets.ProductRibbon.Services
{
    public interface IBestSellerService
    {
        Task<BestsellersReportLine> BestSellerReportAsync(int productId);
    }
}