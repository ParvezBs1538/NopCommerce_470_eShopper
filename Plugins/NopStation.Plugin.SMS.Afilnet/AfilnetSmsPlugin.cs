using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.SMS.Afilnet.Domains;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Menu;
using System.Threading.Tasks;
using NopStation.Plugin.SMS.Afilnet.Services;

namespace NopStation.Plugin.SMS.Afilnet
{
    public class AfilnetSmsPlugin : BasePlugin, IAdminMenuPlugin, ISmsPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<SmsTemplate> _smsTemplateRepository;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ISmsSender _smsSender;

        #endregion

        #region Ctor

        public AfilnetSmsPlugin(IPermissionService permissionService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IRepository<SmsTemplate> smsTemplateRepository,
            IScheduleTaskService scheduleTaskService,
            INopStationCoreService nopStationCoreService,
            ICustomerService customerService,
            IWorkContext workContext,
            ISmsSender smsSender)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _smsTemplateRepository = smsTemplateRepository;
            _scheduleTaskService = scheduleTaskService;
            _nopStationCoreService = nopStationCoreService;
            _customerService = customerService;
            _workContext = workContext;
            _smsSender = smsSender;
        }

        #endregion

        #region Utilities

        protected async Task CreateSampleDataAsync()
        {
            await InsertTemplatesAsync();

            var task = await _scheduleTaskService.GetTaskByTypeAsync(SmsPluginDefaults.QueuedSmsSendTaskType);
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Send SMS",
                    Seconds = 60,
                    Type = SmsPluginDefaults.QueuedSmsSendTaskType,
                });
            }
        }

        protected async Task InsertTemplatesAsync()
        {
            var messageTemplates = new List<SmsTemplate>
            {
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.CUSTOMER_EMAIL_VALIDATION_MESSAGE,
                    Body = $"%Store.Name%, {Environment.NewLine}Check your email to activate your account. {Environment.NewLine}%Store.Name%",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.PRIVATE_MESSAGE_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}You have received a new private message.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.CUSTOMER_WELCOME_MESSAGE,
                    Body = $"We welcome you to %Store.Name%.{Environment.NewLine}You can now take part in the various services we have to offer you. Some of these services include:{Environment.NewLine}Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.{Environment.NewLine}Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.{Environment.NewLine}Order History - View your history of purchases that you have made with us.{Environment.NewLine}Products Reviews - Share your opinions on products with our other customers.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.NEW_FORUM_POST_MESSAGE,
                    Body = $"%Store.Name%, {Environment.NewLine}A new post has been created in the topic %Forums.TopicName% at %Forums.ForumName% forum.{Environment.NewLine}Click here for more info.{Environment.NewLine}Post author: %Forums.PostAuthor%{Environment.NewLine}Post body: %Forums.PostBody%",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.NEW_FORUM_TOPIC_MESSAGE,
                    Body = $"%Store.Name%, {Environment.NewLine}A new topic %Forums.TopicName% has been created at %Forums.ForumName% forum.{Environment.NewLine}Click here for more info.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.CUSTOMER_REGISTERED_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}A new customer registered with your store. Below are the customer's details:{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}Email: %Customer.Email%.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_CANCELLED_CUSTOMER_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been cancelled. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_COMPLETED_CUSTOMER_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been completed. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION,
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Good news! You order has been delivered.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_OTP_NOTIFICATION,
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Here is your OTP code: %Shipment.OTP%",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order Number: %Order.OrderNumber%.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_PLACED_ADMIN_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store.",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.SHIPMENT_SENT_CUSTOMER_NOTIFICATION,
                    Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}Good news! You order has been shipped.{Environment.NewLine}Order Number: %Order.OrderNumber%",
                    Active = true,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_PLACED_VENDOR_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}%Customer.FullName% (%Customer.Email%) has just placed an order.{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}.",
                    Active = false,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_REFUNDED_CUSTOMER_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.",
                    //this template is disabled by default
                    Active = false,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_REFUNDED_ADMIN_NOTIFICATION,
                    Body = $"%Store.Name%. Order #%Order.OrderNumber% refunded', %Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just refunded{Environment.NewLine}Amount refunded: %Order.AmountRefunded%{Environment.NewLine}Date Ordered: %Order.CreatedOn%.",
                    //this template is disabled by default
                    Active = false,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_PAID_ADMIN_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just paid{Environment.NewLine}Date Ordered: %Order.CreatedOn%.",
                    //this template is disabled by default
                    Active = false,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_PAID_CUSTOMER_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been just paid. Order Number: %Order.OrderNumber%.",
                    //this template is disabled by default
                    Active = false,
                },
                new SmsTemplate
                {
                    Name = SmsTemplateSystemNames.ORDER_PAID_VENDOR_NOTIFICATION,
                    Body = $"%Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just paid.{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}.",
                    //this template is disabled by default
                    Active = false,
                }
            };
            await _smsTemplateRepository.InsertAsync(messageTemplates);
        }

        #endregion

        #region Methods

        public async Task SendSmsAsync(string phoneNumber, string messageBoby)
        {
            await _smsSender.SendNotificationAsync(phoneNumber, messageBoby);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AfilnetSms/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AfilnetPermissionProvider());
            await CreateSampleDataAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            var task = await _scheduleTaskService.GetTaskByTypeAsync(SmsPluginDefaults.QueuedSmsSendTaskType);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            await this.UninstallPluginAsync(new AfilnetPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Afilnet.Menu.Afilnet")
            };

            if (await _permissionService.AuthorizeAsync(AfilnetPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AfilnetSms/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Afilnet.Menu.Configuration"),
                    SystemName = "Afilnet.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }
            if (await _permissionService.AuthorizeAsync(AfilnetPermissionProvider.ManageTemplates))
            {
                var template = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AfilnetSmsTemplate/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Afilnet.Menu.SmsTemplates"),
                    SystemName = "Afilnet.SmsTemplates"
                };
                menu.ChildNodes.Add(template);
            }
            if (await _permissionService.AuthorizeAsync(AfilnetPermissionProvider.ManageQueuedSms))
            {
                var queued = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/QueuedAfilnetSms/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Afilnet.Menu.QueuedSmss"),
                    SystemName = "Afilnet.QueuedSmss"
                };
                menu.ChildNodes.Add(queued);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/afilnet-sms-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=afilnet-sms",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }
            
            if (menu.ChildNodes.Any())
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Menu.Afilnet", "Afilnet SMS"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Menu.SmsTemplates", "SMS templates"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Menu.QueuedSmss", "Queued SMS"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SelectTemplate", "Select template"),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Title", "Afilnet settings"),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.Username", "Username"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.Username.Hint", "Afilnet account username."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.Password", "Password"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.Password.Hint", "Afilnet account password."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.From", "From"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.From.Hint", "Define from."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.EnableLog", "Enable log"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.EnableLog.Hint", "Check to enable log."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.PhoneNumberRegex.Hint", "Enter phone number regular expression."),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.SendTestSmsTo", "Send SMS to"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.SendTestSmsTo.Hint", "Send test SMS to ensure that everything is properly configured."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.SendTestSms.Button", "Send test SMS"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.SendTestSms.Success", "SMS has been successfully sent."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.SendTestSms", "Send Test SMS (save settings first by clicking \"Save\" button)"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.SendTestSms.Body", "SMS works fine."),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.List", "SMS templates"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.EditDetails", "Edit template"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.BackToList", "back to template list"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Updated", "SMS template has been updated successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Name.Hint", "The SMS template name."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Body.Hint", "The SMS template body. (HTML is not supported)"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Active.Hint", "Check to active notification template."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.AclCustomerRoles", "Limited to customer roles"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.AclCustomerRoles.Hint", "Select customer roles for which the template will be worked. Leave empty if you want this template to be workable to all users."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.LimitedToStores", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.LimitedToStores.Hint", "Option to limit this template to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.SmsTemplates.Fields.Body.Required", "The 'SMS template body' is required."),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.List", "Queued SMS"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.BackToList", "back to queue list"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.ViewDetails", "SMS details"),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.PhoneNumber", "Phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.PhoneNumber.Hint", "The phone number."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.SentTries", "SentTries"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.SentTries.Hint", "Number of times tries to send SMS."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Error", "Error"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Error.Hint", "The error message."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Customer.Hint", "The customer."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Store", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Store.Hint", "The store."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.Body.Hint", "The notification body."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.CreatedOn.Hint", "The date when SMS was created."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.SentOn", "Sent on"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Fields.SentOn.Hint", "The date when SMS was sent."),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Deleted", "Queued SMS deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.QueuedSmss.Unknown", "Unknown"),

                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.CheckPhoneNumberRegex", "Check phone number reg-ex"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.CheckPhoneNumberRegex.Hint", "Determine whether phone number is checked by regular expression."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.CheckIntlDialCode", "Check intl. dial code"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.CheckIntlDialCode.Hint", "Determine whether phone number is checked by intl dial code."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.IntlDialCode", "Intl. dial code"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.IntlDialCode.Hint", "Enter intl. dial code."),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.RemoveFirstNDigitsWhenLocalNumber", "Remove first N' digits when local number"),
                new KeyValuePair<string, string>("Admin.NopStation.Afilnet.Configuration.Fields.RemoveFirstNDigitsWhenLocalNumber.Hint", "Set first N' digits to remove for local number."),
            };

            return list;
        }

        #endregion
    }
}