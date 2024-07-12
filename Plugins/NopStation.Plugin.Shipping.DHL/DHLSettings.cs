using Nop.Core.Configuration;

namespace NopStation.Plugin.Shipping.DHL
{
    public class DHLSettings : ISettings
    {
        public string Url { get; set; }
        public string SiteId { get; set; }
        public string Password { get; set; }
        public string AccountNumber { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public int DefaultHeight { get; set; }
        public int DefaultDepth { get; set; }
        public int DefaultWidth { get; set; }
        public int DefaultWeight { get; set; }
        public string PickupReadyByTime { get; set; }
        public string PickupCloseTime { get; set; }
        public string PickupPackageLocation { get; set; }
        public bool Tracing { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string TrackingUrl { get; set; }
        public int SelectedCurrencyId { get; set; }
        public decimal SelectedCurrencyRate { get; set; }
    }
}
