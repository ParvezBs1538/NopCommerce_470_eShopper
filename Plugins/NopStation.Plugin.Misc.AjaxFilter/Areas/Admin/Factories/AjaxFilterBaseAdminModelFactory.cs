using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories
{
    public class AjaxFilterBaseAdminModelFactory : IAjaxFilterBaseAdminModelFactory
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        #endregion

        #region Ctor
        public AjaxFilterBaseAdminModelFactory(ILocalizationService localizationService,
        ISpecificationAttributeService specificationAttributeService)
        {
            _localizationService = localizationService;
            _specificationAttributeService = specificationAttributeService;
        }
        #endregion

        #region Methods

        public virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
        }

        public virtual async Task PrepareSpecificationAttributeAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available specification attribute groups
            var availableSpecificationAttribute = await _specificationAttributeService.GetSpecificationAttributesAsync();
            foreach (var attribute in availableSpecificationAttribute)
            {
                items.Add(new SelectListItem { Value = attribute.Id.ToString(), Text = attribute.Name });
            }

            // use empty string for nullable field
            var defaultItemValue = string.Empty;

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText, defaultItemValue);
        }

        #endregion
    }
}
