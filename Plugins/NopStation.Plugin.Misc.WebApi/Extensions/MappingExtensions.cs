using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Extensions
{
    public static class MappingExtensions
    {
        public static async Task SavePaymentRequestAttributeAsync(this IGenericAttributeService genericAttributeService, Customer customer, ProcessPaymentRequest request, int storeId)
        {
            var json = request == null ? null : JsonConvert.SerializeObject(request);
            await genericAttributeService.SaveAttributeAsync(customer, NopStationCustomerDefaults.OrderPaymentInfo, json, storeId);
        }

        public static async Task<ProcessPaymentRequest> GetPaymentRequestAttributeAsync(this IGenericAttributeService genericAttributeService, Customer customer, int storeId)
        {
            var json = await genericAttributeService.GetAttributeAsync<string>(customer, NopStationCustomerDefaults.OrderPaymentInfo, storeId);
            if (string.IsNullOrWhiteSpace(json))
                return new ProcessPaymentRequest();

            try
            {
                return JsonConvert.DeserializeObject<ProcessPaymentRequest>(json);
            }
            catch (System.Exception ex)
            {
                await NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex);
                return new ProcessPaymentRequest();
            }
        }
    }
}
