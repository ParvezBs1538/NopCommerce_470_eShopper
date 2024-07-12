using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories
{
    public partial interface IAjaxFilterSpecificationAttributeModelFactory
    {
        Task<AjaxFilterSpecificationAttributeSearchModel> PrepareAjaxFilterSpecificationAttributeSearchModel(AjaxFilterSpecificationAttributeSearchModel searchModel);
        Task<AjaxFilterSpecificationAttributeListModel> PrepareAjaxFilterSpecificationAttributeListModelAsync(AjaxFilterSpecificationAttributeSearchModel searchModel);
        Task<SpecificationAttributeListModel> PrepareSpecificationAttributeListModelAsync(AjaxFilterSpecificationAttributeSearchModel searchModel);

        Task<AjaxFilterSpecificationAttributeModel> PrepareAjaxFilterSpecificationAttributeAsync(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute);

        Task<AjaxFilterSpecificationAttribute> PrepareSpecificationAttributeAsync(AjaxFilterSpecificationAttributeModel ajaxFilterSpecificationAttributeModel);
    }
}
