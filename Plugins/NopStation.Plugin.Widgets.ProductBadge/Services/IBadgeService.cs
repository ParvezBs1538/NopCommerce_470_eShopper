using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services;

public interface IBadgeService
{
    #region Badges

    Task InsertBadgeAsync(Badge badge);

    Task UpdateBadgeAsync(Badge badge);

    Task DeleteBadgeAsync(Badge badge);

    Task<Badge> GetBadgeByIdAsync(int badgeId);

    Task<IPagedList<Badge>> GetAllBadgesAsync(string keywords = null, int storeId = 0, bool showHidden = false,
        bool? overridePublished = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IList<Badge>> GetActiveBadgesAsync(int storeId = 0);

    Task<IList<Badge>> GetProductBadgesAsync(Product product);

    Task<bool> ValidateBadgeAsync(Badge badge, Product product);

    #endregion

    #region Badge product mappings

    Task<BadgeProductMapping> GetBadgeProductMappingByIdAsync(int badgeProductMappingId);

    Task InsertBadgeProductMappingAsync(BadgeProductMapping badgeProductMapping);

    Task UpdateBadgeProductMappingAsync(BadgeProductMapping badgeProductMapping);

    Task DeleteBadgeProductMappingAsync(BadgeProductMapping badgeProductMapping);

    Task<IList<BadgeProductMapping>> GetBadgeProductMappingsAsync(int badgeId);

    Task<IList<Product>> GetProductsByBadgeIdAsync(int badgeId);

    #endregion

    #region Badge category mappings

    Task<BadgeCategoryMapping> GetBadgeCategoryMappingByIdAsync(int badgeCategoryMappingId);

    Task InsertBadgeCategoryMappingAsync(BadgeCategoryMapping badgeCategoryMapping);

    Task UpdateBadgeCategoryMappingAsync(BadgeCategoryMapping badgeCategoryMapping);

    Task DeleteBadgeCategoryMappingAsync(BadgeCategoryMapping badgeCategoryMapping);

    Task<IList<BadgeCategoryMapping>> GetBadgeCategoryMappingsAsync(int badgeId, bool active = true);

    Task<IList<Category>> GetCategoriesByBadgeIdAsync(int badgeId, bool active = true);

    #endregion

    #region Badge manufacturer mappings

    Task<BadgeManufacturerMapping> GetBadgeManufacturerMappingByIdAsync(int badgeManufacturerMappingId);

    Task InsertBadgeManufacturerMappingAsync(BadgeManufacturerMapping badgeManufacturerMapping);

    Task UpdateBadgeManufacturerMappingAsync(BadgeManufacturerMapping badgeManufacturerMapping);

    Task DeleteBadgeManufacturerMappingAsync(BadgeManufacturerMapping badgeManufacturerMapping);

    Task<IList<BadgeManufacturerMapping>> GetBadgeManufacturerMappingsAsync(int badgeId, bool active = true);

    Task<IList<Manufacturer>> GetManufacturersByBadgeIdAsync(int badgeId, bool active = true);

    #endregion

    #region Badge vendor mappings

    Task<BadgeVendorMapping> GetBadgeVendorMappingByIdAsync(int badgeVendorMappingId);

    Task InsertBadgeVendorMappingAsync(BadgeVendorMapping badgeVendorMapping);

    Task UpdateBadgeVendorMappingAsync(BadgeVendorMapping badgeVendorMapping);

    Task DeleteBadgeVendorMappingAsync(BadgeVendorMapping badgeVendorMapping);

    Task<IList<BadgeVendorMapping>> GetBadgeVendorMappingsAsync(int badgeId, bool active = true);

    Task<IList<Vendor>> GetVendorsByBadgeIdAsync(int badgeId, bool active = true);

    #endregion

}