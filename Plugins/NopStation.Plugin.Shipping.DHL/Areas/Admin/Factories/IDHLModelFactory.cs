using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Models;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Factories
{
    public interface IDHLModelFactory
    {
        Task<DHLServiceListModel> PrepareDHLServiceListModelAsync(DHLServiceSearchModel searchModel);
        Task<DHLOrderListModel> PrepareDHLOrderListModelAsync(DHLOrderSearchModel searchModel);
    }
}
