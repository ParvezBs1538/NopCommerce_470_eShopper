using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages
{
    public class AbandonedCartMessageTokenProvider : IAbandonedCartMessageTokenProvider
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly MessageTemplatesSettings _templatesSettings;

        #endregion

        #region Ctor

        public AbandonedCartMessageTokenProvider(ICustomerService customerService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            MessageTemplatesSettings templatesSettings)
        {
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _templatesSettings = templatesSettings;
        }

        #endregion

        #region Utilities

        //following order(s) from messageTOkenProvider to make this string
        protected virtual async Task<string> ProductListToHtmlTableAsync(IList<ProductInfoModel> productInfoModels, int languageId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Plugins.Widgets.AbandonedCarts.Fields.ProductName", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Plugins.Widgets.AbandonedCarts.Fields.ProductSku", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Plugins.Widgets.AbandonedCarts.Fields.ProductQuantity", languageId)}</th>");
            sb.AppendLine("</tr>");

            var table = productInfoModels;
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var item = table[i];

                if (item == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
                //product name
                var productUrl = $"%Store.URL%{item.SlugValue}";
                var productNameWithUrl = $"<a target =\"_blank\" href=\"{productUrl}\">{item.ProductName}</a>";
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{productNameWithUrl}</td>");
                sb.AppendLine("</td>");
                //SKU
                if (!string.IsNullOrEmpty(item.ProductSku))
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{item.ProductSku}</td>");
                else
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\"></td>");
                sb.AppendLine("</td>");
                //quantity
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{item.ProductQuantity}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        #endregion

        #region Methods

        //adding customer tokens
        public async Task AddCustomerTokensAsync(IList<Token> tokens, int customerId, string jwtToken)
        {
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            tokens.Add(new Token("Customer.jwtToken", jwtToken));
            await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
        }

        //adding product tokens
        public async Task AddProductTokensAsync(IList<Token> tokens, IList<ProductInfoModel> productInfoModels, int languageId)
        {
            tokens.Add(new Token("Product(s)", await ProductListToHtmlTableAsync(productInfoModels, languageId), true));
        }

        //adding store tokens
        public virtual async Task AddStoreTokensAsync(IList<Token> tokens, Store store, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            tokens.Add(new Token("Store.Name", store.Name));
            tokens.Add(new Token("Store.URL", store.Url, true));
            tokens.Add(new Token("Store.Email", emailAccount.Email));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(store, tokens);
        }

        #endregion
    }
}
