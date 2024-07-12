using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Services.Attributes;
using Nop.Services.Common;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.Core.Extensions;

public static class MappingExtensions
{
    public static async Task<string> ParseCustomAddressAttributesAsync(this NameValueCollection form,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService)
    {
        if (form == null)
            return null;

        var attributesXml = string.Empty;
        var attributes = await addressAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id);
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = addressAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = addressAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await addressAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = addressAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.Trim();
                            attributesXml = addressAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }
                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.FileUpload:
                //not supported address attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    public static NameValueCollection ToNameValueCollection(this List<KeyValueApi> formValues)
    {
        var form = new NameValueCollection();
        if (formValues == null)
            return form;

        foreach (var values in formValues)
        {
            form.Add(values.Key, values.Value);
        }
        return form;
    }

    public static List<T> GetFormValues<T>(this NameValueCollection form, string name, char separator = ',',
        StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries)
    {
        if (form[name] != null)
        {
            return form[name]
                .Split(new[] { separator }, splitOptions)
                .Select(idString => (T)Convert.ChangeType(idString, typeof(T)))
                .Distinct().ToList();
        }

        return new List<T>();
    }

    public static T GetFormValue<T>(this NameValueCollection form, string name)
    {
        if (form[name] != null)
            return (T)Convert.ChangeType(form[name], typeof(T));

        return default;
    }

    public static IList<string> GetErrors(this ModelStateDictionary modelState)
    {
        var errors = new List<string>();
        foreach (var ms in modelState.Values)
            foreach (var error in ms.Errors)
                errors.Add(error.ErrorMessage);

        return errors;
    }

    public static GenericResponseModel<T> ToGenericResponse<T>(this T data)
    {
        return new GenericResponseModel<T>() { Data = data };
    }
}
