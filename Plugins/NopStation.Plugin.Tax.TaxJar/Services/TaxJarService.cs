using System.Threading.Tasks;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public class TaxJarService : ITaxJarService
    {
        #region Fields

        private readonly TaxJarSettings _taxJarSettings;

        #endregion

        #region Ctor

        public TaxJarService(TaxJarSettings taxJarSettings)
        {
            _taxJarSettings = taxJarSettings;
        }

        #endregion

        #region Methods

        public virtual async Task<bool> IsTaxJarPluginConfiguredAsync()
        {
            await Task.CompletedTask;
            return !string.IsNullOrEmpty(_taxJarSettings.Token.Trim()) && _taxJarSettings.CountryId > 0;
        }

        #endregion

    }
}
