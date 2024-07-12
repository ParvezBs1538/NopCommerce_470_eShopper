using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Payments.Quickstream.Domains;
using Nop.Services.Media;

namespace NopStation.Plugin.Payments.Quickstream.Services
{
    public class AcceptedCardService : IAcceptedCardService
    {
        private readonly IRepository<AcceptedCard> _acceptedCardRepository;
        private readonly IPictureService _pictureService;

        public AcceptedCardService(IRepository<AcceptedCard> acceptedCardRepository,
            IPictureService pictureService)
        {
            _acceptedCardRepository = acceptedCardRepository;
            _pictureService = pictureService;
        }

        public async Task<IPagedList<AcceptedCard>> GetAllAcceptedCardsAsync(string cardScheme = "", string cardType = "",
            bool? enabled = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from ac in _acceptedCardRepository.Table
                        where (string.IsNullOrEmpty(cardScheme) || ac.CardScheme == cardScheme) &&
                        (string.IsNullOrEmpty(cardType) || ac.CardType == cardType) &&
                        (!enabled.HasValue || ac.IsEnable == enabled.Value)
                        select ac;
            
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<AcceptedCard> GetAcceptedCardByIdAsync(int id)
        {
            return await _acceptedCardRepository.GetByIdAsync(id);
        }

        public async Task<AcceptedCard> GetAcceptedCardByCardSchemeAndTypeAsync(string cardScheme, string cardType)
        {
            return await _acceptedCardRepository.Table
                .Where(x => x.CardScheme == cardScheme && x.CardType == cardType).FirstOrDefaultAsync();
        }

        public async Task InsertAcceptedCardAsync(AcceptedCard acceptedCard)
        {
            await _acceptedCardRepository.InsertAsync(acceptedCard);
        }

        public async Task UpdateAcceptedCardAsync(AcceptedCard acceptedCard)
        {
            await _acceptedCardRepository.UpdateAsync(acceptedCard);
        }

        public async Task DeleteAcceptedCardAsync(AcceptedCard acceptedCard)
        {
            await _acceptedCardRepository.DeleteAsync(acceptedCard);
        }
    }
}
