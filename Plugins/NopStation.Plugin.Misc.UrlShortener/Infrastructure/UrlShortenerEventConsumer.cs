using System.Threading.Tasks;
using Nop.Core.Domain.Seo;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Misc.UrlShortener.Factories;
using NopStation.Plugin.Misc.UrlShortener.Services;

namespace NopStation.Plugin.Misc.UrlShortener
{
    public class UrlShortenerEventConsumer : IConsumer<EntityInsertedEvent<UrlRecord>>,
        IConsumer<EntityUpdatedEvent<UrlRecord>>,
        IConsumer<EntityDeletedEvent<UrlRecord>>
    {
        private readonly IShortenUrlService _shortenUrlService;
        private readonly IShortenUrlModelFactory _shortenUrlModelFactory;

        public UrlShortenerEventConsumer(IShortenUrlService shortenUrlService,
            IShortenUrlModelFactory shortenUrlModelFactory)
        {
            _shortenUrlService = shortenUrlService;
            _shortenUrlModelFactory = shortenUrlModelFactory;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<UrlRecord> eventMessage)
        {
            await ImplementEvent(eventMessage.Entity);
        }

        private async Task ImplementEvent(UrlRecord entity)
        {
            var shortenUrl = await _shortenUrlService.GetShortenUrlByUrlRecordId(entity.Id);
            if (entity.IsActive)
            {
                await _shortenUrlModelFactory.GenerateShortUrl(entity, shortenUrl);
            }
            else
            {
                if (shortenUrl != null)
                {
                    shortenUrl.Deleted = true;
                    await _shortenUrlService.UpdateShortenUrl(shortenUrl);
                }
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<UrlRecord> eventMessage)
        {
            await ImplementEvent(eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<UrlRecord> eventMessage)
        {
            await ImplementEvent(eventMessage.Entity);
        }
    }
}
