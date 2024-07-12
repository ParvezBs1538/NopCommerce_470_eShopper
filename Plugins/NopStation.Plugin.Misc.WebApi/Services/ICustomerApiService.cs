using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface ICustomerApiService
    {
        Task<Customer> InsertDeviceGuestCustomerAsync(string deviceId);
    }
}