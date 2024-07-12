using Nop.Core.Domain.Attributes;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

/// <summary>
/// Represents a form attribute
/// </summary>
public class FormAttribute : BaseAttribute, ILocalizedEntity
{
    /// <summary>
    /// Gets or sets the form attribute description
    /// </summary>
    public string Description { get; set; }
}
