using Nop.Core;
using Nop.Core.Domain.Attributes;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

/// <summary>
/// Represents a form attribute value
/// </summary>
public class FormAttributeValue : BaseAttributeValue, ILocalizedEntity
{
    /// <summary>
    /// Gets or sets the form attribute mapping identifier
    /// </summary>
    public int FormAttributeMappingId { get; set; }

    /// <summary>
    /// Gets or sets the color square in rgb format
    /// </summary>
    public string ColorSquaresRgb { get; set; }

    /// <summary>
    /// Gets or sets the image square identifier
    /// </summary>
    public int ImageSquaresPictureId { get; set; }
}
