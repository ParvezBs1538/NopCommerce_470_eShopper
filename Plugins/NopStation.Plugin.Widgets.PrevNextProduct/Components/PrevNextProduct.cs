using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.PrevNextProduct.Models;
using NopStation.Plugin.Widgets.PrevNextProduct.Services;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Components;

public class PrevNextProductViewComponent : NopStationViewComponent
{
    private readonly IPrevNextProductService _prevNextProductService;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly PrevNextProductSettings _prevNextProductSettings;

    public PrevNextProductViewComponent(IPrevNextProductService prevNextProductService,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        PrevNextProductSettings prevNextProductSettings)
    {
        _prevNextProductService = prevNextProductService;
        _localizationService = localizationService;
        _urlRecordService = urlRecordService;
        _prevNextProductSettings = prevNextProductSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (additionalData.GetType() != typeof(ProductDetailsModel))
            return Content("");

        var productId = (additionalData as ProductDetailsModel).Id;

        var data = await _prevNextProductService.GetProductsAsync(productId);
        if (data.Next == null && data.Previous == null)
            return Content("");

        var model = new PublicInfoModel();
        if (data.Next != null)
        {
            model.Next.HasProduct = true;
            model.Next.Id = data.Next.Id;
            model.Next.Name = await _localizationService.GetLocalizedAsync(data.Next, x => x.Name);
            model.Next.SeName = await _urlRecordService.GetSeNameAsync(data.Next);

            if (_prevNextProductSettings.ProductNameMaxLength > 0 && model.Next.Name.Length > _prevNextProductSettings.ProductNameMaxLength)
                model.Next.Name = model.Next.Name[.._prevNextProductSettings.ProductNameMaxLength];
        }

        if (data.Previous != null)
        {
            model.Previous.HasProduct = true;
            model.Previous.Id = data.Previous.Id;
            model.Previous.Name = await _localizationService.GetLocalizedAsync(data.Previous, x => x.Name);
            model.Previous.SeName = await _urlRecordService.GetSeNameAsync(data.Previous);

            if (_prevNextProductSettings.ProductNameMaxLength > 0 && model.Previous.Name.Length > _prevNextProductSettings.ProductNameMaxLength)
                model.Previous.Name = model.Previous.Name[.._prevNextProductSettings.ProductNameMaxLength];
        }

        return View(model);
    }
}
