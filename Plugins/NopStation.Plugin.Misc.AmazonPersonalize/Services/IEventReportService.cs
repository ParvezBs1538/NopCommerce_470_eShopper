using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public interface IEventReportService
    {
        Task<IList<int>> GetCustomerMinInteractions();
        Task<IPagedList<EventReportLine>> GetEventReportLineAsync(
            int storeId = 0,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
    }
}