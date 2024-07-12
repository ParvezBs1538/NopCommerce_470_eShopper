using System.Collections.Generic;
using System.Threading.Tasks;
using Verifalia.Api.EmailValidations.Models;
using Verifalia.Api;
using Verifalia.Api.Security;
using System.Linq;
using Verifalia.Api.EmailValidations;
using Newtonsoft.Json;
using Nop.Services.Logging;
using System;

namespace NopStation.Plugin.EmailValidator.Verifalia.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger _logger;
        private readonly VerifaliaEmailValidatorSettings _verifaliaEmailValidatorSettings;

        public ValidationService(ILogger logger,
            VerifaliaEmailValidatorSettings verifaliaEmailValidatorSettings)
        {
            _logger = logger;
            _verifaliaEmailValidatorSettings = verifaliaEmailValidatorSettings;
        }

        protected QualityLevelName GetQualityLevelName()
        {
            if (!_verifaliaEmailValidatorSettings.ValidateQuality)
                return null;

            var levels = new string[] { "Standard", "High", "Extreme" };
            if (levels.Contains(_verifaliaEmailValidatorSettings.QualityLevel))
                return new QualityLevelName(_verifaliaEmailValidatorSettings.QualityLevel);

            return new QualityLevelName(levels.First());
        }

        public async Task<Validation> ValidationEmailsAsync(IList<string> emails)
        {
            var verifalia = new VerifaliaRestClient(new BearerAuthenticationProvider(_verifaliaEmailValidatorSettings.Username, _verifaliaEmailValidatorSettings.Password));
            var validation = await verifalia.EmailValidations.SubmitAsync(emails, GetQualityLevelName(), waitingStrategy: new WaitingStrategy(true));

            if (_verifaliaEmailValidatorSettings.EnableLog)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new IPEndPointConverter());
                settings.Formatting = Formatting.Indented;

                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Information,
                    $"Total emails: {emails.Count}, Status: {validation.Entries[0].Status}",
                    "Emails: " + string.Join(", ", emails) + Environment.NewLine +
                    JsonConvert.SerializeObject(validation, settings));
            }

            return validation;
        }

        public async Task<Validation> ValidationEmailAsync(string email)
        {
            var verifalia = new VerifaliaRestClient(new BearerAuthenticationProvider(_verifaliaEmailValidatorSettings.Username, _verifaliaEmailValidatorSettings.Password));
            var validation = await verifalia.EmailValidations.SubmitAsync(email, GetQualityLevelName(), waitingStrategy: new WaitingStrategy(true));

            if (_verifaliaEmailValidatorSettings.EnableLog)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new IPEndPointConverter());
                settings.Formatting = Formatting.Indented;

                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Information,
                    $"Email: {email}, Status: {validation.Entries[0].Status}",
                    JsonConvert.SerializeObject(validation, settings));
            }

            return validation;
        }
    }
}
