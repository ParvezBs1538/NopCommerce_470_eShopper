using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.NopChat.Domains;
using NopStation.Plugin.Widgets.NopChat.Models;

namespace NopStation.Plugin.Widgets.NopChat.Factories
{
    public partial interface INopChatMessageModelFactory
    {
        Task<NopChatMessageModel> PrepareNopChatMessageModelAsync(NopChatMessage data);
        Task<IList<NopChatMessageModel>> PrepareNopChatMessageListAsync(int customerId, int vendorId, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
