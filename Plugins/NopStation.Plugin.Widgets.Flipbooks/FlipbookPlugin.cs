using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Widgets.Flipbooks.Services;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Widgets.Flipbooks.Components;

namespace NopStation.Plugin.Widgets.Flipbooks
{
    public class FlipbookPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IFlipbookService _flipbookService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public FlipbookPlugin(ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService,
            IWebHelper webHelper,
            IProductService productService,
            IPictureService pictureService,
            INopFileProvider fileProvider,
            IFlipbookService flipbookService,
            IUrlRecordService urlRecordService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
            _webHelper = webHelper;
            _productService = productService;
            _pictureService = pictureService;
            _fileProvider = fileProvider;
            _flipbookService = flipbookService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        protected async Task InsertSampleDataAsync()
        {
            var settings = new FlipbookSettings()
            {
                DefaultPageSize = 9,
            };
            await _settingService.SaveSettingAsync(settings);

            var i = 0;
            var j = 0;
            var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.Flipbooks/Contents/images/");
            var products = await _productService.SearchProductsAsync(pageSize: 27);

            var flipbook = new Flipbook()
            {
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                IncludeInTopMenu = true,
                Name = "Flipbook",
                MetaTitle = "Flipbook",
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _flipbookService.InsertFlipbookAsync(flipbook);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(flipbook, null, flipbook.Name, true);
            await _urlRecordService.SaveSlugAsync(flipbook, seName, 0);

            var content1 = new FlipbookContent()
            {
                FlipbookId = flipbook.Id,
                DisplayOrder = ++j,
                IsImage = true,
                RedirectUrl = "#",
                ImageId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "11-large.jpg")), MimeTypes.ImagePJpeg, "11-large")).Id,
            };
            await _flipbookService.InsertFlipbookContentAsync(content1);

            var content2 = new FlipbookContent()
            {
                FlipbookId = flipbook.Id,
                DisplayOrder = ++j,
                IsImage = false,
            };
            await _flipbookService.InsertFlipbookContentAsync(content2);
            while (i < 9 && products.Count > i)
            {
                var product = products[i];
                await _flipbookService.InsertFlipbookContentProductAsync(new FlipbookContentProduct() { FlipbookContentId = content2.Id, ProductId = product.Id });
                i++;
            }

            var content3 = new FlipbookContent()
            {
                FlipbookId = flipbook.Id,
                DisplayOrder = ++j,
                IsImage = true,
                RedirectUrl = "#",
                ImageId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "7-large.jpg")), MimeTypes.ImagePJpeg, "7-large")).Id,
            };
            await _flipbookService.InsertFlipbookContentAsync(content3);

            var content4 = new FlipbookContent()
            {
                FlipbookId = flipbook.Id,
                DisplayOrder = ++j,
                IsImage = false,
            };
            await _flipbookService.InsertFlipbookContentAsync(content4);
            while (i < 18 && products.Count > i)
            {
                var product = products[i];
                await _flipbookService.InsertFlipbookContentProductAsync(new FlipbookContentProduct() { FlipbookContentId = content4.Id, ProductId = product.Id });
                i++;
            }

            var content5 = new FlipbookContent()
            {
                FlipbookId = flipbook.Id,
                DisplayOrder = ++j,
                IsImage = true,
                RedirectUrl = "#",
                ImageId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "8-large.jpg")), MimeTypes.ImagePJpeg, "8-large")).Id,
            };
            await _flipbookService.InsertFlipbookContentAsync(content5);

            var content6 = new FlipbookContent()
            {
                FlipbookId = flipbook.Id,
                DisplayOrder = ++j,
                IsImage = false,
            };
            await _flipbookService.InsertFlipbookContentAsync(content6);
            while (products.Count > i)
            {
                var product = products[i];
                await _flipbookService.InsertFlipbookContentProductAsync(new FlipbookContentProduct() { FlipbookContentId = content6.Id, ProductId = product.Id });
                i++;
            }
        }

        #endregion

        #region Methods

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(FlipbookViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> 
            {
                PublicWidgetZones.HeaderMenuAfter
            });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Menu.Flipbook")
            };

            if (await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/Flipbook/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Menu.Configuration"),
                    SystemName = "Flipbooks.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/Flipbook/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Menu.Flipbooks"),
                    SystemName = "Flipbooks"
                };
                menu.ChildNodes.Add(settings);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/flipbooks-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=flipbooks",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }
        
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Flipbook/Configure";
        }

        public override async Task InstallAsync()
        {
            await InsertSampleDataAsync();

            await this.InstallPluginAsync(new FlipbookPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new FlipbookPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Menu.Flipbook", "Flipbook"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Menu.Flipbooks", "Flipbooks"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Created", "Flipbook content created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Updated", "Flipbook content updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Deleted", "Flipbook content deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Created", "Flipbook created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Updated", "Flipbook updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Deleted", "Flipbook deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.Name.Required", "Flipbook name is required."),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List", "Flipbooks"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchName", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchName.Hint", "A flipbook name."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchStore.Hint", "Search by a specific store."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.Hint", "Search by active column."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.ActiveOnly", "Active only"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.InactiveOnly", "Inactive only"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.Name.Hint", "The name of the flipbook."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.IncludeInTopMenu", "Include in top menu"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.IncludeInTopMenu.Hint", "Check to include this flipbook in the top menu."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.AvailableStartDateTime", "Available start date time"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.AvailableStartDateTime.Hint", "Flipbook available start date time."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.AvailableEndDateTime", "Available end date time"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.AvailableEndDateTime.Hint", "Flipbook available end date time."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaKeywords", "Meta keywords"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaKeywords.Hint", "Meta keywords to be added to flipbook page header."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaDescription", "Meta description"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaDescription.Hint", "Meta description to be added to flipbook page header."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaTitle", "Meta title"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaTitle.Hint", "Override the page title. The default is the name of the flipbook."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.SeName", "Search engine friendly page name"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.SeName.Hint", "	Set a search engine friendly page name e.g. 'the-best-flipbook' to make your page URL 'http://www.yourStore.com/the-best-flipbook'. Leave empty to generate it automatically based on the name of the flipbook."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.Active.Hint", "A value indicating whether the flipbook is active."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.CreatedOn.Hint", "The date/time that the flipbook was created."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.UpdatedOn.Hint", "The date/time that the flipbook was updated."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.DisplayOrder.Hint", "The flipbook display order. 1 represents the first item in the list."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.LimitedToStores", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.LimitedToStores.Hint", "Option to limit this flipbook to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.AclCustomerRoles", "Limited to customer roles"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Fields.AclCustomerRoles.Hint", "Select customer roles for which the flipbook will be shown. Leave empty if you want this flipbook to be visible to all users."),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Configuration", "Flipbook settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Configuration.Fields.DefaultPageSize", "Default page size"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Configuration.Fields.DefaultPageSize.Hint", "Default page size"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.Product", "Product"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.Content", "Content"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContentProducts.Fields.DisplayOrder", "Display order"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Flipbook", "Flipbook"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Content", "Content"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Image", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.IsActive", "Is active"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.IsImage", "Is image"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.RedirectUrl", "Redirect url"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Flipbook.Hint", "The flipbook."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Content.Hint", "Content"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Image.Hint", "Upload image."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.DisplayOrder.Hint", "The flipbook content display order. 1 represents the first item in the list."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.IsActive.Hint", "Is active."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.IsImage.Hint", "Is image content or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Fields.RedirectUrl.Hint", "Enter redirect url."),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContentProducts.AddNew", "Add a new product"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Configure", "Flipbook settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.AddNew", "Add new flipbook"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.BackToList", "back to flipbook list"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.EditDetails", "Edit flipbook details"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.AddNew", "Add new content"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.BackToFlipbookDetails", "back to flipbook details"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.EditDetails", "Edit flipbook content details"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.SaveBeforeEdit", "You need to save the flipbook before you can add contents for this flipbook page."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Contents", "Contents"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.Flipbooks.Seo", "Seo"),

                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Products", "Products"),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Products.SaveBeforeEdit", "You need to save the flipbook content before you can add products for this flipbook content page."),
                new KeyValuePair<string, string>("Admin.NopStation.Flipbooks.FlipbookContents.Info", "Info"),
            };

            return list;
        }

   

        #endregion
    }
}
