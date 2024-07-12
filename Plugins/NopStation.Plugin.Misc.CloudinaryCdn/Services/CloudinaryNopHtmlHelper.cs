using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Seo;
using Nop.Services.Localization;
using Nop.Web.Framework.UI;
using Nop.Web.Framework.WebOptimizer;
using WebOptimizer;

namespace NopStation.Plugin.Misc.CloudinaryCdn.Services
{
    public partial class CloudinaryNopHtmlHelper : NopHtmlHelper
    {
        #region Fields

        private static Cloudinary _cloudinary;
        private static bool _isInitialized;
        private readonly IAssetPipeline _assetPipeline;
        private readonly CloudinaryCdnSettings _cloudinaryCdnSettings;

        private readonly object _locker = new();

        #endregion

        #region Ctor

        public CloudinaryNopHtmlHelper(AppSettings appSettings,
            HtmlEncoder htmlEncoder,
            IActionContextAccessor actionContextAccessor,
            IAssetPipeline assetPipeline,
            Lazy<ILocalizationService> localizationService,
            IStoreContext storeContext,
            INopAssetHelper bundleHelper,
            IUrlHelperFactory urlHelperFactory,
            IWebHostEnvironment webHostEnvironment,
            SeoSettings seoSettings,
            CloudinaryCdnSettings cloudinaryCdnSettings) : base
                (appSettings,
                htmlEncoder,
                actionContextAccessor,
                bundleHelper,
                localizationService,
                storeContext,
                urlHelperFactory,
                webHostEnvironment,
                seoSettings)
        {
            _assetPipeline = assetPipeline;
            _cloudinaryCdnSettings = cloudinaryCdnSettings;
            OneTimeInit();
        }

        #endregion

        #region Utilities

        protected void OneTimeInit()
        {
            if (_isInitialized)
                return;

            if (string.IsNullOrEmpty(_cloudinaryCdnSettings.ApiKey))
                throw new Exception("Cloudinary API key is not specified");

            if (string.IsNullOrEmpty(_cloudinaryCdnSettings.ApiSecret))
                throw new Exception("Cloudinary API Secret is not specified");

            if (string.IsNullOrEmpty(_cloudinaryCdnSettings.CloudName))
                throw new Exception("Cloudinary Cloud Name is not specified");

            lock (_locker)
            {
                if (_isInitialized)
                    return;

                _cloudinary = new Cloudinary(new Account()
                {
                    ApiKey = _cloudinaryCdnSettings.ApiKey,
                    ApiSecret = _cloudinaryCdnSettings.ApiSecret,
                    Cloud = _cloudinaryCdnSettings.CloudName
                });

                _isInitialized = true;
            }
        }

        private string GetAssetKey(string key, ResourceLocation location)
        {
            var keyPrefix = Enum.GetName(location) + key;

            var routeKey = GetRouteName(handleDefaultRoutes: true);

            if (string.IsNullOrEmpty(routeKey))
                return keyPrefix;

            return string.Concat(routeKey, ".", keyPrefix);
        }

        private IAsset GetOrCreateBundle(string bundlePath, Func<string, string[], IAsset> createAsset, WebOptimizerConfig woConfig, bool bundle = false, params string[] sourceFiles)
        {
            if (string.IsNullOrEmpty(bundlePath))
                throw new ArgumentNullException(nameof(bundlePath));

            if (createAsset is null)
                throw new ArgumentNullException(nameof(createAsset));

            if (sourceFiles.Length == 0)
                sourceFiles = new[] { bundlePath };

            //remove the base path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            sourceFiles = sourceFiles.Select(src => src.RemoveApplicationPathFromRawUrl(pathBase)).ToArray();

            if (!_assetPipeline.TryGetAssetFromRoute(bundlePath, out var bundleAsset))
            {
                bundleAsset = createAsset(bundlePath, sourceFiles);
                SaveAsset(bundlePath, bundleAsset, woConfig, bundle);
            }
            else if (bundleAsset.SourceFiles.Count != sourceFiles.Length || !bundleAsset.SourceFiles.SequenceEqual(sourceFiles))
            {
                bundleAsset.SourceFiles.Clear();
                foreach (var source in sourceFiles)
                    bundleAsset.TryAddSourceFile(source);
            }

            return bundleAsset;
        }

