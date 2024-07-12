using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Seo;
using NopStation.Plugin.Misc.UrlShortener.Domains;

namespace NopStation.Plugin.Misc.UrlShortener.Services
{
    public interface IShortenUrlService
    {
        Task InsertShortenUrl(ShortenUrl shortenUrl);

        Task UpdateShortenUrl(ShortenUrl shortenUrl);

        Task DeleteShortenUrl(ShortenUrl shortenUrl);

        Task<ShortenUrl> GetShortenUrlById(int shortenUrlId);

        Task<ShortenUrl> GetShortenUrlByUrlRecordId(int urlRecordId);

        Task<IList<ShortenUrl>> GetShortenUrlsByUrlRecordIds(List<int> urlRecordIds);

        Task<IPagedList<ShortenUrl>> GetAllShortenUrls(string slug = "", int pageIndex = 0, int pageSize = int.MaxValue - 1);

        Task<IPagedList<UrlRecord>> GetAllUrlRecordsAsync(
             string slug = "", string entityName = "", int? languageId = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<SelectListItem>> GetUrlEntityNames(string defaultItemText = null);
    }
}