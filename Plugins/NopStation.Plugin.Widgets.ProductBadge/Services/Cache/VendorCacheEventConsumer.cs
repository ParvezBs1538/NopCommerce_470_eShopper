﻿using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class VendorCacheEventConsumer : CacheEventConsumer<Vendor>
{
    protected override async Task ClearCacheAsync(Vendor entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeVendorsAllPrefix);
    }
}