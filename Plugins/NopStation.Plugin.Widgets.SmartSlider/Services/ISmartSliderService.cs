using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Services;

public interface ISmartSliderService
{
    #region Sliders

    Task<IPagedList<SmartSlider>> GetAllSlidersAsync(string keywords = null, int storeId = 0,
        int productId = 0, bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null,
        bool validScheduleOnly = false, string widgetZone = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<SmartSlider> GetSliderByIdAsync(int sliderId);

    Task InsertSliderAsync(SmartSlider slider);

    Task UpdateSliderAsync(SmartSlider slider);

    Task DeleteSliderAsync(SmartSlider slider, bool deleteReletedData = true);

    #endregion

    #region Slider items

    Task<IList<SmartSliderItem>> GetSliderItemsBySliderIdAsync(int sliderId, int languageId = 0, int recordsToReturn = 0);

    Task<SmartSliderItem> GetSliderItemByIdAsync(int sliderItemId);

    Task InsertSliderItemAsync(SmartSliderItem sliderItem);

    Task UpdateSliderItemAsync(SmartSliderItem sliderItem);

    Task DeleteSliderItemAsync(SmartSliderItem sliderItem);

    #endregion        
}