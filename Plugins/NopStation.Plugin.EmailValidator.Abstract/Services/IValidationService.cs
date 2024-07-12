using System.Threading.Tasks;
using NopStation.Plugin.EmailValidator.Abstract.Domains;

namespace NopStation.Plugin.EmailValidator.Abstract.Services
{
    public interface IValidationService
    {
        Task<ApiResponse> ValidationEmailAsync(string email);
    }
}