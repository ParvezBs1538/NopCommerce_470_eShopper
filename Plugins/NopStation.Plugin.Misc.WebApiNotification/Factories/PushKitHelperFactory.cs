using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.WebApiNotification.Services.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Factories
{
    public class PushKitHelperFactory : IPushKitHelperFactory
    {
        private readonly ILogger _logger;
        private readonly WebApiNotificationSettings _webApiNotificationSettings;

        public PushKitHelperFactory(ILogger logger,
            WebApiNotificationSettings webApiNotificationSettings)
        {
            _logger = logger;
            _webApiNotificationSettings = webApiNotificationSettings;
        }

        public async Task<string> RequestAccessTokenAsync(CancellationToken cancellationToken)
        {
            var content = string.Format("grant_type=client_credentials&client_secret={0}&client_id={1}",
                    _webApiNotificationSettings.PushKitClientSecret, _webApiNotificationSettings.PushKitClientId);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(WebApiNotificationDefaults.HuaweiOauthRequestUri),
                Content = new StringContent(content),
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(WebApiNotificationDefaults.HuaweiRequestMediaTypeHeader);

            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = $"Response status code does not indicate success: {response.StatusCode}";
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var parsed = JsonConvert.DeserializeObject<TokenResponse>(json);
            switch (parsed.Error)
            {
                case 1101:
                    throw new Exception("Get access token failed: invalid request");
                case 1102:
                    throw new Exception("Get access token failed: missing required param");
                case 1104:
                    throw new Exception("Get access token failed: unsupported response type");
                case 1105:
                    throw new Exception("Get access token failed: unsupported grant type");
                case 1107:
                    throw new Exception("Get access token failed: access denied");
                case 1201:
                    throw new Exception("Get access token failed: invalid ticket");
                case 1202:
                    throw new Exception("Get access token failed: invalid sso_st");
                default:
                    break;
            }

            var token = parsed.GetValidAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                var error = "AccessToken return by push kit is null or empty";
                throw new Exception(error);
            }

            return token;
        }
    }
}
