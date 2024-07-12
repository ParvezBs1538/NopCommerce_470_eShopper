using System;
using System.ComponentModel.DataAnnotations;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

/// <summary>
/// Represents a quote request
/// </summary>
public class QuoteRequest : BaseEntity, ISoftDeletedEntity
{
    [Key]
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the store identifier
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Gets or sets the quote form identifier
    /// </summary>
    public int FormId { get; set; }

    /// <summary>
    /// Gets or sets the guest email
    /// </summary>
    public string GuestEmail { get; set; }

    /// <summary>
    /// Gets or sets the original items to restore
    /// </summary>
    public string OriginalRequestItemsXml { get; set; }

    /// <summary>
    /// Gets or sets the form attributes as XML
    /// </summary>
    public string AttributeXml { get; set; }

    /// <summary>
    /// Gets or sets the quote request status identifier
    /// </summary>
    public int RequestStatusId { get; set; }

    /// <summary>
    /// Gets or sets the time when the quote request was created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the time when the quote request was updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the quote request status
    /// </summary>
    public RequestStatus RequestStatus
    {
        get => (RequestStatus)RequestStatusId;
        set => RequestStatusId = (int)value;
    }

    public bool Deleted { get; set; }
}

/// <summary>
/// Represents a quote request status
/// </summary>
public enum RequestStatus
{
    All = 0,

    Pending = 10,

    Processing = 20,

    Complete = 30,

    Cancelled = 40
}
