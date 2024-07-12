using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace NopStation.Plugin.Misc.AdvancedSEO.Helpers
{
    public class SEOTokenProvider : ISEOTokenProvider
    {
        public SEOTokenProvider(
            ILocalizationService localizationService
            )
        {
            _localizationService = localizationService;
        }
        private Dictionary<string, IEnumerable<string>> _allowedTokens;
        private readonly ILocalizationService _localizationService;

        #region Allowed tokens

        /// <summary>
        /// Get all available tokens by token groups
        /// </summary>
        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //Category tokens
                _allowedTokens.Add(SEOTokenGroupNames.CategoryMateTitleTokens, new[]
                {
                    "%Category.Id%",
                    "%Category.Name%",
                    //"%Category.SeName%",
                    "%Category.MetaTitle%",
                    "%Category.MetaKeywords%",
                });

                //Category tokens
                _allowedTokens.Add(SEOTokenGroupNames.CategoryKeywordsTokens, new[]
                {
                    "%Category.Id%",
                    "%Category.Name%",
                    "%Category.MetaTitle%",
                    "%Category.MetaKeywords%",
                });

                //Category tokens
                _allowedTokens.Add(SEOTokenGroupNames.CategoryDescriptionTokens, new[]
                {
                    "%Category.Id%",
                    "%Category.Name%",
                    "%Category.MetaTitle%",
                    "%Category.MetaKeywords%",
                    "%Category.MetaDescription%",
                    "%Category.PriceFrom%",
                    "%Category.PriceTo%",
                });

                //Manufacturer tokens
                _allowedTokens.Add(SEOTokenGroupNames.ManufacturerMateTitleTokens, new[]
                {
                    "%Manufacturer.Id%",
                    "%Manufacturer.Name%",
                    "%Manufacturer.MetaKeywords%",
                    "%Manufacturer.MetaTitle%",
                });

                //Manufacturer tokens
                _allowedTokens.Add(SEOTokenGroupNames.ManufacturerKeywordsTokens, new[]
                {
                    "%Manufacturer.Id%",
                    "%Manufacturer.Name%",
                    "%Manufacturer.MetaKeywords%",
                    "%Manufacturer.MetaTitle%",
                });

                //Manufacturer tokens
                _allowedTokens.Add(SEOTokenGroupNames.ManufacturerDescriptionTokens, new[]
                {
                    "%Manufacturer.Id%",
                    "%Manufacturer.Name%",
                    "%Manufacturer.MetaKeywords%",
                    "%Manufacturer.MetaTitle%",
                    "%Manufacturer.MetaDescription%",
                    "%Manufacturer.PriceFrom%",
                    "%Manufacturer.PriceTo%",
                });

                //Product tokens
                _allowedTokens.Add(SEOTokenGroupNames.ProductMateTitleTokens, new[]
                {
                    "%Product.Id%",
                    "%Product.Name%",
                    "%Product.MetaKeywords%",
                    "%Product.MetaTitle%",
                    "%Product.Sku%",
                    "%Product.ManufacturerPartNumber%",
                    "%Product.Gtin%",
                    //"%Product.ShortDescription%",
                    //"%Product.FullDescription%",
                    //"%Product.MetaDescription%",
                    //"%Product.OrderMinimumQuantity%",
                    //"%Product.OrderMaximumQuantity%",
                    //"%Product.AllowedQuantities%",
                    //"%Product.AvailableForPreOrder%",
                    //"%Product.Price%",
                    //"%Product.Weight%",
                    //"%Product.Length%",
                    //"%Product.Height%",
                    //"%Product.AvailableStartDateTime%",
                    //"%Product.AvailableEndDateTime%",
                    //"%Product.RecurringCyclePeriod%",
                    //"%Product.RentalPricePeriod%",
                });
                
                //Product tokens
                _allowedTokens.Add(SEOTokenGroupNames.ProductKeywordsTokens, new[]
                {
                    "%Product.Id%",
                    "%Product.Name%",
                    "%Product.MetaKeywords%",
                    "%Product.MetaTitle%",
                    "%Product.Sku%",
                    "%Product.ManufacturerPartNumber%",
                    "%Product.Gtin%",
                });
                
                //Product tokens
                _allowedTokens.Add(SEOTokenGroupNames.ProductDescriptionTokens, new[]
                {
                    "%Product.Id%",
                    "%Product.Name%",
                    "%Product.ShortDescription%",
                    "%Product.FullDescription%",
                    "%Product.MetaKeywords%",
                    "%Product.MetaDescription%",
                    "%Product.MetaTitle%",
                    "%Product.Sku%",
                    "%Product.ManufacturerPartNumber%",
                    "%Product.Gtin%",
                    "%Product.OrderMinimumQuantity%",
                    "%Product.OrderMaximumQuantity%",
                    "%Product.AllowedQuantities%",
                    "%Product.AvailableForPreOrder%",
                    "%Product.Price%",
                    "%Product.Weight%",
                    "%Product.Length%",
                    "%Product.Height%",
                    "%Product.AvailableStartDateTime%",
                    "%Product.AvailableEndDateTime%",
                    "%Product.RecurringCyclePeriod%",
                    "%Product.RentalPricePeriod%",
                });

                return _allowedTokens;
            }
        }

        #endregion

        #region Methods

        ///// <summary>
        ///// Add store tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="store">Store</param>
        ///// <param name="emailAccount">Email account</param>
        ///// <returns>A task that represents the asynchronous operation</returns>
        //public virtual async Task AddCategoryMetaTitleTokensAsync(IList<Token> tokens, Category category, int languageId)
        //{
        //    if (category == null)
        //        throw new ArgumentNullException(nameof(category));

        //    tokens.Add(new Token("Category.Id", category.Id));
        //    tokens.Add(new Token("Category.Name", category.Name));
        //    tokens.Add(new Token("Category.SeName", category.Name));
        //    tokens.Add(new Token("Category.Description", category.Description));
        //    tokens.Add(new Token("Category.MetaTitle", category.MetaTitle));
        //    tokens.Add(new Token("Category.MetaKeywords", category.MetaKeywords));
        //    tokens.Add(new Token("Category.MetaDescription", category.MetaDescription));
        //    tokens.Add(new Token("Category.PriceFrom", category.PriceFrom));
        //    tokens.Add(new Token("Category.PriceTo", category.PriceTo));

        //    await Task.CompletedTask;
        //}

        #region Category

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddCategoryMetaTitleTokensAsync(IList<Token> tokens, Category category, int languageId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            tokens.Add(new Token("Category.Id", category.Id));
            tokens.Add(new Token("Category.Name", await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId) ));
            tokens.Add(new Token("Category.MetaTitle", await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle, languageId)));
            tokens.Add(new Token("Category.MetaKeywords", await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords, languageId)));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddCategoryKeywordsTokensAsync(IList<Token> tokens, Category category, int languageId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            tokens.Add(new Token("Category.Id", category.Id));
            tokens.Add(new Token("Category.Name", await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId)));
            tokens.Add(new Token("Category.MetaTitle", await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle, languageId)));
            tokens.Add(new Token("Category.MetaKeywords", await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords, languageId)));

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddCategoryDescriptionTokensAsync(IList<Token> tokens, Category category, int languageId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            tokens.Add(new Token("Category.Id", category.Id));
            tokens.Add(new Token("Category.Name", await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId)));
            tokens.Add(new Token("Category.Description", await _localizationService.GetLocalizedAsync(category, x => x.Description, languageId)));
            tokens.Add(new Token("Category.MetaTitle", await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle, languageId)));
            tokens.Add(new Token("Category.MetaKeywords", await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords, languageId)));
            tokens.Add(new Token("Category.MetaDescription", await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription, languageId)));
            tokens.Add(new Token("Category.PriceFrom", await _localizationService.GetLocalizedAsync(category, x => x.PriceFrom, languageId)));
            tokens.Add(new Token("Category.PriceTo", await _localizationService.GetLocalizedAsync(category, x => x.PriceTo, languageId)));

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddCategoryMetaDescriptionTokensAsync(IList<Token> tokens, Category category, int languageId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            tokens.Add(new Token("Category.Id", category.Id));
            tokens.Add(new Token("Category.Name", await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId)));
            tokens.Add(new Token("Category.SeName", await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId)));
            tokens.Add(new Token("Category.Description", await _localizationService.GetLocalizedAsync(category, x => x.Description, languageId)));
            tokens.Add(new Token("Category.MetaTitle", await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle, languageId)));
            tokens.Add(new Token("Category.MetaKeywords", await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords, languageId)));
            tokens.Add(new Token("Category.MetaDescription", await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription, languageId)));
            tokens.Add(new Token("Category.PriceFrom", await _localizationService.GetLocalizedAsync(category, x => x.PriceFrom, languageId)));
            tokens.Add(new Token("Category.PriceTo", await _localizationService.GetLocalizedAsync(category, x => x.PriceTo, languageId)));

            await Task.CompletedTask;
        }

        #endregion

        #region Manufacturer

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddManufacturerMetaTitleTokensAsync(IList<Token> tokens, Manufacturer manufacturer, int languageId)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            tokens.Add(new Token("Manufacturer.Id", manufacturer.Id));
            tokens.Add(new Token("Manufacturer.Name", manufacturer.Name));
            //tokens.Add(new Token("Manufacturer.SeName", manufacturer.Name));
            tokens.Add(new Token("Manufacturer.MetaTitle", manufacturer.MetaTitle));
            tokens.Add(new Token("Manufacturer.MetaKeywords", manufacturer.MetaKeywords));

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddManufacturerKeywordsTokensAsync(IList<Token> tokens, Manufacturer manufacturer, int languageId)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            tokens.Add(new Token("Manufacturer.Id", manufacturer.Id));
            tokens.Add(new Token("Manufacturer.Name", manufacturer.Name));
            tokens.Add(new Token("Manufacturer.MetaTitle", manufacturer.MetaTitle));
            tokens.Add(new Token("Manufacturer.MetaKeywords", manufacturer.MetaKeywords));

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddManufacturerMetaDescriptionTokensAsync(IList<Token> tokens, Manufacturer manufacturer, int languageId)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            tokens.Add(new Token("Manufacturer.Id", manufacturer.Id));
            tokens.Add(new Token("Manufacturer.Name", manufacturer.Name));
            tokens.Add(new Token("Manufacturer.SeName", manufacturer.Name));
            tokens.Add(new Token("Manufacturer.Description", manufacturer.Description));
            tokens.Add(new Token("Manufacturer.MetaTitle", manufacturer.MetaTitle));
            tokens.Add(new Token("Manufacturer.MetaKeywords", manufacturer.MetaKeywords));
            tokens.Add(new Token("Manufacturer.MetaDescription", manufacturer.MetaDescription));
            tokens.Add(new Token("Manufacturer.PriceFrom", manufacturer.PriceFrom));
            tokens.Add(new Token("Manufacturer.PriceTo", manufacturer.PriceTo));

            await Task.CompletedTask;
        }

        #endregion

        #region Product 

        /// <summary>
        /// Add product tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="product">Product</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddProductMetaTitleTokensAsync(IList<Token> tokens, Product product, int languageId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            tokens.Add(new Token("Product.Id", product.Id));
            tokens.Add(new Token("Product.Name", product.Name));
            //tokens.Add(new Token("Product.ShortDescription", product.ShortDescription));
            //tokens.Add(new Token("Product.FullDescription", product.FullDescription));
            tokens.Add(new Token("Product.MetaKeywords", product.MetaKeywords));
            //tokens.Add(new Token("Product.MetaDescription", product.MetaDescription));
            tokens.Add(new Token("Product.MetaTitle", product.MetaTitle));
            tokens.Add(new Token("Product.Sku", product.Sku));
            tokens.Add(new Token("Product.ManufacturerPartNumber", product.ManufacturerPartNumber));
            tokens.Add(new Token("Product.Gtin", product.Gtin));

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add product tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="product">Product</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddProductKeywordsTokensAsync(IList<Token> tokens, Product product, int languageId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            tokens.Add(new Token("Product.Id", product.Id));
            tokens.Add(new Token("Product.Name", product.Name));
            //tokens.Add(new Token("Product.ShortDescription", product.ShortDescription));
            //tokens.Add(new Token("Product.FullDescription", product.FullDescription));
            tokens.Add(new Token("Product.MetaKeywords", product.MetaKeywords));
            //tokens.Add(new Token("Product.MetaDescription", product.MetaDescription));
            tokens.Add(new Token("Product.MetaTitle", product.MetaTitle));
            tokens.Add(new Token("Product.Sku", product.Sku));
            tokens.Add(new Token("Product.ManufacturerPartNumber", product.ManufacturerPartNumber));
            tokens.Add(new Token("Product.Gtin", product.Gtin));

            await Task.CompletedTask;
        }
        

        /// <summary>
        /// Add product tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="product">Product</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddProductMetaDescriptionTokensAsync(IList<Token> tokens, Product product, int languageId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            tokens.Add(new Token("Product.Id", product.Id));
            tokens.Add(new Token("Product.Name", product.Name));
            tokens.Add(new Token("Product.ShortDescription", product.ShortDescription));
            tokens.Add(new Token("Product.FullDescription", product.FullDescription));
            tokens.Add(new Token("Product.MetaKeywords", product.MetaKeywords));
            tokens.Add(new Token("Product.MetaDescription", product.MetaDescription));
            tokens.Add(new Token("Product.MetaTitle", product.MetaTitle));
            tokens.Add(new Token("Product.Sku", product.Sku));
            tokens.Add(new Token("Product.ManufacturerPartNumber", product.ManufacturerPartNumber));
            tokens.Add(new Token("Product.Gtin", product.Gtin));
            tokens.Add(new Token("Product.OrderMinimumQuantity", product.OrderMinimumQuantity));
            tokens.Add(new Token("Product.OrderMaximumQuantity", product.OrderMaximumQuantity));
            tokens.Add(new Token("Product.AllowedQuantities", product.AllowedQuantities));
            tokens.Add(new Token("Product.AvailableForPreOrder", product.AvailableForPreOrder));
            tokens.Add(new Token("Product.Price", product.Price));
            tokens.Add(new Token("Product.Weight", product.Weight));
            tokens.Add(new Token("Product.Length", product.Length));
            tokens.Add(new Token("Product.Height", product.Height));
            tokens.Add(new Token("Product.AvailableStartDateTime", product.AvailableStartDateTimeUtc));
            tokens.Add(new Token("Product.AvailableEndDateTime", product.AvailableEndDateTimeUtc));
            tokens.Add(new Token("Product.RecurringCyclePeriod", product.RecurringCyclePeriod));
            tokens.Add(new Token("Product.RentalPricePeriod", product.RentalPricePeriod));

            await Task.CompletedTask;
        }

        #endregion


        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public virtual IEnumerable<string> GetTokenGroups(string templateProperty)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            return templateProperty switch
            {
                SEOTemplatePropertySystemName.CategoryDescriptionTokens => new[] { SEOTokenGroupNames.CategoryDescriptionTokens },
                SEOTemplatePropertySystemName.CategoryKeywordsTokens => new[] { SEOTokenGroupNames.CategoryKeywordsTokens },
                SEOTemplatePropertySystemName.CategoryMateTitleTokens => new[] { SEOTokenGroupNames.CategoryMateTitleTokens },
                //SEOTemplatePropertySystemName.CategoryTitleTokens => new[] { SEOTokenGroupNames.CategoryTitleTokens },

                SEOTemplatePropertySystemName.ManufacturerDescriptionTokens => new[] { SEOTokenGroupNames.ManufacturerDescriptionTokens },
                SEOTemplatePropertySystemName.ManufacturerKeywordsTokens => new[] { SEOTokenGroupNames.ManufacturerKeywordsTokens },
                SEOTemplatePropertySystemName.ManufacturerMateTitleTokens => new[] { SEOTokenGroupNames.ManufacturerMateTitleTokens },
                //SEOTemplatePropertySystemName.ManufacturerTitleTokens => new[] { SEOTokenGroupNames.ManufacturerTitleTokens },

                SEOTemplatePropertySystemName.ProductDescriptionTokens => new[] { SEOTokenGroupNames.ProductDescriptionTokens },
                SEOTemplatePropertySystemName.ProductKeywordsTokens => new[] { SEOTokenGroupNames.ProductKeywordsTokens },
                SEOTemplatePropertySystemName.ProductMateTitleTokens => new[] { SEOTokenGroupNames.ProductMateTitleTokens },
                //SEOTemplatePropertySystemName.ProductTitleTokens => new[] { SEOTokenGroupNames.ProductTitleTokens },

                _ => Array.Empty<string>(),
            };
        }


        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of allowed message tokens
        /// </returns>
        public virtual async Task<IList<string>> GetListOfAllowedTokensAsync(string tokenGroup = null)
        {
            //var additionalTokens = new AdditionalTokensAddedEvent();
            //await _eventPublisher.PublishAsync(additionalTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroup == null || tokenGroup == x.Key)
                .SelectMany(x => x.Value).ToList();

            //allowedTokens.AddRange(additionalTokens.AdditionalTokens);

            await Task.CompletedTask;

            return allowedTokens.Distinct().ToList();
        }

        #endregion
    }
}
