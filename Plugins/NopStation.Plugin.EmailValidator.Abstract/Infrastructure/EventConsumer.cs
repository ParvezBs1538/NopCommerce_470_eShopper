using Nop.Core.Domain.Common;
using Nop.Core.Events;
using NopStation.Plugin.EmailValidator.Abstract.Services;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using System.Threading.Tasks;
using Nop.Web.Models.Common;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Caching;

namespace NopStation.Plugin.EmailValidator.Abstract.Infrastructure
{
    public class EventConsumer :
        IConsumer<EntityInsertedEvent<Address>>,
        IConsumer<EntityUpdatedEvent<Address>>,
        IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly IAbstractEmailService _abstractEmailService;
        private readonly IValidationService _validationService;
        private readonly AbstractEmailValidatorSettings _abstractEmailValidatorSettings;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public EventConsumer(IAbstractEmailService abstractEmailService,
            IValidationService validationService,
            AbstractEmailValidatorSettings abstractEmailValidatorSettings,
            IStaticCacheManager staticCacheManager)
        {
            _abstractEmailService = abstractEmailService;
            _validationService = validationService;
            _abstractEmailValidatorSettings = abstractEmailValidatorSettings;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        protected async Task<bool> ValidateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var attribute = new EmailAddressAttribute();
            if (!attribute.IsValid(email))
                return false;

            var domain = email.Split("@")[1];
            if (_abstractEmailValidatorSettings.BlockedDomains.Contains(domain))
                return false;

            var abstractEmail = await _abstractEmailService.GetAbstractEmailByEmailAsync(email);
            if (abstractEmail != null)
            {
                if (abstractEmail.IsValid(_abstractEmailValidatorSettings.AllowRiskyEmails))
                    return true;

                if (abstractEmail.UpdatedOnUtc.AddHours(_abstractEmailValidatorSettings.RevalidateInvalidEmailsAfterHours) > DateTime.UtcNow)
                    return false;
            }

            var apiResponse = await _validationService.ValidationEmailAsync(email);

            await apiResponse.SaveAsync(_abstractEmailService, abstractEmail);
            await _staticCacheManager.RemoveAsync(CacheDefaults.AbstractEmailByEmailCacheKey, email);

            return apiResponse.IsValid(_abstractEmailValidatorSettings.AllowRiskyEmails);
        }

        protected async Task<List<AddressModel>> GetInvalidAddresses(IList<AddressModel> addresses)
        {
            var invalidAddresses = new List<AddressModel>();
            foreach (var addressModel in addresses)
                if (!await ValidateEmailAsync(addressModel.Email))
                    invalidAddresses.Add(addressModel);

            return invalidAddresses;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInsertedEvent<Address> eventMessage)
        {
            await ValidateEmailAsync(eventMessage.Entity.Email);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Address> eventMessage)
        {
            await ValidateEmailAsync(eventMessage.Entity.Email);
        }

        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            if (eventMessage?.Model.GetType() == typeof(CheckoutBillingAddressModel) || eventMessage?.Model.GetType() == typeof(OnePageCheckoutModel))
            {
                CheckoutBillingAddressModel billingAddressModel;
                if (eventMessage.Model.GetType() == typeof(CheckoutBillingAddressModel))
                    billingAddressModel = eventMessage.Model as CheckoutBillingAddressModel;
                else
                    billingAddressModel = (eventMessage.Model as OnePageCheckoutModel).BillingAddress;

                var invalidAddresses = await GetInvalidAddresses(billingAddressModel.ExistingAddresses);

                foreach (var address in invalidAddresses)
                {
                    var index = billingAddressModel.ExistingAddresses.IndexOf(address);
                    if (index >= 0)
                        billingAddressModel.ExistingAddresses.RemoveAt(index);
                    billingAddressModel.InvalidExistingAddresses.Add(address);
                }
            }

            if (eventMessage?.Model is CheckoutShippingAddressModel shippingAddressModel)
            {
                var invalidAddresses = await GetInvalidAddresses(shippingAddressModel.ExistingAddresses);

                foreach (var address in invalidAddresses)
                {
                    var index = shippingAddressModel.ExistingAddresses.IndexOf(address);
                    if (index >= 0)
                        shippingAddressModel.ExistingAddresses.RemoveAt(index);
                    shippingAddressModel.InvalidExistingAddresses.Add(address);
                }
            }
        }

        #endregion
    }
}
