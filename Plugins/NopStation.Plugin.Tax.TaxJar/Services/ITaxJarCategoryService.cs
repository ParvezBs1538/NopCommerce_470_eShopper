using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public interface ITaxJarCategoryService
    {
        Task InsertTaxJarCategoryAsync(TaxJarCategory taxJarCategory);

        Task UpdateTaxJarCategoryAsync(TaxJarCategory taxJarCategory);

        Task DeleteTaxJarCategoryAsync(TaxJarCategory taxJarCategory);

        Task<TaxJarCategory> GetTaxJarCategoryByValueAsync(string value);

        Task<TaxJarCategory> GetTaxJarCategoryByTaxCategoryIdAsync(int taxCategoryId);

        Task<IPagedList<TaxJarCategory>> GetTaxJarCategoriesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IPagedList<TaxJarFormattedCategory>> GetTaxJarFormattedCategoriesAsync(int pageIndex = 0, int pageSize = int.MaxValue);
    }
}