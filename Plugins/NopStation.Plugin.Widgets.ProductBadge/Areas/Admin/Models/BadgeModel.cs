using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public record BadgeModel : BaseNopEntityModel, ILocalizedModel<BadgeLocalizedModel>, IAclSupportedModel, IStoreMappingSupportedModel
{
    #region Ctor

    public BadgeModel()
    {
        Locales = new List<BadgeLocalizedModel>();
        SelectedStoreIds = new List<int>();
        BestSellOrderStatusIds = new List<int>();
        BestSellPaymentStatusIds = new List<int>();
        BestSellShippingStatusIds = new List<int>();
        AvailableStores = new List<SelectListItem>();
        AvailablePositionTypes = new List<SelectListItem>();
        AvailableContentTypes = new List<SelectListItem>();
        AvailableCatalogTypes = new List<SelectListItem>();
        AvailableBadgeTypes = new List<SelectListItem>();
        AvailableShapeTypes = new List<SelectListItem>();
        AvailableDiscountBadgeTextFormats = new List<SelectListItem>();
        BadgeManufactureSearchModel = new BadgeManufacturerSearchModel();
        BadgeProductSearchModel = new BadgeProductSearchModel();
        BadgeCategorySearchModel = new BadgeCategorySearchModel();
        BadgeVendorSearchModel = new BadgeVendorSearchModel();
        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();
        AvailableOrderStatuses = new List<SelectListItem>();
        AvailablePaymentStatuses = new List<SelectListItem>();
        AvailableShippingStatuses = new List<SelectListItem>();
        AvailableSizes = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.ContentType")]
    public int ContentTypeId { get; set; }
    public string ContentTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.ShapeType")]
    public int ShapeTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.Text")]
    public string Text { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.Size")]
    public int SizeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BackgroundColor")]
    public string BackgroundColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.FontColor")]
    public string FontColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.Picture")]
    [UIHint("Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BadgeType")]
    public int BadgeTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.DiscountBadgeTextFormat")]
    public int DiscountBadgeTextFormatId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellSoldInDays")]
    public int BestSellSoldInDays { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellMinimumAmountSold")]
    public decimal BestSellMinimumAmountSold { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellMinimumQuantitySold")]
    public int BestSellMinimumQuantitySold { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellStoreWise")]
    public bool BestSellStoreWise { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellPaymentStatusIds")]
    public IList<int> BestSellPaymentStatusIds { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellOrderStatusIds")]
    public IList<int> BestSellOrderStatusIds { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.BestSellShippingStatusIds")]
    public IList<int> BestSellShippingStatusIds { get; set; }
    public string CurrencyCode { get; set; }


    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.CatalogType")]
    public int CatalogTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.PositionType")]
    public int PositionTypeId { get; set; }
    public string PositionTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.CssClass")]
    public string CssClass { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.FromDateTimeUtc")]
    [UIHint("DateTimeNullable")]
    public DateTime? FromDateTimeUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.ToDateTimeUtc")]
    [UIHint("DateTimeNullable")]
    public DateTime? ToDateTimeUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.LimitedToStores")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.AclCustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public IList<SelectListItem> AvailablePositionTypes { get; set; }
    public IList<SelectListItem> AvailableContentTypes { get; set; }
    public IList<SelectListItem> AvailableShapeTypes { get; set; }
    public IList<SelectListItem> AvailableCatalogTypes { get; set; }
    public IList<SelectListItem> AvailableBadgeTypes { get; set; }
    public IList<SelectListItem> AvailableDiscountBadgeTextFormats { get; set; }
    public IList<SelectListItem> AvailableOrderStatuses { get; set; }
    public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
    public IList<SelectListItem> AvailableShippingStatuses { get; set; }
    public IList<SelectListItem> AvailableSizes { get; set; }
    public IList<BadgeLocalizedModel> Locales { get; set; }

    public BadgeProductSearchModel BadgeProductSearchModel { get; set; }
    public BadgeCategorySearchModel BadgeCategorySearchModel { get; set; }
    public BadgeManufacturerSearchModel BadgeManufactureSearchModel { get; set; }
    public BadgeVendorSearchModel BadgeVendorSearchModel { get; set; }

    #endregion
}

public class BadgeLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.Fields.Text")]
    public string Text { get; set; }
}