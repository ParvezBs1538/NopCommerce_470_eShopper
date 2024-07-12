using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Services.Cache;

namespace NopStation.Plugin.Misc.Core.Services;

public class LicenseService : ILicenseService
{
    #region Fields

    private bool? _cachedLicensed = null;

    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEncryptionService _encryptionService;
    private readonly NopStationCoreSettings _coreSettings;
    private readonly IStoreContext _storeContext;
    private readonly IRepository<License> _licenseRepository;
    private readonly IStaticCacheManager _cacheManager;
    private readonly SecuritySettings _securitySettings;

    #endregion

    #region Ctor

    public LicenseService(ILogger logger,
        IHttpContextAccessor httpContextAccessor,
        IEncryptionService encryptionService,
        NopStationCoreSettings coreSettings,
        IStoreContext storeContext,
        IRepository<License> licenseRepository,
        IStaticCacheManager cacheManager,
        SecuritySettings securitySettings)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _encryptionService = encryptionService;
        _coreSettings = coreSettings;
        _storeContext = storeContext;
        _licenseRepository = licenseRepository;
        _cacheManager = cacheManager;
        _securitySettings = securitySettings;
    }

    #endregion

    #region Utilities

    private string DecryptTextFromMemory(byte[] data, byte[] key, byte[] iv)
    {
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, TripleDES.Create().CreateDecryptor(key, iv), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.Unicode);
        return sr.ReadToEnd();
    }

    private string DecryptText(string cipherText, string encryptionPrivateKey = "")
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        if (string.IsNullOrEmpty(encryptionPrivateKey))
            encryptionPrivateKey = _securitySettings.EncryptionKey;

        using var provider = TripleDES.Create();
        provider.Key = Encoding.ASCII.GetBytes(encryptionPrivateKey[0..16]);
        provider.IV = Encoding.ASCII.GetBytes(encryptionPrivateKey[8..16]);

        var buffer = Convert.FromBase64String(cipherText);
        return DecryptTextFromMemory(buffer, provider.Key, provider.IV);
    }

    private DecryptedLicense DecryptProductKey(string productKey, string encryptionKey)
    {
        try
        {
            var decryptedText = DecryptText(productKey, encryptionKey);

            var decryptedKey = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedText);
            if (decryptedKey == null)
            {
                return null;
            }

            decryptedKey.TryGetValue("ValidationDateUtc", out var validationDateUtc);
            if (!string.IsNullOrWhiteSpace(validationDateUtc))
            {
                var dateTime = DateTime.ParseExact(validationDateUtc, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (DateTime.UtcNow > dateTime)
                    return null;
            }

            var result = new DecryptedLicense();

            decryptedKey.TryGetValue("NOPVersion", out var nopVersion);
            decryptedKey.TryGetValue("Domain", out var domain);

            result.NopVersion = ExtractVersionComponents(nopVersion);

            result.Domain = domain;
            result.IncludesSubdomains = decryptedKey.TryGetValue("IncludesSubdomains", out var includesSubdomains) &&
                bool.TryParse(includesSubdomains, out var isd) && isd;
            result.SkipCheckDomain = decryptedKey.TryGetValue("SkipCheckDomain", out var skipCheckDomain) &&
                bool.TryParse(skipCheckDomain, out var scd) && scd;
            result.SkipCheckFileName = decryptedKey.TryGetValue("SkipCheckFileName", out var skipFileSystemName) &&
                bool.TryParse(skipFileSystemName, out var scs) && scs;

            if (decryptedKey.TryGetValue("FileNames", out var fileNames))
                result.FileNames = ExtractFileNames(fileNames);

            return result;
        }
        catch (Exception ex)
        {
            _logger.InformationAsync($"Failed to decrypt nop-station license product key: {ex.Message}", ex).Wait();
        }

        return null;
    }

    private IList<string> ExtractFileNames(string fileNames)
    {
        if (string.IsNullOrWhiteSpace(fileNames))
            return new List<string>();

        return fileNames.ToLower().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private int[] ExtractVersionComponents(string version)
    {
        if (version == null)
            return null;

        var parts = version.Split('.');
        int major = 0, minor = 0;

        try
        {
            major = Convert.ToInt32(parts[0]);
        }
        catch { }
        try
        {
            minor = (int)(Convert.ToInt32(parts[1]) / Math.Pow(10, parts[1].Length - 1));
        }
        catch { }

        return new int[] { major, minor };
    }

    #endregion

    #region Methods

    public async Task InsertLicenseAsync(License license)
    {
        await _licenseRepository.InsertAsync(license);
    }

    public async Task UpdateLicenseAsync(License license)
    {
        await _licenseRepository.UpdateAsync(license);
    }

    public async Task DeleteLicenseAsync(License license)
    {
        await _licenseRepository.DeleteAsync(license);
    }

    public async Task<IList<License>> GetLicensesAsync()
    {
        var key = _cacheManager.PrepareKey(CoreCacheDefaults.LicenseKey, _storeContext.GetCurrentStore());
        return await _cacheManager.GetAsync(key, () => _licenseRepository.Table.ToListAsync());
    }

    public KeyVerificationResult VerifyProductKey(string key, bool checkFileName = false, string fileName = "")
    {
        try
        {
            var decryptedKey = DecryptProductKey(key, Constants.LicenseKeySeed);
            if (decryptedKey == null)
                return KeyVerificationResult.InvalidProductKey;

            if (decryptedKey.NopVersion != null)
            {
                var currentVersion = ExtractVersionComponents(NopVersion.CURRENT_VERSION);
                if (currentVersion[0] != decryptedKey.NopVersion[0] || currentVersion[1] != decryptedKey.NopVersion[1])
                    return KeyVerificationResult.InvalidForNOPVersion;
            }

            if (!decryptedKey.SkipCheckDomain)
            {
                var host = _httpContextAccessor.HttpContext.Request.Host.Host;
                if (host.StartsWith("www."))
                    host = host[4..];

                if (decryptedKey.Domain.StartsWith("www."))
                    decryptedKey.Domain = decryptedKey.Domain[4..];

                if (host != decryptedKey.Domain)
                {
                    if (!decryptedKey.IncludesSubdomains || (decryptedKey.IncludesSubdomains && !host.EndsWith($".{decryptedKey.Domain}")))
                        return KeyVerificationResult.InvalidForDomain;
                }
            }

            if (!decryptedKey.SkipCheckFileName && checkFileName && !decryptedKey.FileNames.Contains(fileName))
                return KeyVerificationResult.InvalidProduct;

            return KeyVerificationResult.Valid;
        }
        catch (Exception ex)
        {
            _logger.ErrorAsync(ex.Message, ex).Wait();
            return KeyVerificationResult.InvalidProductKey;
        }
    }

    public async Task<bool> IsLicensedAsync(Assembly assembly)
    {
        if (_cachedLicensed.HasValue)
            return _cachedLicensed.Value;

        foreach (var license in await GetLicensesAsync())
        {
            if (VerifyProductKey(license.Key, true, assembly.GetName().Name.ToLower()) == KeyVerificationResult.Valid)
            {
                _cachedLicensed = true;
                break;
            }
        }

        return _cachedLicensed.HasValue ? _cachedLicensed.Value : false;
    }

    #endregion

    #region Inner class

    public static class Constants
    {
        public static string LicenseKeySeed = "22cerfdZX8Uq9LrLHHhYssVD";
    }

    private class DecryptedLicense
    {
        public DecryptedLicense()
        {
            FileNames = new List<string>();
        }

        public int[] NopVersion { get; set; }

        public string Domain { get; set; }

        public bool IncludesSubdomains { get; set; }

        public bool SkipCheckDomain { get; set; }

        public bool SkipCheckFileName { get; set; }

        public IList<string> FileNames { get; set; }
    }

    #endregion
}
