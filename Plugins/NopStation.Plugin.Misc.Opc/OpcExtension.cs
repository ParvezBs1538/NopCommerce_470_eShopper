using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Common;

namespace NopStation.Plugin.Misc.Opc;

public static class OpcExtension
{
    private static SelectList ToSelectList(this IEnumerable<string> items)
    {
        if (items == null)
            return new SelectList(new string[] { });

        return new SelectList(items);
    }

    public static async Task<IList<SelectListItem>> GetAddressPropertiesAsync(bool excludeOrdineryFields = false)
    {
        var namesToRemove = new List<string> { nameof(Address.CustomAttributes), nameof(Address.CreatedOnUtc) };

        if (excludeOrdineryFields)
            namesToRemove.AddRange(new string[] { nameof(Address.FirstName), nameof(Address.LastName), nameof(Address.Email),
                nameof(Address.Company), nameof(Address.PhoneNumber), nameof(Address.FaxNumber) });

        var fields = typeof(Address).GetProperties().Select(x => x.Name).Except(namesToRemove).ToSelectList().ToList();

        var addressAttributeService = EngineContext.Current.Resolve<IAttributeService<AddressAttribute, AddressAttributeValue>>();
        var addressAttributes = await addressAttributeService.GetAllAttributesAsync();

        foreach (var attribute in addressAttributes)
        {
            fields.Add(new SelectListItem()
            {
                Text = attribute.Name,
                Value = string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id)
            });
        }

        return fields;
    }

    public static IList<string> GetValues(this IEnumerable<SelectListItem> items)
    {
        if (items == null)
            return new List<string>();

        return items.Select(x => x.Value).ToList();
    }

    public static bool TryGetValue<T>(this ISession session, string key, out T value)
    {
        value = default;

        var valueString = session.GetString(key);
        if (valueString == null)
            return false;

        value = JsonConvert.DeserializeObject<T>(valueString);
        return true;
    }
}