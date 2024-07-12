namespace NopStation.Plugin.CRM.Zoho.Domain.Zoho
{
    public class ZohoVendor : BaseZohoEntity
    {
        public string Name { get; set; }

        public int AddressId { get; set; }

        public string Email { get; set; }

        public bool Active { get; set; }
    }
}
