using Nop.Core;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

/// <summary>
/// Represents a form submission attribute
/// </summary>
public class FormSubmissionAttribute : BaseEntity
{
    /// <summary>
    /// Gets or sets the form submission identifier
    /// </summary>
    public int QuoteRequestId { get; set; }

    /// <summary>
    /// Gets or sets the form attribute identifier
    /// </summary>
    public int FormAttributeMappingId { get; set; }

    /// <summary>
    /// Gets or sets the form attribute value
    /// </summary>
    public string AttributeValue { get; set; }
}
