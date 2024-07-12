using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Services
{
    public class AlgoliaCatalogService : IAlgoliaCatalogService
    {
        #region Fields

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<ProductAttributeValue> _attributeValueRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

        #endregion

        #region Ctor

        public AlgoliaCatalogService(IRepository<Manufacturer> manufacturerRepository,
                IRepository<ProductAttributeValue> attributeValueRepository,
                IRepository<Product> productRepository,
                IRepository<ProductCategory> productCategoryRepository,
                IRepository<ProductManufacturer> productManufacturerRepository)
        {
            _manufacturerRepository = manufacturerRepository;
            _attributeValueRepository = attributeValueRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
        }

        #endregion

        #region Methods

        public IList<ProductAttributeValue> GetAttributeValuesByIds(int[] ids)
        {
            if (ids == null || !ids.Any())
                return new List<ProductAttributeValue>();
            return _attributeValueRepository.Table.Where(x => ids.Contains(x.Id)).ToList();
        }

        public IList<Manufacturer> GetManufacturersByIds(int[] ids)
        {
            if (ids == null || !ids.Any())
                return new List<Manufacturer>();
            return _manufacturerRepository.Table.Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IPagedList<Product>> SearchProductsAsync(int fromId = 0, int toId = int.MaxValue, IList<int> productIds = null,
            IList<int> categoryIds = null, IList<int> manufacturerIds = null, IList<int> vendorIds = null,
            bool inProductIdsOnly = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize == int.MaxValue)
                pageSize = pageSize - 1;
            if (toId == int.MaxValue)
                toId = toId - 1;

            if (productIds != null && productIds.Contains(0))
                productIds.Remove(0);
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);
            if (manufacturerIds != null && manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);
            if (vendorIds != null && vendorIds.Contains(0))
                vendorIds.Remove(0);

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var query = from p in _productRepository.Table
                        where !p.Deleted && p.Published && p.VisibleIndividually
                        select p;
            if (productIds != null && productIds.Count > 0)
            {
                query = from p in query
                        where productIds.Contains(p.Id)
                        select p;
            }
            if (!inProductIdsOnly)
            {
                query = from p in query
                        where p.Id >= fromId && p.Id <= toId
                        select p;

                if (categoryIds != null && categoryIds.Count > 0)
                {
                    var productCategoryMappingIdQuery = from pcm in _productCategoryRepository.Table
                                                        where categoryIds.Contains(pcm.CategoryId)
                                                        select pcm.ProductId;
                    query = from p in query
                            where productCategoryMappingIdQuery.Contains(p.Id)
                            select p;
                }

                if (manufacturerIds != null && manufacturerIds.Count > 0)
                {
                    var productManufacturerMappingIdQuery = from pmm in _productManufacturerRepository.Table
                                                            where manufacturerIds.Contains(pmm.ManufacturerId)
                                                            select pmm.ProductId;
                    query = from p in query
                            where productManufacturerMappingIdQuery.Contains(p.Id)
                            select p;
                }

                if (vendorIds != null && vendorIds.Count > 0)
                {
                    query = from p in query
                            where vendorIds.Contains(p.Id)
                            select p;
                }
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Product>> GetProductsByEntityIdsAsync(IList<int> productIds = null,
            IList<int> categoryIds = null, IList<int> manufacturerIds = null, IList<int> vendorIds = null, int pageIndex = 0, int pageSize = int.MaxValue, bool deletedOrUnpublishProduct = false)
        {
            if (pageSize == int.MaxValue)
                pageSize = pageSize - 1;

            if (productIds != null && productIds.Contains(0))
                productIds.Remove(0);
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);
            if (manufacturerIds != null && manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);
            if (vendorIds != null && vendorIds.Contains(0))
                vendorIds.Remove(0);

            var query = _productRepository.Table;
            if (deletedOrUnpublishProduct)
                query = from p in query
                        where p.Deleted || !p.Published || !p.VisibleIndividually
                        select p;
            else
            {
                query = from p in _productRepository.Table
                        where !p.Deleted && p.Published && p.VisibleIndividually
                        select p;
            }
            IQueryable<Product> combinedQuery = null;

            if (productIds != null && productIds.Count > 0)
            {
                var products = from q in query
                               where productIds.Contains(q.Id)
                               select q;
                combinedQuery = products;
            }

            if (categoryIds != null && categoryIds.Count > 0)
            {
                var productCategoryMappingIdQuery = from pcm in _productCategoryRepository.Table
                                                    where categoryIds.Contains(pcm.CategoryId)
                                                    select pcm.ProductId;
                var productByCategory = from q in query
                                        where productCategoryMappingIdQuery.Contains(q.Id)
                                        select q;
                if (combinedQuery == null)
                {
                    combinedQuery = productByCategory;
                }
                else
                {
                    combinedQuery = combinedQuery.Concat(productByCategory);
                }
            }
            if (manufacturerIds != null && manufacturerIds.Count > 0)
            {
                var productManufacturerMappingIdQuery = from pmm in _productManufacturerRepository.Table
                                                        where manufacturerIds.Contains(pmm.ManufacturerId)
                                                        select pmm.ProductId;
                var productByManufacturer = from q in query
                                            where productManufacturerMappingIdQuery.Contains(q.Id)
                                            select q;
                if (combinedQuery == null)
                {
                    combinedQuery = productByManufacturer;
                }
                else
                {
                    combinedQuery = combinedQuery.Concat(productByManufacturer);
                }
            }

            if (vendorIds != null && vendorIds.Count > 0)
            {
                var productByVendor = from q in query
                                      where vendorIds.Contains(q.Id)
                                      select q;
                if (combinedQuery == null)
                {
                    combinedQuery = productByVendor;
                }
                else
                {
                    combinedQuery = combinedQuery.Concat(productByVendor);
                }
            }

            return await combinedQuery.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}