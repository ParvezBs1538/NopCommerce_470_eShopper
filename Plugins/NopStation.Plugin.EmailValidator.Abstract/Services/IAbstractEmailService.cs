using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.EmailValidator.Abstract.Domains;

namespace NopStation.Plugin.EmailValidator.Abstract.Services
{
    public interface IAbstractEmailService
    {
        Task DeleteAbstractEmailAsync(AbstractEmail abstractEmail);

        Task DeleteAbstractEmailsAsync(IList<AbstractEmail> abstractEmails);

        Task InsertAbstractEmailAsync(AbstractEmail abstractEmail);

        Task UpdateAbstractEmail(AbstractEmail abstractEmail);

        Task<AbstractEmail> GetAbstractEmailByIdAsync(int abstractEmailId);

        Task<IList<AbstractEmail>> GetAbstractEmailsByIdsAsync(int[] abstractEmailIds);

        Task<AbstractEmail> GetAbstractEmailByEmailAsync(string email);

        Task<IList<AbstractEmail>> GetAbstractEmailsByEmailsAsync(string[] emails);

        Task<IPagedList<AbstractEmail>> SearchAbstractEmailsAsync(string email = null, bool? disposable = null,
            bool? free = null, bool? roleAccount = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            string deliverability = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}