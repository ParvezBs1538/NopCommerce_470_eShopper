using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Tax.TaxJar.Models
{
    public record TaxJarTransactionLogSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.NopStation.TaxJar.Log.Search.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.TaxJar.Log.Search.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }

        #endregion
    }
}
