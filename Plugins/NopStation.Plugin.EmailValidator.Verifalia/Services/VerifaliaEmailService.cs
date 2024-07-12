using Nop.Data;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;
using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;

namespace NopStation.Plugin.EmailValidator.Verifalia.Services
{
    public class VerifaliaEmailService : IVerifaliaEmailService
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<VerifaliaEmail> _verifaliaEmailRepository;

        #endregion

        #region Ctor

        public VerifaliaEmailService(IStaticCacheManager staticCacheManager,
            IRepository<VerifaliaEmail> verifaliaEmailRepository)
        {
            _staticCacheManager = staticCacheManager;
            _verifaliaEmailRepository = verifaliaEmailRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteVerifaliaEmailAsync(VerifaliaEmail verifaliaEmail)
        {
            await _verifaliaEmailRepository.DeleteAsync(verifaliaEmail);
        }

        public async Task DeleteVerifaliaEmailsAsync(IList<VerifaliaEmail> verifaliaEmails)
        {
            await _verifaliaEmailRepository.DeleteAsync(verifaliaEmails);
        }

        public async Task InsertVerifaliaEmailAsync(VerifaliaEmail verifaliaEmail)
        {
            await _verifaliaEmailRepository.InsertAsync(verifaliaEmail);
        }

        public async Task UpdateVerifaliaEmail(VerifaliaEmail verifaliaEmail)
        {
            await _verifaliaEmailRepository.UpdateAsync(verifaliaEmail);
        }

        public async Task<VerifaliaEmail> GetVerifaliaEmailByIdAsync(int verifaliaEmailId)
        {
            if (verifaliaEmailId == 0)
                return null;

            return await _verifaliaEmailRepository.GetByIdAsync(verifaliaEmailId);
        }

        public async Task<IList<VerifaliaEmail>> GetVerifaliaEmailsByIdsAsync(int[] verifaliaEmailIds)
        {
            return await _verifaliaEmailRepository.GetByIdsAsync(verifaliaEmailIds);
        }

        public async Task<VerifaliaEmail> GetVerifaliaEmailByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.VerifaliaEmailByEmailCacheKey, email);

            return await _staticCacheManager.GetAsync(key, async () => 
                await _verifaliaEmailRepository.Table.FirstOrDefaultAsync(ve => ve.Email == email));
        }

        public async Task<IList<VerifaliaEmail>> GetVerifaliaEmailsByEmailsAsync(string[] emails)
        {
            return await _verifaliaEmailRepository.Table.Where(x => emails.Contains(x.Email)).ToListAsync();
        }

        public async Task<IPagedList<VerifaliaEmail>> SearchVerifaliaEmailsAsync(string email = null, bool? disposable = null,
            bool? free = null, bool? roleAccount = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int[] statusIds = null, string classification = null,  int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var verifaliaEmails = await _verifaliaEmailRepository.GetAllPagedAsync(query =>
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

                if(statusIds != null && statusIds.Length > 0)
                    query = query.Where(ve => statusIds.Contains(ve.StatusId));

                if (!string.IsNullOrWhiteSpace(classification))
                    query = query.Where(ve => ve.Classification == classification);

                query = query.OrderByDescending(ve => ve.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);

            return verifaliaEmails;
        }

        #endregion
    }
}