        private void SaveAsset(string bundlePath, IAsset bundleAsset, WebOptimizerConfig woConfig, bool bundle = false)
        {
            var binary = bundleAsset.ExecuteAsync(_actionContextAccessor.ActionContext.HttpContext, woConfig).Result;
            using var ms = new MemoryStream(binary);
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(GetFileName(bundlePath), ms),
                PublicId = GetPublicId(bundlePath, bundle)
            };
            var uploadResult = _cloudinary.Upload(uploadParams);
        }

        private string GetFileName(string bundlePath)
        {
            var index = bundlePath.LastIndexOf('/');
            if (index >= 0)
                bundlePath = bundlePath.Substring(index);

            return bundlePath;
        }

        protected string GetPublicId(string bundlePath, bool bundle = false)
        {
            var path = _cloudinaryCdnSettings.PrependCdnFolderName ? $"{_cloudinaryCdnSettings.CdnFolderName}/" : string.Empty;

            var index = bundlePath.LastIndexOf('.');
            if (index >= 0)
                bundlePath = bundlePath[..index].TrimStart('/');

            if (bundle)
                return $"{path}bundles/{bundlePath}";

            return $"{path}{bundlePath}";
        }

        protected string GeFullUrl(string bundlePath, bool bundle = false)
        {
            var localPath = GetPublicId(bundlePath, bundle);

            var index = bundlePath.LastIndexOf('.');
            if (index >= 0)
                localPath += bundlePath[index..];

            return $"https://res.cloudinary.com/{_cloudinaryCdnSettings.CloudName}/raw/upload/{localPath}";
        }

        #endregion

        #region Methods

        public override void AddScriptParts(ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                base.AddScriptParts(location, src, debugSrc, excludeFromBundle);

            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(src))
                return;

