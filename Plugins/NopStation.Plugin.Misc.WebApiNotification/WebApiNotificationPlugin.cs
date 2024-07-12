using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Components;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification
{
    public class WebApiNotificationPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IRepository<WebApiNotificationTemplate> _pushNotificationTemplateRepository;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        public bool HideInWidgetList => true;

        #endregion

        #region Ctor

        public WebApiNotificationPlugin(ILocalizationService localizationService,
            IRepository<WebApiNotificationTemplate> pushNotificationTemplateRepository,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            _scheduleTaskService = scheduleTaskService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Utilities

        protected async Task InsertInitialDataAsync()
        {
            var scheduleTaskForNotification = await _scheduleTaskService.GetTaskByTypeAsync(WebApiNotificationDefaults.QueuedSendTaskType);
            var scheduleTaskForCheckCampaign = await _scheduleTaskService.GetTaskByTypeAsync(WebApiNotificationDefaults.CampaignSendTaskType);

            if (scheduleTaskForNotification == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Send push notification",
                    Seconds = 60,
                    Type = WebApiNotificationDefaults.QueuedSendTaskType
                });
            }

            if (scheduleTaskForCheckCampaign == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Check notification campaign",
                    Seconds = 60,
                    Type = WebApiNotificationDefaults.CampaignSendTaskType,
                });
            }

            var messageTemplates = new List<WebApiNotificationTemplate>
            {
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.CUSTOMER_EMAIL_VALIDATION_NOTIFICATION,
                    Title = "%Store.Name%. Email validation",
                    Body = $"%Store.Name%, {Environment.NewLine}Check your email to activate your account. {Environment.NewLine}%Store.Name%",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.CUSTOMER_REGISTERED_WELCOME_NOTIFICATION,
                    Title = "Welcome to %Store.Name%",
                    Body = $"We welcome you to %Store.Name%.{Environment.NewLine}You can now take part in the various services we have to offer you. Some of these services include:{Environment.NewLine}Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.{Environment.NewLine}Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.{Environment.NewLine}Order History - View your history of purchases that you have made with us.{Environment.NewLine}Products Reviews - Share your opinions on products with our other customers.",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.CUSTOMER_WELCOME_NOTIFICATION,
                    Title = "Notification subscrition success",
                    Body = $"You have successfully subscribed for notification. {Environment.NewLine}%Store.Name%",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.CUSTOMER_REGISTERED_NOTIFICATION,
                    Title = "%Store.Name%. New customer registration",
                    Body = $"%Store.Name%, {Environment.NewLine}A new customer registered with your store. Below are the customer's details:{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}Email: %Customer.Email%.",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.ORDER_CANCELLED_CUSTOMER_NOTIFICATION,
                    Title = "%Store.Name%. Your order cancelled",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been cancelled. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.ORDER_COMPLETED_CUSTOMER_NOTIFICATION,
                    Title = "%Store.Name%. Your order completed",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been completed. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION,
                    Title = "Your order from %Store.Name% has been delivered.",
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Good news! You order has been delivered.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION,
                    Title = "Order receipt from %Store.Name%.",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order Number: %Order.OrderNumber%.",
                    Active = true,
                    ActionType = NotificationActionType.Order,
                    ActionValue = "%Order.OrderNumber%",
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.SHIPMENT_SENT_CUSTOMER_NOTIFICATION,
                    Title = "Your order from %Store.Name% has been shipped.",
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}Good news! You order has been shipped.{Environment.NewLine}Order Number: %Order.OrderNumber%",
                    Active = true,
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.ORDER_REFUNDED_CUSTOMER_NOTIFICATION,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.",
                    Active = false,
                    ActionType = NotificationActionType.Order,
                    ActionValue = "%Order.OrderNumber%",
                    SendImmediately = true
                },
                new WebApiNotificationTemplate
                {
                    Name = WebApiNotificationTemplateSystemNames.ORDER_PAID_CUSTOMER_NOTIFICATION,
                    Title = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been just paid. Order Number: %Order.OrderNumber%.",
                    Active = false,
                    ActionType = NotificationActionType.Order,
                    ActionValue = "%Order.OrderNumber%",
                    SendImmediately = true
                }
            };

            await _pushNotificationTemplateRepository.InsertAsync(messageTemplates);
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WebApiNotification/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new WebApiNotificationPermissionProvider());
            await InsertInitialDataAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new WebApiNotificationPermissionProvider());

            var scheduleTaskForNotification = await _scheduleTaskService.GetTaskByTypeAsync(WebApiNotificationDefaults.QueuedSendTaskType);
            var scheduleTaskForCheckCampaign = await _scheduleTaskService.GetTaskByTypeAsync(WebApiNotificationDefaults.CampaignSendTaskType);

            if (scheduleTaskForCheckCampaign != null)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTaskForCheckCampaign);
            if (scheduleTaskForNotification != null)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTaskForNotification);

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.Menu.WebApiNotification")
            };

            #region Campaign

            if (await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.Menu.Campaigns"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiNotificationCampaign/List",
                    SystemName = "WebApiNotificationCampaigns"
                };
                menu.ChildNodes.Add(campaign);
            }

            #endregion

            #region Template

            if (await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageTemplates))
            {
                var notificationTemplate = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.Menu.PushNotificationTemplates"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiNotificationTemplate/List",
                    SystemName = "WebApiNotificationTemplates"
                };
                menu.ChildNodes.Add(notificationTemplate);
            }

            #endregion

            #region Queued notification

            if (await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
            {
                var queue = new SiteMapNode
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.Menu.QueuedPushNotifications"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiQueuedNotification/List",
                    SystemName = "WebApiQueuedNotifications"
                };
                menu.ChildNodes.Add(queue);
            }

            #endregion

            #region Others

            if (await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiNotification/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.Menu.Configuration"),
                    SystemName = "WebApiNotifications"
                };
                menu.ChildNodes.Add(settings);
            }

            #endregion

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/app-push-notification-v2-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=app-push-notification-v2",
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
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.None", "None"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Product", "Product"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Category", "Category"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Manufacturer", "Manufacturer"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Vendor", "Vendor"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Order", "Order"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Topic", "Topic"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApiNotification.Domains.NotificationActionType.Account", "Account"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Menu.WebApiNotification", "Web api push notification"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Menu.PushNotificationTemplates", "Notification templates"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Menu.Campaigns", "Campaigns"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Menu.QueuedPushNotifications", "Notification queue"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.GoogleConsoleApiAccessKey", "Google console api access key"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.GoogleConsoleApiAccessKey.Hint", "The Google console api access key."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.ApplicationTypeId", "Application type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.ApplicationTypeId.Hint", "Select mobile application type."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitClientSecret", "Pushkit client secret"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitClientSecret.Hint", "Huawei pushkit client secret."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitClientId", "Pushkit client id"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitClientId.Hint", "Huawei pushkit client id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitAppId", "Pushkit app id"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitAppId.Hint", "Huawei pushkit app id. AppId can be found in console. Go to App Gallery Connect -> My projects -> YOUR_PROJECT -> Project settings."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.EnablePushKit", "Enable huawei pushkit"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration.Fields.EnablePushKit.Hint", "Check to enable huawei pushkit."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.Configuration", "Web api push notification settings"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.AddNew", "Add a new campaign"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.BackToList", "back to campaign list"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.EditDetails", "Edit campaign details"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List", "Campaigns"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Name.Hint", "The name for this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Title.Hint", "The title for this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Body.Hint", "The template body."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ImageId", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ImageId.Hint", "The template image."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.CreatedOn.Hint", "The date when the campaign was created."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.SendingWillStartOn", "Sending will start on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.SendingWillStartOn.Hint", "The date/time that the campaign will be sent."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.AddedToQueueOn", "Added to queue on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.AddedToQueueOn.Hint", "The date/time that the campaign was added to queue."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.CustomerRoles", "Limited to customer roles"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.CustomerRoles.Hint", "Option to limit this campaign to a certain customer role. If you have multiple customer role, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.DeviceTypes", "Limited to device types"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.DeviceTypes.Hint", "Option to limit this campaign to a certain device type. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.AllowedTokens", "Allowed notification tokens"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.AllowedTokens.Hint", "This is a list of the notification tokens you can use in your template"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ActionType", "Action type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ActionType.Hint", "It determines which page will open in mobile app onclick the notification. (i.e. Product details, Category details)"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ActionValue", "Value"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ActionValue.Hint", "It determines the value of action type. (i.e. Product id, Category id)"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.LimitedToStoreId", "Limited to store"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.LimitedToStoreId.Hint", "Choose a store which subscribers will get this notification."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.IconId.Required", "The 'Icon' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Title.Required", "The 'Title' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Body.Required", "The 'Body' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.SendingWillStartOn.Required", "The 'Sending will start on' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Name.Required", "The 'Name' is required."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.CopyCampaign", "Copy campaign"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy", "Copy campaign"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copied", "Campaign has been copied successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy.SendingWillStartOn", "Sending will start on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy.SendingWillStartOn.Hint", "The date/time that the new campaign will be sent."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy.Name.Hint", "The name for new campaign."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchKeyword", "Search keyword"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchKeyword.Hint", "Search campaign(s) by specific keywords."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchSendStartFromDate", "Send start from date"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchSendStartFromDate.Hint", "Search by send start from date."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchSendStartToDate", "Send start to date"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchSendStartToDate.Hint", "Search by send start from date."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.SendTestNotification", "Send test notification"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Created", "Campaign has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Updated", "Campaign has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Deleted", "Campaign has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Name.Hint", "The template name."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Title.Hint", "The template title."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Body.Hint", "The template body."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ImageId", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ImageId.Hint", "The template image."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Active.Hint", "Check to active this template."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.AllowedTokens", "Allowed notification tokens"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.AllowedTokens.Hint", "This is a list of the notification tokens you can use in your template"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.LimitedToStores", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.LimitedToStores.Hint", "Option to limit this notification to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.SendImmediately", "Send immediately"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.SendImmediately.Hint", "Send notification immediately."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend", "Delay before send"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend.Hint", "A delay before sending the notification."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.CreatedOn.Hint", "The date/time that the notification was created."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ActionType", "Action type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ActionType.Hint", "It determines which page will open in mobile app onclick the notification. (i.e. Product details, Category details)"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ActionValue", "Value"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ActionValue.Hint", "It determines the value of action type. (i.e. Product id, Category id)"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchKeyword", "Search keyword"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchKeyword.Hint", "Search template(s) by specific keywords."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId.Hint", "Search by a \"Active\" property."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchTemplateTypeId", "Template type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchTemplateTypeId.Hint", "Search by a template type."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Tabs.Info", "Info"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.AddNew", "Add a new template"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.BackToList", "back to template list"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.EditDetails", "Edit template details"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List", "Templates"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Title.Required", "The 'Title' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Name.Required", "The 'Name' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.IconId.Required", "The 'Icon' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend.Required", "The 'Delay before send' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend.GreaterThanZero", "The 'Delay before send' must be greater than zero."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId.Inactive", "Inactive"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Customer.Hint", "The customer."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Store", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Store.Hint", "A store name in which this notification was sent."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Title.Hint", "The notification title."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Body.Hint", "The notification body."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ImageUrl", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ImageUrl.Hint", "The notification image."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.CreatedOn.Hint", "The date/time that the notification was created."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentOn", "Sent on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentOn.Hint", "The date/time that the notification was sent."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DontSendBeforeDate", "Dont send before date"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DontSendBeforeDate.Hint", "Hint"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentOn.NotSent", "Not sent"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionType", "Action type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionType.Hint", "It determines which page will open in mobile app onclick the notification. (i.e. Product details, Category details)"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionValue", "Value"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionValue.Hint", "It determines the value of action type. (i.e. Product id, Category id)"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SubscriptionId", "Subscription id"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SubscriptionId.Hint", "The subscription id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SubscriptionId", "Subscription id"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SubscriptionId.Hint", "The subscription id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SendImmediately", "Send immediately"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SendImmediately.Hint", "Send notification immediately after adding into queue."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentTries", "Sent tries"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentTries.Hint", "Number of attempts to send this notification."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DeviceType", "Device type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DeviceType.Hint", "The type of the device."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.EditDetails", "Edit queued notification details"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.BackToList", "back to queued notification list"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Requeue", "Requeue"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.List", "Notification queue"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.ViewDetails", "View details"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Unknown", "Unknown"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Guest", "Guest"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Updated", "Push notification campaign has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.SendTestNotification", "Send test notification"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.SendTestNotification.Confirmation", "Are you sure want to send test notification?"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Deleted", "Queued push notification has been deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.TestNotification.Title", "Hello {0}"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.TestNotification.Guest", "Guest"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.TestNotification.Body", "This is a test notification from {0}."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.TestNotification.SentSuccessFully", "Test notification has been sent successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.TestNotification.SentUnSuccessFul", "Test notification has been sent failed."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApiNotification.QueuedPushNotifications.DeleteSent", "Delete (all sent)"),
                new KeyValuePair<string, string>("Account.Login.WrongCredentials.MultiFactorAuthenticationRequired", "Multi-factor authentication is required."),

            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>() { "admin_webapidevice_list_buttons" });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WebApiNotificationViewComponent);
        }

        #endregion
    }
}