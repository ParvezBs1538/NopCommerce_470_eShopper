using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Infrastructure;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Services
{
    public class ProductImageSettingService : IProductImageSettingService
    {
        #region Properties

        private readonly IRepository<ProductImageSetting360> _imageSettingRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctors

        public ProductImageSettingService(IRepository<ProductImageSetting360> imageSettingRepository,
            IStaticCacheManager staticCacheManager)
        {
            _imageSettingRepository = imageSettingRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<ProductImageSetting360> GetImageSettingByIdAsync(int id)
        {
            var query = _imageSettingRepository.Table;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(Picture360CacheKeys.ImageSettingCacheKey, id);

            var cachedSetting = await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync(c => c.Id == id));

            return cachedSetting;
        }

        public virtual async Task AddOrUpdateImageSettingAsync(ImageSetting360Model settingModel)
        {
            if (settingModel == null)
                throw new ArgumentNullException();

            if (settingModel.Id > 0)
            {
                var existingSetting = await _imageSettingRepository.GetByIdAsync(settingModel.Id);
                if (existingSetting == null)
                    throw new ArgumentNullException(nameof(settingModel));

                //map manually
                existingSetting.ProductId = settingModel.ProductId;
                existingSetting.IsEnabled = settingModel.IsEnabled;
                existingSetting.IsLoopEnabled = settingModel.IsLoopEnabled;
                existingSetting.IsPanoramaEnabled = settingModel.IsPanoramaEnabled;
                existingSetting.IsZoomEnabled = settingModel.IsZoomEnabled;
                existingSetting.BehaviorTypeId = settingModel.BehaviorTypeId;

                await _imageSettingRepository.UpdateAsync(existingSetting);

                await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureSetting360Prefix, existingSetting.Id);
                return;
            }

            var query = _imageSettingRepository.Table;
            var oldSetting = await query.Where(c => c.ProductId == settingModel.ProductId).FirstOrDefaultAsync();

            if (oldSetting != null)
            {
                //map manually
                oldSetting.ProductId = settingModel.ProductId;
                oldSetting.IsEnabled = settingModel.IsEnabled;
                oldSetting.IsLoopEnabled = settingModel.IsLoopEnabled;
                oldSetting.IsPanoramaEnabled = settingModel.IsPanoramaEnabled;
                oldSetting.IsZoomEnabled = settingModel.IsZoomEnabled;
                oldSetting.BehaviorTypeId = settingModel.BehaviorTypeId;

                await _imageSettingRepository.UpdateAsync(oldSetting);

                await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureSetting360Prefix, oldSetting.Id);
                return;
            }

            var setting = settingModel.ToEntity<ProductImageSetting360>();

            await _imageSettingRepository.InsertAsync(setting);
        }

        public virtual async Task DeleteImageSettingAsync(ProductImageSetting360 setting)
        {
            await _imageSettingRepository.DeleteAsync(setting);

            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureSetting360Prefix, setting.Id);
        }

        public virtual async Task<ProductImageSetting360> GetImageSettingByProductIdAsync(int productId)
        {
            var imageSettings = from st in _imageSettingRepository.Table
                                where st.ProductId == productId
                                select st;

            return await imageSettings.FirstOrDefaultAsync();
        }

        #endregion
    }
}
