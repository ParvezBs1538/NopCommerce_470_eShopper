using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.Core.Services;

public interface ILicenseService
{
    Task<bool> IsLicensedAsync(Assembly assembly);

    KeyVerificationResult VerifyProductKey(string key, bool checkFileName = false, string fileName = "");

    Task<IList<License>> GetLicensesAsync();

    Task DeleteLicenseAsync(License license);

    Task UpdateLicenseAsync(License license);

    Task InsertLicenseAsync(License license);
}
