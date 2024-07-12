using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Widgets.MegaMenu.Services;

public interface IMegaMenuCoreService
{
    Task<IList<Category>> GetCategoriesByIdsAsync(int storeId = 0, List<int> selectedIds = null, int pageSize = int.MaxValue, bool showHidden = false);

    Task<IList<Manufacturer>> GetManufacturersByIdsAsync(int storeId = 0, List<int> selectedIds = null, int pageSize = int.MaxValue, bool showHidden = false);
}