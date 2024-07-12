using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public interface IQuoteWhitelistModelFactory
{
    Task<ProductListModel> PrepareAddProductListModelAsync(ProductSearchModel searchModel);

    Task<CategoryListModel> PrepareCategoryListModelAsync(CategorySearchModel searchModel);

    Task<ManufacturerListModel> PrepareManufacturerListModelAsync(ManufacturerSearchModel searchModel);
}