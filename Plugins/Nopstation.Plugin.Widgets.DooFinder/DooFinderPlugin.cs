using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Mvc.Routing;
using Nopstation.Plugin.Widgets.DooFinder.Components;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace Nopstation.Plugin.Widgets.DooFinder
{
    public class DooFinderPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly DooFinderSettings _doofinderSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWorkContext _workContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IProductModelFactory _productModelFactory;

        #endregion

        #region Ctor

        public DooFinderPlugin(CurrencySettings currencySettings,
            DooFinderSettings doofinderSettings,
            IActionContextAccessor actionContextAccessor,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            INopFileProvider nopFileProvider,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            ISettingService settingService,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWebHostEnvironment webHostEnvironment,
            IWorkContext workContext,
            IScheduleTaskService scheduleTaskService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IProductModelFactory productModelFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _doofinderSettings = doofinderSettings;
            _languageService = languageService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _scheduleTaskService = scheduleTaskService;
            _nopFileProvider = nopFileProvider;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _productService = productService;
            _settingService = settingService;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _webHostEnvironment = webHostEnvironment;
            _workContext = workContext;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _productModelFactory = productModelFactory;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Removes invalid characters
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="isHtmlEncoded">A value indicating whether input string is HTML encoded</param>
        /// <returns>Valid string</returns>
        protected virtual string StripInvalidChars(string input, bool isHtmlEncoded)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            //Microsoft uses a proprietary encoding (called CP-1252) for the bullet symbol and some other special characters, 
            //whereas most websites and data feeds use UTF-8. When you copy-paste from a Microsoft product into a website, 
            //some characters may appear as junk. Our system generates data feeds in the UTF-8 character encoding, 
            //which many shopping engines now require.

            //http://www.atensoftware.com/p90.php?q=182

            if (isHtmlEncoded)
                input = WebUtility.HtmlDecode(input);

            input = input.Replace("¼", "");
            input = input.Replace("½", "");
            input = input.Replace("¾", "");
            //input = input.Replace("•", "");
            //input = input.Replace("”", "");
            //input = input.Replace("“", "");
            //input = input.Replace("’", "");
            //input = input.Replace("‘", "");
            //input = input.Replace("™", "");
            //input = input.Replace("®", "");
            //input = input.Replace("°", "");

            if (isHtmlEncoded)
                input = WebUtility.HtmlEncode(input);

            return input;
        }

        /// <summary>
        /// Get used currency
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Currency
        /// </returns>
        protected virtual async Task<Currency> GetUsedCurrencyAsync()
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(_doofinderSettings.CurrencyId);
            if (currency == null || !currency.Published)
                currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            return currency;
        }

        /// <summary>
        /// Get UrlHelper
        /// </summary>
        /// <returns>UrlHelper</returns>
        protected virtual IUrlHelper GetUrlHelper()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }

        /// <summary>
        /// Get HTTP protocol
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Protocol name
        /// </returns>
        protected virtual async Task<string> GetHttpProtocolAsync()
        {
            return (await _storeContext.GetCurrentStoreAsync()).SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FeedDooFinder/Configure";
        }

        /// <summary>
        /// Generate a feed
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="store">Store</param>
        /// <returns>Generated feed</returns>
        public async Task GenerateFeedAsync(Stream stream, Store store)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            var doofinderSettings = await _settingService.LoadSettingAsync<DooFinderSettings>(store.Id);

            //language
            var languageId = 0;
            var languages = await _languageService.GetAllLanguagesAsync(storeId: store.Id);
            //if we have only one language, let's use it
            if (languages.Count == 1)
            {
                //let's use the first one
                var language = languages.FirstOrDefault();
                languageId = language != null ? language.Id : 0;
            }
            //otherwise, use the current one
            if (languageId == 0)
                languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            using var writer = XmlWriter.Create(stream, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteStartElement("channel");
            writer.WriteElementString("title", "DooFinder Base feed");
            writer.WriteElementString("description", "Information about products");

            var products1 = await _productService.SearchProductsAsync(storeId: store.Id, visibleIndividuallyOnly: true);
            foreach (var product1 in products1)
            {
                var flagGroupParent = false;
                var productsToProcess = new List<Product>();
                switch (product1.ProductType)
                {
                    case ProductType.SimpleProduct:
                        {
                            //simple product doesn't have child products
                            productsToProcess.Add(product1);
                        }
                        break;
                    case ProductType.GroupedProduct:
                        {
                            //grouped products could have several child products
                            var associatedProducts = await _productService.GetAssociatedProductsAsync(product1.Id, store.Id);
                            productsToProcess.AddRange(associatedProducts);

                            //add parent product also
                            productsToProcess.Add(product1);
                            flagGroupParent = true;
                        }
                        break;
                    default:
                        continue;
                }
                foreach (var product in productsToProcess)
                {
                    writer.WriteStartElement("item");

                    #region Basic Product Information

                    //id [id]- An identifier of the item
                    writer.WriteElementString("id", product.Sku);

                    //title [title] - Title of the item
                    writer.WriteStartElement("title");
                    var title = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
                    //title should be not longer than 70 characters
                    if (title.Length > 70)
                        title = title[..70];
                    writer.WriteCData(title);
                    writer.WriteEndElement(); // title

                    //description [description] - Description of the item
                    writer.WriteStartElement("description");
                    var description = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, languageId);
                    if (string.IsNullOrEmpty(description))
                        description = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, languageId);
                    if (string.IsNullOrEmpty(description))
                        description = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId); //description is required
                                                                                                                      //resolving character encoding issues in your data feed
                    description = StripInvalidChars(description, true);
                    writer.WriteCData(description);
                    writer.WriteEndElement(); // description

                    //product type [product_type] - Your category of the item
                    var defaultProductCategory = (await _categoryService
                        .GetProductCategoriesByProductIdAsync(product.Id))
                        .FirstOrDefault();
                    if (defaultProductCategory != null)
                    {
                        //TODO localize categories
                        var category = await _categoryService.GetFormattedBreadCrumbAsync(
                            category: await _categoryService.GetCategoryByIdAsync(defaultProductCategory.CategoryId),
                            separator: ">",
                            languageId: languageId);
                        if (!string.IsNullOrEmpty(category))
                        {
                            writer.WriteStartElement("category");
                            writer.WriteCData(category);
                            writer.WriteFullEndElement();
                        }
                    }

                    if (doofinderSettings.AddAttributes)
                    {
                        var productdetails = await _productModelFactory.PrepareProductDetailsModelAsync(product, null, false);
                        var productAttributes = productdetails.ProductAttributes;


                        if (productAttributes.Count > 0)
                        {
                            foreach (var attribute in productAttributes)
                            {
                                writer.WriteStartElement("attribute");

                                writer.WriteElementString("name", attribute.Name);
                                if (attribute.Values.Count > 0)
                                {
                                    foreach (var value in attribute.Values)
                                        writer.WriteElementString("value", value.Name);
                                }
                                writer.WriteEndElement(); // Close the "attribute" element
                            }
                        }
                    }

                    if (product.ParentGroupedProductId != 0)
                    {
                        var parentSku = productsToProcess.FirstOrDefault(x => x.Id == product.ParentGroupedProductId)?.Sku;

                        writer.WriteElementString("group_leader", "false");
                        writer.WriteElementString("group_id", parentSku);
                    }

                    if (flagGroupParent && product.ParentGroupedProductId == 0)
                    {
                        writer.WriteElementString("group_leader", "true");
                    }

                    //link [link] - URL directly linking to your item's page on your website
                    var productUrl = GetUrlHelper().RouteUrl<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }, await GetHttpProtocolAsync());
                    writer.WriteElementString("link", productUrl);

                    //image link [image_link] - URL of an image of the item
                    //additional images [additional_image_link]
                    //up to 10 pictures
                    const int maximumPictures = 10;
                    var storeLocation = _webHelper.GetStoreLocation();
                    var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, maximumPictures);
                    for (var i = 0; i < pictures.Count; i++)
                    {
                        var picture = pictures[i];
                        var imageUrl = await _pictureService.GetPictureUrlAsync(picture.Id, doofinderSettings.ProductPictureSize, storeLocation: storeLocation);

                        if (i == 0)
                        {
                            //default image
                            writer.WriteElementString("image_link", imageUrl);
                        }
                        else
                        {
                            //additional image
                            writer.WriteElementString("additional_image_link", imageUrl);
                        }
                    }
                    if (!pictures.Any())
                    {
                        //no picture? submit a default one
                        var imageUrl = await _pictureService.GetDefaultPictureUrlAsync(doofinderSettings.ProductPictureSize, storeLocation: storeLocation);
                        writer.WriteElementString("image_link", imageUrl);
                    }

                    //condition [condition] - Condition or state of the item
                    writer.WriteElementString("condition", "new");
                    writer.WriteElementString("expiration_date", DateTime.Now.AddDays(doofinderSettings.ExpirationNumberOfDays).ToString("yyyy-MM-dd"));

                    #endregion

                    #region Availability & Price

                    //availability [availability] - Availability status of the item
                    var availability = "in stock"; //in stock by default
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                        && product.BackorderMode == BackorderMode.NoBackorders
                        && await _productService.GetTotalStockQuantityAsync(product) <= 0)
                    {
                        availability = "out of stock";
                    }
                    //uncomment th code below in order to support "preorder" value for "availability"
                    //if (product.AvailableForPreOrder &&
                    //    (!product.PreOrderAvailabilityStartDateTimeUtc.HasValue || 
                    //    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow))
                    //{
                    //    availability = "preorder";
                    //}
                    writer.WriteElementString("availability", availability);

                    //price [price] - Price of the item
                    var currency = await GetUsedCurrencyAsync();
                    decimal finalPriceBase;
                    if (doofinderSettings.PricesConsiderPromotions)
                    {
                        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                        var minPossiblePrice = (await _priceCalculationService.GetFinalPriceAsync(product, currentCustomer, store)).finalPrice;

                        if (product.HasTierPrices)
                        {
                            //calculate price for the maximum quantity if we have tier prices, and choose minimal
                            minPossiblePrice = Math.Min(minPossiblePrice,
                                (await _priceCalculationService.GetFinalPriceAsync(product, currentCustomer, store, quantity: int.MaxValue)).finalPrice);
                        }

                        finalPriceBase = (await _taxService.GetProductPriceAsync(product, minPossiblePrice)).price;
                    }
                    else
                    {
                        finalPriceBase = product.Price;
                    }
                    var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, currency);
                    //round price now so it matches the product details page
                    price = await _priceCalculationService.RoundPriceAsync(price);

                    writer.WriteElementString("price",
                                              price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                              currency.CurrencyCode);

                    #endregion

                    #region Unique Product Identifiers

                    /* Unique product identifiers such as UPC, EAN, JAN or ISBN allow us to show your listing on the appropriate product page. If you don't provide the required unique product identifiers, your store may not appear on product pages, and all your items may be removed from Product Search.
                     * We require unique product identifiers for all products - except for custom made goods. For apparel, you must submit the 'brand' attribute. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute. In all cases, we recommend you submit all three attributes.
                     * You need to submit at least two attributes of 'brand', 'gtin' and 'mpn', but we recommend that you submit all three if available. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute, but we recommend that you include 'brand' and 'mpn' if available.
                    */

                    //GTIN [gtin] - GTIN
                    var gtin = product.Gtin;
                    if (!string.IsNullOrEmpty(gtin))
                    {
                        writer.WriteStartElement("gtin");
                        writer.WriteCData(gtin);
                        writer.WriteFullEndElement();
                    }

                    //brand [brand] - Brand of the item
                    var defaultManufacturer = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).FirstOrDefault();
                    if (defaultManufacturer != null)
                    {
                        writer.WriteStartElement("brand");
                        writer.WriteCData((await _manufacturerService.GetManufacturerByIdAsync(defaultManufacturer.ManufacturerId))?.Name);
                        writer.WriteFullEndElement();
                    }

                    //mpn [mpn] - Manufacturer Part Number (MPN) of the item
                    var mpn = product.ManufacturerPartNumber;
                    if (!string.IsNullOrEmpty(mpn))
                    {
                        writer.WriteStartElement("mpn");
                        writer.WriteCData(mpn);
                        writer.WriteFullEndElement();
                    }

                    #endregion
                    writer.WriteEndElement(); // item
                }
            }

            writer.WriteEndElement(); // channel
            writer.WriteEndElement(); // rss
            writer.WriteEndDocument();
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new DooFinderSettings
            {
                PricesConsiderPromotions = false,
                ProductPictureSize = 125,
                StaticFileName = $"doofinder_{CommonHelper.GenerateRandomDigitCode(10)}.xml",
                ExpirationNumberOfDays = 28,
                ScheduleFeedGeneratingHour = 00,
                ScheduleFeedGeneratingMinute = 00,
                ScheduleFeedLastExecutionStartTime = null,
                ScheduleFeedIsExecutedForToday = false,
                ScheduleFeedLastExecutionEndTime = null
            };
            await _settingService.SaveSettingAsync(settings);

            if (await _scheduleTaskService.GetTaskByTypeAsync(DooFinderDefaults.GenerateFeedTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Type = DooFinderDefaults.GenerateFeedTask,
                    Name = DooFinderDefaults.GenerateFeedTaskName,
                    Seconds = DooFinderDefaults.DefaultGenerateFeedPeriod * 20 * 60
                });
            }

            await this.InstallPluginAsync(new DooFinderPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<DooFinderSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Nopstation.Plugin.Widgets.DooFinder");

            var task = await _scheduleTaskService.GetTaskByTypeAsync(DooFinderDefaults.GenerateFeedTask);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            await this.UninstallPluginAsync(new DooFinderPermissionProvider());
            await base.UninstallAsync();
        }

        /// <summary>
        /// Generate a static feed file
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task GenerateStaticFileAsync(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var filePath = _nopFileProvider.Combine(_webHostEnvironment.WebRootPath, "files", "exportimport", store.Id + "-" + _doofinderSettings.StaticFileName);
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            await GenerateFeedAsync(fs, store);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.HeadHtmlTag });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(DooFinderScriptViewComponent);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Store", "Store"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Store.Hint", "Select the store that will be used to generate the feed."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Heading", "DooFinder plugin configuration"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Currency", "Currency"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Currency.Hint", "Select the default currency that will be used to generate the feed."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ExceptionLoadPlugin", "Cannot load the plugin"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.General", "General"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.GeneralInstructions", "<ul><li><p style=\"margin-bottom:0; font-weight: bold\">Sign Up and Create an Account:</p><ul><li>Visit the <a href=\"https://admin.doofinder.com/auth/signup/en\" target=\"_blank\">Doofinder</a> website and sign up for an account.</li><li>Once registered, log in to your Doofinder account.</li></ul></li><br><li><p style=\"margin-bottom:0; font-weight: bold\">Create a Search Engine:</p><ul><li>In your Doofinder dashboard, create a new search engine.</li><li>Follow the on-screen instructions to configure the settings for your search engine, such as indexing options, filters, and appearance.</li></ul></li><br><li><p style=\"margin-bottom:0; font-weight: bold\">Index Your Content:</p><ul><li>Upload or provide the URL to the data feed containing the product information that you want to be searchable.</li><li>Doofinder will index this content, making it searchable through the search engine.</li></ul></li><br><li><p style=\"margin-bottom:0; font-weight: bold\">Generate the Integration Code:</p><ul><li>After creating the search engine, you'll need to generate the integration code.</li><li>Doofinder provides specific integration codes. Follow the platform-specific instructions to generate the code.</li></ul></li><br><li><p style=\"margin-bottom:0; font-weight: bold\">Integrate the Code into Your Website:</p><ul><li>Copy the generated code.</li><li>Paste the code into the appropriate section of your plugin. Activate the script both from widgets and from the plugin config page.</li></ul></li></ul><br>"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Generate", "Generate feed"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Override", "Override product settings"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.PassShippingInfoWeight", "Pass shipping info (weight)"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.PassShippingInfoWeight.Hint", "Check if you want to include shipping information (weight) in generated XML file."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.PassShippingInfoDimensions", "Pass shipping info (dimensions)"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.PassShippingInfoDimensions.Hint", "Check if you want to include shipping information (dimensions) in generated XML file."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.PricesConsiderPromotions", "Prices consider promotions"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.PricesConsiderPromotions.Hint", "Check if you want prices to be calculated with promotions (tier prices] = discounts] = special prices] = tax] = etc). But please note that it can significantly reduce time required to generate the feed file."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ProductPictureSize", "Product thumbnail image size"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ProductPictureSize.Hint", "The default size (pixels) for product thumbnail images."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Products.Size", "Size"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Products.Size.Hint", "Product size."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Products.CustomGoods", "Custom goods"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Products.CustomGoods.Hint", "Custom goods (no identifier exists)."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.SuccessResult", "DooFinder feed has been successfully generated."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.StaticFilePath", "Generated file path (static)"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.StaticFilePath.Hint", "A file path of the generated file. It's static for your store and can be shared with the DooFinder."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedLastExecutionStartTime", "Xml file last generate started on"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedLastExecutionEndTime", "Xml file last generate ended on"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedNextExecutionTime", "Xml file will be Generate again after"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedGeneratingHour", "Schedule feed generating hour"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ScheduleFeedGeneratingMinute", "Schedule feed generating minute"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ApiScript", "Api script"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ApiScript.Hint", "Api script that will be added to page header."),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ActiveScript", "Activate script"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.ActiveScript.Hint", "Activate script wheather DooFinder script will show or not"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Config", "Feed configuration"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Scheduler", "Feed generation scheduler"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Settings", "DooFinder settings"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Menu", "DooFinder"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.AddAttributes", "Add attributes"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.AddAttributes.Hint", "Add attributes"),
                new KeyValuePair<string, string>("Nopstation.Plugin.Widgets.DooFinder.Menu.Configuration", "Configuration")
            };
            return list;
        }


        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Nopstation.Plugin.Widgets.DooFinder.Menu")
            };

            if (await _permissionService.AuthorizeAsync(DooFinderPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/FeedDooFinder/Configure",
                    Title = await _localizationService.GetResourceAsync("Nopstation.Plugin.Widgets.DooFinder.Menu.Configuration"),
                    SystemName = "DooFinder.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/doofinder-integration-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=doofinder-integration",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public bool HideInWidgetList => false;

        #endregion
    }
}
