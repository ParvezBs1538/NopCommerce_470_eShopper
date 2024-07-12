using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class PredefinedFormAttributeValue : BaseEntity, ILocalizedEntity
{
    /// <summary>
    /// Gets or sets the form attribute mapping identifier
    /// </summary>
    public int FormAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the form attribute value
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the form attribute value price adjustment
    /// </summary>
    public bool IsPreSelected { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }
}
