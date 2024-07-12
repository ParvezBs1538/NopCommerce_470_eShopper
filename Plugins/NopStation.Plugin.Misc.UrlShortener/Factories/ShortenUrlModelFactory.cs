using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Generate;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Shortenurls;
using NopStation.Plugin.Misc.UrlShortener.Domains;
using NopStation.Plugin.Misc.UrlShortener.Services;

namespace NopStation.Plugin.Misc.UrlShortener.Factories
{
    public class ShortenUrlModelFactory : IShortenUrlModelFactory
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IShortenUrlService _shortenUrlService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly UrlShortenerSettings _urlShortenerSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ShortenUrlModelFactory(IWebHelper webHelper,
            IShortenUrlService shortenUrlService,
            IUrlRecordService urlRecordService,
            UrlShortenerSettings urlShortenerSettings,
            ILocalizationService localizationService,
            ILogger logger)
        {
            _webHelper = webHelper;
            _shortenUrlService = shortenUrlService;
            _urlRecordService = urlRecordService;
            _urlShortenerSettings = urlShortenerSettings;
            _localizationService = localizationService;
            _logger = logger;
        }

        #endregion

        #region Utilities

        private Task<string> GenerateRequestUrl(UrlRecord record)
        {
            var urlFormat = "https://api-ssl.bitly.com/v3/shorten?access_token={0}&longUrl={1}";
            var longUrl = WebUtility.UrlEncode($"{_webHelper.GetStoreLocation()}{record.Slug}");
            var requestUrl = string.Format(urlFormat, _urlShortenerSettings.AccessToken, longUrl);
            return Task.FromResult(requestUrl);
        }

        private async Task<ShortenUrlModel> PrepareShortenUrlModel(UrlRecord record)
        {
            var shortenUrl = await _shortenUrlService.GetShortenUrlByUrlRecordId(record.Id);
            var model = new ShortenUrlModel()
            {
                GlobalHash = shortenUrl?.GlobalHash,
                Hash = shortenUrl?.Hash,
                Id = shortenUrl?.Id ?? 0,
                NewHash = shortenUrl?.NewHash ?? 0,
                ShortUrl = shortenUrl?.ShortUrl,
                EntityName = record.EntityName,
                Slug = record.Slug,
                UrlRecordId = record.Id
            };
            return model;
        }

        #endregion

        #region Methods

        public async Task<ShortenUrlSearchModel> PrepareShortenUrlSearchModel(ShortenUrlSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableUrlEntityNames = await _shortenUrlService.GetUrlEntityNames();

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ShortenUrlListModel> PrepareShortenUrlListModel(ShortenUrlSearchModel searchModel)
        {
            var urls = await _shortenUrlService.GetAllUrlRecordsAsync(searchModel.Slug, searchModel.UrlEntityName, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var gridModel = await new ShortenUrlListModel().PrepareToGridAsync(searchModel, urls, () =>
            {
                return urls.SelectAwait(async url =>
                {
                    var shortenUrlModel = await PrepareShortenUrlModel(url);
                    return shortenUrlModel;
                });
            });

            return gridModel;
        }

        public async Task<SuccessResponseModel> GenerateShortUrls(GenerateShortUrlModel model, bool generateAll = false)
        {
            if (string.IsNullOrEmpty(_urlShortenerSettings.AccessToken))
                throw new InvalidOperationException(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.AccessToken.IsNull"));

            var successResponseModel = new SuccessResponseModel();
            var records = new List<UrlRecord>();
            if (generateAll)
            {
                records = (await _urlRecordService.GetAllUrlRecordsAsync())
                    .ToList();
            }
            else
            {
                records = (await _urlRecordService.GetUrlRecordsByIdsAsync(model.SelectedUrlRecordIds.ToArray()))
                    .ToList();
            }

            foreach (var record in records)
            {
                try
                {
                    var shortenUrl = await _shortenUrlService.GetShortenUrlByUrlRecordId(record.Id);
                    await GenerateShortUrl(record, shortenUrl);
                    successResponseModel.Success++;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message, ex);
                    successResponseModel.Fail++;
                    if (ex.Message == "INVALID_ARG_ACCESS_TOKEN")
                    {
                        successResponseModel.Message = await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.AccessToken.Invalid");
                    }
                    continue;
                }
            }
            return successResponseModel;
        }

        public async Task GenerateShortUrl(UrlRecord record, ShortenUrl shortenUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(await GenerateRequestUrl(record));

            request.Method = "GET";
            request.ContentType = " application/x-www-form-urlencoded";
            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequired;

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            responseString = responseString.Replace("[]", "null");

            var responseModel = JsonConvert.DeserializeObject<CreateShortenResponseModel>(responseString);
            if (responseModel.StatusCode == 200)
            {
                if (shortenUrl == null)
                {
                    var nsu = new ShortenUrl()
                    {
                        GlobalHash = responseModel.Data.GlobalHash,
                        Hash = responseModel.Data.Hash,
                        NewHash = responseModel.Data.NewHash,
                        ShortUrl = responseModel.Data.ShortUrl,
                        UrlRecordId = record.Id,
                        Slug = record.Slug,
                        Deleted = !record.IsActive
                    };
                    await _shortenUrlService.InsertShortenUrl(nsu);
                }
                else
                {
                    shortenUrl.GlobalHash = responseModel.Data.GlobalHash;
                    shortenUrl.Hash = responseModel.Data.Hash;
                    shortenUrl.NewHash = responseModel.Data.NewHash;
                    shortenUrl.ShortUrl = responseModel.Data.ShortUrl;
                    shortenUrl.UrlRecordId = record.Id;
                    shortenUrl.Slug = record.Slug;
                    shortenUrl.Deleted = !record.IsActive;
                    await _shortenUrlService.UpdateShortenUrl(shortenUrl);
                }
            }
            else if (_urlShortenerSettings.EnableLog)
            {
                await _logger.InformationAsync("URL record id: " + record.Id + "; Message: " + responseModel.StatusText);
                throw new InvalidOperationException(responseModel.StatusText);
            }
            else
            {
                throw new InvalidOperationException(responseModel.StatusText);
            }
        }

        #endregion
    }
}
