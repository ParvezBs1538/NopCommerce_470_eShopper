using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Services
{
    public interface IFlipbookService
    {
        Task<Flipbook> GetFlipbookByIdAsync(int id);

        Task InsertFlipbookAsync(Flipbook flipbook);

        Task UpdateFlipbookAsync(Flipbook flipbook);

        Task DeleteFlipbookAsync(Flipbook flipbook);

        Task<IPagedList<Flipbook>> SearchFlipbooksAsync(string name = null, bool? includeInTopMenu = null,
            bool? active = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<FlipbookContent> GetFlipbookContentByIdAsync(int id);

        Task InsertFlipbookContentAsync(FlipbookContent flipbookContent);

        Task UpdateFlipbookContentAsync(FlipbookContent flipbookContent);

        Task DeleteFlipbookContentAsync(FlipbookContent flipbookContent);

        Task<IList<FlipbookContent>> GetFlipbookContentsByFlipbookIdAsync(int flipbookId);

        Task<IList<FlipbookContentProduct>> GetFlipbookContentProductsByContentIdAsync(int flipbookContentId);

        Task<IPagedList<Product>> GetProductsByFlipbookContentIdAsync(int flipbookContentId, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<FlipbookContentProduct> GetFlipbookContentProductAsync(int flipbookContentId, int productId);

        Task DeleteFlipbookContentProductAsync(FlipbookContentProduct flipbookContentProduct);

        Task<FlipbookContentProduct> GetFlipbookContentProductByIdAsync(int flipbookContentProductId);

        Task InsertFlipbookContentProductAsync(FlipbookContentProduct flipbookContentProduct);

        Task UpdateFlipbookContentProductAsync(FlipbookContentProduct flipbookContentProduct);
    }
}
