using System.Threading.Tasks;
using Nop.Services.Logging;
using System.Net.Http;
using System;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace NopStation.Plugin.Misc.TinyPNG.Services
{
    public class TinyPNGService : ITinyPNGService
    {
        #region Field

        private readonly TinyPNGSettings _tinyPNGSettings;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        #endregion

        #region Ctr
        public TinyPNGService(ILogger logger,
            TinyPNGSettings tinyPNGSettings,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _tinyPNGSettings = tinyPNGSettings;
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Methods

        public virtual async Task<(byte[] imageByte, bool isCompressed)> TinifyImageAsync(byte[] sourceImg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_tinyPNGSettings.Keys))
                {
                    await _logger.ErrorAsync("Error while complessing using Tiny PNG : No key found.");
                    return (sourceImg, false);
                }

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _tinyPNGSettings.ApiUrl);
                httpRequestMessage.Headers.Add(HeaderNames.Accept, "application/json");
                httpRequestMessage.Headers.Add(HeaderNames.UserAgent, "HttpRequestsSample");
                httpRequestMessage.Headers.Add(HeaderNames.Authorization, "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_tinyPNGSettings.Keys)));
                httpRequestMessage.Content = new ByteArrayContent(sourceImg);

                var httpClient = _httpClientFactory.CreateClient();
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<TinyfyResponse>(httpResponseMessage.Content.ReadAsStringAsync().Result);
                    if (res != null)
                    {
                        //_logger.Information(JsonConvert.SerializeObject(res.output));
                        var compressedImgUrl = res.output.url.ToString();

                        byte[] imageBytes;
                        using (var webClient = new WebClient())
                        {
                            imageBytes = webClient.DownloadData(compressedImgUrl);
                        }

                        return (imageBytes, true);
                    }

                }
                else
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                        await _logger.ErrorAsync("Error while complessing using Tiny PNG : Credentials are invalid.");
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.UnsupportedMediaType)
                        await _logger.ErrorAsync("Error while complessing using Tiny PNG : File type is not supported.");
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                        await _logger.ErrorAsync("Error while complessing using Tiny PNG : Your monthly limit has been exceeded.");
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error while complessing using Tiny PNG", ex);
            }

            return (sourceImg, false);
        }

        #endregion
    }

    #region extension class

    public class TinyfyResponse
    {
        public TinyfyResponse()
        {
            output = new OutputResult();
        }
        public OutputResult output { get; set; }
    }

    public class OutputResult
    {
        public int size { get; set; }
        public string type { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public decimal ratio { get; set; }
        public string url { get; set; }
    }

    #endregion
}
