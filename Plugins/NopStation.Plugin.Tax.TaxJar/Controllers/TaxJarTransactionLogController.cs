using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Tax.TaxJar.Models;
using NopStation.Plugin.Tax.TaxJar.Services;

namespace NopStation.Plugin.Tax.TaxJar.Controllers
{
    public class TaxJarTransactionLogController : NopStationAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly TaxjarTransactionLogService _taxJarTransactionLogService;

        #endregion

        #region Ctor

        public TaxJarTransactionLogController(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            TaxjarTransactionLogService taxJarTransactionLogService)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _taxJarTransactionLogService = taxJarTransactionLogService;
        }

        #endregion

        #region Utilites

        // taxtransactionId is the order guid setting at the time of creating taxtransactionLog
        private async Task<int> GetOrderByTaxtransactionIdAsync(string taxtransactionId)
        {
            if (string.IsNullOrEmpty(taxtransactionId))
                return 0;
            try
            {
                var orderGuid = new Guid(taxtransactionId);
                var order = await _orderService.GetOrderByGuidAsync(orderGuid);

                if (order == null)
                    return 0;

                return order.Id;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return 0;
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual async Task<IActionResult> LogList(TaxJarTransactionLogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var createdFromValue = searchModel.CreatedFrom.HasValue
                ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
                : null;
            var createdToValue = searchModel.CreatedTo.HasValue
                ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1)
                : null;

            //get tax transaction log
            var taxtransactionLog = await _taxJarTransactionLogService.GetTaxjarTransactionLogAsync(createdFromUtc: createdFromValue, createdToUtc: createdToValue,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new TaxJarTransactionLogListModel().PrepareToGridAsync(searchModel, taxtransactionLog, () =>
            {
                return taxtransactionLog.SelectAwait(async logItem => new TaxJarTransactionLogModel
                {
                    Id = logItem.Id,
                    TransactionReferanceId = logItem.TransactionReferanceId,
                    TransactionId = logItem.TransactionId,
                    Amount = logItem.Amount,
                    TransactionType = logItem.TransactionType.ToUpper(),
                    CustomerId = logItem.CustomerId,
                    TransactionDate = await _dateTimeHelper.ConvertToUserTimeAsync(logItem.CreatedDateUtc ?? DateTime.UtcNow, DateTimeKind.Utc),
                    OrderId = await GetOrderByTaxtransactionIdAsync(logItem.TransactionId)
                });
            });

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (selectedIds != null)
                await _taxJarTransactionLogService.DeleteTaxjarTransactionLogAsync(selectedIds.ToArray());

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> View(int id)
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //try to get log item with the passed identifier
            var logItem = await _taxJarTransactionLogService.GetTaxjarTransactionLogByIdAsync(id);
            if (logItem == null)
                return RedirectToAction("Configure", "TaxJar");

            var customerEmail = await _localizationService.GetResourceAsync("Admin.Customers.Guest");
            var customer = await _customerService.GetCustomerByIdAsync(logItem.CustomerId);
            if (customer != null)
                customerEmail = string.IsNullOrEmpty(customer.Email) ? customerEmail : customer.Email;


            var model = new TaxJarTransactionLogModel
            {
                Id = logItem.Id,
                TransactionReferanceId = logItem.TransactionReferanceId,
                TransactionId = logItem.TransactionId,
                Amount = logItem.Amount,
                TransactionType = logItem.TransactionType.ToUpper(),
                CustomerId = logItem.CustomerId,
                CustomerEmail = customerEmail,
                TransactionDate = await _dateTimeHelper.ConvertToUserTimeAsync(logItem.CreatedDateUtc ?? DateTime.UtcNow, DateTimeKind.Utc),
                OrderId = await GetOrderByTaxtransactionIdAsync(logItem.TransactionId)
            };

            return View("~/Plugins/NopStation.Plugin.Tax.TaxJar/Views/Log/View.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(TaxJarPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //try to get log item with the passed identifier
            var logItem = await _taxJarTransactionLogService.GetTaxjarTransactionLogByIdAsync(id);
            if (logItem != null)
            {
                await _taxJarTransactionLogService.DeleteTaxjarTransactionLogAsync(logItem);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.TaxJar.Log.Deleted"));
            }

            return RedirectToAction("Configure", "TaxJar");
        }

        #endregion
    }
}
