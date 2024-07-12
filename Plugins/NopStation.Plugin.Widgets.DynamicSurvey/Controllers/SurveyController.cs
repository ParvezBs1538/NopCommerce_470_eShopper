using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Factories;
using NopStation.Plugin.Widgets.DynamicSurvey.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Services;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Controllers
{
    public class SurveyController : NopStationPublicController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISurveyModelFactory _surveyModelFactory;
        private readonly ISurveyService _surveyService;
        private readonly ISurveyAttributeService _surveyAttributeService;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly ISurveyAttributeFormatter _surveyAttributeFormatter;
        private readonly IWorkContext _workContext;
        private readonly DynamicSurveySettings _dynamicSurveySettings;

        #endregion

        #region Ctor

        public SurveyController(IAclService aclService,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            ISurveyModelFactory surveyModelFactory,
            ISurveyService surveyService,
            ISurveyAttributeService surveyAttributeService,
            ISurveyAttributeParser surveyAttributeParser,
            ISurveyAttributeFormatter surveyAttributeFormatter,
            IWorkContext workContext,
            DynamicSurveySettings dynamicSurveySettings)
        {
            _aclService = aclService;
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _surveyModelFactory = surveyModelFactory;
            _surveyService = surveyService;
            _surveyAttributeService = surveyAttributeService;
            _surveyAttributeParser = surveyAttributeParser;
            _surveyAttributeFormatter = surveyAttributeFormatter;
            _workContext = workContext;
            _dynamicSurveySettings = dynamicSurveySettings;
        }

        #endregion

        #region Utilities

        public async Task<bool> CanSubmitResponseAsync(Survey survey, Customer customer)
        {
            if (survey.AllowMultipleSubmissions)
                return true;

            var submission = (await _surveyService.GetSurveySubmissionsBySurveyIdAsync(survey.Id,
                customer.Id, true)).FirstOrDefault();

            return submission == null;
        }

        protected virtual async Task<bool> IsMinimumSubmissionIntervalValidAsync(Survey survey, Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_dynamicSurveySettings.MinimumIntervalToSubmitSurvey == 0)
                return true;

            var submission = (await _surveyService.GetSurveySubmissionsBySurveyIdAsync(survey.Id,
                customer.Id, true)).FirstOrDefault();
            if (submission == null)
                return true;

            var interval = DateTime.UtcNow - submission.CreatedOnUtc;
            return interval.TotalSeconds > _dynamicSurveySettings.MinimumIntervalToSubmitSurvey;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> SurveyDetails(int surveyId)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(surveyId);
            if (survey == null || survey.Deleted)
                return InvokeHttp404();

            //allow administrators to preview any survey
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey);

            var notAvailable =
                //published?
                !survey.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(survey) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(survey);

            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await CanSubmitResponseAsync(survey, customer))
            {
                return View(new SurveyModel()
                {
                    SuccessfullySent = true,
                    Result = await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.YourEnquiryAlreadySent")
                });
            }

            //display "edit" (manage) link
            if (hasAdminAccess)
                DisplayEditLink(Url.Action("Edit", "Survey", new { id = survey.Id, area = AreaNames.ADMIN }));

            var model = await _surveyModelFactory.PrepareSurveyModelByIdAsync(survey);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> SurveyDetails(int surveyId, IFormCollection form)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(surveyId);
            if (survey == null || survey.Deleted)
                return InvokeHttp404();

            //allow administrators to preview any survey
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey);

            var notAvailable =
                //published?
                !survey.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(survey) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(survey);

            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //display "edit" (manage) link
            if (hasAdminAccess)
                DisplayEditLink(Url.Action("Edit", "Survey", new { id = survey.Id, area = AreaNames.ADMIN }));

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await CanSubmitResponseAsync(survey, customer))
            {
                return View(new SurveyModel()
                {
                    SuccessfullySent = true,
                    Result = await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.YourEnquiryAlreadySent")
                });
            }

            var attributeData = await _surveyAttributeParser.ParseSurveyAttributesAsync(survey, form);

            if (!await IsMinimumSubmissionIntervalValidAsync(survey, customer))
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.MinSubmissionInterval"));
                var model1 = await _surveyModelFactory.PrepareSurveyModelByIdAsync(survey, attributeData.AttributesXml);
                return View(model1);
            }

            var warnings = await _surveyAttributeParser.GetSurveyAttributeWarningsAsync(survey, attributeData.AttributesXml);
            if (warnings.Any())
            {
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
            }

            if (ModelState.IsValid)
            {
                var submission = new SurveySubmission()
                {
                    AttributeDescription = await _surveyAttributeFormatter.FormatAttributesAsync(survey, attributeData.AttributesXml),
                    AttributeXml = attributeData.AttributesXml,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    SurveyId = survey.Id
                };

                await _surveyService.InsertSurveySubmissionAsync(submission);

                foreach (var item in attributeData.MetaData.Mappings)
                {
                    await _surveyService.InsertSurveySubmissionAttributeAsync(new SurveySubmissionAttribute
                    {
                        SurveyAttributeMappingId = item.AttributeMapping.Id,
                        AttributeValue = item.AttributeValue,
                        SurveySubmissionId = submission.Id
                    });
                }

                return View(new SurveyModel()
                {
                    SuccessfullySent = true,
                    Result = await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.YourEnquiryHasBeenSent")
                });
            }

            var model = await _surveyModelFactory.PrepareSurveyModelByIdAsync(survey, attributeData.AttributesXml);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> SurveyPost(int surveyId, IFormCollection form)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(surveyId);
            if (survey == null || survey.Deleted)
                return InvokeHttp404();

            //allow administrators to preview any survey
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey);

            var notAvailable =
                //published?
                !survey.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(survey) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(survey);

            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await CanSubmitResponseAsync(survey, customer))
            {
                return Ok(new { Result = true, Errors = new[] { await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.YourEnquiryAlreadySent") } });
            }

            var attributeData = await _surveyAttributeParser.ParseSurveyAttributesAsync(survey, form);

            if (!await IsMinimumSubmissionIntervalValidAsync(survey, customer))
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.MinSubmissionInterval"));
                return Ok(new { Result = false, Errors = ModelState.GetErrors() });
            }

            var warnings = await _surveyAttributeParser.GetSurveyAttributeWarningsAsync(survey, attributeData.AttributesXml);
            if (warnings.Any())
            {
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
            }

            if (ModelState.IsValid)
            {
                var submission = new SurveySubmission()
                {
                    AttributeDescription = await _surveyAttributeFormatter.FormatAttributesAsync(survey, attributeData.AttributesXml),
                    AttributeXml = attributeData.AttributesXml,
                    CreatedOnUtc = System.DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    SurveyId = survey.Id
                };

                await _surveyService.InsertSurveySubmissionAsync(submission);

                foreach (var item in attributeData.MetaData.Mappings)
                {
                    await _surveyService.InsertSurveySubmissionAttributeAsync(new SurveySubmissionAttribute
                    {
                        SurveyAttributeMappingId = item.AttributeMapping.Id,
                        AttributeValue = item.AttributeValue,
                        SurveySubmissionId = submission.Id
                    });
                }

                return Ok(new { Result = true, Errors = new[] { await _localizationService.GetResourceAsync("NopStation.DynamicSurvey.YourEnquiryHasBeenSent") } });

            }

            return Ok(new { Result = false, Errors = ModelState.GetErrors() });
        }

        [HttpPost]
        public virtual async Task<IActionResult> Survey_AttributeChange(int surveyId, IFormCollection form)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(surveyId);
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();

            if (survey == null)
            {
                return Ok(new
                {
                    enabledAttributeMappingIds,
                    disabledAttributeMappingIds,
                    message = new[] { "Survey not found" }
                });
            }

            var errors = new List<string>();
            var (attributeXml, _) = await _surveyAttributeParser.ParseSurveyAttributesAsync(survey, form);

            var attributes = await _surveyAttributeService.GetSurveyAttributeMappingsBySurveyIdAsync(survey.Id);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _surveyAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (!conditionMet.HasValue)
                    continue;

                if (conditionMet.Value)
                    enabledAttributeMappingIds.Add(attribute.Id);
                else
                    disabledAttributeMappingIds.Add(attribute.Id);
            }

            return Ok(new
            {
                enabledAttributeMappingIds,
                disabledAttributeMappingIds,
                message = errors.Any() ? errors.ToArray() : null
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> UploadSurveyFormAttribute(int attributeId)
        {
            var attribute = await _surveyAttributeService.GetSurveyAttributeMappingByIdAsync(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid field",
                    downloadGuid = Guid.Empty
                });
            }

            if (Request.Form.Files.Count <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty
                });
            }

            var httpPostedFile = Request.Form.Files[0];

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);
            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid
            });
        }


        #endregion
    }
}
