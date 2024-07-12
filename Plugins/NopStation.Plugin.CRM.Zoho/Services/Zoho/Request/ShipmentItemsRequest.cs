﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class ShipmentItemsRequest : BaseZohoParentType
    {
        [JsonProperty("data")]
        public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
    }
}
