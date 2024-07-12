using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class QuoteForm : BaseEntity, IStoreMappingSupported, ILocalizedEntity, IAclSupported, ISoftDeletedEntity
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string Info { get; set; }

    /// <summary>
    /// Gets or sets whether the form is active
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
    /// </summary>
    public bool LimitedToStores { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send email to customer
    /// </summary>
    public bool SendEmailToCustomer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send email to admin
    /// </summary>
    public bool SendEmailToStoreOwner { get; set; }

    /// <summary>
    /// Gets or sets the email address of store owner
    /// </summary>
    public string StoreOwnerEmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the form submit button text
    /// </summary>
    public string SubmitButtonText { get; set; }

    /// <summary>
    /// Gets or sets whether to show terms and conditions
    /// </summary>
    public bool ShowTermsAndConditionsCheckbox { get; set; }

    public string TermsAndConditions { get; set; }

    /// <summary>
    /// Gets or sets the default email for sending emails
    /// </summary>
    public int DefaultEmailAccountId { get; set; }

    /// <summary>
    /// Gets or sets the order of display
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is limited/restricted to certain customer roles
    /// </summary>
    public bool SubjectToAcl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted
    /// </summary>
    public bool Deleted { get; set; }
}
