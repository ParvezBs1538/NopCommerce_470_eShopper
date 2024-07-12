using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Factories;

public interface IBadgeModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();

    Task<BadgeSearchModel> PrepareBadgeSearchModelAsync(BadgeSearchModel searchModel);

    Task<BadgeListModel> PrepareBadgeListModelAsync(BadgeSearchModel searchModel);

    Task<BadgeModel> PrepareBadgeModelAsync(BadgeModel model, Badge badge, bool excludeProperties = false);

    Task<BadgeCategoryListModel> PrepareBadgeCategoryListModelAsync(BadgeCategorySearchModel searchModel, Badge badge);

    Task<AddCategoryToBadgeSearchModel> PrepareAddCategoryToBadgeSearchModelAsync(AddCategoryToBadgeSearchModel searchModel);

    Task<AddCategoryToBadgeListModel> PrepareAddCategoryToBadgeListModelAsync(AddCategoryToBadgeSearchModel searchModel);

    Task<BadgeManufacturerListModel> PrepareBadgeManufacturerListModelAsync(BadgeManufacturerSearchModel searchModel, Badge badge);

    Task<AddManufacturerToBadgeSearchModel> PrepareAddManufacturerToBadgeSearchModelAsync(AddManufacturerToBadgeSearchModel searchModel);

    Task<AddManufacturerToBadgeListModel> PrepareAddManufacturerToBadgeListModelAsync(AddManufacturerToBadgeSearchModel searchModel);

    Task<BadgeProductListModel> PrepareBadgeProductListModelAsync(BadgeProductSearchModel searchModel, Badge badge);

    Task<AddProductToBadgeSearchModel> PrepareAddProductToBadgeSearchModelAsync(AddProductToBadgeSearchModel searchModel);

    Task<AddProductToBadgeListModel> PrepareAddProductToBadgeListModelAsync(AddProductToBadgeSearchModel searchModel);

    Task<BadgeVendorListModel> PrepareBadgeVendorListModelAsync(BadgeVendorSearchModel searchModel, Badge badge);

    Task<AddVendorToBadgeSearchModel> PrepareAddVendorToBadgeSearchModelAsync(AddVendorToBadgeSearchModel searchModel);

    Task<AddVendorToBadgeListModel> PrepareAddVendorToBadgeListModelAsync(AddVendorToBadgeSearchModel searchModel);
}