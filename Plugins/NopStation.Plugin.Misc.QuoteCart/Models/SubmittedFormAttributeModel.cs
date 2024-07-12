using System.Collections.Generic;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record SubmittedFormAttributeModel : BaseNopModel
{
    public SubmittedFormAttributeModel()
    {
        Values = [];
    }

    public int FormAttributeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public IList<string> Values { get; set; }

    public AttributeControlType AttributeControlType { get; set; }
}
