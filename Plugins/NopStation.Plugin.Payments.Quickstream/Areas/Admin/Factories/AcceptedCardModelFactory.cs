using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Payments.Quickstream.Areas.Admin.Models;
using Nop.Services.Media;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Payments.Quickstream.Services;
using NopStation.Plugin.Payments.Quickstream.Domains;
using System;

namespace NopStation.Plugin.Payments.Quickstream.Areas.Admin.Factories
{
    public class AcceptedCardModelFactory : IAcceptedCardModelFactory
    {
        private readonly IAcceptedCardService _acceptedCardService;
        private readonly IPictureService _pictureService;

        public AcceptedCardModelFactory(IAcceptedCardService acceptedCardService,
            IPictureService pictureService)
        {
            _acceptedCardService = acceptedCardService;
            _pictureService = pictureService;
        }

        public virtual Task<AcceptedCardSearchModel> PrepareAcceptedCardSearchModelAsync(AcceptedCardSearchModel searchModel, bool prepareExchangeRates = false)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<AcceptedCardListModel> PrepareAcceptedCardListModelAsync(AcceptedCardSearchModel searchModel)
        {
            var acceptCards = await _acceptedCardService.GetAllAcceptedCardsAsync(pageIndex: searchModel.Page -1, pageSize: searchModel.PageSize);

            var model = await new AcceptedCardListModel().PrepareToGridAsync(searchModel, acceptCards, () =>
            {
                return acceptCards.SelectAwait(async card =>
                {
                    return await PrepareAcceptedCardModelAsync(null, card, true);
                });
            });

            return model;
        }

        public async Task<AcceptedCardModel> PrepareAcceptedCardModelAsync(AcceptedCardModel model, AcceptedCard acceptedCard, bool excludeProperties = false)
        {
            if (model == null)
            {
                if (acceptedCard != null)
                {
                    return new AcceptedCardModel
                    {
                        CardScheme = acceptedCard.CardScheme,
                        CardType = acceptedCard.CardType,
                        IsEnable = acceptedCard.IsEnable,
                        Surcharge = acceptedCard.Surcharge,
                        PictureId = acceptedCard.PictureId,
                        PictureUrl = await _pictureService.GetPictureUrlAsync(acceptedCard.PictureId, 200),
                        Id = acceptedCard.Id
                    };
                }
            }

            return model;
        }
    }
}
