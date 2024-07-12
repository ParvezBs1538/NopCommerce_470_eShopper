using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;
using NopStation.Plugin.EmailValidator.Verifalia.Models;
using NopStation.Plugin.EmailValidator.Verifalia.Services;
using ValidationEntryStatus = Verifalia.Api.EmailValidations.Models.ValidationEntryStatus;

namespace NopStation.Plugin.EmailValidator.Verifalia.Factories
{
    public class VerifaliaEmailModelFactory : IVerifaliaEmailModelFactory
    {

        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IVerifaliaEmailService _verifaliaEmailService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public VerifaliaEmailModelFactory(ILocalizationService localizationService,
            IVerifaliaEmailService verifaliaEmailService,
            IDateTimeHelper dateTimeHelper)
        {
            _localizationService = localizationService;
            _verifaliaEmailService = verifaliaEmailService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<VerifaliaEmailSearchModel> PrepareVerifaliaEmailSearchModelAsync(VerifaliaEmailSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableStatusItems = (await ValidationEntryStatus.AtSignNotFound.ToSelectListAsync()).ToList();
            searchModel.AvailableStatusItems.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Common.All"),
                Value = "0"
            });

            searchModel.AvailableClassificationItems.Add(new SelectListItem
            {
                Text = Defaults.Deliverable,
                Value = Defaults.Deliverable
            });
            searchModel.AvailableClassificationItems.Add(new SelectListItem
            {
                Text = Defaults.Risky,
                Value = Defaults.Risky
            });
            searchModel.AvailableClassificationItems.Add(new SelectListItem
            {
                Text = Defaults.Undeliverable,
                Value = Defaults.Undeliverable
            });
            searchModel.AvailableClassificationItems.Add(new SelectListItem
            {
                Text = Defaults.Unknown,
                Value = Defaults.Unknown
            });
            searchModel.AvailableClassificationItems.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Common.All"),
                Value = ""
            });

            searchModel.AvailableBooleanItems.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Common.Yes"),
                Value = "1"
            });
            searchModel.AvailableBooleanItems.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Common.No"),
                Value = "2"
            });
            searchModel.AvailableBooleanItems.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Common.All"),
                Value = "0"
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<VerifaliaEmailListModel> PrepareVerifaliaEmailListModelAsync(VerifaliaEmailSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            bool? disposable = null;
            if (searchModel.SearchDisposableId == 1)
                disposable = true;
            else if (searchModel.SearchDisposableId == 2)
                disposable = false;

            bool? free = null;
            if (searchModel.SearchFreeId == 1)
                free = true;
            else if (searchModel.SearchFreeId == 2)
                free = false;

            bool? roleAccount = null;
            if (searchModel.SearchRoleAccountId == 1)
                roleAccount = true;
            else if (searchModel.SearchRoleAccountId == 2)
                roleAccount = false;

            DateTime? createdFromUtc = null;
            if (searchModel.SearchCreatedFrom.HasValue)
                createdFromUtc = _dateTimeHelper.ConvertToUtcTime(searchModel.SearchCreatedFrom.Value);
            DateTime? createdToUtc = null;
            if (searchModel.SearchCreatedTo.HasValue)
                createdToUtc = _dateTimeHelper.ConvertToUtcTime(searchModel.SearchCreatedTo.Value);

            //get verifaliaEmails
            var verifaliaEmails = await _verifaliaEmailService.SearchVerifaliaEmailsAsync(
                email: searchModel.SearchEmail,
                disposable: disposable,
                free: free,
                roleAccount: roleAccount,
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                statusIds: searchModel.SearchStatusIds.ToArray(),
                classification: searchModel.SearchClassification,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new VerifaliaEmailListModel().PrepareToGridAsync(searchModel, verifaliaEmails, () =>
            {
                return verifaliaEmails.SelectAwait(async verifaliaEmail =>
                {
                    return await PrepareVerifaliaEmailModelAsync(null, verifaliaEmail, true);
                });
            });

            return model;
        }

        protected virtual async Task<VerifaliaEmailModel> PrepareVerifaliaEmailModelAsync(VerifaliaEmailModel model, VerifaliaEmail verifaliaEmail, bool excludeProperties = false)
        {
            if (verifaliaEmail != null)
            {
                if (model == null)
                {
                    model = verifaliaEmail.ToModel<VerifaliaEmailModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(verifaliaEmail.CreatedOnUtc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(verifaliaEmail.UpdatedOnUtc);
                    model.Status = await _localizationService.GetLocalizedEnumAsync(verifaliaEmail.Status);
                }
            }

            if (!excludeProperties)
            {

            }

            return model;
        }

        #endregion
    }
}
