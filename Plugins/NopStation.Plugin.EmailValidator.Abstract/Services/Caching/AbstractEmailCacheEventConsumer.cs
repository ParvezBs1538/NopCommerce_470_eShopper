using Nop.Services.Caching;
using NopStation.Plugin.EmailValidator.Abstract.Domains;
using System.Threading.Tasks;

namespace NopStation.Plugin.EmailValidator.Abstract.Services.Caching
{
    public partial class AbstractEmailCacheEventConsumer : CacheEventConsumer<AbstractEmail>
    {
        protected override async Task ClearCacheAsync(AbstractEmail entity)
        {
            await RemoveAsync(CacheDefaults.AbstractEmailByEmailCacheKey, entity.Email);
        }
    }
}
