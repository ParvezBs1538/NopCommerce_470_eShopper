using Nop.Data;
using NopStation.Plugin.EmailValidator.Abstract.Domains;
using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;

namespace NopStation.Plugin.EmailValidator.Abstract.Services
{
    public class AbstractEmailService : IAbstractEmailService
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<AbstractEmail> _abstractEmailRepository;

        #endregion

        #region Ctor

        public AbstractEmailService(IStaticCacheManager staticCacheManager,
            IRepository<AbstractEmail> abstractEmailRepository)
        {
            _staticCacheManager = staticCacheManager;
            _abstractEmailRepository = abstractEmailRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteAbstractEmailAsync(AbstractEmail abstractEmail)
        {
            await _abstractEmailRepository.DeleteAsync(abstractEmail);
        }

        public async Task DeleteAbstractEmailsAsync(IList<AbstractEmail> abstractEmails)
        {
            await _abstractEmailRepository.DeleteAsync(abstractEmails);
        }

        public async Task InsertAbstractEmailAsync(AbstractEmail abstractEmail)
        {
            await _abstractEmailRepository.InsertAsync(abstractEmail);
        }

        public async Task UpdateAbstractEmail(AbstractEmail abstractEmail)
        {
            await _abstractEmailRepository.UpdateAsync(abstractEmail);
        }

        public async Task<AbstractEmail> GetAbstractEmailByIdAsync(int abstractEmailId)
        {
            if (abstractEmailId == 0)
                return null;

            return await _abstractEmailRepository.GetByIdAsync(abstractEmailId);
        }

        public async Task<IList<AbstractEmail>> GetAbstractEmailsByIdsAsync(int[] abstractEmailIds)
        {
            return await _abstractEmailRepository.GetByIdsAsync(abstractEmailIds);
        }

        public async Task<AbstractEmail> GetAbstractEmailByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.AbstractEmailByEmailCacheKey, email);

            return await _staticCacheManager.GetAsync(key, async () => 
                await _abstractEmailRepository.Table.FirstOrDefaultAsync(ve => ve.Email == email));
        }

        public async Task<IList<AbstractEmail>> GetAbstractEmailsByEmailsAsync(string[] emails)
        {
            return await _abstractEmailRepository.Table.Where(x => emails.Contains(x.Email)).ToListAsync();
        }

        public async Task<IPagedList<AbstractEmail>> SearchAbstractEmailsAsync(string email = null, bool? disposable = null,
            bool? free = null, bool? roleAccount = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            string deliverability = null,  int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var abstractEmails = await _abstractEmailRepository.GetAllPagedAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(ve => ve.Email.Contains(email));

                if (disposable.HasValue)
                    query = query.Where(ve => ve.IsDisposable == disposable);

                if (free.HasValue)
                    query = query.Where(ve => ve.IsFree == free);

                if (roleAccount.HasValue)
                    query = query.Where(ve => ve.IsRoleAccount == roleAccount);

                if (createdFromUtc.HasValue)
                    query = query.Where(ve => ve.CreatedOnUtc >= createdFromUtc);

                if (createdToUtc.HasValue)
                    query = query.Where(ve => ve.CreatedOnUtc <= createdToUtc);

                if (!string.IsNullOrWhiteSpace(deliverability))
                    query = query.Where(ve => ve.Deliverability == deliverability);

                query = query.OrderByDescending(ve => ve.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);

            return abstractEmails;
        }

        #endregion
    }
}
