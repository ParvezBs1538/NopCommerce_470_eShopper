using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.Quickstream.Domains;

namespace NopStation.Plugin.Payments.Quickstream.Services
{
    public interface IAcceptedCardService
    {
        Task<IPagedList<AcceptedCard>> GetAllAcceptedCardsAsync(string cardScheme = "", string cardType = "",
            bool? enabled = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<AcceptedCard> GetAcceptedCardByIdAsync(int id);

        Task<AcceptedCard> GetAcceptedCardByCardSchemeAndTypeAsync(string cardScheme, string cardType);

        Task InsertAcceptedCardAsync(AcceptedCard acceptedCard);

        Task UpdateAcceptedCardAsync(AcceptedCard acceptedCard);

        Task DeleteAcceptedCardAsync(AcceptedCard acceptedCard);
    }
}
