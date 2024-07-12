using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.ProductBadge.Domains;

public class Badge : BaseEntity, IStoreMappingSupported, IAclSupported, ILocalizedEntity, ISoftDeletedEntity
{
    public string Name { get; set; }

    public bool Active { get; set; }

    public int BadgeTypeId { get; set; }

    public int SizeId { get; set; }

    public int DiscountBadgeTextFormatId { get; set; }

    public int BestSellSoldInDays { get; set; }

    public decimal BestSellMinimumAmountSold { get; set; }

    public int BestSellMinimumQuantitySold { get; set; }

    public bool BestSellStoreWise { get; set; }

    public string BestSellPaymentStatusIds { get; set; }

    public string BestSellOrderStatusIds { get; set; }

    public string BestSellShippingStatusIds { get; set; }

    public int ContentTypeId { get; set; }

    public int ShapeTypeId { get; set; }

    public int CatalogTypeId { get; set; }

    public string Text { get; set; }

    public string BackgroundColor { get; set; }

    public string FontColor { get; set; }

    public int PictureId { get; set; }

    public int PositionTypeId { get; set; }

    public string CssClass { get; set; }

    public DateTime? FromDateTimeUtc { get; set; }

    public DateTime? ToDateTimeUtc { get; set; }

    public bool LimitedToStores { get; set; }

    public bool SubjectToAcl { get; set; }

    public bool Deleted { get; set; }

    public BadgeType BadgeType
    {
        get => (BadgeType)BadgeTypeId;
        set => BadgeTypeId = (int)value;
    }

    public DiscountBadgeTextFormat DiscountBadgeTextFormat
    {
        get => (DiscountBadgeTextFormat)DiscountBadgeTextFormatId;
        set => DiscountBadgeTextFormatId = (int)value;
    }

    public PositionType PositionType
    {
        get => (PositionType)PositionTypeId;
        set => PositionTypeId = (int)value;
    }

    public ContentType ContentType
    {
        get => (ContentType)ContentTypeId;
        set => ContentTypeId = (int)value;
    }

    public ShapeType ShapeType
    {
        get => (ShapeType)ShapeTypeId;
        set => ShapeTypeId = (int)value;
    }

    public CatalogType CatalogType
    {
        get => (CatalogType)CatalogTypeId;
        set => CatalogTypeId = (int)value;
    }

    public Size Size
    {
        get => (Size)SizeId;
        set => SizeId = (int)value;
    }
}