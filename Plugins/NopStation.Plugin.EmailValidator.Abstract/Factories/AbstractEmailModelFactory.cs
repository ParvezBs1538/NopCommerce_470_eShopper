using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.EmailValidator.Abstract.Domains;
using NopStation.Plugin.EmailValidator.Abstract.Models;
using NopStation.Plugin.EmailValidator.Abstract.Services;

namespace NopStation.Plugin.EmailValidator.Abstract.Factories
{
    public class AbstractEmailModelFactory : IAbstractEmailModelFactory
    {

        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IAbstractEmailService _abstractEmailService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public AbstractEmailModelFactory(ILocalizationService localizationService,
            IAbstractEmailService abstractEmailService,
            IDateTimeHelper dateTimeHelper)
        {
            _localizationService = localizationService;
            _abstractEmailService = abstractEmailService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<AbstractEmailSearchModel> PrepareAbstractEmailSearchModelAsync(AbstractEmailSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableDeliverabilityItems.Add(new SelectListItem
            {
                Text = Defaults.Deliverable,
                Value = Defaults.Deliverable
            });
            searchModel.AvailableDeliverabilityItems.Add(new SelectListItem
            {
                Text = Defaults.Risky,
                Value = Defaults.Risky
            });
            searchModel.AvailableDeliverabilityItems.Add(new SelectListItem
            {
                Text = Defaults.Undeliverable,
                Value = Defaults.Undeliverable
            });
            searchModel.AvailableDeliverabilityItems.Add(new SelectListItem
            {
                Text = Defaults.Unknown,
                Value = Defaults.Unknown
            });
            searchModel.AvailableDeliverabilityItems.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Common.All"),
                Value = ""
            });

            searchModel.AvailableBooleanItems.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Common.Yes"),
                Value = "1"
            });
            searchModel.AvailableBooleanItems.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Common.No"),
                Value = "2"
            });
            searchModel.AvailableBooleanItems.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Common.All"),
                Value = "0"
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<AbstractEmailListModel> PrepareAbstractEmailListModelAsync(AbstractEmailSearchModel searchModel)
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

            //get abstractEmails
            var abstractEmails = await _abstractEmailService.SearchAbstractEmailsAsync(
                email: searchModel.SearchEmail,
                disposable: disposable,
                free: free,
                roleAccount: roleAccount,
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                deliverability: searchModel.SearchDeliverability,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new AbstractEmailListModel().PrepareToGridAsync(searchModel, abstractEmails, () =>
            {
                return abstractEmails.SelectAwait(async abstractEmail =>
                {
                    return await PrepareAbstractEmailModelAsync(null, abstractEmail, true);
                });
            });

            return model;
        }

        protected virtual async Task<AbstractEmailModel> PrepareAbstractEmailModelAsync(AbstractEmailModel model, AbstractEmail abstractEmail, bool excludeProperties = false)
        {
            if (abstractEmail != null)
            {
                if (model == null)
                {
                    model = abstractEmail.ToModel<AbstractEmailModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(abstractEmail.CreatedOnUtc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(abstractEmail.UpdatedOnUtc);
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
