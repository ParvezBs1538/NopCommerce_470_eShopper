using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Services
{
    public class TicketService : ITicketService
    {
        #region Fields

        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TicketResponse> _ticketResponseRepository;

        #endregion

        #region Ctor

        public TicketService(IRepository<Ticket> ticketRepository,
            IRepository<TicketResponse> ticketResponseRepository)
        {
            _ticketRepository = ticketRepository;
            _ticketResponseRepository = ticketResponseRepository;
        }

        #endregion

        #region Methods

        #region Tickets

        public async Task DeleteTicketAsync(Ticket ticket)
        {
            await _ticketRepository.DeleteAsync(ticket);
        }

        public async Task InsertTicketAsync(Ticket ticket)
        {
            await _ticketRepository.InsertAsync(ticket);
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            if (ticketId == 0)
                return null;

            return await _ticketRepository.GetByIdAsync(ticketId, cache => default);
        }

        public async Task<Ticket> GetTicketByGuidAsync(Guid ticketGuid)
        {
            if (ticketGuid == Guid.Empty)
                return null;

            return await _ticketRepository.Table.FirstOrDefaultAsync(x => x.TicketGuid == ticketGuid);
        }

        public async Task<IPagedList<Ticket>> GetAllTickets(string email = null, IList<int> catagoriesIds = null,
            IList<int> priorotiesId = null, IList<int> statusId = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _ticketRepository.Table;

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(x => x.Email.Contains(email));

            if (catagoriesIds != null && catagoriesIds.Any())
            {
                if (!catagoriesIds.Contains(0))
                {
                    query = query.Where(x => catagoriesIds.Contains(x.CategoryId));
                }
            }

            if (priorotiesId != null && priorotiesId.Any())
            {
                if (!priorotiesId.Contains(0))
                {
                    query = query.Where(x => priorotiesId.Contains(x.PriorityId));
                }
            }

            if (statusId != null && statusId.Any())
            {
                if (!statusId.Contains(0))
                {
                    query = query.Where(x => statusId.Contains(x.StatusId));
                }
            }

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Ticket>> GetAllTicketsAsync(string email = "", string phoneNumber = "",
            string keyword = "", int categoryId = 0, int orderId = 0, int productId = 0,
            int departmentId = 0, int staffId = 0, int customerId = 0, int priorityId = 0,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _ticketRepository.Table;

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(x => x.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                query = query.Where(x => x.PhoneNumber.Contains(phoneNumber));

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Body.Contains(keyword) || x.Subject.Contains(keyword));

            if (categoryId > 0)
                query = query.Where(x => x.CategoryId == categoryId);

            if (orderId > 0)
                query = query.Where(x => x.OrderId == orderId);

            if (productId > 0)
                query = query.Where(x => x.ProductId == productId);

            if (departmentId > 0)
                query = query.Where(x => x.DepartmentId == departmentId);

            if (staffId > 0)
                query = query.Where(x => x.StaffId == staffId);

            if (customerId > 0)
                query = query.Where(x => x.CustomerId == customerId);

            if (priorityId > 0)
                query = query.Where(x => x.PriorityId == priorityId);

            if (storeId > 0)
                query = query.Where(x => x.StoreId == storeId);

            query = query.OrderByDescending(e => e.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #region Ticket responses

        public async Task DeleteTicketResponseAsync(TicketResponse ticketResponse)
        {
            await _ticketResponseRepository.DeleteAsync(ticketResponse);
        }

        public async Task InsertTicketResponseAsync(TicketResponse ticketResponse)
        {
            await _ticketResponseRepository.InsertAsync(ticketResponse);
        }

        public async Task UpdateTicketResponseAsync(TicketResponse ticketResponse)
        {
            await _ticketResponseRepository.UpdateAsync(ticketResponse);
        }

        public async Task<TicketResponse> GetTicketResponseByIdAsync(int ticketResponseId)
        {
            if (ticketResponseId == 0)
                return null;

            return await _ticketResponseRepository.GetByIdAsync(ticketResponseId, cache => default);
        }

        public async Task<IList<TicketResponse>> GetTicketResponsesByTicketIdAsync(int ticketId)
        {
            var query = _ticketResponseRepository.Table
                .Where(x => x.TicketId == ticketId);

            return await query.ToListAsync();
        }

        public async Task<IPagedList<TicketResponse>> GetTicketResponsesByCustomerIdAsync(int customerId, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from t in _ticketRepository.Table
                        join tr in _ticketResponseRepository.Table on t.Id equals tr.TicketId
                        where t.CreatedByCustomerId == customerId && t.StoreId == storeId
                        orderby tr.CreatedOnUtc descending
                        select tr;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #endregion
    }
}
