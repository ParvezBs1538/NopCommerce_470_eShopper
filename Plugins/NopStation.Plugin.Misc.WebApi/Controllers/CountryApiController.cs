using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Models.Directory;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/country")]
    public class CountryApiController : BaseApiController
    {
        #region Fields

        private readonly ICountryModelFactory _countryModelFactory;

        #endregion

        #region Ctor

        public CountryApiController(ICountryModelFactory countryModelFactory)
        {
            _countryModelFactory = countryModelFactory;
        }

        #endregion

        #region States / provinces

        [HttpGet("getstatesbycountryid/{countryId}/{addSelectStateItem?}")]
        public async Task<IActionResult> GetStatesByCountryId(int countryId, bool addSelectStateItem = false)
        {
            var response = new GenericResponseModel<List<StateProvinceModel>>();
            response.Data = (await _countryModelFactory.GetStatesByCountryIdAsync(countryId, addSelectStateItem)).ToList();
            return Ok(response);
        }

        #endregion
    }
}
