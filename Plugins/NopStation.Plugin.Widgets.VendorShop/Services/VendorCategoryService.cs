using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Data;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public class VendorCategoryService : IVendorCategoryService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;

        public VendorCategoryService(IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<ProductCategory> productCategoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _vendorRepository = vendorRepository;
            _productCategoryRepository = productCategoryRepository;
        }
        public async Task<IList<Category>> GetAllCategoriesByVendorId(int vendorId)
        {
            var query = from c in _categoryRepository.Table
                        join pcm in _productCategoryRepository.Table on c.Id equals pcm.CategoryId
                        join p in _productRepository.Table on pcm.ProductId equals p.Id
                        where p.Published == true
                            && p.Deleted == false
                            && p.VendorId == vendorId
                        select c;

            return await query.Distinct().ToListAsync();
        }
    }
}
