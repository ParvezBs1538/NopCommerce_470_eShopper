using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public class CategoryIconService : ICategoryIconService
    {
        #region Fields

        private readonly IRepository<ApiCategoryIcon> _categoryIconRepository;

        #endregion

        #region Ctor

        public CategoryIconService(IRepository<ApiCategoryIcon> categoryIconRepository)
        {
            _categoryIconRepository = categoryIconRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteCategoryIconAsync(ApiCategoryIcon categoryIcon)
        {
            if (categoryIcon == null)
                throw new ArgumentNullException(nameof(categoryIcon));

            await _categoryIconRepository.DeleteAsync(categoryIcon);
        }

        public async Task InsertCategoryIconAsync(ApiCategoryIcon categoryIcon)
        {
            if (categoryIcon == null)
                throw new ArgumentNullException(nameof(categoryIcon));

            await _categoryIconRepository.InsertAsync(categoryIcon);
        }

        public async Task UpdateCategoryIconAsync(ApiCategoryIcon categoryIcon)
        {
            if (categoryIcon == null)
                throw new ArgumentNullException(nameof(categoryIcon));

            await _categoryIconRepository.UpdateAsync(categoryIcon);
        }

        public async Task<ApiCategoryIcon> GetCategoryIconByIdAsync(int categoryIconId)
        {
            if (categoryIconId == 0)
                return null;

            return await _categoryIconRepository.GetByIdAsync(categoryIconId, cache => default);
        }

        public async Task<ApiCategoryIcon> GetCategoryIconByCategoryIdAsync(int categoryId)
        {
            if (categoryId == 0)
                return null;

            return await _categoryIconRepository.Table.FirstOrDefaultAsync(x => x.CategoryId == categoryId);
        }

        public async Task<IPagedList<ApiCategoryIcon>> GetAllCategoryIconsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryIcons = _categoryIconRepository.Table;

            categoryIcons = categoryIcons.OrderByDescending(e => e.Id);

            return await categoryIcons.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<ApiCategoryIcon>> GetCategoryIconByIdsAsync(int[] categoryIconIds)
        {
            if (categoryIconIds == null || categoryIconIds.Length == 0)
                return new List<ApiCategoryIcon>();

            var query = _categoryIconRepository.Table.Where(x => categoryIconIds.Contains(x.Id));

            return await query.ToListAsync();
        }

        public async Task DeleteCategoryIconsAsync(List<ApiCategoryIcon> categoryIcons)
        {
            if (categoryIcons == null)
                throw new ArgumentNullException(nameof(categoryIcons));

            await _categoryIconRepository.DeleteAsync(categoryIcons);
        }

        #endregion
    }
}
