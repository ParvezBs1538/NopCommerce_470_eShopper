using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.ProgressiveWebApp;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;
using NopStation.Plugin.Widgets.PushNop.Domains;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop
{
    public class SmartNotificationSendTask : IScheduleTask
    {
        private readonly ISmartGroupNotificationService _smartGroupNotificationService;
        private readonly IStoreContext _storeContext;
        private readonly IPushNopDeviceService _pushNopDeviceService;
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ITokenizer _tokenizer;
        private readonly ILocalizationService _localizationService;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;

        public SmartNotificationSendTask(ISmartGroupNotificationService smartGroupNotificationService,
            IStoreContext storeContext,
            IPushNopDeviceService pushNopDeviceService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            ILanguageService languageService,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            IPictureService pictureService,
            ITokenizer tokenizer,
            ILocalizationService localizationService,
            IQueuedPushNotificationService queuedPushNotificationService,
            ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _smartGroupNotificationService = smartGroupNotificationService;
            _storeContext = storeContext;
            _pushNopDeviceService = pushNopDeviceService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _languageService = languageService;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _pictureService = pictureService;
            _tokenizer = tokenizer;
            _localizationService = localizationService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        #region Utilities

        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }

        #endregion

        public async Task ExecuteAsync()
        {
            var smartGroupNotifications = await _smartGroupNotificationService.GetAllSmartGroupNotificationsAsync(
                searchTo: DateTime.UtcNow,
                addedToQueueStatus: false,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

            for (var i = 0; i < smartGroupNotifications.Count; i++)
            {
                var smartGroupNotification = smartGroupNotifications[i];
                await SendSmartGroupNotificationAsync(smartGroupNotification);

                smartGroupNotification.AddedToQueueOnUtc = DateTime.UtcNow;
                await _smartGroupNotificationService.UpdateSmartGroupNotificationAsync(smartGroupNotification);
            }
        }

        public virtual async Task<IList<int>> SendSmartGroupNotificationAsync(SmartGroupNotification smartGroupNotification)
        {
            if (smartGroupNotification == null)
                throw new ArgumentNullException(nameof(smartGroupNotification));

            var i = 0;
            var ids = new List<int>();

            var store = await _storeService.GetStoreByIdAsync(smartGroupNotification.LimitedToStoreId);
            if (store == null)
                store = await _storeContext.GetCurrentStoreAsync();

            while (true)
            {
                var devices = await _pushNopDeviceService.GetCampaignDevicesAsync(smartGroupNotification, i, 100);
                if (!devices.Any())
                    break;

                foreach (var device in devices)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(device.CustomerId);
                    var languageId = customer == null ? 0 : customer?.LanguageId ?? 0;
                    languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

                    //tokens
                    var commonTokens = new List<Token>();
                    if (customer != null)
                        await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

                    var tokens = new List<Token>(commonTokens);
                    await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, store);

                    ids.Add(await SendNotificationAsync(device, smartGroupNotification, languageId, tokens, store.Id));
                }
                i++;
            }
            return ids;
        }

        public virtual async Task<int> SendNotificationAsync(WebAppDevice device, SmartGroupNotification smartGroupNotification,
            int languageId, IEnumerable<Token> tokens, int storeId)
        {
            if (smartGroupNotification == null)
                throw new ArgumentNullException(nameof(smartGroupNotification));

            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var title = await _localizationService.GetLocalizedAsync(smartGroupNotification, mt => mt.Title, languageId);
            var titleReplaced = _tokenizer.Replace(title, tokens, true);

            var body = await _localizationService.GetLocalizedAsync(smartGroupNotification, mt => mt.Body, languageId);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var url = smartGroupNotification.Url;
            var urlReplaced = !string.IsNullOrWhiteSpace(url) ? _tokenizer.Replace(url, tokens, true) : null;

            var iconUrl = "";
            if (smartGroupNotification.UseDefaultIcon)
                iconUrl = await _pictureService.GetPictureUrlAsync(_progressiveWebAppSettings.DefaultIconId, 80);
            else
                iconUrl = await _pictureService.GetPictureUrlAsync(smartGroupNotification.IconId, 80);

            var imageUrl = await _pictureService.GetPictureUrlAsync(smartGroupNotification.ImageId, showDefaultPicture: false);
            if (string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = null;

            var queuedPushNotification = new QueuedPushNotification
            {
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = device.CustomerId,
                IconUrl = iconUrl,
                StoreId = storeId,
                Title = title,
                ImageUrl = imageUrl,
                Url = url,
                Rtl = language.Rtl
            };

            await _queuedPushNotificationService.InsertQueuedPushNotificationAsync(queuedPushNotification);

            return queuedPushNotification.Id;
        }
    }
}
