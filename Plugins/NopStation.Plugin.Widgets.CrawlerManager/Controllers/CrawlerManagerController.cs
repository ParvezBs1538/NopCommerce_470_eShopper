using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.CrawlerManager.Domain;
using NopStation.Plugin.Widgets.CrawlerManager.Factories;
using NopStation.Plugin.Widgets.CrawlerManager.Models;
using NopStation.Plugin.Widgets.CrawlerManager.Services;

namespace NopStation.Plugin.Widgets.CrawlerManager.Controllers
{
    public class CrawlerManagerController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICrawlerFactory _crawlerFactory;
        private readonly ICrawlerService _crawlerService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IGeoLookupService _geoLookupService;
        private readonly IWorkContext _workContext;
        private readonly INopFileProvider _fileProvider;
        private readonly AppSettings _appSettings;

        #endregion

        #region Ctor

        public CrawlerManagerController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ICrawlerFactory crawlerFactory,
            ILogger logger,
            ICrawlerService crawlerService,
            ICustomerService customerService,
            CustomerSettings customerSettings,
            IGeoLookupService geoLookupService,
            IWorkContext workContext,
            INopFileProvider fileProvider,
            AppSettings appSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _crawlerFactory = crawlerFactory;
            _logger = logger;
            _crawlerService = crawlerService;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _geoLookupService = geoLookupService;
            _workContext = workContext;
            _fileProvider = fileProvider;
            _appSettings = appSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var crawlersSettings = await _settingService.LoadSettingAsync<CrawlerManagerSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IsEnabled = crawlersSettings.IsEnabled
            };
            if (storeScope > 0)
            {
                model.IsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(crawlersSettings, x => x.IsEnabled, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var crawlersSettings = await _settingService.LoadSettingAsync<CrawlerManagerSettings>(storeScope);


            //save settings
            crawlersSettings.IsEnabled = model.IsEnabled;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(crawlersSettings, x => x.IsEnabled, model.IsEnabled_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        public virtual async Task<IActionResult> GuestList()
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return AccessDeniedView();

            //prepare model
            var model = await _crawlerFactory.PrepareOnlineGuestCustomerSearchModelAsync(new OnlineCustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GuestList(OnlineCustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _crawlerFactory.PrepareOnlineGuestCustomerListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> AddCrawler(int selectedId)
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return AccessDeniedView();
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(selectedId);

                if (customer == null)
                    return Json(new { result = false });

                var crawler = new Crawler()
                {
                    CrawlerInfo = ReplaceVersionWithWildcard(customer.AdminComment),
                    IPAddress = _customerSettings.StoreIpAddresses
                        ? customer.LastIpAddress : await _localizationService.GetResourceAsync("Admin.Customers.OnlineCustomers.Fields.IPAddress.Disabled"),
                    Location = _geoLookupService.LookupCountryName(customer.LastIpAddress),
                    AddedBy = (await _workContext.GetCurrentCustomerAsync()).Email
                };

                // Create XML document using XElement
                XElement newRootElement = new XElement("browscap");

                XElement instanceElement = new XElement("browscapitem");

                // Replace version numbers with *
                string instanceName = crawler.CrawlerInfo;
                instanceElement.SetAttributeValue("name", instanceName);

                // Create Crawler item for instances
                XElement instanceCrawlerElement = new XElement("item");
                instanceCrawlerElement.SetAttributeValue("name", "Crawler");
                instanceCrawlerElement.SetAttributeValue("value", "true");
                instanceElement.Add(instanceCrawlerElement);

                // Add instanceElement to the root
                newRootElement.Add(instanceElement);

                var xmlFilePath2 = !string.IsNullOrEmpty(_appSettings.Get<CommonConfig>().CrawlerOnlyUserAgentStringsPath)
                    ? _fileProvider.MapPath(_appSettings.Get<CommonConfig>().CrawlerOnlyUserAgentStringsPath) : string.Empty;
                // Load XML from the main file 
                XElement mainXml = XElement.Load(xmlFilePath2);

                // Include only those items  that don't exist in the existing file.
                var newItems = newRootElement.Elements("browscapitem")
                    .Where(item1 => mainXml.Elements("browscapitem").All(item2 => (string)item1.Attribute("name") != (string)item2.Attribute("name")));

                if (newItems != null && newItems.Count() > 0)
                {
                    // Add the new items to the main XML
                    mainXml.Add(newItems);

                    // Save the updated XML to the main file 
                    mainXml.Save(xmlFilePath2);

                    await _crawlerService.AddOrUpdateCrawlerAsync(crawler);

                    return Json(new { result = true });
                }
                return Json(new { result = false, exist = true });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(_localizationService.GetResourceAsync("NopStation.Plugin.Widgets.CrawlerManager.LogError.AddFailed").Result, ex);
                return Json(new { result = false });
            }
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return AccessDeniedView();

            //prepare model
            var model = await _crawlerFactory.PrepareCrawlersSearchModelAsync(new OnlineCustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(OnlineCustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _crawlerFactory.PrepareCrawlersListModelAsync(searchModel);

            return Json(model);
        }

        // Helper method to replace version numbers with *
        private static string ReplaceVersionWithWildcard(string userAgent)
        {
            // Use a regular expression to replace version numbers with *
            var withoutVersion = Regex.Replace(userAgent, @"(?<!Mozilla\/)\d+(\.\d+)+", "*");
            return Regex.Replace(withoutVersion, @"\\/", "");
        }

        #endregion
    }
}