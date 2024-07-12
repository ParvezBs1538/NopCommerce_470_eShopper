namespace NopStation.Plugin.Payments.CopyAndPay.Models
{
    public class CopyAndPayModel
    {
        public string FormId { get; set; }
        public string ValidateUrl { get; set; }
        public string RequestUrl { get; set; }
        public string PaymentUrl { get; set; }
        public string DataBrands { get; set; }
        public string SelectedBrand { get; set; }
    }
}
