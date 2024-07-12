using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Services
{
    public interface IPickupInStoreMessageService
    {
        Task<IList<int>> SendOrderReadyCustomerNotificationAsync(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null);
        Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId);
    }
}