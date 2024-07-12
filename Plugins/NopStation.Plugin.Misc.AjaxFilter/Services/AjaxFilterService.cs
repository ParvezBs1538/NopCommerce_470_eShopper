using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.AjaxFilter.Domains;
using NopStation.Plugin.Misc.AjaxFilter.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Services
{
    public class AjaxFilterService : IAjaxFilterService
    {

        #region Fields

        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<AjaxFilterSpecificationAttribute> _ajaxFilterSpecificationAttributeRepository;
        private readonly INopDataProvider _nopDataProvider;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public AjaxFilterService(
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<Product> productRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<AjaxFilterSpecificationAttribute> ajaxFilterSpecificationAttributeRepository,
            INopDataProvider nopDataProvider,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            IStaticCacheManager staticCacheManager,
            ISpecificationAttributeService specificationAttributeService,
            CatalogSettings catalogSettings,
            ILogger logger)
        {
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _productRepository = productRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productCategoryRepository = productCategoryRepository;
            _ajaxFilterSpecificationAttributeRepository = ajaxFilterSpecificationAttributeRepository;
            _nopDataProvider = nopDataProvider;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _staticCacheManager = staticCacheManager;
            _specificationAttributeService = specificationAttributeService;
            _catalogSettings = catalogSettings;
            _logger = logger;
        }

        #endregion

        public virtual async Task<int> GetNumberOfProductsInManufacturerAsync(int categoryId, IList<int> manufacturerIds = null, int storeId = 0)
        {
            //validate "manufacturerIds" parameter
            if (manufacturerIds != null && manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);

            var query = _productRepository.Table.Where(p => p.Published && !p.Deleted && p.VisibleIndividually);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            query = await _aclService.ApplyAcl(query, customerRoleIds);

            //manufacturer filtering
            if (manufacturerIds != null && manufacturerIds.Any())
            {
                query = from p in query
                        join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                        join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                        where manufacturerIds.Contains(pm.ManufacturerId) && pc.CategoryId == categoryId
                        select p;
            }

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(AjaxFilterDefaults.ManufacturerProductsNumberCacheKey, customerRoleIds, storeId, manufacturerIds, categoryId);

            //only distinct products
            return await _staticCacheManager.GetAsync(cacheKey, () => query.Select(p => p.Id).Count());
        }

        public virtual async Task<int> GetNumberOfProductsUsingSpecificationAttributeAsync(int categoryId, int specificationAttributeOptionId, int storeId = 0)
        {
            if (categoryId == 0)
            {
                return 0;
            }
            var query = _productRepository.Table.Where(p => p.Published && !p.Deleted && p.VisibleIndividually);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            query = await _aclService.ApplyAcl(query, customerRoleIds);

            query = from product in _productRepository.Table
                    join pc in _productCategoryRepository.Table on product.Id equals pc.ProductId
                    join psa in _productSpecificationAttributeRepository.Table on product.Id equals psa.ProductId
                    join spao in _specificationAttributeOptionRepository.Table on psa.SpecificationAttributeOptionId equals spao.Id
                    where spao.Id == specificationAttributeOptionId && pc.CategoryId == categoryId
                    select product;

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(AjaxFilterDefaults.SpecificationAttributeProductsNumberCacheKey, customerRoleIds, storeId, categoryId, specificationAttributeOptionId);

            //only distinct products
            return await _staticCacheManager.GetAsync(cacheKey, () => query.Count());
        }

        public async Task<SearchFilterResult> SearchProducts(SearchModel model, string typ = "", bool showproducts = false)
        {
            var dataSettings = DataSettingsManager.LoadSettings();

            #region Prepare data parameters

            var categoryIdsForSP = "";
            if (model.CategoryIds != null)
            {
                categoryIdsForSP = string.Join(",", model.CategoryIds);
                if (model.CategoryIds.Count == 1 && _catalogSettings.ShowProductsFromSubcategories)
                {
                    var childCategoryIds = await GetChildCategoryIdsAsync(model.CategoryIds.FirstOrDefault());
                    if (childCategoryIds.Count > 0)
                    {
                        categoryIdsForSP += ",";

                        categoryIdsForSP += string.Join(",", childCategoryIds);
                    }
                }
            }
            var pCategoryIds = new DataParameter
            {
                Name = "CategoryIds",
                Value = categoryIdsForSP,
                DataType = DataType.NVarChar
            };

            var manufacturerIdsForSp = "";
            if (model.ManufacturerIds != null)
            {
                manufacturerIdsForSp = string.Join(",", model.ManufacturerIds);
            }
            var pManufacturerIds = new DataParameter
            {
                Name = "ManufacturerIds",
                Value = manufacturerIdsForSp,
                DataType = DataType.NVarChar
            };

            var pStoreId = new DataParameter
            {
                Name = "StoreId",
                Value = ((!_catalogSettings.IgnoreStoreLimitations) ? model.StoreId : 0),
                DataType = DataType.Int32
            };

            var pPriceMin = new DataParameter
            {
                Name = "PriceMin",
                Value = model.PriceMin.HasValue ?
                (object)await _currencyService.ConvertToPrimaryStoreCurrencyAsync(model.PriceMin.Value, (await _workContext.GetWorkingCurrencyAsync())) : 0,
                DataType = DataType.Decimal
            };

            var pPriceMax = new DataParameter
            {
                Name = "PriceMax",
                Value = model.PriceMax.HasValue ?
                (object)await _currencyService.ConvertToPrimaryStoreCurrencyAsync(model.PriceMax.Value, (await _workContext.GetWorkingCurrencyAsync())) : int.MaxValue,
                DataType = DataType.Decimal
            };

            var filteredSpecsForSp = "";
            if (model.FilteredSpecs != null)
            {
                filteredSpecsForSp = string.Join(",", model.FilteredSpecs);
            }
            var pFilteredSpecs = new DataParameter
            {
                Name = "FilteredSpecs",
                Value = filteredSpecsForSp,
                DataType = DataType.NVarChar
            };

            var allowedCustomerRoleIdsForSp = "";
            if (model.AllowedCustomerRolesIds != null)
            {
                allowedCustomerRoleIdsForSp = string.Join(",", model.AllowedCustomerRolesIds);
            }
            var pAllowedCustomerRoleIds = new DataParameter
            {
                Name = "AllowedCustomerRoleIds",
                Value = allowedCustomerRoleIdsForSp,
                DataType = DataType.NVarChar
            };

            var attributesIdsForSp = "";
            if (model.AttributeIds != null)
            {
                attributesIdsForSp = string.Join(",", model.AttributeIds);
            }
            var pAttributesIds = new DataParameter
            {
                Name = "AttributesIds",
                Value = model.SelectedAttributeOptions,
                DataType = DataType.NVarChar
            };

            var productTagIdsForSp = "";
            if (model.ProductTagIds != null)
            {
                productTagIdsForSp = string.Join(",", model.ProductTagIds);
            }
            var pTagIds = new DataParameter
            {
                Name = "ProductTagIds",
                Value = productTagIdsForSp,
                DataType = DataType.NVarChar
            };

            var pProductRatingsIdsForSp = 0;
            if (model.ProductRatingIds.Count > 0)
            {
                pProductRatingsIdsForSp = model.ProductRatingIds[0];
            }
            var pRatingIds = new DataParameter
            {
                Name = "ProductRatingIds",
                Value = pProductRatingsIdsForSp,
                DataType = DataType.Int32
            };

            var vendorIdsForSp = "";
            if (model.VendorIds != null)
            {
                vendorIdsForSp = string.Join(",", model.VendorIds);
            }

            var pVendorIds = new DataParameter
            {
                Name = "VendorIds",
                Value = vendorIdsForSp,
                DataType = DataType.NVarChar
            };

            var pShowHidden = new DataParameter
            {
                Name = "ShowHidden",
                Value = model.ShowHidden,
                DataType = DataType.Boolean
            };

            var pViewMore = new DataParameter
            {
                Name = "ViewMore",
                Value = model.ViewMoreSpecificationId > 0 ? true : false,
                DataType = DataType.Boolean
            };

            var pSelectedSpecificationAttributeId = new DataParameter
            {
                Name = "SelectedSpecificationAttributeId",
                Value = model.ViewMoreSpecificationId,
                DataType = DataType.Int32
            };

            var ppFilterFreeShipping = new DataParameter
            {
                Name = "FilterFreeShipping",
                Value = model.FilterFreeShipping,
                DataType = DataType.Boolean
            };
            var pFilterTaxExcemptProduct = new DataParameter
            {
                Name = "FilterTaxExcemptProduct",
                Value = model.FilterTaxExempt,
                DataType = DataType.Boolean
            };
            var ppFilterDiscountedProduct = new DataParameter
            {
                Name = "FilterDiscountedProduct",
                Value = model.FilterDiscountedProduct,
                DataType = DataType.Boolean
            };
            var ppFilterNewProduct = new DataParameter
            {
                Name = "FilterNewProduct",
                Value = model.FilterNewProduct,
                DataType = DataType.Boolean
            };

            var pReturnSize = new DataParameter
            {
                Name = "ReturnSize",
                Value = 10,
                DataType = DataType.Int32
            };

            var ajaxFilterSpecificationAttributeIds = model.AvaliableSpecificationAttributes.Select(s => s.SpecificationId).ToList();
            var settingIndex = -1;

            if (model.ViewMoreSpecificationId != 0)
            {
                settingIndex = ajaxFilterSpecificationAttributeIds.IndexOf(model.ViewMoreSpecificationId);
            }

            var ajaxFilterSpecificationAttributeIdsAsString = (ajaxFilterSpecificationAttributeIds).ConvertAll<string>(x => x.ToString());
            var value = string.Join(",", ajaxFilterSpecificationAttributeIdsAsString);
            value = value.Length == 0 ? value : value + ",";
            var pSpecificationAttributeIds = new DataParameter
            {
                Name = "SpecificationAttributeIds",
                Value = value,
                DataType = DataType.NVarChar
            };

            var specificationAttributeIdsSettings = model.AvaliableSpecificationAttributes.Select(s => s.MaxSpecificationAttributesToDisplay).ToList();

            //set default size for specOptions if a specOption size is set to 0
            for (int i = 0; i < specificationAttributeIdsSettings.Count; i++)
            {
                if (specificationAttributeIdsSettings.ElementAt(i) == 0)
                {
                    specificationAttributeIdsSettings[i] = model.MaxSpecificationAttributeToDisplayByDefault;
                }
            }

            List<int> selectedSpecificationAttributeIds = null;
            List<int> selectedSpecificationAttributeOptionIds = null;
            if (model.SelectedSpecificationAttributes != null && model.SelectedSpecificationAttributeOptions != null)
            {
                var specElemnts = model.SelectedSpecificationAttributes.TrimEnd(',').Split(',');
                selectedSpecificationAttributeIds = Array.ConvertAll(specElemnts, s => int.Parse(s)).ToList();
                var specElemntsOptions = model.SelectedSpecificationAttributeOptions.TrimEnd(',').Split(',');
                selectedSpecificationAttributeOptionIds = Array.ConvertAll(specElemntsOptions, s => int.Parse(s)).ToList();

                for (int i = 0; i < ajaxFilterSpecificationAttributeIds.Count; i++)
                {
                    for (int j = 0; j < selectedSpecificationAttributeOptionIds.Count; j++)
                    {
                        if (ajaxFilterSpecificationAttributeIds[i] == selectedSpecificationAttributeIds[j])
                        {
                            specificationAttributeIdsSettings[i] = Math.Max(specificationAttributeIdsSettings[i], selectedSpecificationAttributeOptionIds[j]);
                        }
                    }
                }

            }

            var specificationOptionsSize = 0;
            if (settingIndex != -1)
            {
                specificationOptionsSize = await GetSpecificationAttributeOptionsCountBySpecificationId(model.ViewMoreSpecificationId);
                specificationAttributeIdsSettings[settingIndex] = specificationOptionsSize;
            }

            var specificationAttributeIdsSettingAsString = (specificationAttributeIdsSettings).ConvertAll<string>(x => x.ToString());
            value = string.Join(",", specificationAttributeIdsSettingAsString);
            value = value.Length == 0 ? value : value + ",";
            var pSpecificationAttributeIdsSetting = new DataParameter
            {
                Name = "SpecificationAttributeIdsSetting",
                Value = value,
                DataType = DataType.NVarChar
            };

            var pDefaultSize = new DataParameter
            {
                Name = "DefaultSize",
                Value = 5,
                DataType = DataType.Int32
            };

            var pPageSize = new DataParameter
            {
                Name = "PageSize",
                Value = 30,
                DataType = DataType.Int32
            };

            var pPageIndexAt = new DataParameter
            {
                Name = "PageIndexAt",
                Value = model.PageIndex,
                DataType = DataType.Int32
            };

            var pFilterBy = new DataParameter
            {
                Name = "FilterBy",
                Value = model.FilterBy,
                DataType = DataType.NVarChar
            };

            #endregion

            IList<FilterModel> source = null;

            try
            {
                var qString = string.Empty;

                if (dataSettings != null)
                {
                    if (dataSettings.DataProvider == DataProviderType.SqlServer)
                    {
                        qString = @"Exec AjaxFilter
                            @CategoryIds = @CategoryIds, 
                            @ManufacturerIds = @ManufacturerIds, 
                            @StoreId = @StoreId, 
                            @VendorIds = @VendorIds, 
                            @ProductTagIds = @ProductTagIds,
                            @ProductRatingIds = @ProductRatingIds, 
                            @PriceMin = @PriceMin, 
                            @PriceMax = @PriceMax, 
                            @FilteredSpecs = @FilteredSpecs, 
                            @AllowedCustomerRoleIds = @AllowedCustomerRoleIds, 
                            @AttributesIds = @AttributesIds, 
                            @ShowHidden = @ShowHidden, 
                            @ViewMore = @ViewMore, 
                            @SelectedSpecificationAttributeId=@SelectedSpecificationAttributeId, 
                            @ReturnSize= @ReturnSize, 
                            @SpecificationAttributeIds = @SpecificationAttributeIds, 
                            @SpecificationAttributeIdsSetting = @SpecificationAttributeIdsSetting,
	                        @FilterFreeShipping = @FilterFreeShipping,
	                        @FilterTaxExcemptProduct = @FilterTaxExcemptProduct,
	                        @FilterDiscountedProduct = @FilterDiscountedProduct,
	                        @FilterNewProduct = @FilterNewProduct, 
                            @DefaultSize = @DefaultSize, 
                            @PageSize = @PageSize, 
                            @PageIndexAt = @PageIndexAt, 
                            @FilterBy = @FilterBy";
                        source = (await _nopDataProvider.QueryAsync<FilterModel>
                                (qString,
                                pCategoryIds,
                                pManufacturerIds,
                                pStoreId,
                                pVendorIds,
                                pTagIds,
                                pRatingIds,
                                pPriceMin,
                                pPriceMax,
                                pFilteredSpecs,
                                pAllowedCustomerRoleIds,
                                pAttributesIds,
                                pShowHidden,
                                pViewMore,
                                pSelectedSpecificationAttributeId,
                                pReturnSize,
                                pSpecificationAttributeIds,
                                pSpecificationAttributeIdsSetting,
                                ppFilterFreeShipping,
                                pFilterTaxExcemptProduct,
                                ppFilterDiscountedProduct,
                                ppFilterNewProduct,
                                pDefaultSize,
                                pPageSize,
                                pPageIndexAt,
                                pFilterBy
                                )).ToList();
                    }
                    if (dataSettings.DataProvider == DataProviderType.MySql)
                    {
                        qString = @"CALL AjaxFilter(
                                    @CategoryIds := '{0}',  
                                    @ManufacturerIds := '{1}', 
                                    @StoreId := '{2}', 
                                    @VendorIds := '{3}', 
                                    @ProductTagIds := '{4}',
                                    @ProductRatingIds := '{5}',
                                    @PriceMin := '{6}',
                                    @PriceMax := '{7}', 
                                    @FilteredSpecs := '{8}',
                                    @AllowedCustomerRoleIds := '{9}', 
                                    @AttributesIds := '{10}',
                                    @ShowHidden := {11},
                                    @ViewMore := {12},
                                    @SelectedSpecificationAttributeId := '{13}', 
                                    @ReturnSize := '{14}',
                                    @SpecificationAttributeIds := '{15}',
                                    @SpecificationAttributeIdsSetting := '{16}',
	                                @FilterFreeShipping := {17},
	                                @FilterTaxExcemptProduct := {18},
	                                @FilterDiscountedProduct := {19},
	                                @FilterNewProduct := {20},
                                    @DefaultSize := '{21}',
                                    @PageSize := '{22}',
                                    @PageIndexAt := '{23}',
                                    @FilterBy := '{24}');";
                        qString = string.Format(qString,
                                                pCategoryIds.Value,
                                                pManufacturerIds.Value,
                                                pStoreId.Value,
                                                pVendorIds.Value,
                                                pTagIds.Value,
                                                pRatingIds.Value,
                                                pPriceMin.Value,
                                                pPriceMax.Value,
                                                pFilteredSpecs.Value,
                                                pAllowedCustomerRoleIds.Value,
                                                pAttributesIds.Value,
                                                pShowHidden.Value,
                                                pViewMore.Value,
                                                pSelectedSpecificationAttributeId.Value,
                                                pReturnSize.Value,
                                                pSpecificationAttributeIds.Value,
                                                pSpecificationAttributeIdsSetting.Value,
                                                ppFilterFreeShipping.Value,
                                                pFilterTaxExcemptProduct.Value,
                                                ppFilterDiscountedProduct.Value,
                                                ppFilterNewProduct.Value,
                                                pDefaultSize.Value,
                                                pPageSize.Value,
                                                pPageIndexAt.Value,
                                                pFilterBy.Value);



                        source = await _nopDataProvider.QueryAsync<FilterModel>(qString);
                    }
                }

            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Ajax Filter Store Procedure Error", ex);
            }

            #region Prepare filters from result 

            var searchFilterResult = new SearchFilterResult();

            var priceMin = source.Where(x => x.FilterType == "MIN").FirstOrDefault();

            var priceMax = source.Where(x => x.FilterType == "MAX").FirstOrDefault();

            if (priceMin != null)
                searchFilterResult.PriceRange.PriceMin = priceMin.FilterId;

            if (priceMax != null)
                searchFilterResult.PriceRange.PriceMax = priceMax.FilterId;


            searchFilterResult.Vendors = source.Where(x => x.FilterType == "VEN").Select(x =>
                new SearchFilterResult.Vendor
                {
                    Id = x.FilterId,
                    Count = x.FilterCount
                }).ToList();

            searchFilterResult.ProductTags = source.Where(x => x.FilterType == "TAG").Select(x =>
                new SearchFilterResult.ProductTag
                {
                    Id = x.FilterId,
                    Count = x.FilterCount
                }).ToList();

            searchFilterResult.Ratings = source.Where(x => x.FilterType == "RAT").Select(x =>
                new SearchFilterResult.Rating
                {
                    Id = x.FilterId,
                    Count = x.FilterCount
                }).ToList();

            searchFilterResult.ProductAttributes = source.Where(x => x.FilterType == "PAT").Select(x =>
                new SearchFilterResult.ProductAttribute
                {
                    ProductAttributeId = x.ProductAttributeId,
                    Name = x.Name,
                    Count = x.FilterCount
                }).ToList();

            if (typ != "m")
            {
                searchFilterResult.Manufacturers = source.Where(x => x.FilterType == "MAN").Select(x =>
                new SearchFilterResult.Manufacturer
                {
                    Id = x.FilterId,
                    Count = x.FilterCount
                }).ToList();
            }

            if (!typ.StartsWith("s"))
            {
                searchFilterResult.Specifications = source.Where(x => x.FilterType == "SPE").Select(x =>
                new SearchFilterResult.Specification
                {
                    Id = x.FilterId,
                    Count = x.FilterCount,
                    Name = x.Name,
                    Color = x.Color,
                    SpecificationAttributeId = x.SpecificationAttributeId
                }).ToList();
            }
            else
            {
                typ.Split('-').LastOrDefault();
                searchFilterResult.Specifications = source.Where(x => x.FilterType == "SPE").Select(x =>
                new SearchFilterResult.Specification
                {
                    Id = x.FilterId,
                    Count = x.FilterCount,
                    Name = x.Name,
                    Color = x.Color,
                    SpecificationAttributeId = x.SpecificationAttributeId
                }).ToList();
            }
            if (!typ.StartsWith("a"))
            {
                searchFilterResult.Attributes = source.Where(x => x.FilterType == "ATR").Select(x => new
                SearchFilterResult.Attribute
                {
                    Id = x.FilterId,
                    Count = x.FilterCount,
                    Name = x.Name,
                    Color = x.Color
                }).ToList();
            }
            else
            {
                typ.Split('-').LastOrDefault();
                searchFilterResult.Attributes = source.Where(x => x.FilterType == "ATR").Select(x =>
                  new SearchFilterResult.Attribute
                  {
                      Id = x.FilterId,
                      Count = x.FilterCount,
                      Name = x.Name,
                      Color = x.Color
                  }).ToList();
            }

            #endregion

            if (showproducts)
            {

                #region Prepare data parameters 

                //parameter11
                var pCategoryIdsForPrd = new DataParameter
                {
                    Name = "CategoryIds",
                    Value = categoryIdsForSP,
                    DataType = DataType.NVarChar
                };

                //parameter12
                var pManufacturerIdsForPrd = new DataParameter
                {
                    Name = "ManufacturerIds",
                    Value = manufacturerIdsForSp,
                    DataType = DataType.NVarChar
                };

                //parameter13
                var pStoreIdForPrd = new DataParameter
                {
                    Name = "StoreId",
                    Value = ((!_catalogSettings.IgnoreStoreLimitations) ? model.StoreId : 0),
                    DataType = DataType.Int32
                };

                //parameter14
                var pPriceMinForPrd = new DataParameter
                {
                    Name = "PriceMin",
                    Value = (model.PriceMin.HasValue ?
                    ((object)await _currencyService.ConvertToPrimaryStoreCurrencyAsync(model.PriceMin.Value, (await _workContext.GetWorkingCurrencyAsync()))) : DBNull.Value),
                    DataType = DataType.Decimal
                };

                //parameter15
                var pPriceMaxPrd = new DataParameter
                {
                    Name = "PriceMax",
                    Value = (model.PriceMax.HasValue ?
                    ((object)await _currencyService.ConvertToPrimaryStoreCurrencyAsync(model.PriceMax.Value, (await _workContext.GetWorkingCurrencyAsync()))) : DBNull.Value),
                    DataType = DataType.Decimal
                };

                //parameter16
                var pPageIndex = new DataParameter
                {
                    Name = "PageIndex",
                    Value = model.PageIndex,
                    DataType = DataType.Int32
                };

                //parameter17
                var pPageSizeForPrd = new DataParameter
                {
                    Name = "PageSize",
                    Value = model.PageSize,
                    DataType = DataType.Int32
                };

                //parameter18 
                var pOrderByForPrd = new DataParameter
                {
                    Name = "OrderBy",
                    Value = (int)model.OrderBy,
                    DataType = DataType.Int32
                };

                //parameter19
                var pTotalRecordsForPrd = new DataParameter
                {
                    Name = "TotalRecords",
                    Direction = ParameterDirection.Output,
                    DataType = DataType.Int32
                };

                //parameter20 
                var pFilteredSpecsForPrd = new DataParameter
                {
                    Name = "FilteredSpecs",
                    Value = filteredSpecsForSp,
                    DataType = DataType.NVarChar
                };

                //parameter21 
                var pAllowedCustomerRoleIdsForPrd = new DataParameter
                {
                    Name = "AllowedCustomerRoleIds",
                    Value = allowedCustomerRoleIdsForSp,
                    DataType = DataType.NVarChar
                };

                //parameter22
                var pAttributesIdsForPrd = new DataParameter
                {
                    Name = "AttributesIds",
                    Value = model.FilteredProductAttributes,
                    DataType = DataType.NVarChar
                };

                //parameter23
                var pVendorIdsForPrd = new DataParameter
                {
                    Name = "VendorIds",
                    Value = vendorIdsForSp,
                    DataType = DataType.NVarChar
                };

                //parameter24 
                var pShowHiddenForPrd = new DataParameter
                {
                    Name = "ShowHidden",
                    Value = model.ShowHidden,
                    DataType = DataType.Boolean
                };

                var pFilterInStock = new DataParameter
                {
                    Name = "FilterInStock",
                    Value = model.OnlyInStock,
                    DataType = DataType.Boolean
                };

                var pFilternNotInStockQuantity = new DataParameter
                {
                    Name = "FilterNotInStock",
                    Value = model.NotInStock,
                    DataType = DataType.Boolean
                };

                var pFilterInStockQuantity = new DataParameter
                {
                    Name = "StockQuantity",
                    Value = model.OnlyInStockQuantity,
                    DataType = DataType.NVarChar
                };

                var pFilterFreeShipping = new DataParameter
                {
                    Name = "FilterFreeShipping",
                    Value = model.FilterFreeShipping,
                    DataType = DataType.Boolean
                };

                var pFilterTaxExempt = new DataParameter
                {
                    Name = "FilterTaxExcemptProduct",
                    Value = model.FilterTaxExempt,
                    DataType = DataType.Boolean
                };

                var pFilterDiscountedProduct = new DataParameter
                {
                    Name = "FilterDiscountedProduct",
                    Value = model.FilterDiscountedProduct,
                    DataType = DataType.Boolean
                };

                var pFilterNewProduct = new DataParameter
                {
                    Name = "FilterNewProduct",
                    Value = model.FilterNewProduct,
                    DataType = DataType.Boolean
                };


                var productRatingIdsForSp = 0;
                if (model.ProductRatingIds.Count > 0)
                {
                    productRatingIdsForSp = model.ProductRatingIds[0];
                }

                var pFilterProductReview = new DataParameter
                {
                    Name = "ProductRatingIds",
                    Value = productRatingIdsForSp,
                    DataType = DataType.Int32
                };

                var pFilterProductTag = new DataParameter
                {
                    Name = "FilterProductTag",
                    Value = true,
                    DataType = DataType.Boolean
                };

                #endregion

                #region Make Query

                IList<Product> searchedProduct = (await _nopDataProvider.QueryProcAsync<Product>(
                        "AjaxFilterProduct",
                    pCategoryIdsForPrd,
                    pManufacturerIdsForPrd,
                    pStoreIdForPrd,
                    pVendorIdsForPrd,
                    pPriceMinForPrd,
                    pPriceMaxPrd,
                    pFilteredSpecsForPrd,
                    pAllowedCustomerRoleIdsForPrd,
                    pAttributesIdsForPrd,
                    pShowHiddenForPrd,
                    pOrderByForPrd,
                    pPageIndex,
                    pPageSizeForPrd,
                    pFilterInStock,
                    pFilternNotInStockQuantity,
                    pFilterInStockQuantity,
                    pFilterFreeShipping,
                    pFilterTaxExempt,
                    pFilterDiscountedProduct,
                    pFilterNewProduct,
                    pFilterProductReview,
                    pFilterProductTag,
                    pTagIds,
                    pTotalRecordsForPrd)).ToList();

                var totalProducts = int.Parse(pTotalRecordsForPrd.Value.ToString());
                searchFilterResult.Products = new PagedList<Product>(searchedProduct, model.PageIndex, model.PageSize, totalCount: totalProducts);

                #endregion
            }
            return searchFilterResult;
        }

        public async Task<List<AjaxFilterSpecificationAttribute>> GetAllAjaxFilterSpecificationAttributeIdsFromCategoryId(int categoryId)
        {
            var query = (from productCategory in _productCategoryRepository.Table
                         join productSpecificationAttribute in _productSpecificationAttributeRepository.Table
                         on productCategory.ProductId equals productSpecificationAttribute.ProductId
                         join specificationAttributeOption in _specificationAttributeOptionRepository.Table
                         on productSpecificationAttribute.SpecificationAttributeOptionId equals specificationAttributeOption.Id
                         join ajaxFilterSpecificationAttribute in _ajaxFilterSpecificationAttributeRepository.Table
                         on specificationAttributeOption.SpecificationAttributeId equals ajaxFilterSpecificationAttribute.SpecificationId
                         where productCategory.CategoryId == categoryId
                         orderby specificationAttributeOption.DisplayOrder
                         select ajaxFilterSpecificationAttribute);

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(AjaxFilterDefaults.AllAjaxFilterSpecificationAttributeIdsFromCategoryId, categoryId);

            return await _staticCacheManager.GetAsync(cacheKey, () => (query.Distinct()).ToList());

        }

        protected async Task<List<int>> GetChildCategoryIdsAsync(int parentCategoryId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AjaxFilterDefaults.AjaxFilterChildCategoryIdsCacheKey, parentCategoryId, customerRoleIds, (await _storeContext.GetCurrentStoreAsync()).Id);
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var list = new List<int>();
                var allCategoriesByParentCategoryId = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId);
                foreach (var item in allCategoriesByParentCategoryId)
                {
                    list.Add(item.Id);
                    list.AddRange(await GetChildCategoryIdsAsync(item.Id));
                }
                return list;
            });
        }

        public Task<int> GetSpecificationAttributeOptionsCountBySpecificationId(int id)
        {
            var result = (from specificationAttributeOption in _specificationAttributeOptionRepository.Table
                          where specificationAttributeOption.SpecificationAttributeId == id
                          select specificationAttributeOption).Count();
            return Task.FromResult(result);
        }
    }
}
