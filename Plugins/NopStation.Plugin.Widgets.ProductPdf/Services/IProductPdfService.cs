using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.ProductPdf.Services
{
    public interface IProductPdfService
    {
        Task PrintProductToPdfAsync(Stream stream, Product product, Language language = null);
    }
}