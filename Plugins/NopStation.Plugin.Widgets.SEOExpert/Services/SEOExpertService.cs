using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using NopStation.Plugin.Widgets.SEOExpert.Domains;
using NopStation.Plugin.Widgets.SEOExpert.Extensions;

namespace NopStation.Plugin.Widgets.SEOExpert.Services
{
    public class SEOExpertService : ISEOExpertService
    {
        #region Fields

        private readonly SEOExpertSettings _seoExpertSettings;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public SEOExpertService(SEOExpertSettings seoExpertSettings,
            ILogger logger,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreContext storeContext)
        {
            _seoExpertSettings = seoExpertSettings;
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilities
        private bool ShouldBeGenerate<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var regenerateConditions = _seoExpertSettings.GetRegenerateConditionIds();

            var entityType = entity.GetType();

            var toGenerate = false;

            var hasMetaTitle = entityType.GetProperty("MetaTitle");
            var metaTitle = hasMetaTitle?.GetValue(entity) as string;
            if (string.IsNullOrEmpty(metaTitle))
                toGenerate |= regenerateConditions.Contains((int)RegenerateCondition.RegenerateIfNotExistMetaTitle);

            var hasMetaKeywords = entityType.GetProperty("MetaKeywords");
            var metaKeywords = hasMetaKeywords?.GetValue(entity) as string;
            if (string.IsNullOrEmpty(metaKeywords))
                toGenerate |= regenerateConditions.Contains((int)RegenerateCondition.RegenerateIfNotExistMetaKeywords);

            var hasMetaDescription = entityType.GetProperty("MetaDescription");
            var metaDescription = hasMetaDescription?.GetValue(entity) as string;
            if (string.IsNullOrEmpty(metaDescription))
                toGenerate |= regenerateConditions.Contains((int)RegenerateCondition.RegenerateIfNotExistMetaDescription);

            return toGenerate;

        }

        private async Task<string> PrepareCommandAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var content = "";
            var additionalCommand = SEOExpertDefaults.AdditionalCommandproduct;
            if (entity is Product product)
            {
                content = JsonConvert.SerializeObject(new
                {
                    Id = product.Id,
                    Sku = product.Sku,
                    Name = product.Name + (string.IsNullOrEmpty(_seoExpertSettings.AdditionalInfoWithName) ? string.Empty : " " + _seoExpertSettings.AdditionalInfoWithName),
                    ShortDescription = product.ShortDescription + (string.IsNullOrEmpty(_seoExpertSettings.AdditionalInfoWithShortDescription) ? string.Empty : " " + _seoExpertSettings.AdditionalInfoWithShortDescription),
                    FullDescription = product.FullDescription + (string.IsNullOrEmpty(_seoExpertSettings.AdditionalInfoWithFullDescription) ? string.Empty : " " + _seoExpertSettings.AdditionalInfoWithFullDescription)
                });
            }
            else if (entity is Category category)
            {
                additionalCommand = SEOExpertDefaults.AdditionalCommandcategory;
                content = JsonConvert.SerializeObject(new
                {
                    Id = category.Id,
                    Description = category.Description,
                });
            }
            else if (entity is Manufacturer manufacturer)
            {
                additionalCommand = SEOExpertDefaults.AdditionalCommandmanufacturer;
                content = JsonConvert.SerializeObject(new
                {
                    Id = manufacturer.Id,
                    Description = manufacturer.Description,
                });
            }


            content += string.Format(SEOExpertDefaults.AdditionalStoreNameCommand, (await _storeContext.GetCurrentStoreAsync()).Name);

            content += additionalCommand;

            return content;
        }

        private string FormatOpenAIApiKey(string key)
        {
            key = SEOExpertDefaults.OpenAIAuthKeyPrefix + key.TrimStart(SEOExpertDefaults.OpenAIAuthKeyPrefix.ToArray());

            return key;
        }

        #endregion

        #region Methods
        public async Task<SEOContent> GenerateSEOAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var messages = new JArray()
            {
                new JObject
                {
                    ["role"] = "user",
                    ["content"] = await PrepareCommandAsync(entity)
                }
            };

            var requestBody = new JObject
            {
                ["model"] = string.IsNullOrEmpty(_seoExpertSettings.ModelName) ? "gpt-3.5-turbo" : _seoExpertSettings.ModelName,
                ["messages"] = messages,
                ["temperature"] = _seoExpertSettings.Temperature
            };
            var entityType = entity.GetType();
            var nameProperty = entityType.GetProperty("Name");
            var name = nameProperty?.GetValue(entity) as string;

            try
            {
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _seoExpertSettings.Endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _seoExpertSettings.OpenAIApiKey);

                request.Content = new StringContent(requestBody.ToString());
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Handle the response content as needed
                    var aiResponse = JsonConvert.DeserializeObject<AiResponse>(responseContent);
                    var messageBody = aiResponse.Choices.FirstOrDefault()?.Message ?? new MessageBody();
                    var content = messageBody?.Content ?? string.Empty;

                    var seoContent = JsonConvert.DeserializeObject<SEOContent>(content);

                    return seoContent;
                }
                else
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"Failed to update SEO Contents of {name}", response.ReasonPhrase);
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"Failed to update SEO Contents of - {name}", ex.Message);
                return null;
            }
        }

        public async Task GenerateAndUpdateSEOAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            if (entities is IEnumerable<Product> products)
            {
                (await products.ToListAsync()).ForEach(async product =>
                {
                    if (ShouldBeGenerate(product))
                    {
                        var seoContent = await GenerateSEOAsync(product);
                        if (seoContent != null)
                        {
                            product.MetaTitle = seoContent.MetaTitle;
                            product.MetaDescription = seoContent.MetaDescription;
                            product.MetaKeywords = seoContent.MetaKeywords;

                            await _productService.UpdateProductAsync(product);
                        }
                    }
                });
            }
            else if (entities is IEnumerable<Category> categories)
            {
                (await categories.ToListAsync()).ForEach(async category =>
                {
                    if (ShouldBeGenerate(category))
                    {
                        var seoContent = await GenerateSEOAsync(category);
                        if (seoContent != null)
                        {
                            category.MetaTitle = seoContent.MetaTitle;
                            category.MetaDescription = seoContent.MetaDescription;
                            category.MetaKeywords = seoContent.MetaKeywords;

                            await _categoryService.UpdateCategoryAsync(category);
                        }
                    }
                });
            }
            else if (entities is IEnumerable<Manufacturer> manufactureres)
            {
                (await manufactureres.ToListAsync()).ForEach(async manufacturer =>
                {
                    if (ShouldBeGenerate(manufacturer))
                    {
                        var seoContent = await GenerateSEOAsync(manufacturer);
                        if (seoContent != null)
                        {
                            manufacturer.MetaTitle = seoContent.MetaTitle;
                            manufacturer.MetaDescription = seoContent.MetaDescription;
                            manufacturer.MetaKeywords = seoContent.MetaKeywords;

                            await _manufacturerService.UpdateManufacturerAsync(manufacturer);
                        }
                    }
                });
            }
        }

        #endregion
    }
}