using System;
using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class FormAttributeMapping : BaseEntity, ILocalizedEntity
{
    /// <summary>
    /// Gets or sets the form ID
    /// </summary>
    public int QuoteFormId { get; set; }

    /// <summary>
    /// Gets or sets the form attribute ID
    /// </summary>
    public int FormAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the text to prompt
    /// </summary>
    public string TextPrompt { get; set; }

    /// <summary>
    /// Gets or sets whether field is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the attribute control type
    /// </summary>
    public int AttributeControlTypeId { get; set; }

    /// <summary>
    /// Gets or sets the order of display
    /// </summary>
    public int DisplayOrder { get; set; }

    #region Validations

    public int? ValidationMinLength { get; set; }

    public int? ValidationMaxLength { get; set; }

    public DateTime? DefaultDate { get; set; }

    public DateTime? ValidationMinDate { get; set; }

    public DateTime? ValidationMaxDate { get; set; }

    public string ValidationFileAllowedExtensions { get; set; }

    public int? ValidationFileMaximumSize { get; set; }

    #endregion

    /// <summary>
    /// Gets or sets the default value
    /// </summary>
    public string DefaultValue { get; set; }

    public string ConditionAttributeXml { get; set; }

    public AttributeControlType AttributeControlType
    {
        get => (AttributeControlType)AttributeControlTypeId;
        set => AttributeControlTypeId = (int)value;
    }
}
