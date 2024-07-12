using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages
{
    public interface IAbandonedCartMessageService
    {
        Task SendCustomerEmailAsync(Customer customer, IList<ProductInfoModel> productInfoModels, string jwtToken, int languageId);
    }
}