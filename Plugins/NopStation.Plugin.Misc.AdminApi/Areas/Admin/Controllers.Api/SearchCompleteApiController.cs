using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/searchcomplete/[action]")]
public class SearchCompleteApiController : BaseAdminApiController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public SearchCompleteApiController(
        IPermissionService permissionService,
        IProductService productService,
        IWorkContext workContext)
    {
        _permissionService = permissionService;
        _productService = productService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    [HttpGet("{term}")]
    public virtual async Task<IActionResult> SearchAutoComplete(string term)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AdminApiAccessDenied();

        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return BadRequest($"Search term minimum length should be {searchTermMinimumLength}");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        var vendorId = 0;
        if (currentVendor != null)
        {
            vendorId = currentVendor.Id;
        }

        //products
        const int productNumber = 15;
        var products = await _productService.SearchProductsAsync(0,
            vendorId: vendorId,
            keywords: term,
            pageSize: productNumber,
            showHidden: true);

        var result = products
            .Select(p => new
            {
                label = p.Name,
                productid = p.Id
            }).ToList();

        var response = new GenericResponseModel<object>
        {
            Data = result
        };

        return Ok(response);
    }

    #endregion
}