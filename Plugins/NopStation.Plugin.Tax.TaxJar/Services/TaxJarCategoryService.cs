using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Tax;
using Nop.Data;
using NopStation.Plugin.Tax.TaxJar.Domains;
using NopStation.Plugin.Tax.TaxJar.Services.Cache;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public class TaxJarCategoryService : ITaxJarCategoryService
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<TaxJarCategory> _taxJarCategoryRepository;

        public TaxJarCategoryService(IStaticCacheManager cacheManager,
            IRepository<TaxCategory> taxCategoryRepository,
            IRepository<TaxJarCategory> taxJarCategoryRepository)
        {
            _cacheManager = cacheManager;
            _taxCategoryRepository = taxCategoryRepository;
            _taxJarCategoryRepository = taxJarCategoryRepository;
        }

        public async Task InsertTaxJarCategoryAsync(TaxJarCategory taxJarCategory)
        {
            await _taxJarCategoryRepository.InsertAsync(taxJarCategory);
        }

        public async Task UpdateTaxJarCategoryAsync(TaxJarCategory taxJarCategory)
        {
            await _taxJarCategoryRepository.UpdateAsync(taxJarCategory);
        }

        public async Task DeleteTaxJarCategoryAsync(TaxJarCategory taxJarCategory)
        {
            await _taxJarCategoryRepository.DeleteAsync(taxJarCategory);
        }

        public async Task<TaxJarCategory> GetTaxJarCategoryByValueAsync(string value)
        {
            var cacheKey = _cacheManager.PrepareKey(TaxJarCacheDefaults.CategoryByValueKey, value);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                return await _taxJarCategoryRepository.Table.FirstOrDefaultAsync(x => x.TaxCode == value);
            });
        }

        public async Task<TaxJarCategory> GetTaxJarCategoryByTaxCategoryIdAsync(int taxCategoryId)
        {
            var cacheKey = _cacheManager.PrepareKey(TaxJarCacheDefaults.CategoryByCategoryIdKey, taxCategoryId);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                return await _taxJarCategoryRepository.Table.FirstOrDefaultAsync(x => x.TaxCategoryId == taxCategoryId);
            });
        }

        public async Task<IPagedList<TaxJarCategory>> GetTaxJarCategoriesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _cacheManager.PrepareKey(TaxJarCacheDefaults.CategoryListKey, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from tjc in _taxJarCategoryRepository.Table select tjc;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task<IPagedList<TaxJarFormattedCategory>> GetTaxJarFormattedCategoriesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _cacheManager.PrepareKey(TaxJarCacheDefaults.CategoryFormattedListKey, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from tjc in _taxJarCategoryRepository.Table
                            join tc in _taxCategoryRepository.Table on tjc.TaxCategoryId equals tc.Id
                            orderby tc.Name
                            select new TaxJarFormattedCategory()
                            {
                                Name = tc.Name,
                                TaxCategoryId = tc.Id,
                                Value = tjc.TaxCode,
                                DisplayOrder = tc.DisplayOrder
                            };

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }
    }
}
