using System.Collections.Generic;

namespace NopStation.Plugin.Widget.BlogNews.Models;

public class PublicModel
{
    public PublicModel()
    {
        BlogPosts = new List<BlogPostModel>();
        NewsItems = new List<NewsItemModel>();
    }

    public IList<BlogPostModel> BlogPosts { get; set; }
    public IList<NewsItemModel> NewsItems { get; set; }
}
