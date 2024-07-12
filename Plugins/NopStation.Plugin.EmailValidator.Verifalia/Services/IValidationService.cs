using System.Collections.Generic;
using System.Threading.Tasks;
using Verifalia.Api.EmailValidations.Models;

namespace NopStation.Plugin.EmailValidator.Verifalia.Services
{
    public interface IValidationService
    {
        Task<Validation> ValidationEmailsAsync(IList<string> emails);

        Task<Validation> ValidationEmailAsync(string email);
    }
}