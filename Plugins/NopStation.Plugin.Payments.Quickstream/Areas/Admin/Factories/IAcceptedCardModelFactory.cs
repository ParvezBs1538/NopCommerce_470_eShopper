using System.Threading.Tasks;
using NopStation.Plugin.Payments.Quickstream.Areas.Admin.Models;
using NopStation.Plugin.Payments.Quickstream.Domains;

namespace NopStation.Plugin.Payments.Quickstream.Areas.Admin.Factories
{
    public interface IAcceptedCardModelFactory
    {
        Task<AcceptedCardSearchModel> PrepareAcceptedCardSearchModelAsync(AcceptedCardSearchModel searchModel, bool prepareExchangeRates = false);

        Task<AcceptedCardListModel> PrepareAcceptedCardListModelAsync(AcceptedCardSearchModel searchModel);

        Task<AcceptedCardModel> PrepareAcceptedCardModelAsync(AcceptedCardModel model, AcceptedCard acceptedCard, bool excludeProperties = false);
    }
}
