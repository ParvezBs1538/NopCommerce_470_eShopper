using Nop.Web.Framework.Models;
using Nop.Web.Models.Customer;

namespace NopStation.Plugin.Widgets.DMS.Models.Shippers
{
    public record LogInResponseModel : BaseNopModel
    {
        public LogInResponseModel()
        {
            CustomerInfo = new CustomerInfoModel();
        }

        public CustomerInfoModel CustomerInfo { get; set; }

        public int ShipperId { get; set; }

        public string Token { get; set; }
    }
}
