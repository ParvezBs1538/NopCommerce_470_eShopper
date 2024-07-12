using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widget.BlogNews.Areas.Admin.Components;
using NopStation.Plugin.Widget.BlogNews.Components;

namespace NopStation.Plugin.Widget.BlogNews;

public class BlogNewsPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    public bool HideInWidgetList => false;

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly BlogNewsSettings _blogNewsSettings;
    private readonly ISettingService _settingService;
    private readonly INopDataProvider _nopDataProvider;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public BlogNewsPlugin(IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        BlogNewsSettings blogNewsSettings,
        ISettingService settingService,
        INopDataProvider nopDataProvider,
        ILogger logger
        )
    {
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _blogNewsSettings = blogNewsSettings;
        _settingService = settingService;
        _nopDataProvider = nopDataProvider;
        _logger = logger;
    }

    #endregion


    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/BlogNews/Configure";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var widgetZone = string.IsNullOrWhiteSpace(_blogNewsSettings.WidgetZone) ?
            PublicWidgetZones.HomepageBeforeNews : _blogNewsSettings.WidgetZone;

        return Task.FromResult<IList<string>>(new List<string>
        {
            widgetZone,
            AdminWidgetZones.BlogPostDetailsBlock,
            AdminWidgetZones.NewsItemsDetailsBlock
        });
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(BlogNewsPermissionProvider.ManageBlogNews))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("NopStation.BlogNews.Menu.BlogNews"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var categoryIconItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("NopStation.BlogNews.Menu.Configuration"),
                Url = "/Admin/BlogNews/Configure",
                Visible = true,
                IconClass = "fa fa-genderless",
                SystemName = "BlogNews.Configure"
            };
            menuItem.ChildNodes.Add(categoryIconItem);

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/blogs-and-news?categoryId=16",
                Visible = true,
                IconClass = "fa fa-genderless",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }

    public override async Task InstallAsync()
    {
        var settings = new BlogNewsSettings()
        {
            NumberOfBlogPostsToShow = 2,
            BlogPostPictureSize = 400,
            NewsItemPictureSize = 400,
            NumberOfNewsItemsToShow = 2,
            ShowBlogsInStore = true,
            ShowNewsInStore = true,
            WidgetZone = PublicWidgetZones.HomepageBeforeNews
        };
        await _settingService.SaveSettingAsync(settings);
        await this.InstallPluginAsync(new BlogNewsPermissionProvider());


        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new BlogNewsPermissionProvider());


        await base.UninstallAsync();
    }

    #endregion


    #region Nop-station

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>();

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Menu.BlogNews", "Blogs and news"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Menu.Configuration", "Configuration"));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Tab.BlogPost.Picture", "Picture"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Tab.NewsItem.Picture", "Picture"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.BlogPost.Picture.SaveSuccess", "Blog post picture saved successfully."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.NewsItem.Picture.SaveSuccess", "News item picture saved successfully."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.BlogPost.Picture.SaveFailed", "Failed to save blog post picture."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.NewsItem.Picture.SaveFailed", "Failed to save news item picture."));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.Picture", "Picture"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.Picture.Hint", "Choose a picture to upload. If the picture size exceeds your stores max image size setting, it will be automatically resized."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.OverrideAltAttribute", "Alt"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.OverrideAltAttribute.Hint", "Override \"alt\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. blog title)."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.OverrideTitleAttribute", "Title"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.OverrideTitleAttribute.Hint", "Override \"title\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. blog title)."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.ShowInStore", "Show in store"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Blogs.Fields.ShowInStore.Hint", "Check to display this blog on your store. Recommended for your most popular blogs."));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.Picture", "Picture"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.Picture.Hint", "Choose a picture to upload. If the picture size exceeds your stores max image size setting, it will be automatically resized."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.OverrideAltAttribute", "Alt"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.OverrideAltAttribute.Hint", "Override \"alt\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. news title)."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.OverrideTitleAttribute", "Title"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.OverrideTitleAttribute.Hint", "Override \"title\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. news title)."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.ShowInStore", "Show in store"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.News.Fields.ShowInStore.Hint", "Check to display this blog on your store. Recommended for your most popular blogs."));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Button.SaveBlogPostPicture", "Save picture"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Button.SaveNewsItemPicture", "Save picture"));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.BlogPost.SaveBeforeEdit", "You need to save the blog post before you can upload picture for this page."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.NewsItem.SaveBeforeEdit", "You need to save the news item before you can upload picture for this page."));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration", "Blog News settings"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.ShowBlogsInStore", "Show blogs in store"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.ShowBlogsInStore.Hint", "Check to display blog posts in store page."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.BlogPostPictureSize", "Blog post picture size"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.BlogPostPictureSize.Hint", "Determines the value of blog post picture size display in store."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.NumberOfBlogPostsToShow", "Number of blog posts to show"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.NumberOfBlogPostsToShow.Hint", "Determines the number of blog posts to be displayed in store. Keep it 0 to show all blog posts."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.ShowNewsInStore", "Show news in store"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.ShowNewsInStore.Hint", "Check to display news items in store."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.NewsItemPictureSize", "News item picture size"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.NewsItemPictureSize.Hint", "Determines the value of news item picture size display in store."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.NumberOfNewsItemsToShow", "Number of news items to show"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.NumberOfNewsItemsToShow.Hint", "Determines the number of news items to be displayed in store. Keep it 0 to show all news items."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.BlogPosts.Hint", "Go to \"Content management > Blog posts > Edit\", add picture to the blog and check \"Show in store\""));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.NewsItems.Hint", "Go to \"Content management > News items > Edit\", add picture to the news and check \"Show in store\""));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.WidgetZone", "Widget zone"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.WidgetZone.Hint", "Specify the widget zone where the blog and news will be appeared. (i.e. home_page_before_news)"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Fields.WidgetZone.Required", "Widget zone is required."));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Configuration.Updated", "Blog News configuration updated successfully."));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Home.LatestBlog", "Latest Blog"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Home.LatestNews", "Latest News"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Home.LatestBlog.ReadMore", "Read More"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Home.LatestNews.ReadMore", "Read More"));

        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Home.ViewAll.Posts", "View All Posts"));
        list.Add(new KeyValuePair<string, string>("NopStation.BlogNews.Home.ViewAll.News", "View News Archive"));

        return list;
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == AdminWidgetZones.BlogPostDetailsBlock)
            return typeof(BlogPostPictureViewComponent);
        if (widgetZone == AdminWidgetZones.NewsItemsDetailsBlock)
            return typeof(NewsItemPictureViewComponent);
        return typeof(BlogNewsViewComponent);
    }

    public string SystemName => "NopStation.BlogNews";

    #endregion
}
