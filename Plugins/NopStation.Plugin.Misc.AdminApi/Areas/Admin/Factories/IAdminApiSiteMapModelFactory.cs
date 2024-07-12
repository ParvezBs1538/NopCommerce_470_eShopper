using System.Threading.Tasks;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public interface IAdminApiSiteMapModelFactory
{
    Task<ApiSiteMapNodeModel> LoadFromAsync(string physicalPath);
}