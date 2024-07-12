using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.SMS.Messente.Domains;

namespace NopStation.Plugin.SMS.Messente.Services
{
    public class QueuedSmsService : IQueuedSmsService
    {
        #region Fields

        private readonly IRepository<QueuedSms> _queuedSmsRepository;

        #endregion

        #region Ctor

        public QueuedSmsService(
            IRepository<QueuedSms> queuedSmsRepository)
        {
            _queuedSmsRepository = queuedSmsRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteQueuedSmsAsync(QueuedSms queuedSms)
        {
            await _queuedSmsRepository.DeleteAsync(queuedSms);
        }

        public async Task InsertQueuedSmsAsync(QueuedSms queuedSms)
        {
            await _queuedSmsRepository.InsertAsync(queuedSms);
        }

        public async Task UpdateQueuedSmsAsync(QueuedSms queuedSms)
        {
            await _queuedSmsRepository.UpdateAsync(queuedSms);
        }

        public async Task<QueuedSms> GetQueuedSmsByIdAsync(int queuedSmsId)
        {
            if (queuedSmsId == 0)
                return null;

            return await _queuedSmsRepository.GetByIdAsync(queuedSmsId, cache => default);
        }

        public async Task<IPagedList<QueuedSms>> GetAllQueuedSmsAsync(bool loadOnlyItemsToBeSent, int maxSentTries = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _queuedSmsRepository.Table;

            if (loadOnlyItemsToBeSent)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);

            if (maxSentTries > 0)
                query = query.Where(x => x.SentTries <= maxSentTries);

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
