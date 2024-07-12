using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class OTPRecordService : IOTPRecordService
    {
        #region Fields

        private readonly IRepository<OTPRecord> _otpRecordRepository;

        #endregion

        #region Ctor

        public OTPRecordService(IRepository<OTPRecord> otpRecordRepository)
        {
            _otpRecordRepository = otpRecordRepository;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        /// <summary>
        /// Delete an otpRecord
        /// </summary>
        /// <param name="otpRecord">OTPRecord</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteOTPRecordAsync(OTPRecord oTPRecord)
        {
            if (oTPRecord == null)
                throw new ArgumentNullException(nameof(oTPRecord));

            await _otpRecordRepository.DeleteAsync(oTPRecord);
        }


        /// <summary>
        /// Gets an otpRecord by otpRecord identifier
        /// </summary>
        /// <param name="otpRecordId">OTP Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the otpRecord
        /// </returns>
        public virtual async Task<OTPRecord> GetOTPRecordByIdAsync(int otpRecordId)
        {
            return await _otpRecordRepository.GetByIdAsync(otpRecordId);
        }

        /// <summary>
        /// Gets otp records by identifier
        /// </summary>
        /// <param name="otpRecordIds">OTP Record identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the otp records
        /// </returns>
        public virtual async Task<IList<OTPRecord>> GetOTPRecordsByIdsAsync(int[] otpRecordIds)
        {
            return await _otpRecordRepository.GetByIdsAsync(otpRecordIds);
        }

        /// <summary>
        /// Inserts an otpRecord
        /// </summary>
        /// <param name="otpRecord">OTPRecord</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertOTPRecordAsync(OTPRecord oTPRecord)
        {
            if (oTPRecord == null)
                throw new ArgumentNullException(nameof(oTPRecord));

            await _otpRecordRepository.InsertAsync(oTPRecord);
        }

        public virtual async Task<IPagedList<OTPRecord>> SearchOTPRecordAsync(
            int? customerId = null,
            int? shipmentId = null,
            int? orderId = null,
            int? verifiedByShipperId = null,
            string authenticationCode = null,
            bool? verified = null,
            bool? deleted = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            return await _otpRecordRepository.GetAllPagedAsync(query =>
            {
                if (deleted.HasValue)
                    query = query.Where(q => q.Deleted == deleted.Value);

                if (!string.IsNullOrEmpty(authenticationCode))
                    query = query.Where(q => q.AuthenticationCode == authenticationCode);

                if (customerId.HasValue)
                    query = query.Where(q => q.CustomerId == customerId.Value);

                if (shipmentId.HasValue)
                    query = query.Where(q => q.ShipmentId == shipmentId.Value);

                if (orderId.HasValue)
                    query = query.Where(q => q.OrderId == orderId.Value);

                if (verifiedByShipperId.HasValue)
                    query = query.Where(q => q.VerifiedByShipperId != null && q.VerifiedByShipperId == verifiedByShipperId.Value);

                if (verified.HasValue)
                    query = query.Where(q => q.Verified == verified.Value);

                if (createdFromUtc.HasValue)
                    query = query.Where(q => createdFromUtc.Value <= q.CreatedOnUtc);

                if (createdToUtc.HasValue)
                    query = query.Where(q => createdToUtc.Value >= q.CreatedOnUtc);

                return query.OrderByDescending(q => q.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Update an otpRecord
        /// </summary>
        /// <param name="otpRecord">OTPRecord</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateOTPRecordAsync(OTPRecord oTPRecord)
        {
            if (oTPRecord == null)
                throw new ArgumentNullException(nameof(oTPRecord));

            await _otpRecordRepository.UpdateAsync(oTPRecord);
        }

        #endregion
    }
}
