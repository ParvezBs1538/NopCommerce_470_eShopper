using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Security;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public class AdminApiSiteMapModelFactory : IAdminApiSiteMapModelFactory
{
    private readonly INopFileProvider _nopFileProvider;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;

    public AdminApiSiteMapModelFactory(INopFileProvider nopFileProvider,
        IPermissionService permissionService,
        ILocalizationService localizationService)
    {
        _nopFileProvider = nopFileProvider;
        _permissionService = permissionService;
        _localizationService = localizationService;
    }

    /// <param name="physicalPath">File path to load a sitemap</param>
    public virtual async Task<ApiSiteMapNodeModel> LoadFromAsync(string physicalPath)
    {
        var rootNode = new ApiSiteMapNodeModel();
        var filePath = _nopFileProvider.MapPath(physicalPath);
        var content = await _nopFileProvider.ReadAllTextAsync(filePath, Encoding.UTF8);

        if (!string.IsNullOrEmpty(content))
        {
            var doc = new XmlDocument();
            using (var sr = new StringReader(content))
            {
                using var xr = XmlReader.Create(sr,
                    new XmlReaderSettings
                    {
                        CloseInput = true,
                        IgnoreWhitespace = true,
                        IgnoreComments = true,
                        IgnoreProcessingInstructions = true
                    });

                doc.Load(xr);
            }
            if ((doc.DocumentElement != null) && doc.HasChildNodes)
            {
                var xmlRootNode = doc.DocumentElement.FirstChild;
                await IterateAsync(rootNode, xmlRootNode);
            }
        }

        return rootNode;
    }

    private async Task IterateAsync(ApiSiteMapNodeModel siteMapNode, XmlNode xmlNode)
    {
        await PopulateNodeAsync(siteMapNode, xmlNode);

        foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
        {
            if (xmlChildNode.LocalName.Equals("siteMapNode", StringComparison.InvariantCultureIgnoreCase))
            {
                var siteMapChildNode = new ApiSiteMapNodeModel();
                siteMapNode.ChildNodes.Add(siteMapChildNode);

                await IterateAsync(siteMapChildNode, xmlChildNode);
            }
        }
    }

    private async Task PopulateNodeAsync(ApiSiteMapNodeModel siteMapNode, XmlNode xmlNode)
    {
        //system name
        siteMapNode.SystemName = GetStringValueFromAttribute(xmlNode, "SystemName");

        //title
        var nopResource = GetStringValueFromAttribute(xmlNode, "nopResource");
        siteMapNode.Title = await _localizationService.GetResourceAsync(nopResource);

        //routes, url
        var controllerName = GetStringValueFromAttribute(xmlNode, "controller");
        var actionName = GetStringValueFromAttribute(xmlNode, "action");
        var url = GetStringValueFromAttribute(xmlNode, "url");
        if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
        {
            siteMapNode.Url = $"api/a/{controllerName}/{actionName}";
        }
        else if (!string.IsNullOrEmpty(url))
        {
            siteMapNode.Url = url;
        }

        //image URL
        siteMapNode.IconClass = GetStringValueFromAttribute(xmlNode, "IconClass");

        //permission name
        var permissionNames = GetStringValueFromAttribute(xmlNode, "PermissionNames");
        if (!string.IsNullOrEmpty(permissionNames))
        {
            siteMapNode.Visible = await permissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .AnyAwaitAsync(async permissionName => await _permissionService.AuthorizeAsync(permissionName.Trim()));
        }
        else
        {
            siteMapNode.Visible = true;
        }

        // Open URL in new tab
        var openUrlInBrowser = GetStringValueFromAttribute(xmlNode, "OpenUrlInBrowser");
        if (!string.IsNullOrWhiteSpace(openUrlInBrowser) && bool.TryParse(openUrlInBrowser, out var booleanResult))
        {
            siteMapNode.OpenUrlInBrowser = booleanResult;
        }
    }

    private static string GetStringValueFromAttribute(XmlNode node, string attributeName)
    {
        string value = null;

        if (node.Attributes == null || node.Attributes.Count == 0)
            return value;

        var attribute = node.Attributes[attributeName];

        if (attribute != null)
        {
            value = attribute.Value;
        }

        return value;
    }
}
