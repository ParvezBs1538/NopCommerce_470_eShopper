using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record FormAttributeConditionModel : BaseNopModel
{
    public FormAttributeConditionModel()
    {
        FormAttributes = [];
    }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.EnableCondition")]
    public bool EnableCondition { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.Attributes")]
    public int SelectedFormAttributeId { get; set; }
    public IList<FormAttributeModel> FormAttributes { get; set; }

    public int FormAttributeMappingId { get; set; }

    #region Nested classes

    public partial record FormAttributeModel : BaseNopEntityModel
    {
        public FormAttributeModel()
        {
            Values = [];
        }

        public int FormAttributeId { get; set; }

        public string Name { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<FormAttributeValueModel> Values { get; set; }
    }

    public partial record FormAttributeValueModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }

    #endregion
}
