using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Opc.Models;

public partial record EstimateShippingModel : BaseNopModel
{
    public EstimateShippingModel()
    {
        AvailableCountries = new List<SelectListItem>();
        AvailableStates = new List<SelectListItem>();
    }

    public int RequestDelay { get; set; }

    public bool Enabled { get; set; }

    [NopResourceDisplayName("NopStation.Opc.ShoppingCart.EstimateShipping.Country")]
    public int? CountryId { get; set; }
    [NopResourceDisplayName("NopStation.Opc.ShoppingCart.EstimateShipping.StateProvince")]
    public int? StateProvinceId { get; set; }
    [NopResourceDisplayName("NopStation.Opc.ShoppingCart.EstimateShipping.ZipPostalCode")]
    public string ZipPostalCode { get; set; }
    public bool UseCity { get; set; }
    [NopResourceDisplayName("NopStation.Opc.ShoppingCart.EstimateShipping.City")]
    public string City { get; set; }

    public IList<SelectListItem> AvailableCountries { get; set; }
    public IList<SelectListItem> AvailableStates { get; set; }
}

public partial record EstimateShippingResultModel : BaseNopModel
{
    public EstimateShippingResultModel()
    {
        ShippingOptions = new List<ShippingOptionModel>();
        Errors = new List<string>();
    }

    public IList<ShippingOptionModel> ShippingOptions { get; set; }

    public bool Success => !Errors.Any();

    public IList<string> Errors { get; set; }

    #region Nested Classes

    public partial record ShippingOptionModel : BaseNopModel
    {
        public string Name { get; set; }

        public string ShippingRateComputationMethodSystemName { get; set; }

        public string Description { get; set; }

        public string Price { get; set; }

        public decimal Rate { get; set; }

        public string DeliveryDateFormat { get; set; }

        public bool Selected { get; set; }
    }

    #endregion Nested Classes
}