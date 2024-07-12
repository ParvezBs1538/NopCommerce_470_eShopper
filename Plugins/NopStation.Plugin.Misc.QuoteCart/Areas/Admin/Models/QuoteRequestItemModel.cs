using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record QuoteRequestItemModel : BaseNopEntityModel
{
    public QuoteRequestItemModel()
    {
        ProductAttributes = [];
    }

    public int QuoteRequestId { get; set; }

    public int StoreId { get; set; }

    public int CustomerId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductId")]
    public int ProductId { get; set; }

    public string ProductName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductSku")]
    public string ProductSku { get; set; }

    public string AttributesXml { get; set; }

    public string FormattedAttributes { get; set; }


    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.DiscountedPrice")]
    public decimal DiscountedPrice { get; set; }

    public string DiscountedPriceStr { get; set; }

    public string TotalPriceStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.Quantity")]
    public int Quantity { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductPrice")]
    public decimal ProductPrice { get; set; }

    public string ProductPriceStr { get; set; }

    public IList<ProductAttributeModel> ProductAttributes { get; set; }

    public string RentalInfo { get; set; }

    #region Nested classes

    public partial record ProductAttributeModel : BaseNopEntityModel
    {
        public ProductAttributeModel()
        {
            Values = [];
        }

        public int ProductAttributeId { get; set; }

        public string Name { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        public bool HasCondition { get; set; }

        /// <summary>
        /// Allowed file extensions for customer uploaded files
        /// </summary>
        public IList<string> AllowedFileExtensions { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<ProductAttributeValueModel> Values { get; set; }
    }

    public partial record ProductAttributeValueModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }

        public string PriceAdjustment { get; set; }

        public decimal PriceAdjustmentValue { get; set; }

        public bool CustomerEntersQty { get; set; }

        public int Quantity { get; set; }
    }

    #endregion
}
