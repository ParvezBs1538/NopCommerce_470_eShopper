using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Flipbooks.Models;
using NopStation.Plugin.Widgets.Flipbooks.Services;
using Nop.Services.Localization;
using Nop.Services.Seo;

namespace NopStation.Plugin.Widgets.Flipbooks.Components
{
    public class FlipbookViewComponent : NopStationViewComponent
    {
        private readonly IFlipbookService _flipbookService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;

        public FlipbookViewComponent(IFlipbookService flipbookService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService)
        {
            _flipbookService = flipbookService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var flipbooks = await (await _flipbookService.SearchFlipbooksAsync(active: true, includeInTopMenu: true))
                .SelectAwait(async t => new TopMenuModel.FlipbookModel
                {
                    Id = t.Id,
                    Name = await _localizationService.GetLocalizedAsync(t, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(t)
                }).ToListAsync();

            var model = new TopMenuModel();
            model.Flipbooks = flipbooks;

            return View(model);
        }
    }
}
