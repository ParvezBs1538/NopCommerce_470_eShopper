using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Models.News;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widget.BlogNews.Areas.Admin.Models;
using NopStation.Plugin.Widget.BlogNews.Domains;
using NopStation.Plugin.Widget.BlogNews.Services;

namespace NopStation.Plugin.Widget.BlogNews.Areas.Admin.Components;

public partial class NewsItemPictureViewComponent : NopStationViewComponent
{
    private readonly IPictureService _pictureService;
    private readonly IBlogNewsPictureService _blogNewsPictureService;
    private readonly BlogNewsSettings _blogNewsSettings;

    public NewsItemPictureViewComponent(IPictureService pictureService,
        IBlogNewsPictureService blogNewsPictureService,
        BlogNewsSettings blogNewsSettings)
    {
        _blogNewsPictureService = blogNewsPictureService;
        _blogNewsSettings = blogNewsSettings;
        _pictureService = pictureService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        //if (!_licenseService.IsLicensed())
        //    return Content("");

        var newsItemModel = (NewsItemModel)additionalData;
        var model = new NewsPictureModel();
        model.Id = newsItemModel.Id;

        var blogNewsPicture = _blogNewsPictureService.GetBlogNewsPictureByEntytiId(newsItemModel.Id, EntityType.News);
        if (blogNewsPicture != null)
        {
            model.PictureId = blogNewsPicture.PictureId;
            model.ShowInStore = blogNewsPicture.ShowInStore;

            var picture = await _pictureService.GetPictureByIdAsync(model.PictureId);
            if (picture != null)
            {
                model.OverrideAltAttribute = picture.AltAttribute;
                model.OverrideTitleAttribute = picture.TitleAttribute;
            }
        }

        return View(model);
    }
}
