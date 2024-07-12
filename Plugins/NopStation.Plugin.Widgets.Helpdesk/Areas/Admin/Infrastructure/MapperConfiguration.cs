using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Ticket

            CreateMap<Ticket, TicketModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.AvailableCategories, options => options.Ignore())
                .ForMember(model => model.AvailableDepartments, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.AvailableStatuses, options => options.Ignore())
                .ForMember(model => model.AvailableStaffs, options => options.Ignore())
                .ForMember(model => model.AvailablePriorities, options => options.Ignore())
                .ForMember(model => model.CreatedByCustomerEmail, options => options.Ignore())
                .ForMember(model => model.ResponseAddModel, options => options.Ignore())
                .ForMember(model => model.ResponseSearchModel, options => options.Ignore());
            CreateMap<TicketModel, Ticket>()
                .ForMember(entity => entity.TicketGuid, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<TicketResponse, ResponseModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<ResponseModel, TicketResponse>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            CreateMap<HelpdeskStaff, StaffModel>();
            CreateMap<StaffModel, HelpdeskStaff>();

            CreateMap<HelpdeskDepartment, DepartmentModel>();
            CreateMap<DepartmentModel, HelpdeskDepartment>();

            CreateMap<HelpdeskSettings, ConfigurationModel>()
                .ForMember(model => model.SendEmailOnNewTicket_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SendEmailOnNewResponse_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SendEmailsTo_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EmailAccountId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowCustomerToCreateTicketFromOrderPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowCustomerToSetPriority_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultTicketDepartmentId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableTicketCategory_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableTicketDepartment_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NavigationWidgetZone_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.OrderPageWidgetZone_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowMenuInCustomerNavigation_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultTicketCategoryId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TicketCategoryRequired_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TicketDepartmentRequired_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MinimumTicketCreateInterval_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MinimumResponseCreateInterval_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, HelpdeskSettings>();

            #endregion
        }
        public int Order => 0;
    }
}
