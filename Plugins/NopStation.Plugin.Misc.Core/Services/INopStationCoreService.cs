using System.Threading.Tasks;
using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.Core.Services;

public interface INopStationCoreService
{
    Task ManageSiteMapAsync(SiteMapNode rootNode, SiteMapNode childNode, NopStationMenuType menuType = NopStationMenuType.Root);
}