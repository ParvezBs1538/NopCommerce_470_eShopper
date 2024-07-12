using System.Threading.Tasks;
using Nop.Services.Common;

namespace NopStation.Plugin.Misc.Core.Services;

public interface ISmsPlugin : IMiscPlugin
{
    Task SendSmsAsync(string phoneNumber, string messageBoby);
}
