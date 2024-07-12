using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.CrawlerManager.Domain;

namespace NopStation.Plugin.Widgets.CrawlerManager.Services
{
    public class CrawlerService : ICrawlerService
    {
        #region Fields

        private readonly IRepository<Crawler> _crawlerRepository;

        #endregion

        #region Ctors
        public CrawlerService(IRepository<Crawler> crawlerRepository)
        {
            _crawlerRepository = crawlerRepository;
        }

        #endregion

        #region Methods

        public virtual async Task AddOrUpdateCrawlerAsync(Crawler crawler)
        {
            if (crawler == null)
                throw new ArgumentNullException();

            crawler.AddedOnUtc = DateTime.UtcNow;

            await _crawlerRepository.InsertAsync(crawler);
        }

        public virtual async Task<IPagedList<Crawler>> GetCrawlersAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _crawlerRepository.Table;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
