using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IVendorShopNotificationService
    {
        MessageTemplate PrepareMessageTemplate(string name, string subject, string body, int delayHour);
        Task SendEmailAsync(IList<int> subscribedIds, MessageTemplate messageTemplate, int storeId);
    }
}
