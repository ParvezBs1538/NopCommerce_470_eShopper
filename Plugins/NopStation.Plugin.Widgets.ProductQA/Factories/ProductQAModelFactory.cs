using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Models;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Services;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Factories
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
        #endregion

        #region Ctor
        public ProductQAModelFactory(IProductQAService productQAService,
            IStoreService storeService,
            IProductService productService,
            ICustomerService customerService,
            IStoreContext storeContext,
            ISettingService settingService,
            IWorkContext workContext,
            ILocalizationService localizationService
            )
        {
            _productQAService = productQAService;
            _storeService = storeService;
            _productService = productService;
            _customerService = customerService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _localizationService = localizationService;
        }
        #endregion

        #region Methods

        public async Task<ProductQuestionAnswerModel> PrepareGetProductQuestionAnswerModelAsync(ProductQuestionAnswerModel model)
        {
            model.NoResults = true;
            var data = await _productQAService.GetProductQnAsByProductIdAsync(productId: model.Id, pageIndex: model.CurrentPageNumber - 1, pageSize: 10);
            model.TotalPages = (data.Count%10 == 0) ? data.Count/10 : (data.Count/10)+1;
            var productQAs = new List<ProductQnA>();
            productQAs.AddRange(await data.Skip((model.CurrentPageNumber - 1) * 10).Take(10).ToListAsync());

            if (productQAs.Count > 0)
            {
                model.NoResults = false;

                foreach (var productQA in productQAs)
                {
                    string questionByCustomerName = await _localizationService.GetResourceAsync("NopStation.ProductQuestionAnswer.ProductQnA.GuestCustomer");
                    string answerByCustomerName = "";
                    var questionBycustomer = await _customerService.GetCustomerByIdAsync(productQA.CustomerId);

                    if (questionBycustomer != null && !questionBycustomer.Deleted && !await _customerService.IsGuestAsync(questionBycustomer))
                    {
                        string customerName = await _customerService.GetCustomerFullNameAsync(questionBycustomer);
                        if (productQA.IsAnonymous)
                        {
                            questionByCustomerName = await _localizationService.GetResourceAsync("NopStation.ProductQuestionAnswer.ProductQnA.AnonymousCustomer");
                        }
                        else
                        {
                            var questionByCustomerFirstName = customerName.Split(' ');
                            questionByCustomerName = questionByCustomerFirstName[0];
                        }
                    }

                    if (productQA.AnswerText != null)
                    {
                        var answerBycustomer = await _customerService.GetCustomerByIdAsync(productQA.UpdatedByCustomerId);
                        if (answerBycustomer != null && !answerBycustomer.Deleted && !await _customerService.IsGuestAsync(answerBycustomer))
                        {
                            string customerName = await _customerService.GetCustomerFullNameAsync(answerBycustomer);
                            var answerByCustomerFirstName = customerName.Split(' ');
                            answerByCustomerName = answerByCustomerFirstName[0];
                        }
                    }

                    var questionAnswer = new ProductQuestionAnswerModel.ProductQuestionAnswerPublicInfoModel()
                    {
                        QuestionText = productQA.QuestionText,
                        QuestionByCustomerName = questionByCustomerName,
                        QuestionAskedDate = productQA.CreatedOnUtc,
                        AnswerText = productQA.AnswerText,
                        AnswerByCustomerName = answerByCustomerName,
                        AnswerGivenDate = productQA.UpdatedOnUtc
                    };
                    model.ProductQuestionAnswerPublicInfoModels.Add(questionAnswer);
                }
            }
            return model;
        }

        public async Task<ProductQAPublicInfoModel> PrepareProductQAPublicInfoModelAsync(int productId)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var productQASettings = await _settingService.LoadSettingAsync<ProductQAConfigurationSettings>(storeScope);

            if (productQASettings == null)
                return null;

            var model = new ConfigurationModel
            {
                IsEnable = productQASettings.IsEnable,
                QuestionAnonymous = productQASettings.QuestionAnonymous,
                LimitedCustomerRole = productQASettings.LimitedCustomerRole,
                AnswerdCustomerRole = productQASettings.AnswerdCustomerRole,
                LimitedStoreId = productQASettings.LimitedStoreId,
                ActiveStoreScopeConfiguration = storeScope
            };

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return null;

            
            if (!string.IsNullOrEmpty(model.LimitedCustomerRole))
            {
                var splitLimitedCustomerRole = model.LimitedCustomerRole.Split(',');
                foreach (var splitedLimitedCustomerRole in splitLimitedCustomerRole)
                {
                    model.SelectedLimitedCustomerRoleIds.Add(Convert.ToInt32(splitedLimitedCustomerRole));
                }
            }

            if (!string.IsNullOrEmpty(model.LimitedStoreId))
            {
                string[] splitStore = model.LimitedStoreId.Split(',');
                foreach (var splitedStore in splitStore)
                {
                    model.SelectedLimitedStoreId.Add(Convert.ToInt32(splitedStore));
                }
            }
            
            if (!model.LimitedStoreId.Contains("0"))
            {
                var checkStore = model.SelectedLimitedStoreId.Select(async c => c == (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!checkStore.Any())
                    return null;
            }

            var publicInfoModel = new ProductQAPublicInfoModel();
            publicInfoModel.ProductQAConfigurationModel = model;
            publicInfoModel.IsAccessToAskQuestion = false;
            publicInfoModel.IsQuestionAsAAnonymous = false;
            if (model.QuestionAnonymous)
                publicInfoModel.IsQuestionAsAAnonymous = true;

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null || customer.Deleted)
                return null;
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
                
            foreach (var customerRole in customerRoles)
            {
                if (model.SelectedLimitedCustomerRoleIds.Contains(customerRole.Id))
                {
                    publicInfoModel.IsAccessToAskQuestion = true;
                    break;
                }
            }

            return publicInfoModel;
        }

        #endregion
    }
}
