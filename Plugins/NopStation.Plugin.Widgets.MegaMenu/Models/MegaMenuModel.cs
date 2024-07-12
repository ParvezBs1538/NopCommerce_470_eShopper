using System.Collections.Generic;
using System.Linq;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.MegaMenu.Models;

public record MegaMenuModel
{
    public MegaMenuModel()
    {
        Categories = new List<CategoryMenuModel>();
        Manufacturers = new List<ManufacturerMenuModel>();
        Topics = new List<TopicModel>();
    }

    public int MaxCategoryLevelsToShow { get; set; }
    public IList<CategoryMenuModel> Categories { get; set; }
    public IList<TopicModel> Topics { get; set; }
    public bool HideManufacturers { get; set; }
    public IList<ManufacturerMenuModel> Manufacturers { get; set; }

    public bool BlogEnabled { get; set; }
    public bool NewProductsEnabled { get; set; }
    public bool ForumEnabled { get; set; }

    public bool DisplayHomePageMenuItem { get; set; }
    public bool DisplayNewProductsMenuItem { get; set; }
    public bool DisplayProductSearchMenuItem { get; set; }
    public bool DisplayCustomerInfoMenuItem { get; set; }
    public bool DisplayBlogMenuItem { get; set; }
    public bool DisplayForumsMenuItem { get; set; }
    public bool DisplayContactUsMenuItem { get; set; }


    public bool HasOnlyCategories
    {
        get
        {
            return Categories.Any()
                   && !Topics.Any()
                   && !DisplayHomePageMenuItem
                   && !(DisplayNewProductsMenuItem && NewProductsEnabled)
                   && !DisplayProductSearchMenuItem
                   && !DisplayCustomerInfoMenuItem
                   && !(DisplayBlogMenuItem && BlogEnabled)
                   && !(DisplayForumsMenuItem && ForumEnabled)
                   && !DisplayContactUsMenuItem;
        }
    }

    public record TopicModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
    }

    public record CategoryLineModel : BaseNopModel
    {
        public CategoryLineModel()
        {
            Category = new CategoryMenuModel();
        }

        public int MaxLevel { get; set; }
        public int Level { get; set; }
        public CategoryMenuModel Category { get; set; }
    }
}
