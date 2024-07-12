using System.Threading.Tasks;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Services.Zoho.Response;

namespace NopStation.Plugin.CRM.Zoho.Services
{
    public interface IZohoService
    {
        string GetAuthorizationUrl(string clientId);

        Task<OAuthResponse> AuthorizeAccessTokenAsync(string code);

        Task<OAuthResponse> RefreshAccessTokenAsync();

        Task<ModulesResponse> GetModulesAsync();

        Task<ModuleFieldsResponse> GetModuleFieldsAsync(string moduleName);

        Task SyncStoresAsync(SyncType syncType);

        Task SyncVendorsAsync(SyncType syncType);

        Task SyncCustomersAsync(SyncType syncType);

        Task SyncProductsAsync(SyncType syncType);

        Task SyncOrdersAsync(SyncType syncType); 
        
        Task SyncShipmentsAsync(SyncType syncType);

        Task SyncShipmentItemsAsync(SyncType syncType);
    }
}