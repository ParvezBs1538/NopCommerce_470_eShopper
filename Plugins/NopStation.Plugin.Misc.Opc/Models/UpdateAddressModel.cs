using System.Collections.Generic;

namespace NopStation.Plugin.Misc.Opc.Models;

public class UpdateAddressModel
{
    public UpdateAddressModel()
    {
        AddressFieldsValues = new List<AddressFieldsValues>();
    }

    public string Field { get; set; }


    public string Value { get; set; }
    public int AddressId { get; set; }

    public string AddressType { get; set; }
    public IList<AddressFieldsValues> AddressFieldsValues { get; set; }
    public bool IsNewAddress { get; set; }
}

public class AddressFieldsValues
{
    public string Field { get; set; }
    public string Value { get; set; }
}