using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Services
{
    public interface ITicketService
    {
        #region Tickets

        Task DeleteTicketAsync(Ticket ticket);

        Task InsertTicketAsync(Ticket ticket);

        Task UpdateTicketAsync(Ticket ticket);

        Task<Ticket> GetTicketByIdAsync(int ticketId);

        Task<Ticket> GetTicketByGuidAsync(Guid ticketGuid);

        Task<IPagedList<Ticket>> GetAllTickets(string email = null,
            IList<int> catagoriesIds = null, IList<int> priorotiesId = null,
            IList<int> statusId = null, DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IPagedList<Ticket>> GetAllTicketsAsync(string email = "", string phoneNumber = "",
            string keyword = "", int categoryId = 0, int orderId = 0, int productId = 0,
            int departmentId = 0, int staffId = 0, int customerId = 0, int priorityId = 0,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Ticket responses

        Task DeleteTicketResponseAsync(TicketResponse ticketResponse);

        Task InsertTicketResponseAsync(TicketResponse ticketResponse);

        Task UpdateTicketResponseAsync(TicketResponse ticketResponse);

        Task<TicketResponse> GetTicketResponseByIdAsync(int ticketResponseId);

        Task<IList<TicketResponse>> GetTicketResponsesByTicketIdAsync(int ticketId);

        Task<IPagedList<TicketResponse>> GetTicketResponsesByCustomerIdAsync(int customerId, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion
    }
}
