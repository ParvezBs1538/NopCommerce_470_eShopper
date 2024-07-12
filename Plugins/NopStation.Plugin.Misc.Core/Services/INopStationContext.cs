using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.Core.Services;

public interface INopStationContext
{
    bool MobileDevice { get; }

    Task<string> GetRouteNameAsync();

    T GetRouteValue<T>(string key, T defaultValue = default);
}