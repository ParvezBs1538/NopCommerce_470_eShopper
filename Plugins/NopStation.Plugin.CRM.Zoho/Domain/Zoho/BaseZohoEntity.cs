using Nop.Core;

namespace NopStation.Plugin.CRM.Zoho.Domain.Zoho
{
    public abstract class BaseZohoEntity : BaseEntity
    {
        public string ZohoId { get; set; }
    }
}
