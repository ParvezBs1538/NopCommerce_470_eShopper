using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace NopStation.Plugin.Misc.Core.Helpers;

public class NopInstance
{
    public static T Load<T>(IServiceScope scope = null) where T : class
    {
        return EngineContext.Current.Resolve<T>(scope);
    }
}
