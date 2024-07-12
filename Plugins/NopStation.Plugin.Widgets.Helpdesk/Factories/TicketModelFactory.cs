using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Models;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Factories
{
    public class TicketModelFactory : ITicketModelFactory
    {
        private readonly HelpdeskSettings _helpdeskSettings;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ITicketService _ticketService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly IDepartmentService _departmentService;
        private readonly IStaffService _staffService;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;

        public TicketModelFactory(HelpdeskSettings ticketSettings,
            IOrderService orderService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ITicketService ticketService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICustomerService customerService,
            IDepartmentService departmentService,
            IStaffService staffService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService)
        {
            _helpdeskSettings = ticketSettings;
            _orderService = orderService;
            _storeContext = storeContext;
            _workContext = workContext;
            _ticketService = ticketService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
            _departmentService = departmentService;
            _staffService = staffService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
        }

        protected async Task PrepareResponseListModelAsync(TicketDetailsModel model, Ticket ticket)
        {
            var responses = await _ticketService.GetTicketResponsesByTicketIdAsync(ticket.Id);
            foreach (var response in responses)
            {
                if (response.DisplayToCustomer)
                {
                    var rm = new TicketResponseModel()
                    {
                        TicketId = ticket.Id,
                        Body = response.Body,
                        CreatedByCustomer = (await _customerService.GetCustomerByIdAsync(response.CreatedByCustomerId))?.Email,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(response.CreatedOnUtc, DateTimeKind.Utc),
                        Id = response.Id
                    };

                    if (_helpdeskSettings.AllowCustomerToUploadAttachmentInResponse)
                    {
                        var download = await _downloadService.GetDownloadByIdAsync(response.DownloadId);
                        rm.DownloadId = download?.Id ?? 0;
                        rm.DownloadGuid = download?.DownloadGuid ?? Guid.Empty;
                    }

                    model.TicketResponses.Add(rm);
                }
            }
        }

        public async Task<TicketModel> PrepareAddNewTicketModelAsync(TicketModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (model == null)
            {
                model = new TicketModel()
                {
                    AllowCustomerToSetPriority = _helpdeskSettings.AllowCustomerToSetPriority,
                    AllowCustomerToUploadAttachmentInTicket = _helpdeskSettings.AllowCustomerToUploadAttachmentInTicket,
                    CategoryId = _helpdeskSettings.DefaultTicketCategoryId,
                    DepartmentId = _helpdeskSettings.DefaultTicketDepartmentId,
                    Name = customer.FirstName + " " + customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.Phone,
                };
            }

            var orders = await _orderService.SearchOrdersAsync(
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id);
            model.AvailableOrders = await orders.SelectAwait(async x => new SelectListItem()
            {
                Text = $"{x.Id:0000000} - {await _dateTimeHelper.ConvertToUserTimeAsync(x.CreatedOnUtc, DateTimeKind.Utc)}",
                Value = x.Id.ToString()
            }).ToListAsync();

            if (_helpdeskSettings.EnableTicketCategory)
            {
                model.CategoryId = _helpdeskSettings.DefaultTicketCategoryId;
                model.AvailableCategories = (await TicketCategory.Order.ToSelectListAsync()).ToList();
                if (!_helpdeskSettings.TicketCategoryRequired)
                    model.AvailableCategories.Insert(0, new SelectListItem()
                    {
                        Text = "--",
                        Value = "0"
                    });
            }

            if (_helpdeskSettings.EnableTicketDepartment)
            {
                var departmets = await _departmentService.GetAllHelpdeskDepartmentsAsync();
                model.DepartmentId = _helpdeskSettings.DefaultTicketDepartmentId;
                model.AvailableDepartments = departmets.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                if (!_helpdeskSettings.TicketDepartmentRequired)
                    model.AvailableDepartments.Insert(0, new SelectListItem()
                    {
                        Text = "--",
                        Value = "0"
                    });
            }

            if (_helpdeskSettings.AllowCustomerToSetPriority)
            {
                model.PriorityId = _helpdeskSettings.DefaultTicketPriorityId;
                model.AvailablePriorities = (await TicketPriority.High.ToSelectListAsync()).ToList();
            }

            model.EnableTicketCategory = _helpdeskSettings.EnableTicketCategory;
            model.EnableTicketDepartment = _helpdeskSettings.EnableTicketDepartment;
            model.TicketCategoryRequired = _helpdeskSettings.TicketCategoryRequired;
            model.TicketDepartmentRequired = _helpdeskSettings.TicketDepartmentRequired;

            return model;
        }

        public async Task<TicketDetailsModel> PrepareTicketDetailsModelAsync(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            var departmentName = "";
            if (_helpdeskSettings.EnableTicketDepartment)
            {
                var department = await _departmentService.GetHelpdeskDepartmentByIdAsync(ticket.DepartmentId);
                departmentName = department?.Name;
            }

            var download = await _downloadService.GetDownloadByIdAsync(ticket.DownloadId);

            var model = new TicketDetailsModel()
            {
                Body = ticket.Body,
                Category = await _localizationService.GetLocalizedEnumAsync(ticket.Category),
                CategoryId = ticket.CategoryId,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ticket.CreatedOnUtc, DateTimeKind.Utc),
                Department = departmentName,
                DepartmentId = ticket.DepartmentId,
                DownloadId = ticket.DownloadId,
                Email = ticket.Email,
                Id = ticket.Id,
                Subject = ticket.Subject,
                TicketGuid = ticket.TicketGuid,
                Status = await _localizationService.GetLocalizedEnumAsync(ticket.Status),
                Priority = await _localizationService.GetLocalizedEnumAsync(ticket.Priority),
                PhoneNumber = ticket.PhoneNumber,
                Name = ticket.Name,
                OrderId = ticket.OrderId,
                ProductId = ticket.ProductId,
                PriorityId = ticket.PriorityId,
                UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ticket.UpdatedOnUtc, DateTimeKind.Utc),
                EnableTicketCategory = _helpdeskSettings.EnableTicketCategory,
                EnableTicketDepartment = _helpdeskSettings.EnableTicketDepartment,
                AllowCustomerToUploadAttachmentInTicket = _helpdeskSettings.AllowCustomerToUploadAttachmentInTicket,
                DownloadGuid = download?.DownloadGuid ?? Guid.Empty,
                AllowCustomerToUploadAttachmentInResponse = _helpdeskSettings.AllowCustomerToUploadAttachmentInResponse
            };

            await PrepareResponseListModelAsync(model, ticket);

            return model;
        }

        public async Task<TicketListModel> PrepareTicketListModelAsync(IPagedList<Ticket> tickets)
        {
            if (tickets == null)
                throw new ArgumentNullException(nameof(tickets));

            var model = new TicketListModel();

            foreach (var ticket in tickets)
            {
                model.Tickets.Add(await PrepareTicketOverviewModelAsync(ticket));
            }

            return model;
        }

        public async Task<TicketOverviewModel> PrepareTicketOverviewModelAsync(Ticket ticket)
        {
            var department = await _departmentService.GetHelpdeskDepartmentByIdAsync(ticket.DepartmentId);
            var download = await _downloadService.GetDownloadByIdAsync(ticket.DownloadId);

            var model = new TicketOverviewModel()
            {
                Body = ticket.Body,
                Category = await _localizationService.GetLocalizedEnumAsync(ticket.Category),
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ticket.CreatedOnUtc, DateTimeKind.Utc),
                Department = department?.Name,
                DownloadId = download?.Id ?? 0,
                DownloadGuid = download?.DownloadGuid ?? Guid.Empty,
                Id = ticket.Id,
                Status = await _localizationService.GetLocalizedEnumAsync(ticket.Status),
                OrderId = ticket.OrderId,
                Priority = await _localizationService.GetLocalizedEnumAsync(ticket.Priority),
                ProductId = ticket.ProductId,
                Subject = ticket.Subject,
                TicketGuid = ticket.TicketGuid,
                UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ticket.UpdatedOnUtc, DateTimeKind.Utc)
            };

            return model;
        }
    }
}