            if (!string.IsNullOrEmpty(debugSrc) && _webHostEnvironment.IsDevelopment())
                src = debugSrc;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            _scriptParts[location].Add(new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsLocal = urlHelper.IsLocalUrl(src),
                Src = urlHelper.Content(src)
            });
        }

        public override void AppendScriptParts(ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                base.AppendScriptParts(location, src, debugSrc, excludeFromBundle);

            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(src))
                return;

            if (!string.IsNullOrEmpty(debugSrc) && _webHostEnvironment.IsDevelopment())
                src = debugSrc;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            _scriptParts[location].Insert(0, new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsLocal = urlHelper.IsLocalUrl(src),
                Src = urlHelper.Content(src)
            });
        }

        public override IHtmlContent GenerateScripts(ResourceLocation location)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                return base.GenerateScripts(location);

            if (!_scriptParts.ContainsKey(location) || _scriptParts[location] == null)
                return HtmlString.Empty;

            if (!_scriptParts.Any())
                return HtmlString.Empty;

            var result = new StringBuilder();
            var woConfig = _appSettings.Get<WebOptimizerConfig>();
            if (woConfig.EnableJavaScriptBundling && _scriptParts[location].Any(item => !item.ExcludeFromBundle))
            {
                var bundleKey = string.Concat("/js/", GetAssetKey(woConfig.JavaScriptBundleSuffix, location), ".js");

                var sources = _scriptParts[location]
                    .Where(item => !item.ExcludeFromBundle && item.IsLocal)
                    .Select(item => item.Src)
                    .Distinct().ToArray();

                var bundleAsset = GetOrCreateBundle(bundleKey, _assetPipeline.AddJavaScriptBundle, woConfig, true, sources);

                var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
                result.AppendFormat("<script type=\"{0}\" src=\"{1}?v={2}\"></script>",
                    MimeTypes.TextJavascript, GeFullUrl(bundleKey, true), bundleAsset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext, woConfig));
            }

            var scripts = _scriptParts[location]
                .Where(item => !woConfig.EnableJavaScriptBundling || item.ExcludeFromBundle || !item.IsLocal)
                .Distinct();

            foreach (var item in scripts)
            {
                if (!item.IsLocal)
                {
                    result.AppendFormat("<script type=\"{0}\" src=\"{1}\"></script>", MimeTypes.TextJavascript, item.Src);
                    result.Append(Environment.NewLine);
                    continue;
                }

                var asset = GetOrCreateBundle(item.Src, _assetPipeline.AddJavaScriptBundle, woConfig);

                result.AppendFormat("<script type=\"{0}\" src=\"{1}?v={2}\"></script>",
                    MimeTypes.TextJavascript, GeFullUrl(asset.Route), asset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext, woConfig));

                result.Append(Environment.NewLine);
            }
            return new HtmlString(result.ToString());
        }

        public override IHtmlContent GenerateCssFiles()
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                return base.GenerateCssFiles();

            if (!_cloudinaryCdnSettings.EnableCssCdn)
                return base.GenerateCssFiles();

            if (_cssParts.Count == 0)
                return HtmlString.Empty;

            var debugModel = _webHostEnvironment.IsDevelopment();

            var result = new StringBuilder();

            var woConfig = _appSettings.Get<WebOptimizerConfig>();

            if (woConfig.EnableCssBundling && _cssParts.Any(item => !item.ExcludeFromBundle))
            {
                var bundleSuffix = woConfig.CssBundleSuffix;

                if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                    bundleSuffix += ".rtl";

                var bundleKey = string.Concat("/css/", GetAssetKey(bundleSuffix, ResourceLocation.Head), ".css");

                var sources = _cssParts
                    .Where(item => !item.ExcludeFromBundle && item.IsLocal)
                    .Distinct()
                    //remove the application path from the generated URL if exists
                    .Select(item => item.Src).ToArray();

                var bundleAsset = GetOrCreateBundle(bundleKey, (bundleKey, assetFiles) =>
                {
                    return _assetPipeline.AddBundle(bundleKey, $"{MimeTypes.TextCss}; charset=UTF-8", assetFiles)
                        .EnforceFileExtensions(".css")
                        .AdjustRelativePaths()
                        .Concatenate()
                        .AddResponseHeader(HeaderNames.XContentTypeOptions, "nosniff")
                        .MinifyCss();
                }, woConfig, true, sources);

                var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
                result.AppendFormat("<link rel=\"stylesheet\" type=\"{0}\" href=\"{1}?v={2}\" />",
                    MimeTypes.TextCss, GeFullUrl(bundleKey, true), bundleAsset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext, woConfig));
            }

            var styles = _cssParts
                    .Where(item => !woConfig.EnableCssBundling || item.ExcludeFromBundle || !item.IsLocal)
                    .Distinct();

            foreach (var item in styles)
            {
                if (!item.IsLocal)
                {
                    result.AppendFormat("<link rel=\"stylesheet\" type=\"{0}\" href=\"{1}\" />", MimeTypes.TextCss, item.Src);
                    result.Append(Environment.NewLine);
                    continue;
                }

                var asset = GetOrCreateBundle(item.Src, (bundleKey, assetFiles) =>
                {
                    return _assetPipeline.AddBundle(bundleKey, $"{MimeTypes.TextCss}; charset=UTF-8", assetFiles)
                        .EnforceFileExtensions(".css")
                        .AdjustRelativePaths()
                        .Concatenate()
                        .AddResponseHeader(HeaderNames.XContentTypeOptions, "nosniff")
                        .MinifyCss();
                }, woConfig);

                result.AppendFormat("<link rel=\"stylesheet\" type=\"{0}\" href=\"{1}?v={2}\" />",
                    MimeTypes.TextCss, GeFullUrl(asset.Route), asset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext, woConfig));
                result.AppendLine();
            }

            return new HtmlString(result.ToString());
        }

        public override void AddCssFileParts(string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                base.AddCssFileParts(src, debugSrc, excludeFromBundle);

            if (string.IsNullOrEmpty(src))
                return;

            if (!string.IsNullOrEmpty(debugSrc) && _webHostEnvironment.IsDevelopment())
                src = debugSrc;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            _cssParts.Add(new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsLocal = urlHelper.IsLocalUrl(src),
                Src = urlHelper.Content(src)
            });
        }

        public override void AppendCssFileParts(string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                base.AppendCssFileParts(src, debugSrc, excludeFromBundle);

            if (string.IsNullOrEmpty(src))
                return;

            if (!string.IsNullOrEmpty(debugSrc) && _webHostEnvironment.IsDevelopment())
                src = debugSrc;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            _cssParts.Insert(0, new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsLocal = urlHelper.IsLocalUrl(src),
                Src = urlHelper.Content(src)
            });
        }

        #endregion
    }
}