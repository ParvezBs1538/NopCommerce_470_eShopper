using Nop.Core;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class QuoteRequestWhitelist : BaseEntity
{
    /// <summary>
    /// Gets or sets the entity identifier
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Gets or sets the entity name
    /// </summary>
    public string EntityName { get; set; }
}
