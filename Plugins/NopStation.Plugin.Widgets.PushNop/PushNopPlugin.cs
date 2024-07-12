using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Widgets.PushNop
{
    public class PushNopPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public PushNopPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ProgressiveWebApp/Configure";
        }

        public override async Task InstallAsync()
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(PushNopDefaults.SmartNotificationSendTaskType);
            if (scheduleTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Check notification campaign",
                    Seconds = 60,
                    Type = PushNopDefaults.SmartNotificationSendTaskType
                });
            }

            await this.InstallPluginAsync(new PushNopPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(PushNopDefaults.SmartNotificationSendTaskType);
            if (scheduleTask != null)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

            await this.UninstallPluginAsync(new PushNopPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.Menu.PushNop")
            };

            #region Dashboard

            if (await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageReports))
            {
                var dashboard = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/PushNop/Index",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.Menu.Dashboard"),
                    SystemName = "PushNop.Dashboard"
                };
                menu.ChildNodes.Add(dashboard);
            }

            #endregion

            #region Campaigns

            if (await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.Menu.GroupNotifications"),
                    Url = "~/Admin/PushNopGroupNotification/List",
                    SystemName = "PushNop.SmartGroupNotifications"
                };
                menu.ChildNodes.Add(campaign);
            }

            #endregion

            #region Smart group

            if (await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
            {
                var smartGroup = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/PushNopSmartGroup/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.Menu.SmartGroups"),
                    SystemName = "PushNop.SmartGroups"
                };
                menu.ChildNodes.Add(smartGroup);
            }

            #endregion

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/push-nop-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=push-nop",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Menu.PushNop", "Push-nop"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Menu.Dashboard", "Dashboard"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Menu.SmartGroups", "Smart groups"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Menu.GroupNotifications", "Group notifications"),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Name.Hint", "The name for this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SendToAll", "Send to all"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SendToAll.Hint", "Determines whether it will be sent to all subscribers or not."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SmartGroup", "Smart group"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SmartGroup.Hint", "The smart group for this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.PushNotificationTemplate", "Notification template"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.PushNotificationTemplate.Hint", "The push notification template for this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.CreatedOn.Hint", "The date when the campaign was created."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SendingWillStartOn", "Sending will start on"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SendingWillStartOn.Hint", "The date/time that the campaign will be sent."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.AddedToQueueOn", "Added to queue on"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.AddedToQueueOn.Hint", "The date/time that the campaign was added to queue."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.AddNew", "Add a new group notification"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.BackToList", "back to group notification list"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.EditDetails", "Edit group notification details"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.AreYouSure", "Are you sure want to perform this action?"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.SendNow", "Send now"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.List", "Group notifications"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SmartGroup.Required", "The 'Smart group' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Name.Required", "The 'Name' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.SendingWillStartOn.Required", "The 'Sending will start on' is required."),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.Value", "Value"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.Value.Hint", "The condition value."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.ConditionColumnType", "Column type"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.ConditionColumnType.Hint", "The condition column type."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.ConditionType", "Condition type"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.ConditionType.Hint", "The condition type."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.LogicType", "Logic type"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.LogicType.Hint", "the logic type."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.SmartGroup", "Smart group"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Fields.SmartGroup.Hint", "Smart group."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Combinations.AddNew", "Add a new combination"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.AddNew", "Add a new condition"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.BackToSmartGroup", "back to smart group"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.EditDetails", "Edit condition details"),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Conditions.SaveBeforeEdit", "You need to save the smart group before you can add conditions for this smart group page."),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Fields.Name.Hint", "The smart group name."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Fields.CreatedOn.Hint", "The date/time that the smart group was created."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.AddNew", "Add a new smart group"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.BackToList", "back to smart group list"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.EditDetails", "Edit smart group details"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.BackToList", "back to smart group list"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.List", "Smart groups"),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Conditions", "Conditions"),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Dashboard", "Push nop dashboard"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Dashboard.List", "Latest group notifications"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationTemplates.Updated", "Push Notification Campaign has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Created", "Smart group has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Updated", "Smart group has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Deleted", "Smart group has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationDashboard.WebAppDeviceInfo", "Subscribers info"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationDashboard.NumberOfNewSubscribersByWeek", "Subscribers last week"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationDashboard.NumberOfNewSubscribersByMonth", "Subscribers last month"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationDashboard.NumberOfNewSubscribersByYear", "Subscribers last year"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationDashboard.NumberOfNewSubscribersByDay", "Subscribers last day"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationTemplates.Dashboard.MoreInfo", "More info"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.WebAppDevices.ViewDetails", "View details"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.WebAppDevices.BackToList", "back to subscription list"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.WebAppDevices.SendTestNotification", "Send test notification"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.WebAppDevices.List", "Subscriptions"),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.WebAppDevices.LatestSubscriberList", "Latest subscribers"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationDashboard.MoreInfo", "More info"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Title.Hint", "The title for this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Tabs.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Tabs.ViewAs", "View as"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.AllowedTokens", "Allowed notification tokens"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.AllowedTokens.Hint", "This is a list of the notification tokens you can use in your template"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Body.Hint", "The template body."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.UseDefaultIcon", "Default icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.UseDefaultIcon.Hint", "Check to use default icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.IconId", "Icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.IconId.Hint", "The template icon."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.ImageId", "Image"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.ImageId.Hint", "The template image."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Url", "Url"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Url.Hint", "The template url."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.LimitedToStoreId", "Limited to store"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.LimitedToStoreId.Hint", "Choose a store which subscribers will get this notification."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Created", "SmartGroupCondition has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Configuration.Fields.BadgeIconId", "Badge icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Configuration.Fields.BadgeIconId.Hint", "The notification badge icon"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.IconId.Required", "The 'Icon' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Title.Required", "The 'Title' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Body.Required", "The 'Body' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.Fields.Name.Required", "The 'Name' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Created", "Smart group condition has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Updated", "Smart group condition has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroupConditions.Deleted", "Smart group condition has been deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Warning.AllowNotification", "Allow send notification for this site."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.SendTestNotification", "Send test notification"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.CopyCampaign", "Copy campaign"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Copy", "Copy campaign"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Copied", "Campaign has been copied successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Copy.SendingWillStartOn", "Sending will start on"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Copy.SendingWillStartOn.Hint", "The date/time that the new campaign will be sent."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Copy.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Copy.Name.Hint", "The name for new campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Subscriptions", "Subscriptions"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Fields.Subscriptions.Hint", "Total number of subscriptions."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Created", "Campaign has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Updated", "Campaign has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.GroupNotifications.Deleted", "Campaign has been deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationTemplates.Fields.SentTries", "Sent attempts"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.PushNotificationTemplates.Fields.SentTries.Hint", "The number of times to attempt to send this notification."),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.SmartGroups.AlreadyInUse", "Smart group is already in use."),

                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Dashboard.CommonStatistics", "Common statistics"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Dashboard.NumberOfSubscribers", "Number of subscribers"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Dashboard.NumberOfNotifications", "Number of notifications"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Dashboard.NumberOfCampaignsSent", "Number of campaigns sent"),
                new KeyValuePair<string, string>("Admin.NopStation.PushNop.Dashboard.NumberOfCampaignsScheduled", "Latest campaigns scheduled")
            };

            return list;
        }
    }
}