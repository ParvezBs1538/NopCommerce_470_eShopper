using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Reports;
using NopStation.Plugin.Misc.AdminReportExporter.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Controllers
{
    public class AdminExportController : NopStationAdminController
    {
        private readonly IAdminReportExportManager _exportManager;
        private readonly IPermissionService _permissionService;
        private readonly IReportModelFactory _reportModelFactory;

        public AdminExportController(
            IAdminReportExportManager exportManager,
            IPermissionService permissionService,
            IReportModelFactory reportModelFactory)
        {
            _exportManager = exportManager;
            _permissionService = permissionService;
            _reportModelFactory = reportModelFactory;
        }

        #region Methods

        [HttpPost]
        public async Task<IActionResult> SalesSummary(SalesSummarySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            var reportModel = await _reportModelFactory.PrepareSalesSummaryListModelAsync(searchModel);

            var bytes = await _exportManager.ExportSalesSummaryToXlsxAsync(reportModel.Data);
            return File(bytes, MimeTypes.TextXlsx, "SalesSummary.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> LowStock(LowStockProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            var reportModel = await _reportModelFactory.PrepareLowStockProductListModelAsync(searchModel);

            var bytes = await _exportManager.ExportLowStockProductsToXlsxAsync(reportModel.Data);
            return File(bytes, MimeTypes.TextXlsx, "LowStock.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> Bestsellers(BestsellerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            var reportModel = await _reportModelFactory.PrepareBestsellerListModelAsync(searchModel);

            var bytes = await _exportManager.ExportBestsellersToXlsxAsync(reportModel.Data);
            return File(bytes, MimeTypes.TextXlsx, "Bestsellers.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> NeverSold(NeverSoldReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);

            var reportModel = await _reportModelFactory.PrepareNeverSoldListModelAsync(searchModel);

            var bytes = await _exportManager.ExportNeverSoldProductsToXlsxAsync(reportModel.Data);
            return File(bytes, MimeTypes.TextXlsx, "NeverSold.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> CountrySales(CountryReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            var report = await _reportModelFactory.PrepareCountrySalesListModelAsync(searchModel);

            var bytes = await _exportManager.ExportCountrySalesToXlsxAsync(report.Data);
            return File(bytes, MimeTypes.TextXlsx, "CountrySales.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> RegisteredCustomers(RegisteredCustomersReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            var report = await _reportModelFactory.PrepareRegisteredCustomersReportListModelAsync(searchModel);

            var bytes = await _exportManager.ExportRegisteredCustomersReportToXlsxAsync(report.Data);
            return File(bytes, MimeTypes.TextXlsx, "RegisteredCustomers.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> BestCustomersByOrderTotal(CustomerReportsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            searchModel.BestCustomersByOrderTotal.OrderBy = OrderByEnum.OrderByTotalAmount;
            var report = await _reportModelFactory.PrepareBestCustomersReportListModelAsync(searchModel.BestCustomersByOrderTotal);

            var bytes = await _exportManager.ExportBestCustomersReportToXlsxAsync(report.Data);
            return File(bytes, MimeTypes.TextXlsx, "BestCustomersByOrderTotal.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> BestCustomersByNumberOfOrders(CustomerReportsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
                return AccessDeniedView();

            searchModel.SetGridPageSize(int.MaxValue);
            searchModel.BestCustomersByNumberOfOrders.OrderBy = OrderByEnum.OrderByQuantity;
            var report = await _reportModelFactory.PrepareBestCustomersReportListModelAsync(searchModel.BestCustomersByNumberOfOrders);

            var bytes = await _exportManager.ExportBestCustomersReportToXlsxAsync(report.Data);
            return File(bytes, MimeTypes.TextXlsx, "BestCustomersByNumberOfOrders.xlsx");
        }

        #endregion
    }
}
