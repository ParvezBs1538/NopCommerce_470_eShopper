using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;

namespace NopStation.Plugin.EmailValidator.Verifalia.Services
{
    public interface IVerifaliaEmailService
    {
        Task DeleteVerifaliaEmailAsync(VerifaliaEmail verifaliaEmail);

        Task DeleteVerifaliaEmailsAsync(IList<VerifaliaEmail> verifaliaEmails);

        Task InsertVerifaliaEmailAsync(VerifaliaEmail verifaliaEmail);

        Task UpdateVerifaliaEmail(VerifaliaEmail verifaliaEmail);

        Task<VerifaliaEmail> GetVerifaliaEmailByIdAsync(int verifaliaEmailId);

        Task<IList<VerifaliaEmail>> GetVerifaliaEmailsByIdsAsync(int[] verifaliaEmailIds);

        Task<VerifaliaEmail> GetVerifaliaEmailByEmailAsync(string email);

        Task<IList<VerifaliaEmail>> GetVerifaliaEmailsByEmailsAsync(string[] emails);

        Task<IPagedList<VerifaliaEmail>> SearchVerifaliaEmailsAsync(string email = null, bool? disposable = null,
            bool? free = null, bool? roleAccount = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int[] statusIds = null, string classification = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}