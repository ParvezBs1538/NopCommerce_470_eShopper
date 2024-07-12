using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Services.Logging;
using System;
using NopStation.Plugin.EmailValidator.Abstract.Domains;
using RestSharp;

namespace NopStation.Plugin.EmailValidator.Abstract.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger _logger;
        private readonly AbstractEmailValidatorSettings _abstractEmailValidatorSettings;

        public ValidationService(ILogger logger,
            AbstractEmailValidatorSettings abstractEmailValidatorSettings)
        {
            _logger = logger;
            _abstractEmailValidatorSettings = abstractEmailValidatorSettings;
        }

        public async Task<ApiResponse> ValidationEmailAsync(string email)
        {
            var uri = new Uri($"https://emailvalidation.abstractapi.com/v1/?api_key={_abstractEmailValidatorSettings.ApiKey}&email={email}");

            var request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.None
            };
            var client = new RestClient(uri);
            var response = await client.ExecuteAsync(request);

            var data = JsonConvert.DeserializeObject<ApiResponse>(response.Content);

            if (_abstractEmailValidatorSettings.EnableLog)
            {
                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Information,
                    $"Email: {email}, Deliverability: {data.Deliverability}",
                    JsonConvert.SerializeObject(data));
            }

            return data;
        }
    }
}
