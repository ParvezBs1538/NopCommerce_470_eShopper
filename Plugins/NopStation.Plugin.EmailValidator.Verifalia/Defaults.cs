using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;
using NopStation.Plugin.EmailValidator.Verifalia.Services;
using Verifalia.Api.EmailValidations.Models;

namespace NopStation.Plugin.EmailValidator.Verifalia
{
    public static class Defaults
    {
        public static bool IsValid(this VerifaliaEmail verifaliaEmail, bool allowRiskyEmails = false)
        {
            if (verifaliaEmail == null)
                throw new ArgumentNullException(nameof(verifaliaEmail));

            return verifaliaEmail.Classification == Deliverable ||  (allowRiskyEmails && verifaliaEmail.Classification == Risky);
        }

        public static bool IsValid(this ValidationEntry validationEntry, bool allowRiskyEmails = false)
        {
            if (validationEntry == null)
                throw new ArgumentNullException(nameof(validationEntry));

            return validationEntry.Classification.Name == Deliverable || (allowRiskyEmails && validationEntry.Classification.Name == Risky);
        }

        public static async Task<VerifaliaEmail> SaveAsync(this ValidationEntry validationEntry, 
            IVerifaliaEmailService verifaliaEmailService, VerifaliaEmail verifaliaEmail = null)
        {
            if (validationEntry == null)
                throw new ArgumentNullException(nameof(validationEntry));

            verifaliaEmail = verifaliaEmail ?? await verifaliaEmailService.GetVerifaliaEmailByEmailAsync(validationEntry.EmailAddress);

            if (verifaliaEmail == null)
            {
                verifaliaEmail = new VerifaliaEmail
                {
                    Classification = validationEntry.Classification.Name,
                    Email = validationEntry.EmailAddress,
                    Status = validationEntry.Status,
                    IsDisposable = validationEntry.IsDisposableEmailAddress ?? false,
                    IsRoleAccount = validationEntry.IsRoleAccount ?? false,
                    IsFree = validationEntry.IsFreeEmailAddress ?? false,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    ValidationCount = 1
                };

                await verifaliaEmailService.InsertVerifaliaEmailAsync(verifaliaEmail);
                return verifaliaEmail;
            }
            else
            {
                verifaliaEmail.Classification = validationEntry.Classification.Name;
                verifaliaEmail.Status = validationEntry.Status;
                verifaliaEmail.IsDisposable = validationEntry.IsDisposableEmailAddress ?? false;
                verifaliaEmail.IsRoleAccount = validationEntry.IsRoleAccount ?? false;
                verifaliaEmail.IsFree = validationEntry.IsFreeEmailAddress ?? false;
                verifaliaEmail.UpdatedOnUtc = DateTime.UtcNow;
                verifaliaEmail.ValidationCount++;

                await verifaliaEmailService.UpdateVerifaliaEmail(verifaliaEmail);
                return verifaliaEmail;
            }
        }

        public static async Task<IList<VerifaliaEmail>> SaveAsync(this IList<ValidationEntry> validationEntries, 
            IVerifaliaEmailService verifaliaEmailService, IList<VerifaliaEmail> verifaliaEmails = null)
        {
            if (validationEntries == null)
                throw new ArgumentNullException(nameof(validationEntries));

            var emails = validationEntries.Select(ve => ve.EmailAddress).ToArray();
            verifaliaEmails = verifaliaEmails ?? await verifaliaEmailService.GetVerifaliaEmailsByEmailsAsync(emails);

            var newEmails = new List<VerifaliaEmail>();
            foreach (var validationEntry in validationEntries)
            {
                var verifaliaEmail = verifaliaEmails.FirstOrDefault(ve => ve.Email.Equals(validationEntry.EmailAddress, StringComparison.InvariantCultureIgnoreCase));
                newEmails.Add(await validationEntry.SaveAsync(verifaliaEmailService, verifaliaEmail));
            }

            return newEmails;
        }

        public static string Deliverable = "Deliverable";
        public static string Risky = "Risky";
        public static string Undeliverable = "Undeliverable";
        public static string Unknown = "Unknown";
    }

    public class CacheDefaults
    {
        public static CacheKey VerifaliaEmailByEmailCacheKey => new("Nopstation.verifaliavalidator.verifaliaemail.byemail.{0}");
    }
}