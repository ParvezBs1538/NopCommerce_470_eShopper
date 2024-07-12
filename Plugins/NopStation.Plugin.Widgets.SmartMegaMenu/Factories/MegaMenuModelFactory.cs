using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;
using NopStation.Plugin.Widgets.SmartMegaMenu.Infrastructure.Cache;
using NopStation.Plugin.Widgets.SmartMegaMenu.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Services;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Factories;

public class MegaMenuModelFactory : IMegaMenuModelFactory
{
    #region Fields

    private readonly IMegaMenuService _megaMenuService;
    private readonly IAclService _aclService;
    private readonly ILocalizationService _localizationService;
    private readonly IPictureService _pictureService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;
    private readonly IProductTagService _productTagService;
    private readonly ITopicService _topicService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IUrlRecordService _urlRecordService;
    private readonly OrderSettings _orderSettings;
    private readonly SmartMegaMenuSettings _smartMegaMenuSettings;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public MegaMenuModelFactory(IMegaMenuService megaMenuService,
        IAclService aclService,
        ILocalizationService localizationService,
        IPictureService pictureService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IVendorService vendorService,
        IProductTagService productTagService,
        ITopicService topicService,
        IStaticCacheManager staticCacheManager,
        IUrlRecordService urlRecordService,
        OrderSettings orderSettings,
        SmartMegaMenuSettings smartMegaMenuSettings,
        IWebHelper webHelper)
    {
        _megaMenuService = megaMenuService;
        _aclService = aclService;
        _localizationService = localizationService;
        _pictureService = pictureService;
        _workContext = workContext;
        _storeContext = storeContext;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _vendorService = vendorService;
        _productTagService = productTagService;
        _topicService = topicService;
        _staticCacheManager = staticCacheManager;
        _urlRecordService = urlRecordService;
        _orderSettings = orderSettings;
        _smartMegaMenuSettings = smartMegaMenuSettings;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    protected async Task<MegaMenuModel> PrepareMegaMenuModelAsync(MegaMenu megaMenu, IList<MegaMenuItem> menuItems)
    {
        var model = new MegaMenuModel()
        {
            CssClass = megaMenu.CssClass,
            DisplayOrder = megaMenu.DisplayOrder,
            Id = megaMenu.Id,
            Name = await _localizationService.GetLocalizedAsync(megaMenu, mm => mm.Name),
            ViewType = megaMenu.ViewType,
            WithoutImages = megaMenu.WithoutImages
        };

        await PrepareMegaMenuItemsAsync(model.MenuItems, 0, menuItems, megaMenu, await _workContext.GetWorkingLanguageAsync());

        return model;
    }

    protected async Task<PageMenuModel> GetPageDetails(PageType pageType)
    {
        return pageType switch
        {
            PageType.HomePage => new PageMenuModel()
            {
                RouteName = "Homepage",
                Title = await _localizationService.GetResourceAsync("Homepage")
            },
            PageType.ContactUs => new PageMenuModel()
            {
                RouteName = "ContactUs",
                Title = await _localizationService.GetResourceAsync("ContactUs")
            },
            PageType.MyAccount => new PageMenuModel()
            {
                RouteName = "CustomerInfo",
                Title = await _localizationService.GetResourceAsync("Account.CustomerInfo")
            },
            PageType.Login => new PageMenuModel()
            {
                RouteName = "Login",
                Title = await _localizationService.GetResourceAsync("Account.Login")
            },
            PageType.Register => new PageMenuModel()
            {
                RouteName = "Register",
                Title = await _localizationService.GetResourceAsync("Account.Register")
            },
            PageType.NewProducts => new PageMenuModel()
            {
                RouteName = "NewProducts",
                Title = await _localizationService.GetResourceAsync("Products.NewProducts")
            },
            PageType.CompareProducts => new PageMenuModel()
            {
                RouteName = "CompareProducts",
                Title = await _localizationService.GetResourceAsync("Products.Compare.List")
            },
            PageType.RecentlyViewedProducts => new PageMenuModel()
            {
                RouteName = "RecentlyViewedProducts",
                Title = await _localizationService.GetResourceAsync("Products.RecentlyViewedProducts")
            },
            PageType.Manufacturers => new PageMenuModel()
            {
                RouteName = "ManufacturerList",
                Title = await _localizationService.GetResourceAsync("Manufacturers")
            },
            PageType.Vendors => new PageMenuModel()
            {
                RouteName = "VendorList",
                Title = await _localizationService.GetResourceAsync("Vendors")
            },
            PageType.ProductTags => new PageMenuModel()
            {
                RouteName = "ProductTagsAll",
                Title = await _localizationService.GetResourceAsync("Products.Tags.All")
            },
            PageType.Search => new PageMenuModel()
            {
                RouteName = "SearchProducts",
                Title = await _localizationService.GetResourceAsync("Search")
            },
            PageType.Cart => new PageMenuModel()
            {
                RouteName = "ShoppingCart",
                Title = await _localizationService.GetResourceAsync("ShoppingCart")
            },
            PageType.Wishlist => new PageMenuModel()
            {
                RouteName = "Wishlist",
                Title = await _localizationService.GetResourceAsync("Wishlist")
            },
            PageType.Blog => new PageMenuModel()
            {
                RouteName = "Blog",
                Title = await _localizationService.GetResourceAsync("Blog")
            },
            PageType.News => new PageMenuModel()
            {
                RouteName = "NewsArchive",
                Title = await _localizationService.GetResourceAsync("News")
            },
            PageType.Forums => new PageMenuModel()
            {
                RouteName = "ActiveDiscussions",
                Title = await _localizationService.GetResourceAsync("Forum.ActiveDiscussions")
            },
            PageType.Checkout => _orderSettings.OnePageCheckoutEnabled ? new PageMenuModel()
            {
                RouteName = "CheckoutOnePage",
                Title = await _localizationService.GetResourceAsync("Checkout")
            } : new PageMenuModel()
            {
                RouteName = "Checkout",
                Title = await _localizationService.GetResourceAsync("Checkout")
            },
            PageType.Sitemap => new PageMenuModel()
            {
                RouteName = "Sitemap",
                Title = await _localizationService.GetResourceAsync("Sitemap")
            },
            PageType.ApplyVendor => new PageMenuModel()
            {
                RouteName = "ApplyVendorAccount",
                Title = await _localizationService.GetResourceAsync("Vendors.ApplyAccount")
            },
            _ => new PageMenuModel()
            {
                RouteName = "PrivateMessages",
                Title = await _localizationService.GetResourceAsync("PageTitle.PrivateMessages")
            },
        };
    }

    protected async Task PrepareMegaMenuItemsAsync(IList<MegaMenuModel.MenuTreeItemModel> model,
        int parentItemId, IList<MegaMenuItem> menuItems, MegaMenu megaMenu, Language language)
    {
        var children = menuItems.Where(mi => mi.ParentMenuItemId == parentItemId).ToList();

        foreach (var menuItem in children)
        {
            var mim = new MegaMenuModel.MenuTreeItemModel()
            {
                CssClass = menuItem.CssClass,
                Id = menuItem.Id,
                MegaMenuId = menuItem.MegaMenuId,
                PageType = menuItem.PageType,
                MenuItemType = menuItem.MenuItemType,
                OpenInNewTab = menuItem.OpenInNewTab,
                ParentMenuItemId = menuItem.ParentMenuItemId,
                RibbonText = menuItem.RibbonText,
                RibbonBackgroundColor = menuItem.RibbonBackgroundColor,
                ShowRibbonText = menuItem.ShowRibbonText,
                RibbonTextColor = menuItem.RibbonTextColor
            };

            switch (menuItem.MenuItemType)
            {
                case MenuItemType.Category:
                    var category = await _categoryService.GetCategoryByIdAsync(menuItem.CategoryId);
                    if (category == null || category.Deleted || !category.Published)
                        continue;

                    if (!await _aclService.AuthorizeAsync(category))
                        continue;

                    mim.Title = await _localizationService.GetLocalizedAsync(category, c => c.Name);
                    mim.RouteName = "Category";
                    mim.RouteParameter = new { seName = await _urlRecordService.GetSeNameAsync(category) };
                    break;
                case MenuItemType.Manufacturer:
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(menuItem.ManufacturerId);
                    if (manufacturer == null || manufacturer.Deleted || !manufacturer.Published)
                        continue;

                    if (!await _aclService.AuthorizeAsync(manufacturer))
                        continue;

                    mim.Title = await _localizationService.GetLocalizedAsync(manufacturer, m => m.Name);
                    mim.RouteName = "Manufacturer";
                    mim.RouteParameter = new { seName = await _urlRecordService.GetSeNameAsync(manufacturer) };
                    break;
                case MenuItemType.Vendor:
                    var vendor = await _vendorService.GetVendorByIdAsync(menuItem.VendorId);
                    if (vendor == null || vendor.Deleted || !vendor.Active)
                        continue;

                    mim.Title = await _localizationService.GetLocalizedAsync(vendor, v => v.Name);
                    mim.RouteName = "Vendor";
                    mim.RouteParameter = new { seName = await _urlRecordService.GetSeNameAsync(vendor) };
                    break;
                case MenuItemType.Topic:
                    var topic = await _topicService.GetTopicByIdAsync(menuItem.TopicId);
                    if (topic == null || !topic.Published)
                        continue;

                    if (!await _aclService.AuthorizeAsync(topic))
                        continue;

                    mim.Title = await _localizationService.GetLocalizedAsync(menuItem, mi => mi.Title);
                    mim.RouteName = "Topic";
                    mim.RouteParameter = new { seName = await _urlRecordService.GetSeNameAsync(topic) };
                    mim.SeName = await _urlRecordService.GetSeNameAsync(topic);
                    break;
                case MenuItemType.ProductTag:
                    var productTag = await _productTagService.GetProductTagByIdAsync(menuItem.ProductTagId);
                    if (productTag == null)
                        continue;

                    mim.Title = await _localizationService.GetLocalizedAsync(menuItem, mi => mi.Title);
                    mim.RouteName = "ProductsByTag";
                    mim.RouteParameter = new { seName = await _urlRecordService.GetSeNameAsync(productTag) };
                    break;
                case MenuItemType.Page:
                    var pageDetail = GetPageDetails(menuItem.PageType).Result;
                    mim.Title = pageDetail.Title;
                    mim.RouteName = pageDetail.RouteName;
                    break;
                case MenuItemType.CustomLink:
                default:
                    mim.Title = await _localizationService.GetLocalizedAsync(menuItem, mi => mi.Title);
                    mim.Url = await _localizationService.GetLocalizedAsync(menuItem, mi => mi.Url);
                    break;
            }

            if (menuItem.ShowPicture && !megaMenu.WithoutImages)
            {
                var cacheKey = _staticCacheManager.PrepareKey(CacheDefaults.MegaMenuItemPictureModelKey,
                    menuItem,
                    language,
                    _smartMegaMenuSettings.MenuItemPictureSize,
                    _webHelper.IsCurrentConnectionSecured(),
                    _storeContext.GetCurrentStore());

                mim.PictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    string imageUrl, fullSizeImageUrl;
                    var picture = await _pictureService.GetPictureByIdAsync(menuItem.PictureId);
                    (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, _smartMegaMenuSettings.MenuItemPictureSize);
                    (fullSizeImageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);

                    var pm = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        ThumbImageUrl = imageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("NopStation.SmartMegaMenu.Image.Title"), mim.Title),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.SmartMegaMenu.Image.AlterText"), mim.Title),
                    };

                    return pm;
                });

                mim.ShowPicture = true;
            }

            await PrepareMegaMenuItemsAsync(mim.Children, menuItem.Id, menuItems, megaMenu, language);

            model.Add(mim);
        }
    }

    #endregion

    #region Methods

    public async Task<IList<MegaMenuModel>> PrepareMegaMenuModelsAsync(string widgetZone)
    {
        var store = _storeContext.GetCurrentStore();

        var megaMenus = await _megaMenuService.GetAllMegaMenusAsync(
            storeId: store.Id,
            widgetZone: widgetZone);

        var model = new List<MegaMenuModel>();
        foreach (var megaMenu in megaMenus)
        {
            var menuItems = await _megaMenuService.GetAllMegaMenuItemsAsync(megaMenu.Id);
            if (menuItems.Count == 0)
                continue;

            model.Add(await PrepareMegaMenuModelAsync(megaMenu, menuItems));
        }

        return model;
    }

    #endregion
}
