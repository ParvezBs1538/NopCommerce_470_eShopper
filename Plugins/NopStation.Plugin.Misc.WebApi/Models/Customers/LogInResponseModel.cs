using Nop.Web.Models.Customer;

namespace NopStation.Plugin.Misc.WebApi.Models.Customers
{
    public class LogInResponseModel
    {
        public LogInResponseModel()
        {
            CustomerInfo = new CustomerInfoModel();
        }

        public CustomerInfoModel CustomerInfo { get; set; }

        public bool MultiFactorAuthenticationRequired { get; set; }

        public string Token { get; set; }
    }
}
