using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record AddCustomerToConditionSearchModel : BaseSearchModel, IAclSupportedModel
{
    #region Ctor

    public AddCustomerToConditionSearchModel()
    {
        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public int EntityId { get; set; }

    public string EntityName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.CustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }

    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchEmail")]
    public string SearchEmail { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchUsername")]
    public string SearchUsername { get; set; }

    public bool UsernamesEnabled { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchFirstName")]
    public string SearchFirstName { get; set; }
    public bool FirstNameEnabled { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchLastName")]
    public string SearchLastName { get; set; }
    public bool LastNameEnabled { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchDateOfBirth")]
    public string SearchDayOfBirth { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchDateOfBirth")]
    public string SearchMonthOfBirth { get; set; }

    public bool DateOfBirthEnabled { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchCompany")]
    public string SearchCompany { get; set; }

    public bool CompanyEnabled { get; set; }

    [DataType(DataType.PhoneNumber)]
    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchPhone")]
    public string SearchPhone { get; set; }

    public bool PhoneEnabled { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchZipCode")]
    public string SearchZipPostalCode { get; set; }

    public bool ZipPostalCodeEnabled { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Customers.List.SearchIpAddress")]
    public string SearchIpAddress { get; set; }

    public bool AvatarEnabled { get; internal set; }

    #endregion
}