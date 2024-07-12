using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(QuoteCartItem), "NS_" + nameof(QuoteCartItem) },
        { typeof(QuoteForm), "NS_" + nameof(QuoteForm) },
        { typeof(QuoteRequest), "NS_" + nameof(QuoteRequest) },
        { typeof(QuoteRequestItem), "NS_" + nameof(QuoteRequestItem) },
        { typeof(QuoteRequestMessage), "NS_" + nameof(QuoteRequestMessage) },
        { typeof(QuoteRequestWhitelist), "NS_" + nameof(QuoteRequestWhitelist) },
        { typeof(FormAttribute), "NS_QuoteForm_" + nameof(FormAttribute) },
        { typeof(FormAttributeMapping), "NS_QuoteForm_" + nameof(FormAttributeMapping) },
        { typeof(FormAttributeValue), "NS_QuoteForm_" + nameof(FormAttributeValue) },
        { typeof(PredefinedFormAttributeValue), "NS_QuoteForm_" + nameof(PredefinedFormAttributeValue) },
        { typeof(FormSubmissionAttribute), "NS_QuoteForm_" + nameof(FormSubmissionAttribute) },
    };

    public Dictionary<(Type, string), string> ColumnName => new()
    {
    };
}
