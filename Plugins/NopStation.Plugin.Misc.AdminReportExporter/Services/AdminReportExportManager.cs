using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Services.ExportImport.Help;
using Nop.Web.Areas.Admin.Models.Reports;

namespace NopStation.Plugin.Misc.AdminReportExporter.Services
{
    public class AdminReportExportManager : IAdminReportExportManager
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public AdminReportExportManager(CatalogSettings catalogSettings)
        {
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        public async Task<byte[]> ExportSalesSummaryToXlsxAsync(IEnumerable<SalesSummaryModel> items)
        {
            return await new PropertyManager<SalesSummaryModel, Language>(new PropertyByName<SalesSummaryModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<SalesSummaryModel>(nameof(SalesSummaryModel.Summary)), (x, l) => x.Summary),
                new(NopModelHelper.PropertyLabel<SalesSummaryModel>(nameof(SalesSummaryModel.NumberOfOrders)), (x, l) => x.NumberOfOrders),
                new(NopModelHelper.PropertyLabel<SalesSummaryModel>(nameof(SalesSummaryModel.ProfitStr)), (x, l) => x.ProfitStr),
                new(NopModelHelper.PropertyLabel<SalesSummaryModel>(nameof(SalesSummaryModel.Shipping)), (x, l) => x.Shipping),
                new(NopModelHelper.PropertyLabel<SalesSummaryModel>(nameof(SalesSummaryModel.Tax)), (x, l) => x.Tax),
                new(NopModelHelper.PropertyLabel<SalesSummaryModel>(nameof(SalesSummaryModel.OrderTotal)), (x, l) => x.OrderTotal),
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        public async Task<byte[]> ExportNeverSoldProductsToXlsxAsync(IEnumerable<NeverSoldReportModel> items)
        {
            return await new PropertyManager<NeverSoldReportModel, Language>(new PropertyByName<NeverSoldReportModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<NeverSoldReportModel>(nameof(NeverSoldReportModel.ProductId)), (x, l) => x.ProductId),
                new(NopModelHelper.PropertyLabel<NeverSoldReportModel>(nameof(NeverSoldReportModel.ProductName)), (x, l) => x.ProductName),
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        public async Task<byte[]> ExportLowStockProductsToXlsxAsync(IEnumerable<LowStockProductModel> items)
        {
            return await new PropertyManager<LowStockProductModel, Language>(new PropertyByName<LowStockProductModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<LowStockProductModel>(nameof(LowStockProductModel.Id)), (x, l) => x.Id),
                new(NopModelHelper.PropertyLabel<LowStockProductModel>(nameof(LowStockProductModel.Name)), (x, l) => x.Name),
                new(NopModelHelper.PropertyLabel<LowStockProductModel>(nameof(LowStockProductModel.Attributes)), (x, l) => x.Attributes),
                new(NopModelHelper.PropertyLabel<LowStockProductModel>(nameof(LowStockProductModel.ManageInventoryMethod)), (x, l) => x.ManageInventoryMethod),
                new(NopModelHelper.PropertyLabel<LowStockProductModel>(nameof(LowStockProductModel.StockQuantity)), (x, l) => x.StockQuantity),
                new(NopModelHelper.PropertyLabel<LowStockProductModel>(nameof(LowStockProductModel.Published)), (x, l) => x.Published),
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        public async Task<byte[]> ExportBestsellersToXlsxAsync(IEnumerable<BestsellerModel> items)
        {
            return await new PropertyManager<BestsellerModel, Language>(new PropertyByName<BestsellerModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<BestsellerModel>(nameof(BestsellerModel.ProductId)), (x, l) => x.ProductId),
                new(NopModelHelper.PropertyLabel<BestsellerModel>(nameof(BestsellerModel.ProductName)), (x, l) => x.ProductName),
                new(NopModelHelper.PropertyLabel<BestsellerModel>(nameof(BestsellerModel.TotalQuantity)), (x, l) => x.TotalQuantity),
                new(NopModelHelper.PropertyLabel<BestsellerModel>(nameof(BestsellerModel.TotalAmount)), (x, l) => x.TotalAmount)
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        public async Task<byte[]> ExportCountrySalesToXlsxAsync(IEnumerable<CountryReportModel> items)
        {
            return await new PropertyManager<CountryReportModel, Language>(new PropertyByName<CountryReportModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<CountryReportModel>(nameof(CountryReportModel.CountryName)), (x, l) => x.CountryName),
                new(NopModelHelper.PropertyLabel<CountryReportModel>(nameof(CountryReportModel.TotalOrders)), (x, l) => x.TotalOrders),
                new(NopModelHelper.PropertyLabel<CountryReportModel>(nameof(CountryReportModel.SumOrders)), (x, l) => x.SumOrders),
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        public async Task<byte[]> ExportRegisteredCustomersReportToXlsxAsync(IEnumerable<RegisteredCustomersReportModel> items)
        {
            return await new PropertyManager<RegisteredCustomersReportModel, Language>(new PropertyByName<RegisteredCustomersReportModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<RegisteredCustomersReportModel>(nameof(RegisteredCustomersReportModel.Period)), (x, l) => x.Period),
                new(NopModelHelper.PropertyLabel<RegisteredCustomersReportModel>(nameof(RegisteredCustomersReportModel.Customers)), (x, l) => x.Customers),
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        public async Task<byte[]> ExportBestCustomersReportToXlsxAsync(IEnumerable<BestCustomersReportModel> items)
        {
            return await new PropertyManager<BestCustomersReportModel, Language>(new PropertyByName<BestCustomersReportModel, Language>[]
            {
                new(NopModelHelper.PropertyLabel<BestCustomersReportModel>(nameof(BestCustomersReportModel.CustomerId)), (x, l) => x.CustomerId),
                new(NopModelHelper.PropertyLabel<BestCustomersReportModel>(nameof(BestCustomersReportModel.CustomerName)), (x, l) => x.CustomerName),
                new(NopModelHelper.PropertyLabel<BestCustomersReportModel>(nameof(BestCustomersReportModel.OrderCount)), (x, l) => x.OrderCount),
                new(NopModelHelper.PropertyLabel<BestCustomersReportModel>(nameof(BestCustomersReportModel.OrderTotal)), (x, l) => x.OrderTotal),
            }, _catalogSettings).ExportToXlsxAsync(items);
        }

        #endregion
    }
}
