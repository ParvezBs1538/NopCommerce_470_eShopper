using System.Collections.Generic;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class SpecificationAttributesModel
    {
        public SpecificationAttributesModel()
        {
            SpecificationAttributeOptions = new List<SpecificationAttributeOptionsModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxDisplayForSpecifiation { get; set; }
        public bool CloseSpecificationAttributeByDefault { get; set; }
        public string AlternateName { get; set; }
        public IList<SpecificationAttributeOptionsModel> SpecificationAttributeOptions { get; set; }
        public int DisplayOrder { get; set; }
        public bool HideProductCount { get; set; }


    }
}
