using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.ProductBadge.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Factories;

public interface IBadgeModelFactory
{
    Task<BadgeInfoModel> PrepareProductBadgeInfoModelAsync(Product product, bool detailsPage = true);
}