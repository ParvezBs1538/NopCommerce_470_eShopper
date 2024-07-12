using System.Threading;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.WebApiNotification.Factories
{
    public interface IPushKitHelperFactory
    {
        Task<string> RequestAccessTokenAsync(CancellationToken cancellationToken);
    }
}
