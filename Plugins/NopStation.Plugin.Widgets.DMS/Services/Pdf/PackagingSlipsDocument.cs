using System.Linq;
using Nop.Services.Common.Pdf;
using Nop.Services.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NopStation.Plugin.Widgets.DMS.Services.Pdf
{
    /// <summary>
    /// Represents the product document
    /// </summary>
    public partial class PackagingSlipsDocument : PdfDocument<PackagingSlipsSource>
    {
        #region Ctor

        public PackagingSlipsDocument(PackagingSlipsSource packagingSlipsSource, ILocalizationService localizationService)
            : base(packagingSlipsSource, localizationService)
        {
        }

        #endregion

        #region Utils

        protected void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {

                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(ComposeAddress);
                    row.ConstantItem(100).Height(100).Image(Source.QRimage, ImageScaling.FitArea);
                });
                if (!string.IsNullOrEmpty(Source.WeightInfo))
                    column.Item().Text(t => ComposeField(t, Source, x => x.WeightInfo, delimiter: ": "));

                if (Source.Products.Any())
                    column.Item().Element(ComposeProducts);
            });
        }
        protected void ComposeHeader(IContainer container)
        {
            container.DefaultTextStyle(tStyle => tStyle.Bold()).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(t => ComposeField(t, Source, x => x.OrderNumberText, delimiter: " #"));
                    column.Item().Text(t => ComposeField(t, Source, x => x.ShipmentNumberText, delimiter: " #"));
                });

            });
        }

        protected void ComposeProducts(IContainer container)
        {

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeaderStyle).Text(t => ComposeLabel<ProductItem>(t, x => x.Name));
                    header.Cell().Element(CellHeaderStyle).Text(t => ComposeLabel<ProductItem>(t, x => x.Sku));
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text(t => ComposeLabel<ProductItem>(t, x => x.Quantity));
                });

                foreach (var product in Source.Products)
                {
                    table.Cell().Element(CellContentStyle).Element(productContainer =>
                    {
                        productContainer.Column(pColumn =>
                        {
                            pColumn.Item().Text(product.Name);

                            foreach (var attribute in product.ProductAttributes)
                                pColumn.Item().DefaultTextStyle(s => s.Italic().FontSize(9)).Text(attribute);
                        });
                    });

                    table.Cell().Element(CellContentStyle).Text(product.Sku);
                    table.Cell().Element(CellContentStyle).AlignRight().Text(product.Quantity);
                }

                static IContainer CellHeaderStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }

                static IContainer CellContentStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }
        protected void ComposeAddress(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(2);
                column.Item()
                    .BorderBottom(1)
                    .PaddingBottom(5)
                    .DefaultTextStyle(style => style.SemiBold())
                    .Text(t => ComposeLabel<ShipmentSource>(t, x => x.Address));

                column.Item().Text(t => ComposeField(t, Source.Address, x => x.Company, delimiter: ": "));
                column.Item().Text(t => ComposeField(t, Source.Address, x => x.Name, delimiter: ": "));
                column.Item().Text(t => ComposeField(t, Source.Address, x => x.Phone, delimiter: ": "));
                column.Item().Text(t => ComposeField(t, Source.Address, x => x.Address, delimiter: ": "));
                column.Item().Text(t => ComposeField(t, Source.Address, x => x.Address2, delimiter: ": "));
                column.Item().Text(Source.Address.AddressLine);
                column.Item().Text(t => ComposeField(t, Source.Address, x => x.VATNumber, delimiter: ": "));
                column.Item().Text(Source.Address.Country);

                foreach (var attribute in Source.Address.AddressAttributes)
                    column.Item().Text(attribute);

                column.Item().Text(t => ComposeField(t, Source.Address, x => x.PaymentMethod, delimiter: ": "));
                column.Item().Text(t => ComposeField(t, Source.Address, x => x.ShippingMethod, delimiter: ": "));

                foreach (var (key, value) in Source.Address.CustomValues)
                {
                    column.Item().Text(text =>
                    {
                        text.Span(key);
                        text.Span(":");
                        text.Span(value?.ToString());
                    });
                }
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compose document's structure
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
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
        }

        #endregion
    }
}