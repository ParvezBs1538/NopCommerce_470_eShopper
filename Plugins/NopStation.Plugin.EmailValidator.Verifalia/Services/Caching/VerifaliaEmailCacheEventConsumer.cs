using Nop.Services.Caching;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;
using System.Threading.Tasks;

namespace NopStation.Plugin.EmailValidator.Verifalia.Services.Caching
{
    public partial class VerifaliaEmailCacheEventConsumer : CacheEventConsumer<VerifaliaEmail>
    {
        protected override async Task ClearCacheAsync(VerifaliaEmail entity)
        {
            await RemoveAsync(CacheDefaults.VerifaliaEmailByEmailCacheKey, entity.Email);
        }
    }
}
