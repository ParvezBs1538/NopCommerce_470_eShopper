using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Components;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure;
using WebPush;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp
{
    public class ProgressiveWebAppPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<PushNotificationTemplate> _pushNotificationTemplateRepository;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IStoreService _storeService;
        private readonly INopStationCoreService _nopStationCoreService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public ProgressiveWebAppPlugin(IPermissionService permissionService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IRepository<PushNotificationTemplate> pushNotificationTemplateRepository,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IPictureService pictureService,
            INopFileProvider fileProvider,
            IStoreService storeService,
            INopStationCoreService nopStationCoreService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _pictureService = pictureService;
            _fileProvider = fileProvider;
            _storeService = storeService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Utilities

        protected async Task CreateSampleDataAsync()
        {
            var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/");
            var picture = await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "nop-station.png")), MimeTypes.ImagePng, "nop-station");

            var keys = VapidHelper.GenerateVapidKeys();

            var pwaSettings = new ProgressiveWebAppSettings()
            {
                ManifestName = "Nop-Station",
                ManifestShortName = "NopStation",
                ManifestStartUrl = ".",
                ManifestThemeColor = "#0385c6",
                ManifestBackgroundColor = "#019ede",
                ManifestDisplay = "standalone",
                ManifestPictureId = picture.Id,
                VapidPrivateKey = keys.PrivateKey,
                VapidPublicKey = keys.PublicKey,
                VapidSubjectEmail = "sales@nop-station.com",
                DefaultIconId = picture.Id
            };
            await _settingService.SaveSettingAsync(pwaSettings);

            await InsertTemplatesAsync();
            await PreparePWAStaticFileAsync();

            var task = await _scheduleTaskService.GetTaskByTypeAsync(ProgressiveWebAppDefaults.QueuedTaskType);
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Send push notification",
                    Seconds = 60,
                    Type = ProgressiveWebAppDefaults.QueuedTaskType
                });
            }

            var task1 = await _scheduleTaskService.GetTaskByTypeAsync(ProgressiveWebAppDefaults.AbadonedCartTaskType);
            if (task1 == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Check abandoned carts",
                    Seconds = 60,
                    Type = ProgressiveWebAppDefaults.AbadonedCartTaskType
                });
            }
        }

        protected async Task InsertTemplatesAsync()
        {
            var messageTemplates = new List<PushNotificationTemplate>
            {
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.CustomerEmailValidationMessage,
                    Title = "%Store.Name%. Email validation",
                    Body = $"%Store.Name%, {Environment.NewLine}Check your email to activate your account. {Environment.NewLine}%Store.Name%",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.PrivateMessageNotification,
                    Title = "%Store.Name%. You have received a new private message",
                    Body = $"%Store.Name%, {Environment.NewLine}You have received a new private message.",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.CustomerWelcomeMessage,
                    Title = "Welcome to %Store.Name%",
                    Body = $"We welcome you to %Store.Name%.{Environment.NewLine}You can now take part in the various services we have to offer you. Some of these services include:{Environment.NewLine}Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.{Environment.NewLine}Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.{Environment.NewLine}Order History - View your history of purchases that you have made with us.{Environment.NewLine}Products Reviews - Share your opinions on products with our other customers.",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.NewForumPostMessage,
                    Title = "%Store.Name%. New Post Notification.",
                    Body = $"%Store.Name%, {Environment.NewLine}A new post has been created in the topic %Forums.TopicName% at %Forums.ForumName% forum.{Environment.NewLine}Click here for more info.{Environment.NewLine}Post author: %Forums.PostAuthor%{Environment.NewLine}Post body: %Forums.PostBody%",
                    Active = true,
                    UseDefaultIcon = true,
                    Url = "%Forums.TopicURL%"
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.NewForumTopicMessage,
                    Title = "%Store.Name%. New Topic Notification.",
                    Body = $"%Store.Name%, {Environment.NewLine}A new topic %Forums.TopicName% has been created at %Forums.ForumName% forum.{Environment.NewLine}Click here for more info.",
                    Active = true,
                    UseDefaultIcon = true,
                    Url = "%Forums.TopicURL%"
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.CustomerRegisteredNotification,
                    Title = "%Store.Name%. New customer registration",
                    Body = $"%Store.Name%, {Environment.NewLine}A new customer registered with your store. Below are the customer's details:{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}Email: %Customer.Email%.",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderCancelledCustomerNotification,
                    Title = "%Store.Name%. Your order cancelled",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been cancelled. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderCompletedCustomerNotification,
                    Title = "%Store.Name%. Your order completed",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been completed. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.ShipmentDeliveredCustomerNotification,
                    Title = "Your order from %Store.Name% has been delivered.",
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Good news! You order has been delivered.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderPlacedCustomerNotification,
                    Title = "Order receipt from %Store.Name%.",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order Number: %Order.OrderNumber%.",
                    Active = true,
                    UseDefaultIcon = true,
                    Url = "%Store.URL%orderdetails/%Order.OrderNumber%"
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderPlacedAdminNotification,
                    Title = "%Store.Name%. Purchase Receipt for Order #%Order.OrderNumber%",
                    Body = $"%Store.Name%, {Environment.NewLine}%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store.",
                    Active = true,
                    UseDefaultIcon = true,
                    Url = "%Store.URL%Admin/Order/Edit/%Order.OrderNumber%"
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.ShipmentSentCustomerNotification,
                    Title = "Your order from %Store.Name% has been shipped.",
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}Good news! You order has been shipped.{Environment.NewLine}Order Number: %Order.OrderNumber%",
                    Active = true,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderPlacedVendorNotification,
                    Title = "%Store.Name%. Order placed",
                    Body = $"%Store.Name%, {Environment.NewLine}%Customer.FullName% (%Customer.Email%) has just placed an order.{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}.",
                    //this template is disabled by default
                    Active = false,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderRefundedCustomerNotification,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.",
                    //this template is disabled by default
                    Active = false,
                    UseDefaultIcon = true,
                    Url = "%Order.OrderURLForCustomer%"
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderRefundedAdminNotification,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                    Body = $"%Store.Name%. Order #%Order.OrderNumber% refunded', %Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just refunded{Environment.NewLine}Amount refunded: %Order.AmountRefunded%{Environment.NewLine}Date Ordered: %Order.CreatedOn%.",
                    //this template is disabled by default
                    Active = false,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderPaidAdminNotification,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"%Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just paid{Environment.NewLine}Date Ordered: %Order.CreatedOn%.",
                    //this template is disabled by default
                    Active = false,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderPaidCustomerNotification,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been just paid. Order Number: %Order.OrderNumber%.",
                    //this template is disabled by default
                    Active = false,
                    UseDefaultIcon = true,
                    Url = "%Order.OrderURLForCustomer%"
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.OrderPaidVendorNotification,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"%Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just paid.{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}.",
                    //this template is disabled by default
                    Active = false,
                    UseDefaultIcon = true
                },
                new PushNotificationTemplate
                {
                    Name = PushNotificationTemplateSystemNames.AbandonedCartNotification,
                    Title = "%Store.Name%. Abandoned cart",
                    Body = $"%Store.Name%, {Environment.NewLine}Dear %Customer.FullName%,{Environment.NewLine}We noticed you left item(s) in your cart and this is a friendly reminder to complete your purchase.{Environment.NewLine}",
                    Active = true,
                    UseDefaultIcon = true
                }
            };
            await _pushNotificationTemplateRepository.InsertAsync(messageTemplates);
        }

        protected async Task PreparePWAStaticFileAsync()
        {
            var stores = await _storeService.GetAllStoresAsync();
            foreach (var store in stores)
            {
                await PrepareManifestStringAsync(store.Id);
            }

            var swJsMainUrl = _fileProvider.Combine(_fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/"), PWADefaults.ServiceWorkerJsUrl);
            var swJsUrl = _fileProvider.GetAbsolutePath(PWADefaults.ServiceWorkerJsUrl);
            _fileProvider.WriteAllText(swJsUrl, _fileProvider.ReadAllText(swJsMainUrl, Encoding.UTF8), Encoding.UTF8);
        }

        private async Task PrepareManifestStringAsync(int storeId)
        {
            var progressiveWebAppSettings = await _settingService.LoadSettingAsync<ProgressiveWebAppSettings>(storeId);

            var model = new ManifestModel()
            {
                BackgroundColor = progressiveWebAppSettings.ManifestBackgroundColor,
                Display = progressiveWebAppSettings.ManifestDisplay,
                Name = progressiveWebAppSettings.ManifestName,
                ShortName = progressiveWebAppSettings.ManifestShortName,
                StartUrl = progressiveWebAppSettings.ManifestStartUrl,
                ThemeColor = progressiveWebAppSettings.ManifestThemeColor,
                SplashPages = null,
                ApplicationScope = progressiveWebAppSettings.ManifestAppScope
            };

            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_72.png",
                Sizes = "72x72",
                Type = "image/png"
            });
            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_96.png",
                Sizes = "96x96",
                Type = "image/png"
            });
            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_128.png",
                Sizes = "128x128",
                Type = "image/png"
            });
            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_144.png",
                Sizes = "144x144",
                Type = "image/png"
            });
            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_192.png",
                Sizes = "192x192",
                Type = "image/png"
            });
            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_384.png",
                Sizes = "384x384",
                Type = "image/png"
            });
            model.Icons.Add(new ManifestModel.ManifestIconModel()
            {
                Source = $"{_webHelper.GetStoreLocation()}Plugins/NopStation.Plugin.Widgets.ProgressiveWebApp/Contents/sample/nop-station_512.png",
                Sizes = "512x512",
                Type = "image/png"
            });

            var json = JsonConvert.SerializeObject(model);
            if (string.IsNullOrWhiteSpace(progressiveWebAppSettings.ManifestAppScope))
            {
                var o = (JObject)JsonConvert.DeserializeObject(json);
                o.Property("scope").Remove();
                json = o.ToString();
            }

            var manifestPath = string.Format(PWADefaults.ManifestUrl, storeId.ToString("000000"));
            var manifestFilePath = _fileProvider.GetAbsolutePath(manifestPath);
            _fileProvider.CreateFile(manifestFilePath);
            _fileProvider.WriteAllText(manifestFilePath, json, Encoding.UTF8);
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ProgressiveWebApp/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new ProgressiveWebAppPermissionProvider());
            await CreateSampleDataAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            var task = await _scheduleTaskService.GetTaskByTypeAsync(ProgressiveWebAppDefaults.QueuedTaskType);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            var task1 = await _scheduleTaskService.GetTaskByTypeAsync(ProgressiveWebAppDefaults.AbadonedCartTaskType);
            if (task1 != null)
                await _scheduleTaskService.DeleteTaskAsync(task1);

            await this.UninstallPluginAsync(new ProgressiveWebAppPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.Menu.ProgressiveWebApp")
            };

            if (await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/ProgressiveWebApp/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.Menu.Configuration"),
                    SystemName = "PWA.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }
            if (await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageTemplates))
            {
                var template = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/PushNotificationTemplate/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.Menu.PushNotificationTemplates"),
                    SystemName = "PWA.PushNotificationTemplates"
                };
                menu.ChildNodes.Add(template);
            }
            if (await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
            {
                var announce = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/PushNotificationAnnouncement/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.Menu.PushNotificationAnnouncements"),
                    SystemName = "PWA.PushNotificationAnnouncements"
                };
                menu.ChildNodes.Add(announce);
            }
            if (await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageQueuedNotifications))
            {
                var queued = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/QueuedPushNotification/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.Menu.QueuedPushNotifications"),
                    SystemName = "PWA.QueuedPushNotifications"
                };
                menu.ChildNodes.Add(queued);
            }
            if (await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
            {
                var device = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/WebAppDevice/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.Menu.Devices"),
                    SystemName = "PWA.WebAppDevices"
                };
                menu.ChildNodes.Add(device);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/progressive-web-app-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=progressive-web-app",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.Footer, PublicWidgetZones.HeadHtmlTag
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(PWAFooterViewComponent);
            return typeof(PWAHeadHtmlTagViewComponent);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Menu.ProgressiveWebApp", "Progressive web app"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Menu.Devices", "Devices"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Menu.PushNotificationTemplates", "Notification templates"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Menu.PushNotificationAnnouncements", "Announcements"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Menu.QueuedPushNotifications", "Queued notifications"),
                new KeyValuePair<string, string>("NopStation.PWA.UploadMp3", "Upload an mp3"),
                new KeyValuePair<string, string>("NopStation.PWA.InvalidMp3", "Invalid .mp3 file."),
                new KeyValuePair<string, string>("NopStation.PWA.Mp3Upload.Success", ".mp3 file uploaded successfully."),
                new KeyValuePair<string, string>("NopStation.PWA.Play", "Play"),
                new KeyValuePair<string, string>("NopStation.PWA.SelectTemplate", "Select template"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Title", "Progressive web app settings"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.GenerateKeys", "Generate keys"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.BlockTitle.VapidDetails", "Vapid details"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.BlockTitle.Notification", "Notification"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.BlockTitle.Manifest", "Manifest"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidSubjectEmail", "Subject email"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidSubjectEmail.Hint", "Enter subject email for vapid details, i.e info@example.com."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidPublicKey", "Public key"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidPublicKey.Hint", "Enter public key for vapid details."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidPrivateKey", "Private key"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidPrivateKey.Hint", "Enter private key for vapid details."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.DisableSilent", "Disable silent"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.DisableSilent.Hint", "Check to disable silent notification."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.Vibration", "Vibration"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.Vibration.Hint", "Enter comma separated notification vibration pattern, i.e. 50, 10, 50."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.DefaultIconId", "Default icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.DefaultIconId.Hint", "The notification default icon. It displays to the side of the notification's title and message. Valid file extensions are .png, .gif and .bmp."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.SoundFileUrl", "Sound"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.SoundFileUrl.Hint", "The notification sound. Valid file extension is .mp3."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestName", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestName.Hint", "The name is used in the app install prompt."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestShortName", "Short name"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestShortName.Hint", "The short name is used on the user's home screen, launcher, or other places where space may be limited, when it is provided."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestThemeColor", "Theme color"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestThemeColor.Hint", "It sets the color of the tool bar, and may be reflected in the app's preview in task switchers."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestBackgroundColor", "Background color"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestBackgroundColor.Hint", "This property is used on the splash screen when the application is first launched."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestDisplay", "Display"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestDisplay.Hint", "This property helps customize what browser UI is shown when your app is launched. For example, you can hide the address bar and browser chrome. Or games may want to go completely full screen."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestStartUrl", "Start URL"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestStartUrl.Hint", "The start url tells the browser where your application should start when it is launched, and prevents the app from starting on whatever page the user was on when they added your app to their home screen."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestAppScope", "Application scope"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestAppScope.Hint", "It defines the set of URLs that the browser considers to be within your app, and is used to decide when the user has left the app. "),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestPictureId", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestPictureId.Hint", "The picture for the icons. \"icons\" is an array of image objects. Each object should include the src, a sizes property, and the type of image."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidTitleEmail.Required", "The 'Vapid subject email' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidPublicKey.Required", "The 'Vapid public key' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.VapidPrivateKey.Required", "The 'Vapid private key' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.ManifestNameOrShortName.Required", "You must provide at least the \"Short name\" or \"Name\" property."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.List", "Notification templates"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.EditDetails", "Edit template"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.BackToList", "back to template list"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Updated", "Notification template has been updated successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Name.Hint", "The notification template name."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Title.Hint", "The notification template title."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Body.Hint", "The notification template body. (HTML is not supported)"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.UseDefaultIcon", "Use default icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.UseDefaultIcon.Hint", "Check to use default icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.IconId", "Icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.IconId.Hint", "The notification template icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.ImageId", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.ImageId.Hint", "The notification template image."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Url", "Url"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Url.Hint", "The notification template url."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Active.Hint", "Check to active notification template."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.LimitedToStores", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.LimitedToStores.Hint", "Option to limit this category to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Title.Required", "The 'Notification template title' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationTemplates.Fields.IconId.Required", "The 'Notification template icon' is required."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.List", "Notification announcements"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.EditDetails", "Edit announcement"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.AddNew", "Add new announcement"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.BackToList", "back to announcement list"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Updated", "Notification announcement has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Created", "Notification announcement has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Deleted", "Notification announcement has been deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Sent", "Notification announcement added into queue."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.SendNow", "Send now"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.AreYouSure", "Are you sure want to send notification now?"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Title.Hint", "The notification announcement title."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Body.Hint", "The notification announcement body. (HTML is not supported)"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.UseDefaultIcon", "Use default icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.UseDefaultIcon.Hint", "Check to use default icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.IconId", "Icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.IconId.Hint", "The notification announcement icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.ImageId", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.ImageId.Hint", "The notification announcement image."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Url", "Url"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Url.Hint", "The notification announcement url."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.CreatedOn.Hint", "The date when notification announcement was created."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Title.Required", "The 'Notification announcement title' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.IconId.Required", "The 'Notification announcement icon' is required."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.List", "Web app devices"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.BackToList", "back to device list"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.ViewDetails", "Device details"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.PushEndpoint", "Push end point"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.PushEndpoint.Hint", "The device 'Push end point'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.PushP256DH", "Push p256dh"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.PushP256DH.Hint", "The device 'Push p256dh'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.PushAuth", "Push auth"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.PushAuth.Hint", "The device 'Push auth'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.Customer.Hint", "The device 'Customer'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.Store", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.Store.Hint", "The device 'Store'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.VapidPublicKey", "Vapid public key"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.VapidPublicKey.Hint", "The device 'Vapid public key'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.VapidPrivateKey", "Vapid private key"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.VapidPrivateKey.Hint", "The device 'Vapid private key'."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Fields.CreatedOn.Hint", "The create date."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.TestPushTitle", "This is a test title."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.TestPushBody", "This is a test Body."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.TestPushSent", "Test push notification sent successfully!"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.SendTestNotification", "Send test notification"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.List", "Queued push notifications"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.BackToList", "back to queue list"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.ViewDetails", "Notification details"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Customer.Hint", "The customer."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Store", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Store.Hint", "The store."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Title.Hint", "The notification title."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Body.Hint", "The notification body."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.IconUrl", "Icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.IconUrl.Hint", "The notification icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.ImageUrl", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.ImageUrl.Hint", "The notification image."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Url", "Url"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Url.Hint", "The notification url."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Rtl", "Right-to-left"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Rtl.Hint", "The notification right-to-left."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.CreatedOn.Hint", "The date when notification was created."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.SentOn", "Sent on"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Fields.SentOn.Hint", "The date when notification was sent."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Deleted", "Queued push notification deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.QueuedPushNotifications.Unknown", "Unknown"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Manifest.PictureId.Hint", "The picture aspect ratio should be 1:1 (square). Supported format is PNG. For better view, use a transparent backgroud image."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Manifest.Tooltip", "A typical manifest file includes information about the app <code>name</code>, <code>icons</code> it should use, the <code>start_url</code> it should start at when launched, and more. Click <a href=\"https://developers.google.com/web/fundamentals/web-app-manifest/\" target=\"_blank\">here</a> to know more about web app manifest."),

                new KeyValuePair<string, string>("NopStation.PWA.AllowPushNotifications", "Want to get notifications from us?"),
                new KeyValuePair<string, string>("NopStation.PWA.AllowPushNotifications.Ok", "Ok"),
                new KeyValuePair<string, string>("NopStation.PWA.AllowPushNotifications.No", "No"),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.WebAppDevices.Deleted", "Device has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.AbandonedCartCheckingOffset", "Abandoned cart checking offset"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.UnitTypeId", "Unit type"),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.AbandonedCartCheckingOffset.Hint", "Abandoned cart checking offset."),
                new KeyValuePair<string, string>("Admin.NopStation.PWA.Configuration.Fields.UnitTypeId.Hint", "Select unit type.")
            };

            return list;
        }

        #endregion
    }
}