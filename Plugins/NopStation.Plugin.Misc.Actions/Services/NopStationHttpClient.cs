using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public partial class NopStationHttpClient
{
    private const string INSTALLATION_URL = "https://www.nop-station.com/plugin-installation/";
    private const string UNINSTALLATION_URL = "https://www.nop-station.com/plugin-uninstallation/";

    #region Fields

    private readonly HttpClient _httpClient;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public NopStationHttpClient(HttpClient client,
        IWebHelper webHelper)
    {
        //configure client
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CURRENT_VERSION}");
        client.DefaultRequestHeaders.Add("X-Version", "1");

        _httpClient = client;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    private async Task RequestNopStationAsync(PluginDescriptor plugin, string url)
    {
        try
        {
            var requestContent = new StringContent($"url={_webHelper.GetStoreLocation()}&product={plugin.SystemName}&version={plugin.Version}",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStringAsync();
        }
        catch
        {
        }
    }

    public async Task OnInstallPluginAsync(PluginDescriptor plugin)
    {
        await RequestNopStationAsync(plugin, INSTALLATION_URL);
    }

    public async Task OnUninstallPluginAsync(PluginDescriptor plugin)
    {
        await RequestNopStationAsync(plugin, UNINSTALLATION_URL);
    }

    #endregion
}
