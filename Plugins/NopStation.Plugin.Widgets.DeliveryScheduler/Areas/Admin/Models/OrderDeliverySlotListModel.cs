using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record OrderDeliverySlotListModel : BasePagedListModel<OrderDeliverySlotModel>
    {
    }
}
