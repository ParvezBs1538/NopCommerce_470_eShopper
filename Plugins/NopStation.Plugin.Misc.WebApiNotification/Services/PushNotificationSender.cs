using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Factories;
using NopStation.Plugin.Misc.WebApiNotification.Services.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public class WebApiNotificationSender : IPushNotificationSender
    {
        private readonly ILogger _logger;
        private readonly WebApiNotificationSettings _webApiNotificationSettings;
        private readonly IPushKitHelperFactory _pushKitHelperFactory;

        public WebApiNotificationSender(ILogger logger,
            WebApiNotificationSettings webApiNotificationSettings,
            IPushKitHelperFactory pushKitHelperFactory)
        {
            _logger = logger;
            _webApiNotificationSettings = webApiNotificationSettings;
            _pushKitHelperFactory = pushKitHelperFactory;
        }

        private NotificationBaseModel PrepareHuaweiNotificationModel(string title, string body, string subscriptionId, int actionTypeId = 0, 
            string actionValue = "", string imageUrl = "")
        {
            var dataModel = new HuaweiNotificationDataModel()
            {
                ActionType = actionTypeId.ToString(),
                ActionValue = actionValue,
                BigPicture = imageUrl,
                Body = body,
                Title = title
            };
            var huaweiNotificationModel = new HuaweiNotificationModel()
            {
                Message = new Message
                {
                    Data = JsonConvert.SerializeObject(dataModel).ToString(),
                    Token = new List<string> { subscriptionId }
                },
                ValidateOnly = false
            };

            return huaweiNotificationModel;
        }

        private NotificationBaseModel PrepareNotificationModel(DeviceType deviceType, string title,
            string body, string subscriptionId, int actionTypeId = 0, string actionValue = "", string imageUrl = "")
        {
            if (_webApiNotificationSettings.ApplicationTypeId == (int)ApplicationType.Native)
            {
                if (deviceType == DeviceType.IPhone)
                {
                    var model = new IOsNotificationModel()
                    {
                        SubscriptionId = subscriptionId,
                        Data = new IOsNotificationModel.DataModel()
                        {
                            ActionType = actionTypeId,
                            ActionValue = actionValue,
                            ImageUrl = imageUrl
                        },
                        Notification = new IOsNotificationModel.NotificationModel()
                        {
                            Body = body,
                            Title = title,
                            MutableContent = true
                        }
                    };

                    return model;
                }
                else if (deviceType == DeviceType.Huawei)
                {
                    return PrepareHuaweiNotificationModel(title, body, subscriptionId, actionTypeId, actionValue, imageUrl);
                }
                else if (deviceType == DeviceType.Android)
                {
                    var model = new AndroidNotificationModel()
                    {
                        SubscriptionId = subscriptionId,
                        Data = new AndroidNotificationModel.DataModel()
                        {
                            ActionType = actionTypeId,
                            ActionValue = actionValue,
                            BigPicture = imageUrl,
                            Body = body,
                            Title = title
                        }
                    };

                    return model;
                }
            }
            else
            {
                if (deviceType == DeviceType.Huawei)
                {
                    return PrepareHuaweiNotificationModel(title, body, subscriptionId, actionTypeId, actionValue, imageUrl);
                }

                var model = new IonicNotificationModel()
                {
                    SubscriptionId = subscriptionId,
                    Data = new IonicNotificationModel.DataModel()
                    {
                        ActionType = actionTypeId,
                        ActionValue = actionValue,
                        ImageUrl = imageUrl,
                        NotificationForeground = "true",
                        Body = body,
                        Title = title
                    },
                    Notification = new IonicNotificationModel.NotificationModel()
                    {
                        Body = body,
                        Title = title,
                        Image = imageUrl
                    }
                };

                return model;
            }

            throw new NotImplementedException();
        }

        public async Task<bool> SendNotification(WebApiQueuedNotification notification)
        {
            return await SendNotification(notification.DeviceType, notification.Title, notification.Body,
                notification.SubscriptionId, notification.ActionTypeId, notification.ActionValue,
                notification.ImageUrl);
        }

        public async Task<bool> SendNotification(DeviceType deviceType, string title, string body,
            string subscriptionId, int actionTypeId = 0, string actionValue = "", string imageUrl = "")
        {
            try
            {
                var notificationModel = PrepareNotificationModel(deviceType, title, body, subscriptionId,
                    actionTypeId, actionValue, imageUrl);

                var data = JsonConvert.SerializeObject(notificationModel);
                var result = new HttpResponseMessage();
                if (deviceType == DeviceType.Huawei)
                {
                    var token = await _pushKitHelperFactory.RequestAccessTokenAsync(default(CancellationToken));
                    if (string.IsNullOrEmpty(token))
                    {
                        var error = "AccessToken return by push kit is null or empty";
                        throw new Exception(error);
                    }
                    result = await SendPushKitNotificationAsync(data, token, _webApiNotificationSettings.PushKitAppId);
                }
                else
                {
                    var token = _webApiNotificationSettings.GoogleConsoleApiAccessKey;
                    result = await SendFirebaseNotificationAsync(data, token);
                }

                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Push notification send error for: " + title, ex);
                return false;
            }
        }

        private static async Task<HttpResponseMessage> SendFirebaseNotificationAsync(string data, string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            HttpContent httpContent = new StringContent(data, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", httpContent);
        }

        private static async Task<HttpResponseMessage> SendPushKitNotificationAsync(string data, string token, string appId)
        {
            var httpClient = new HttpClient();
            var apiBaseUri = $"{WebApiNotificationDefaults.HuaweiPushAPIUri}/{appId}/messages:send";
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiBaseUri),
                Content = content,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request, default(CancellationToken)).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonConvert.DeserializeObject<SingleMessageResponse>(json);

            if (parsed.Code != WebApiNotificationDefaults.HUAWEI_PUSH_SUCCESS_CODE)
            {
                throw new Exception($"Send message failed: {parsed.Code}{Environment.NewLine}{parsed.Message}");
            }
            return response;
        }
    }
}
