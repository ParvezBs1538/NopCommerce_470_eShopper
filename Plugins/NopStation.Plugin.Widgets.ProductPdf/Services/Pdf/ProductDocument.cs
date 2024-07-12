using Nop.Services.Common.Pdf;
using Nop.Services.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NopStation.Plugin.Widgets.ProductPdf.Services.Pdf
{
    /// <summary>
    /// Represents the product document
    /// </summary>
    public partial class ProductDocument : PdfDocument<ProductSource>
    {
        #region Ctor

        public ProductDocument(ProductSource productSource, ILocalizationService localizationService) : base(productSource, localizationService)
        {
        }

        #endregion

        #region Utils

        /// <summary>
        /// Compose the product
        /// </summary>
        /// <param name="container">Content placement container</param>
        protected void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(20);
                column.Item().Element(x => ComposeProductImage(x));
                column.Item().Element(x => ComposeProductInfo(x));
                column.Item().Element(x => ComposeProductDetails(x));
            });
        }

        /// <summary>
        /// Compose the header
        /// </summary>
        /// <param name="container">Content placement container</param>
        protected void ComposeHeader(IContainer container)
        {
            container.DefaultTextStyle(tStyle => tStyle.Bold().FontSize(18)).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignCenter().Text(t => ComposeField(t, Source, x => x.StoreName));
                });
            });
        }

        /// <summary>
        /// Compose product image
        /// </summary>
        /// <param name="container">Content placement container</param>
        /// <param name="productItem">Catalog item</param>
        protected void ComposeProductImage(IContainer container)
        {
            container.DefaultTextStyle(tStyle => tStyle.SemiBold()).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignCenter().AlignTop().Element(x => x.Width(300f).Image(Source.PicturePath));
                });
            });
        }


        /// <summary>
        /// Compose a generic product info
        /// </summary>
        /// <param name="container">Content placement container</param>
        /// <param name="productItem">Catalog item</param>
        protected void ComposeProductInfo(IContainer container)
        {
            container.DefaultTextStyle(tStyle => tStyle.FontSize(13)).Column(column =>
            {
                column.Item().AlignCenter().Text(t => ComposeField(t, Source, x => x.Name));
                column.Item().AlignCenter().Text(t => ComposeField(t, Source, x => x.Sku));
                column.Item().AlignCenter().Text(t => ComposeField(t, Source, x => x.Price));;
            });
        }

        /// <summary>
        /// Compose a generic product details
        /// </summary>
        /// <param name="container">Content placement container</param>
        /// <param name="productItem">Catalog item</param>
        protected void ComposeProductDetails(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
                table.Header(header =>
                {
                    header.Cell().Element(cellStyle).AlignCenter().Text(t => ComposeLabel<ProductSource>(t, x => x.SpecificationAttributes));
                    header.Cell().Element(cellStyle).AlignCenter().Text(t => ComposeLabel<ProductSource>(t, x => x.ShortDescription));
                    static IContainer cellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });
                table.Cell().Table(table2 =>
                {
                    table2.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    foreach (var item in Source.SpecificationAttributes)
                    {
                        table2.Cell().AlignLeft().Text(item.Key);
                        table2.Cell().AlignLeft().Text(item.Value);
                    }
                });
                table.Cell().AlignLeft().Text(Source.ShortDescription);
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compose a document's structure
        /// </summary>
        /// <param name="container">Content placement container</param>
        public override void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    var titleStyle = DefaultStyle.FontSize(10).NormalWeight();
                    page.DefaultTextStyle(titleStyle);

                    if (Source.IsRightToLeft)
                        page.ContentFromRightToLeft();

                    page.Size(Source.PageSize);
                    page.Margin(35);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                });
        }

        #endregion
    }
}