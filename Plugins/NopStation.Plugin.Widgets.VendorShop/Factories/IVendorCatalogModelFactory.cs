using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Widgets.VendorShop.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public interface IVendorCatalogModelFactory
    {
        Task<CatalogProductsModel> PrepareVendorProductsModelAsync(Vendor vendor, CatalogProductsExtensionCommand command);
        Task<VendorModel> PrepareVendorModelAsync(Vendor vendor, CatalogProductsExtensionCommand command);
    }
}