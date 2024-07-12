using System.Threading.Tasks;
using Nop.Core.Domain.Seo;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Generate;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Shortenurls;
using NopStation.Plugin.Misc.UrlShortener.Domains;

namespace NopStation.Plugin.Misc.UrlShortener.Factories
{
    public interface IShortenUrlModelFactory
    {
        Task<ShortenUrlSearchModel> PrepareShortenUrlSearchModel(ShortenUrlSearchModel searchModel);

        Task<ShortenUrlListModel> PrepareShortenUrlListModel(ShortenUrlSearchModel searchModel);

        Task<SuccessResponseModel> GenerateShortUrls(GenerateShortUrlModel model, bool generateAll = false);

        Task GenerateShortUrl(UrlRecord record, ShortenUrl shortenUrl);
    }
}