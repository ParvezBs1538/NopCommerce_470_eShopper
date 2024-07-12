using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Media;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Services
{
    public interface ISmartSliderVideoService
    {
        Task<SmartSliderVideo> GetVideoByIdAsync(int id);
        Task<SmartSliderVideo> InsertVideoAsync(IFormFile formFile, string defaultFileName = "", string virtualPath = "");
        Task<(string Url, SmartSliderVideo Video)> GetVideoUrlAsync(SmartSliderVideo picture, PictureType defaultPictureType = PictureType.Entity);

    }

}