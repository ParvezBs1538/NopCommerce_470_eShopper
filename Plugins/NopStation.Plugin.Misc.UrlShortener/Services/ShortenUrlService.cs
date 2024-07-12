using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.UrlShortener.Domains;

namespace NopStation.Plugin.Misc.UrlShortener.Services
{
    public class ShortenUrlService : IShortenUrlService
    {
        private readonly IRepository<ShortenUrl> _shortenUrlRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ILocalizationService _localizationService;

        public ShortenUrlService(IRepository<ShortenUrl> shortenUrlRepository, IRepository<UrlRecord> urlRecordRepository,
            ILocalizationService localizationService)
        {
            this._shortenUrlRepository = shortenUrlRepository;
            _urlRecordRepository = urlRecordRepository;
            _localizationService = localizationService;
        }

        public async Task DeleteShortenUrl(ShortenUrl shortenUrl)
        {
            await _shortenUrlRepository.DeleteAsync(shortenUrl);
        }

        public async Task<IPagedList<ShortenUrl>> GetAllShortenUrls(string slug = "", int pageIndex = 0, int pageSize = int.MaxValue - 1)
        {
            var query = _shortenUrlRepository.Table.Where(x => !x.Deleted);

            if (!string.IsNullOrWhiteSpace(slug))
                query = query.Where(x => x.Slug.Contains(slug));

            var shrtenUrls = await query.OrderByDescending(x => x.Id).ToListAsync();

            return new PagedList<ShortenUrl>(shrtenUrls, pageIndex, pageSize);
        }

        public async Task<ShortenUrl> GetShortenUrlById(int shortenUrlId)
        {
            return await _shortenUrlRepository.GetByIdAsync(shortenUrlId);
        }

        public async Task<ShortenUrl> GetShortenUrlByUrlRecordId(int urlRecordId)
        {
            return await _shortenUrlRepository.Table.FirstOrDefaultAsync(x => x.UrlRecordId == urlRecordId);
        }

        public async Task<IList<ShortenUrl>> GetShortenUrlsByUrlRecordIds(List<int> urlRecordIds)
        {
            return await _shortenUrlRepository.Table.Where(x => urlRecordIds.Contains(x.UrlRecordId)).ToListAsync();
        }

        public async Task InsertShortenUrl(ShortenUrl shortenUrl)
        {
            await _shortenUrlRepository.InsertAsync(shortenUrl);
        }

        public async Task UpdateShortenUrl(ShortenUrl shortenUrl)
        {
            await _shortenUrlRepository.UpdateAsync(shortenUrl);
        }

        #region Url Record

        public virtual async Task<IPagedList<UrlRecord>> GetAllUrlRecordsAsync(
             string slug = "", string entityName = "", int? languageId = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var urlRecords = (await _urlRecordRepository.GetAllAsync(query =>
            {
                query = query.OrderBy(ur => ur.Slug);

                return query;
            }, cache => default)).AsQueryable();


            if (!string.IsNullOrWhiteSpace(slug))
                urlRecords = urlRecords.Where(ur => ur.Slug.Contains(slug));

            if (!string.IsNullOrWhiteSpace(entityName))
                urlRecords = urlRecords.Where(ur => ur.EntityName.Contains(entityName));

            if (languageId.HasValue)
                urlRecords = urlRecords.Where(ur => ur.LanguageId == languageId);

            if (isActive.HasValue)
                urlRecords = urlRecords.Where(ur => ur.IsActive == isActive);

            var result = urlRecords.ToList();

            return new PagedList<UrlRecord>(result, pageIndex, pageSize);
        }

        public async Task<IList<SelectListItem>> GetUrlEntityNames(string defaultItemText = null)
        {
            var items = await _urlRecordRepository.Table.GroupBy(x => x.EntityName).Select(x => new SelectListItem
            {
                Text = x.Key,
                Value = x.Key
            }).ToListAsync();

            //at now we use "0" as the default value
            const string value = "";

            //prepare item text
            defaultItemText = defaultItemText ?? await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
            return items;
        }

        #endregion
    }
}
