using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Models;
using NopStation.Plugin.Payments.MPay24.Services;
using Nop.Web.Framework.Models.Extensions;
using System;
using NopStation.Plugin.Payments.MPay24.Domains;
using Nop.Web.Framework.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Factories;
using Nop.Services.Media;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Factories
{
    public class PaymentOptionModelFactory : IPaymentOptionModelFactory
    {
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IPictureService _pictureService;

        public PaymentOptionModelFactory(IPaymentOptionService paymentOptionService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            IPictureService pictureService)
        {
            _paymentOptionService = paymentOptionService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _pictureService = pictureService;
        }

        public async Task<PaymentOptionSearchModel> PreparePaymentOptionSearchModelAsync(PaymentOptionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
            return searchModel;
        }

        public async Task<PaymentOptionListModel> PreparePaymentOptionListModelAsync(PaymentOptionSearchModel searchModel)
        {
            var paymentOptions = await _paymentOptionService.GetAllMPay24PaymentOptionsAsync(name: searchModel.SearchName, 
                brand: searchModel.SearchBrand,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new PaymentOptionListModel().PrepareToGridAsync(searchModel, paymentOptions, () =>
            {
                return paymentOptions.SelectAwait(async option =>
                {
                    return await PreparePaymentOptionModelAsync(null, option, true);
                });
            });

            return model;
        }

        public async Task<PaymentOptionModel> PreparePaymentOptionModelAsync(PaymentOptionModel model, PaymentOption paymentOption,
            bool excludeProperties = false)
        {
            if (paymentOption != null)
            {
                if (model == null)
                {
                    model = paymentOption.ToModel<PaymentOptionModel>();
                    model.Logo = await _pictureService.GetPictureUrlAsync(paymentOption.PictureId, 200);
                }
            }

            if (!excludeProperties)
            {
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, paymentOption, excludeProperties);
            }

            return model;
        }
    }
}
