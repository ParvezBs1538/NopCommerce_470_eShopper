using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public interface IOrderDeliverySlotModelFactory
    {
        Task<OrderDeliverySlotSearchModel> PrepareOrderDeliverySlotSearchModelAsync(OrderDeliverySlotSearchModel searchModel);
        Task<OrderDeliverySlotListModel> PrepareOrderDeliverySlotModelListAsync(OrderDeliverySlotSearchModel searchModel);

    }
}
