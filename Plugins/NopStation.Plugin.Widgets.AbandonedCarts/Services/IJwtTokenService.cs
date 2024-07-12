using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    public interface IJwtTokenService
    {
        public string GenerateJSONWebToken(Customer userInfo);
    }
}
