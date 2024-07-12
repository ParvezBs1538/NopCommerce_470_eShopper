using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Common;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Factories;
using NopStation.Plugin.Misc.WebApi.Filters;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/common")]
    public class CommonApiController : BaseApiController
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly VendorSettings _vendorSettings;
        private readonly ICommonApiModelFactory _commonApiModelFactory;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IHtmlFormatter _htmlFormatter;

        #endregion

        #region Ctor

        public CommonApiController(CommonSettings commonSettings,
            ICommonModelFactory commonModelFactory,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            VendorSettings vendorSettings,
            ICommonApiModelFactory commonApiModelFactory,
            LocalizationSettings localizationSettings,
            IHtmlFormatter htmlFormatter)
        {
            _commonSettings = commonSettings;
            _commonModelFactory = commonModelFactory;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _languageService = languageService;
            _localizationService = localizationService;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _vendorSettings = vendorSettings;
            _commonApiModelFactory = commonApiModelFactory;
            _localizationSettings = localizationSettings;
            _htmlFormatter = htmlFormatter;
        }

        #endregion

        #region Methods

        [CheckAccessPublicStore(true)]
        [HttpGet("setlanguage/{langid}")]
        public virtual async Task<IActionResult> SetLanguage(int langid)
        {
            var language = await _languageService.GetLanguageByIdAsync(langid);
            if (!language?.Published ?? false)
                language = await _workContext.GetWorkingLanguageAsync();

            var message = await _localizationService.GetResourceAsync("NopStation.WebApi.Response.LanguageChanged");
            await _workContext.SetWorkingLanguageAsync(language);
            return Ok(message);
        }

        [CheckAccessPublicStore(true)]
        [HttpGet("setcurrency/{customerCurrency}")]
        public virtual async Task<IActionResult> SetCurrency(int customerCurrency)
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(customerCurrency);
            if (currency != null)
                await _workContext.SetWorkingCurrencyAsync(currency);

            return Ok(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.CurrencyChanged"));
        }

        [CheckAccessPublicStore(true)]
        [HttpGet("settaxtype/{customerTaxType}")]
        public virtual async Task<IActionResult> SetTaxType(int customerTaxType)
        {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            await _workContext.SetTaxDisplayTypeAsync(taxDisplayType);

            return Ok(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.TaxTypeChanged"));
        }

        [HttpGet("contactus")]
        public virtual async Task<IActionResult> ContactUs()
        {
            var model = await _commonModelFactory.PrepareContactUsModelAsync(new ContactUsModel(), false);
            return OkWrap(model);
        }

        [HttpPost("contactus")]
        public virtual async Task<IActionResult> ContactUs([FromBody] BaseQueryModel<ContactUsModel> queryModel)
        {
            var model = queryModel.Data;
            model = await _commonModelFactory.PrepareContactUsModelAsync(model, true);

            if (ModelState.IsValid)
            {
                var subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
                var body = _htmlFormatter.FormatText(model.Enquiry, false, true, false, false, false, false);

                await _workflowMessageService.SendContactUsMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                    model.Email.Trim(), model.FullName, subject, body);

                model.SuccessfullySent = true;

                //activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.ContactUs",
                    await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ContactUs"));

                return OkWrap(model, await _localizationService.GetResourceAsync("ContactUs.YourEnquiryHasBeenSent"));
            }

            return BadRequestWrap(model, ModelState);
        }

        [HttpGet("contactvendor/{vendorId}")]
        public virtual async Task<IActionResult> ContactVendor(int vendorId)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return Unauthorized();

            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return NotFound();

            var model = new ContactVendorModel();
            model = await _commonModelFactory.PrepareContactVendorModelAsync(model, vendor, false);

            return OkWrap(model);
        }

        [HttpPost("contactvendor")]
        public virtual async Task<IActionResult> ContactVendor([FromBody] BaseQueryModel<ContactVendorModel> queryModel)
        {
            var model = queryModel.Data;
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return Unauthorized();

            var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return NotFound();

            model = await _commonModelFactory.PrepareContactVendorModelAsync(model, vendor, true);

            if (ModelState.IsValid)
            {
                var subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
                var body = _htmlFormatter.FormatText(model.Enquiry, false, true, false, false, false, false);

                await _workflowMessageService.SendContactVendorMessageAsync(vendor, (await _workContext.GetWorkingLanguageAsync()).Id,
                    model.Email.Trim(), model.FullName, subject, body);

                model.SuccessfullySent = true;
                return OkWrap(model, await _localizationService.GetResourceAsync("ContactVendor.YourEnquiryHasBeenSent"));
            }

            return BadRequestWrap(model, ModelState);
        }

        [HttpGet("getstringresources/{languageId?}")]
        public virtual async Task<IActionResult> GetStringResources(int? languageId)
        {
            var response = new GenericResponseModel<List<KeyValueApi>>();
            response.Data = (await _commonApiModelFactory.GetStringRsourcesAsync(languageId)).ToList();

            return Ok(response);
        }

        #endregion
    }
}
