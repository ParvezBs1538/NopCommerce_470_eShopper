using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Services;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Factories
{
    public class ProductQAModelFactory : IProductQAModelFactory
    {
        #region Fields
        private readonly IProductQAService _productQAService;
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor
        public ProductQAModelFactory(IProductQAService productQAService,
            IStoreService storeService,
            IProductService productService,
            ICustomerService customerService,
            IStoreContext storeContext,
            ISettingService settingService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper)
        {
            _productQAService = productQAService;
            _storeService = storeService;
            _productService = productService;
            _customerService = customerService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
        }
        #endregion

        #region Methods

        public async Task<ProductQAListModel> PrepareProdouctQAListModelAsync(ProductQASearchModel searchModel)
        {
            bool? approved = null;
            if (searchModel.SearchApproveOptionId == 1)
                approved = true;
            else if (searchModel.SearchApproveOptionId == 2)
                approved = false;

            bool? hasAnswer = null;
            if (searchModel.SearchAnswerOptionId == 1)
                hasAnswer = true;
            else if (searchModel.SearchAnswerOptionId == 2)
                hasAnswer = false;

            var productsQAs = await _productQAService.GetAllProductQnAsAsync(
                storeId: searchModel.SearchStoreId,
                productId: searchModel.SearchProductId,
                approved: approved,
                hasAnswer: hasAnswer,
                createdFrom: !searchModel.CreatedFrom.HasValue ? null
                    : _dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()),
                createdTo: !searchModel.CreatedTo.HasValue ? null
                    : _dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1),
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            
            var model = await new ProductQAListModel().PrepareToGridAsync(searchModel, productsQAs, () =>
            {
                return productsQAs.SelectAwait(async productQA =>
                {
                    return await PrepareProductQAModelAsync(null, productQA);
                });
            });
            return model;
        }

        public async Task<ProductQAModel> PrepareProductQAModelAsync(ProductQAModel model, ProductQnA productQA)
        {
            if (productQA != null)
            {
                var product = await _productService.GetProductByIdAsync(productQA.ProductId);
                var customer = await _customerService.GetCustomerByIdAsync(productQA.CustomerId);

                var customerName = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.GuestCustomer");
                var answeredByCustomerName = "";
                string prepareCustomerInformation = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.GuestCustomer");
                string prepareProductInformation = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.PrepareProductInformation");

                if (customer != null && !customer.Deleted && !await _customerService.IsGuestAsync(customer))
                {
                    customerName = await _customerService.GetCustomerFullNameAsync(customer);
                    prepareCustomerInformation = customerName + " (" + customer.Email + ")";
                }

                if (product != null && !product.Deleted)
                {
                    prepareProductInformation = product.Name + " (" + product.Sku + ")";
                }
                else
                {
                    prepareProductInformation = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Warning.NoProductFound");
                }

                if (productQA.AnswerText != null)
                {
                    var answeredByCustomer = await _customerService.GetCustomerByIdAsync(productQA.UpdatedByCustomerId);
                    if (answeredByCustomer != null && !answeredByCustomer.Deleted && !await _customerService.IsGuestAsync(answeredByCustomer))
                        answeredByCustomerName = await _customerService.GetCustomerFullNameAsync(answeredByCustomer) + " (" + answeredByCustomer.Email + ")";
                }

                model = new ProductQAModel
                {
                    Id = productQA.Id,
                    QuestionText = productQA.QuestionText,
                    AnswerText = productQA.AnswerText,
                    StoreId = productQA.StoreId,
                    IsApproved = productQA.IsApproved,
                    IsAnonymous = productQA.IsAnonymous,
                    Deleted = productQA.Deleted,
                    ProductId = productQA.ProductId,
                    ProductVendorId = product != null ? product.VendorId : 0,
                    CustomerId = productQA.CustomerId,
                    CustomerUserName = customer != null ? customer.Username : "",
                    CustomerInformation = prepareCustomerInformation,
                    ProductInformation = prepareProductInformation,
                    UpdatedByCustomerId = productQA.UpdatedByCustomerId,
                    AnsweredByCustomerName = answeredByCustomerName,
                    CreatedOnUtc = productQA.CreatedOnUtc,
                    UpdatedOnUtc = productQA.UpdatedOnUtc,
                };
            }
            return model;
        }

        public async Task<ProductQASearchModel> PrepareProductQASearchModelAsync(ProductQASearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var allStore = await _storeService.GetAllStoresAsync();
            searchModel.AvailableStores.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.All")
            });
            if (allStore.Count > 0)
            {
                foreach (var store in allStore)
                {
                    searchModel.AvailableStores.Add(new SelectListItem
                    {
                        Value = store.Id.ToString(),
                        Text = store.Name,
                    });
                }
            }
            searchModel.ApprovalOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.All")
            });
            searchModel.ApprovalOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Approved")
            });
            searchModel.ApprovalOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Pending")
            });

            searchModel.AnswerOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.All")
            });
            searchModel.AnswerOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Answered")
            });
            searchModel.AnswerOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.NoAnswerYet")
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ProductQAModel> PrepareModelForEditPageAsync(int productQAId)
        {
            var productQA = await _productQAService.GetProductQnAByIdAsync(productQAId);
            if (productQA == null || productQA.Deleted)
                return null;

            return await PrepareProductQAModelAsync(null, productQA);
        }

        public async Task<bool> IsAccessToEditAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var productQASettings = await _settingService.LoadSettingAsync<ProductQAConfigurationSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IsEnable = productQASettings.IsEnable,
                QuestionAnonymous = productQASettings.QuestionAnonymous,
                LimitedCustomerRole = productQASettings.LimitedCustomerRole,
                AnswerdCustomerRole = productQASettings.AnswerdCustomerRole,
                LimitedStoreId = productQASettings.LimitedStoreId,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (!string.IsNullOrEmpty(model.AnswerdCustomerRole))
            {
                var splitLimitedCustomerRole = model.AnswerdCustomerRole.Split(',');
                foreach (var splitedLimitedCustomerRole in splitLimitedCustomerRole)
                {
                    model.SelectedAnsweredCustomerRoleIds.Add(Convert.ToInt32(splitedLimitedCustomerRole));
                }
            }

            var isAccessToAskQuestion = false;

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null || customer.Deleted)
                return false;
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);

            foreach (var customerRole in customerRoles)
            {
                if (model.SelectedAnsweredCustomerRoleIds.Contains(customerRole.Id))
                {
                    isAccessToAskQuestion = true;
                    break;
                }
            }

            return isAccessToAskQuestion;
        }

        #endregion
    }
}
