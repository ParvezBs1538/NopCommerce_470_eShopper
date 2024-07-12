using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public class TicketModelFactory : ITicketModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ITicketService _ticketService;
        private readonly IDepartmentService _departmentService;
        private readonly IStaffService _staffService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;

        #endregion

        #region Ctor

        public TicketModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ITicketService ticketService,
            IDepartmentService departmentService,
            IStaffService staffService,
            ICustomerService customerService,
            IDownloadService downloadService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _ticketService = ticketService;
            _departmentService = departmentService;
            _staffService = staffService;
            _customerService = customerService;
            _downloadService = downloadService;
        }

        #endregion

        #region Methods

        protected async void PrepareAvailableCategory(IList<SelectListItem> availableCategories)
        {
            var availablePositionItems = await TicketCategory.Order.ToSelectListAsync(false);
            foreach (var positionItem in availablePositionItems)
            {
                availableCategories.Add(positionItem);
            }
        }

        protected async void PrepareAvailableStatus(IList<SelectListItem> availableStatus)
        {
            var availablePositionItems = await TicketStatus.Open.ToSelectListAsync(false);
            foreach (var positionItem in availablePositionItems)
            {
                availableStatus.Add(positionItem);
            }
        }

        protected async void PrepareAvailablePriority(IList<SelectListItem> availablePriorities)
        {
            var availablePositionItems = await TicketPriority.High.ToSelectListAsync(false);
            foreach (var positionItem in availablePositionItems)
            {
                availablePriorities.Add(positionItem);
            }
        }

        public virtual async Task<TicketSearchModel> PrepareTicketSearchModel(TicketSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            PrepareAvailableCategory(searchModel.AvailableCategory);
            searchModel.AvailableCategory.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.List.AvailableCategory.All"),
                Value = "0"
            });

            PrepareAvailablePriority(searchModel.AvailablePriority);
            searchModel.AvailablePriority.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.List.AvailablePriority.All"),
                Value = "0"
            });

            PrepareAvailableStatus(searchModel.AvailableStatus);
            searchModel.AvailableStatus.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.List.AvailableStatus.All"),
                Value = "0"
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<TicketListModel> PrepareTicketListModelAsync(TicketSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var tickets = await _ticketService.GetAllTickets(searchModel.Email, searchModel.CategoryId, searchModel.PriorityId, searchModel.StatusId, searchModel.CreatedFrom, searchModel.CreatedTo, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new TicketListModel().PrepareToGridAsync(searchModel, tickets, () =>
            {
                return tickets.SelectAwait(async ticket =>
                {
                    var ticketModel = await PrepareTicketModelAsync(null, ticket, true);

                    return ticketModel;
                });
            });

            return model;
        }

        public virtual async Task<TicketModel> PrepareTicketModelAsync(TicketModel model, Ticket ticket, bool excludeProperties = false)
        {
            if (ticket != null)
            {
                if (model == null)
                {
                    model = ticket.ToModel<TicketModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ticket.CreatedOnUtc, DateTimeKind.Utc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ticket.UpdatedOnUtc, DateTimeKind.Utc);
                    model.Priority = await _localizationService.GetLocalizedEnumAsync(ticket.Priority);
                    model.Status = await _localizationService.GetLocalizedEnumAsync(ticket.Status);
                    model.Category = await _localizationService.GetLocalizedEnumAsync(ticket.Category);
                    var download = await _downloadService.GetDownloadByIdAsync(ticket.DownloadId);
                    model.DownloadGuid = download?.DownloadGuid ?? Guid.Empty;
                    var customer = await _customerService.GetCustomerByIdAsync(ticket.CreatedByCustomerId);
                    model.CreatedByCustomerEmail = customer?.Email ?? "Unknown";
                    model.ResponseSearchModel.Ticketid = ticket.Id;
                }
            }

            if (!excludeProperties)
            {
                await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores, false);
                model.AvailableCategories = (await TicketCategory.Order.ToSelectListAsync()).ToList();
                model.AvailableStatuses = (await TicketStatus.Open.ToSelectListAsync()).ToList();
                model.AvailablePriorities = (await TicketPriority.High.ToSelectListAsync()).ToList();
                model.AvailableDepartments = (await _departmentService.GetAllHelpdeskDepartmentsAsync())
                    .Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                model.AvailableStaffs = (await _staffService.GetAllHelpdeskStaffsAsync())
                    .Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
            }
            return model;
        }

        public async Task<ResponseListModel> PrepareResponseListModelAsync(ResponseSearchModel searchModel, Ticket ticket)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var responses = (await _ticketService.GetTicketResponsesByTicketIdAsync(ticket.Id)).ToPagedList(searchModel);

            var model = await new ResponseListModel().PrepareToGridAsync(searchModel, responses, () =>
            {
                return responses.SelectAwait(async response =>
                {
                    var responseModel = response.ToModel<ResponseModel>();
                    var download = await _downloadService.GetDownloadByIdAsync(response.DownloadId);
                    responseModel.DownloadGuid = download?.DownloadGuid ?? Guid.Empty;
                    responseModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(response.CreatedOnUtc, DateTimeKind.Utc);
                    var customer = await _customerService.GetCustomerByIdAsync(response.CreatedByCustomerId);
                    responseModel.CreatedByCustomerEmail = customer?.Email ?? "Unknown";

                    return responseModel;
                });
            });

            return model;
        }

        #endregion
    }
}
