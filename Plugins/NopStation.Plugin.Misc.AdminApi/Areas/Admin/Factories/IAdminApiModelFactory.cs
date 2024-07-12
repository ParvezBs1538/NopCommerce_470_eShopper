using System.Threading.Tasks;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public interface IAdminApiModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();
}