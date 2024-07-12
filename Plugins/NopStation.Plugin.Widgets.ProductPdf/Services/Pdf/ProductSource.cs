using System.Collections.Generic;
using System.ComponentModel;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common.Pdf;

namespace NopStation.Plugin.Widgets.ProductPdf.Services.Pdf
{
    /// <summary>
    /// Represents the data source for an product document
    /// </summary>
    public partial class ProductSource : DocumentSource
    {
        #region Properties

        /// <summary>
        /// Gets or sets the store name
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Gets or sets a set of paths to product image
        /// </summary>
        public string PicturePath { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the product SKU
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the product price
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Gets or sets the product overview
        /// </summary>
        [DisplayName("NopStation.ProductPdf.Overview")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the product specification attribute
        /// </summary>
        [DisplayName("NopStation.ProductPdf.Specifications")]
        public Dictionary<string, string> SpecificationAttributes { get; set; }

        #endregion
    }
}