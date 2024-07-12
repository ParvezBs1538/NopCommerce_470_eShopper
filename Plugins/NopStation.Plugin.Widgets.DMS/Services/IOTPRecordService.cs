using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IOTPRecordService
    {
        Task DeleteOTPRecordAsync(OTPRecord oTPRecord);

        Task<OTPRecord> GetOTPRecordByIdAsync(int otpRecordId);

        Task<IList<OTPRecord>> GetOTPRecordsByIdsAsync(int[] otpRecordIds);

        Task InsertOTPRecordAsync(OTPRecord oTPRecord);

        Task<IPagedList<OTPRecord>> SearchOTPRecordAsync(
            int? customerId = null, int? shipmentId = null,
            int? orderId = null,
            int? verifiedByShipperId = null,
            string authenticationCode = null,
            bool? verified = null,
            bool? deleted = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
             int pageIndex = 0, int pageSize = int.MaxValue);

        Task UpdateOTPRecordAsync(OTPRecord oTPRecord);
    }
}
