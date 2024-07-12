using System.Threading.Tasks;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public interface ITaxJarService
    {
        Task<bool> IsTaxJarPluginConfiguredAsync();
    }
}
