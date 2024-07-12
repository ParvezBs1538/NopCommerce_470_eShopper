using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.Core;
using Nop.Services.Security;
using RestSharp;
using Newtonsoft.Json;
using NopStation.Plugin.ExchangeRate.Abstract.Domains;
using Nop.Core;
using Nop.Core.Domain.Logging;

namespace NopStation.Plugin.ExchangeRate.Abstract
{
    public class AbstractExchangeRateProvider : BasePlugin, IExchangeRateProvider, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly AbstractExchangeRateSettings _abstractExchangeRateSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ICurrencyService _currencyService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public AbstractExchangeRateProvider(AbstractExchangeRateSettings abstractExchangeRateSettings,
            IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            ICurrencyService currencyService,
            IWebHelper webHelper)
        {
            _abstractExchangeRateSettings = abstractExchangeRateSettings;
            _httpClientFactory = httpClientFactory;
            _localizationService = localizationService;
            _logger = logger;
            _settingService = settingService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _currencyService = currencyService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AbstractExchangeRate/Configure";
        }

        public async Task<IList<Nop.Core.Domain.Directory.ExchangeRate>> GetCurrencyLiveRatesAsync(string baseCurrencyCode)
        {
            if (baseCurrencyCode == null)
                throw new ArgumentNullException(nameof(baseCurrencyCode));

            var uri = new Uri($"https://exchange-rates.abstractapi.com/v1/live?api_key={_abstractExchangeRateSettings.ApiKey}&base={baseCurrencyCode}");

            var request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.None
            };
            var client = new RestClient(uri);
            var response = await client.ExecuteAsync(request);

            var data = JsonConvert.DeserializeObject<ApiResponse>(response.Content);

            var rates = new List<Nop.Core.Domain.Directory.ExchangeRate>
            {
                new Nop.Core.Domain.Directory.ExchangeRate
                {
                    CurrencyCode = baseCurrencyCode,
                    Rate = 1,
                    UpdatedOn = DateTime.UtcNow
                }
            };

            if (!string.IsNullOrWhiteSpace(data?.Error?.Message))
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"Abstract exchange rate: {data.Error.Code} {data.Error.Message}",
                    string.Join(Environment.NewLine, data.Error.Details.Target));

                return rates;
            }

            foreach (var item in data.ExchangeRates)
            {
                rates.Add(new Nop.Core.Domain.Directory.ExchangeRate()
                {
                    CurrencyCode = item.Key,
                    Rate = item.Value,
                    UpdatedOn = DateTime.TryParseExact( data.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime) ? datetime : DateTime.UtcNow
                });
            }

            return rates;
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AbstractExchangeRatePermissionProvider());
            await base.InstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractExchangeRate.Menu.AbstractExchangeRate")
            };

            if (await _permissionService.AuthorizeAsync(AbstractExchangeRatePermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AbstractExchangeRate/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractExchangeRate.Menu.Configuration"),
                    SystemName = "AbstractExchangeRate.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/abstract-exchange-rate-provider-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=abstract-exchange-rate-provider",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new Dictionary<string, string>
            {
                ["Admin.NopStation.AbstractExchangeRate.Menu.AbstractExchangeRate"] = "Exchange rate (Abstract)",
                ["Admin.NopStation.AbstractExchangeRate.Menu.Configuration"] = "Configuration",

                ["Admin.NopStation.AbstractExchangeRate.Configuration"] = "Abstract exchange rate settings",

                ["Admin.NopStation.AbstractExchangeRate.Configuration.Fields.ApiKey"] = "Api key",
                ["Admin.NopStation.AbstractExchangeRate.Configuration.Fields.ApiKey.Hint"] = "Enter unique api key of Abstract Api.",
            };

            return list.ToList();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AbstractExchangeRatePermissionProvider());
            await base.UninstallAsync();
        }

        #endregion
    }
}