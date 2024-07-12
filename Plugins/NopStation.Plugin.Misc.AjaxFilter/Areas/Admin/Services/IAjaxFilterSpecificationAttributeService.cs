using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services
{
    public interface IAjaxFilterSpecificationAttributeService
    {
        Task<AjaxFilterSpecificationAttribute> GetSpecificationAttributeByIdAsync(int id);
        Task<AjaxFilterSpecificationAttribute> GetSpecificationAttributeByNameAsync(string name);
        Task<IPagedList<SpecificationAttribute>> GetSpecificationAttributesAsync(AjaxFilterSpecificationAttributeSearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<AjaxFilterSpecificationAttribute>> GetAjaxFilterSpecificationAttributesAsync(AjaxFilterSpecificationAttributeSearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<SpecificationAttribute> GetSpecificationAttributeBySpecificationIdAsync(int id);
        Task DeleteAjaxFilterSpecificationAttribute(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute);
        Task InsertAjaxFilterSpecificationAttributeAsync(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute);
        Task<IList<AjaxFilterSpecificationAttribute>> GetAvailableSpecificationAttributesAsync();
        Task<List<SpecificationAttributeOption>> GetSpecificationAttributeOptionBySpecificationName(string name);
        Task<AjaxFilterSpecificationAttribute> GetAjaxFilterSpecificationAttributeBySpecificationAttributeId(int id);
        Task UpdateAjaxFilterSpecificationAttribute(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute);
    }
}
