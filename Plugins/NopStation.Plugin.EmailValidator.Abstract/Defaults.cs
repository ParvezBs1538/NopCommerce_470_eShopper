using System;
using System.Threading.Tasks;
using Nop.Core.Caching;
using NopStation.Plugin.EmailValidator.Abstract.Domains;
using NopStation.Plugin.EmailValidator.Abstract.Services;

namespace NopStation.Plugin.EmailValidator.Abstract
{
    public static class Defaults
    {
        public static bool IsValid(this AbstractEmail abstractEmail, bool allowRiskyEmails = false)
        {
            if (abstractEmail == null)
                throw new ArgumentNullException(nameof(abstractEmail));

            return abstractEmail.Deliverability == Deliverable || (allowRiskyEmails && abstractEmail.Deliverability == Risky);
        }

        public static bool IsValid(this ApiResponse apiResponse, bool allowRiskyEmails = false)
        {
            if (apiResponse == null)
                throw new ArgumentNullException(nameof(apiResponse));

            return apiResponse.Deliverability == Deliverable || (allowRiskyEmails && apiResponse.Deliverability == Risky);
        }

        public static async Task<AbstractEmail> SaveAsync(this ApiResponse apiResponse,
            IAbstractEmailService abstractEmailService, AbstractEmail abstractEmail = null)
        {
            if (apiResponse == null)
                throw new ArgumentNullException(nameof(apiResponse));

            abstractEmail = abstractEmail ?? await abstractEmailService.GetAbstractEmailByEmailAsync(apiResponse.Email);

            if (abstractEmail == null)
            {
                abstractEmail = new AbstractEmail
                {
                    Deliverability = apiResponse.Deliverability,
                    Email = apiResponse.Email,
                    IsDisposable = apiResponse.IsDisposableEmail.Value ?? false,
                    IsRoleAccount = apiResponse.IsRoleEmail.Value ?? false,
                    IsFree = apiResponse.IsFreeEmail.Value ?? false,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    ValidationCount = 1
                };

                await abstractEmailService.InsertAbstractEmailAsync(abstractEmail);
                return abstractEmail;
            }
            else
            {
                abstractEmail.Deliverability = apiResponse.Deliverability;
                abstractEmail.IsDisposable = apiResponse.IsDisposableEmail.Value ?? false;
                abstractEmail.IsRoleAccount = apiResponse.IsRoleEmail.Value ?? false;
                abstractEmail.IsFree = apiResponse.IsFreeEmail.Value ?? false;
                abstractEmail.UpdatedOnUtc = DateTime.UtcNow;
                abstractEmail.ValidationCount++;

                await abstractEmailService.UpdateAbstractEmail(abstractEmail);
                return abstractEmail;
            }
        }

        public static string Deliverable = "DELIVERABLE";
        public static string Risky = "RISKY";
        public static string Undeliverable = "UNDELIVERABLE";
        public static string Unknown = "UNKNOWN";
    }

    public class CacheDefaults
    {
        public static CacheKey AbstractEmailByEmailCacheKey => new("Nopstation.abstractvalidator.abstractemail.byemail.{0}");
    }
}