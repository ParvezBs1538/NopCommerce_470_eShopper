using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.NopChat.Domains;
using NopStation.Plugin.Widgets.NopChat.Models;

namespace NopStation.Plugin.Widgets.NopChat.Services
{
    public partial interface INopChatMessageService
    {
        Task<IPagedList<NopChatMessage>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task<NopChatMessage> GetByIdAsync(int id);
        Task<IPagedList<NopChatMessage>> GetByCustomerIdAsync(int customerId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<NopChatMessage>> GetByVendorIdAsync(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<NopChatMessage>> GetByVendorCustomerIdAsync(int vendorCustomerId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IList<ChatListModel>> GetCustomerChatListListAsync(int customerId);
        Task<IList<ChatListModel>> GetVendorChatListListAsync(int vendorId);
        Task InsertNopChatMessageAsync(NopChatMessage nopChatMessage);
        Task UpdateNopChatMessageAsync(NopChatMessage nopChatMessage);
        Task DeleteNopChatMessageAsync(NopChatMessage nopChatMessage);
        Task<IList<NopChatMessageModel>> GetChatHistoryAsync(int customerId, int vendorId);
        Task<IPagedList<NopChatMessageModel>> GetChatHistoryPagedAsync(int customerId, int vendorId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<string> GetVendorNameByCustomerNameIfExxistAsync(string customerName);
    }
}
