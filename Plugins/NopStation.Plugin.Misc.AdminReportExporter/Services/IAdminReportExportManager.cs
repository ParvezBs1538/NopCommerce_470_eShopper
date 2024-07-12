using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Reports;

namespace NopStation.Plugin.Misc.AdminReportExporter.Services
{
    public interface IAdminReportExportManager
    {
        Task<byte[]> ExportBestCustomersReportToXlsxAsync(IEnumerable<BestCustomersReportModel> items);
        Task<byte[]> ExportBestsellersToXlsxAsync(IEnumerable<BestsellerModel> items);
        Task<byte[]> ExportCountrySalesToXlsxAsync(IEnumerable<CountryReportModel> items);
        Task<byte[]> ExportLowStockProductsToXlsxAsync(IEnumerable<LowStockProductModel> items);
        Task<byte[]> ExportNeverSoldProductsToXlsxAsync(IEnumerable<NeverSoldReportModel> items);
        Task<byte[]> ExportRegisteredCustomersReportToXlsxAsync(IEnumerable<RegisteredCustomersReportModel> items);
        Task<byte[]> ExportSalesSummaryToXlsxAsync(IEnumerable<SalesSummaryModel> items);
    }
}