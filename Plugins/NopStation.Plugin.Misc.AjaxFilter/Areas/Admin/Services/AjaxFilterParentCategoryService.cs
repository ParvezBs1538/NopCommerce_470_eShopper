using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services
{
    public class AjaxFilterParentCategoryService : IAjaxFilterParentCategoryService
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<AjaxFilterParentCategory> _ajaxFilterParentCategoryRepository;
        #endregion

        #region Ctor 

        public AjaxFilterParentCategoryService(
            IStoreContext storeContext,
            IWorkContext workContext,
            ICustomerService customerService,
            IStaticCacheManager staticCacheManager,
            IRepository<Category> categoryRepository,
            IRepository<AjaxFilterParentCategory> ajaxFilterParentCategoryRepository)
        {
            _storeContext = storeContext;
            _workContext = workContext;
            _customerService = customerService;
            _staticCacheManager = staticCacheManager;
            _categoryRepository = categoryRepository;
            _ajaxFilterParentCategoryRepository = ajaxFilterParentCategoryRepository;

        }

        #endregion

        #region Methods 


        public async Task DeleteParentCategoryAsync(AjaxFilterParentCategory ajaxFilterParentCategory)
        {
            await _ajaxFilterParentCategoryRepository.DeleteAsync(ajaxFilterParentCategory);
        }

        public async Task<IPagedList<Category>> GetParentCategoriesAsync(AjaxFilterParentCategorySearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from category in _categoryRepository.Table
                            //where s.ParentCategoryId == 0
                        where category.Name.Contains(searchModel.SearchParentCategoryName)
                        select category;
            var result = await query.ToPagedListAsync(pageIndex, pageSize);
            return result;

        }

        public async Task<IPagedList<AjaxFilterParentCategory>> GetSelectedParentCategoriesAsync(AjaxFilterParentCategorySearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from parentCategory in _ajaxFilterParentCategoryRepository.Table
                        join category in _categoryRepository.Table on parentCategory.CategoryId equals category.Id
                        where category.Name.Contains(searchModel.SearchParentCategoryName)
                        select parentCategory;

            var result = await query.ToPagedListAsync(pageIndex, pageSize);
            return result;

        }

        public async Task<AjaxFilterParentCategory> GetParentCategoryByIdAsync(int id)
        {
            return await _ajaxFilterParentCategoryRepository.GetByIdAsync(id);
        }

        public async Task<AjaxFilterParentCategory> GetParentCategoryByCategoryIdAsync(int id)
        {
            var result = from s in _ajaxFilterParentCategoryRepository.Table
                         where s.CategoryId == id
                         select s;

            return await result.FirstOrDefaultAsync();
        }

        public async Task InsertParentCategoryAsync(AjaxFilterParentCategory ajaxFilterParentCategory)
        {
            await _ajaxFilterParentCategoryRepository.InsertAsync(ajaxFilterParentCategory);
        }

        public async Task UpdateParentCategoryAsync(AjaxFilterParentCategory ajaxFilterParentCategory)
        {
            await _ajaxFilterParentCategoryRepository.UpdateAsync(ajaxFilterParentCategory);
        }

        public async Task<bool> CanOverrideFilterSetAsync(int categoryId)
        {
            var query = _ajaxFilterParentCategoryRepository.Table;

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(AjaxFilterDefaults.AjaxFilterOverrideFilterSetCacheKey, customerRoleIds, storeId, categoryId);

            return await _staticCacheManager.GetAsync(cacheKey, () => query.Any(x => x.CategoryId == categoryId));
        }

        #endregion
    }
}
