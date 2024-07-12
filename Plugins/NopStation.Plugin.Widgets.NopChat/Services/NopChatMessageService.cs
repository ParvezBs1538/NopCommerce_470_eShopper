using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Vendors;
using NopStation.Plugin.Widgets.NopChat.Domains;
using NopStation.Plugin.Widgets.NopChat.Models;

namespace NopStation.Plugin.Widgets.NopChat.Services
{
    public partial class NopChatMessageService : INopChatMessageService
    {
        #region Fields

        private readonly IRepository<NopChatMessage> _ncmRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;
        private readonly ILogger _logger;

        public NopChatMessageService(IRepository<NopChatMessage> ncmRepository,
            IRepository<Customer> customerRepository,
            IRepository<Vendor> vendorRepository,
            IPictureService pictureService,
            IVendorService vendorService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            ILogger logger)
        {
            _ncmRepository = ncmRepository;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
            _logger = logger;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<NopChatMessage>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _ncmRepository.GetAllAsync(query =>
            {
                return from ncm in query
                       orderby ncm.Id, ncm.Text, ncm.DateCreated, ncm.CustomerId, ncm.VendorId, ncm.VendorCustomerId, ncm.IsVendorResponse, ncm.IsChecked
                       select ncm;
            });

            var records = new PagedList<NopChatMessage>(rez, pageIndex, pageSize);

            return records;
        }

        public virtual async Task<NopChatMessage> GetByIdAsync(int id)
        {
            return await _ncmRepository.GetByIdAsync(id);
        }

        public virtual async Task<IPagedList<NopChatMessage>> GetByCustomerIdAsync(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _ncmRepository.GetAllAsync(query =>
            {
                return from ncm in query
                       where ncm.CustomerId == customerId
                       orderby ncm.Id, ncm.Text, ncm.DateCreated, ncm.CustomerId, ncm.VendorId, ncm.VendorCustomerId, ncm.IsVendorResponse, ncm.IsChecked
                       select ncm;
            });
            var records = new PagedList<NopChatMessage>(rez, pageIndex, pageSize);

            return records;
        }

        public virtual async Task<IPagedList<NopChatMessage>> GetByVendorIdAsync(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _ncmRepository.GetAllAsync(query =>
            {
                return from ncm in query
                       where ncm.VendorId == vendorId
                       orderby ncm.Id, ncm.Text, ncm.DateCreated, ncm.CustomerId, ncm.VendorId, ncm.VendorCustomerId, ncm.IsVendorResponse, ncm.IsChecked
                       select ncm;
            });
            var records = new PagedList<NopChatMessage>(rez, pageIndex, pageSize);

            return records;
        }

        public virtual async Task<IPagedList<NopChatMessage>> GetByVendorCustomerIdAsync(int vendorCustomerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _ncmRepository.GetAllAsync(query =>
            {
                return from ncm in query
                       where ncm.VendorCustomerId == vendorCustomerId
                       orderby ncm.Id, ncm.Text, ncm.DateCreated, ncm.CustomerId, ncm.VendorId, ncm.VendorCustomerId, ncm.IsVendorResponse, ncm.IsChecked
                       select ncm;
            });
            var records = new PagedList<NopChatMessage>(rez, pageIndex, pageSize);

            return records;
        }

        public virtual async Task<string> GetVendorAvatar(int vendorId)
        {

            var vendor = await _vendorRepository.GetByIdAsync(vendorId);

            if (vendor != null)
            {
                var avatar = await _pictureService.GetPictureUrlAsync(vendor.PictureId);
                await _logger.InformationAsync("Picture Id" + vendor.PictureId);
                return avatar;
            }
            else
            {
                return await _pictureService.GetDefaultPictureUrlAsync();
            }
        }

        public virtual async Task<string> GetCustomerAvatar(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            var avatar = await _pictureService.GetPictureUrlAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                _mediaSettings.AvatarPictureSize,
                false);

            if (string.IsNullOrEmpty(avatar))
            {
                avatar = await _pictureService.GetDefaultPictureUrlAsync();
            }
            return avatar;
        }

        public virtual async Task<IList<ChatListModel>> GetCustomerChatListListAsync(int customerId)
        {
            var result = await _ncmRepository.Table
                        .Where(r => r.CustomerId == customerId)
                        .Join(_vendorRepository.Table,
                         m => m.VendorId,
                         r => r.Id,
                         (m, r) => new
                         {
                             Id = m.VendorId,
                             Name = r.Name,
                             DateCreated = m.DateCreated,
                         }
                         )
                        .OrderBy(m => m.DateCreated)
                        .GroupBy(m =>
                            m.Id
                        )
                        .Select(m =>
                            new ChatListModel
                            {
                                Id = m.Key,
                                Name = m.FirstOrDefault().Name,
                                AvatarURL = GetVendorAvatar(m.Key).Result,
                                LastMesageDate = m.FirstOrDefault().DateCreated,
                                NumberOfMessages = m.Count()
                            }
                        ).ToListAsync();
            return result;
        }

        public virtual async Task<IList<ChatListModel>> GetVendorChatListListAsync(int vendorId)
        {
            var result = await _ncmRepository.Table
                        .Where(r => r.VendorId == vendorId)
                        .Join(_customerRepository.Table,
                         m => m.CustomerId,
                         r => r.Id,
                         (m, r) => new
                         {
                             Id = m.CustomerId,
                             Name = r.Username,
                             DateCreated = m.DateCreated
                         }
                         )
                        .OrderBy(m => m.DateCreated)
                        .GroupBy(m =>
                            m.Id
                        )
                        .Select(m =>
                            new ChatListModel
                            {
                                Id = m.Key,
                                Name = m.FirstOrDefault().Name,
                                AvatarURL = GetCustomerAvatar(m.Key).Result,
                                LastMesageDate = m.FirstOrDefault().DateCreated,
                                NumberOfMessages = m.Count()
                            }
                        ).ToListAsync();
            return result;
        }

        public virtual async Task InsertNopChatMessageAsync(NopChatMessage nopChatMessage)
        {
            await _ncmRepository.InsertAsync(nopChatMessage, false);
        }

        public virtual async Task UpdateNopChatMessageAsync(NopChatMessage nopChatMessage)
        {
            await _ncmRepository.UpdateAsync(nopChatMessage, false);
        }

        public virtual async Task DeleteNopChatMessageAsync(NopChatMessage nopChatMessage)
        {
            await _ncmRepository.DeleteAsync(nopChatMessage, false);
        }
        public virtual async Task<IList<NopChatMessageModel>> GetChatHistoryAsync(int customerId, int vendorId)
        {
            var result = await _ncmRepository.Table
                        .Where(r => r.CustomerId == customerId && r.VendorId == vendorId)
                        .Join(_vendorRepository.Table,
                         m => m.VendorId,
                         v => v.Id,
                         (message, vendor) => new
                         {
                             Id = message.Id,
                             DateCreated = message.DateCreated,
                             Text = message.Text,
                             VendorId = message.VendorId,
                             VendorName = vendor.Name,
                             CustomerId = message.CustomerId,
                             VendorCustomerId = message.VendorCustomerId,
                             IsVendorResponse = message.IsVendorResponse,
                             IsChecked = message.IsChecked
                         }
                         )
                        .Join(_customerRepository.Table,
                         m => m.CustomerId,
                         c => c.Id,
                         (message, customer) => new
                         {
                             Id = message.Id,
                             DateCreated = message.DateCreated,
                             Text = message.Text,
                             VendorId = message.VendorId,
                             VendorName = message.VendorName,
                             CustomerId = message.CustomerId,
                             CustomerName = customer.Username,
                             VendorCustomerId = message.VendorCustomerId,
                             IsVendorResponse = message.IsVendorResponse,
                             IsChecked = message.IsChecked
                         }
                         )
                        .GroupJoin(_customerRepository.Table,
                         m => m.VendorCustomerId,
                         vc => vc.Id,
                         (message, vendorCustomer) => new
                         {
                             Id = message.Id,
                             DateCreated = message.DateCreated,
                             Text = message.Text,
                             VendorId = message.VendorId,
                             VendorName = message.VendorName,
                             CustomerId = message.CustomerId,
                             CustomerName = message.CustomerName,
                             VendorCustomerId = message.VendorCustomerId,
                             IsVendorResponse = message.IsVendorResponse,
                             IsChecked = message.IsChecked,
                             VendorCustomer = vendorCustomer
                         }
                         )
                        .OrderBy(r => r.DateCreated)
                        .SelectMany(r => r.VendorCustomer.DefaultIfEmpty(),
                           (r, x) => new NopChatMessageModel
                           {
                               Id = r.Id,
                               DateCreated = r.DateCreated,
                               Text = r.Text,
                               VendorId = r.VendorId,
                               VendorName = r.VendorName,
                               CustomerId = r.CustomerId,
                               CustomerName = r.CustomerName,
                               VendorCustomerId = r.VendorCustomerId,
                               VendorCustomerName = x.Username,
                               IsVendorResponse = r.IsVendorResponse,
                               IsChecked = r.IsChecked
                           }
                        ).ToListAsync();
            return result;
        }

        public virtual async Task<IPagedList<NopChatMessageModel>> GetChatHistoryPagedAsync(int customerId, int vendorId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var result = await _ncmRepository.Table
                        .Where(r => r.CustomerId == customerId && r.VendorId == vendorId)
                        .Join(_vendorRepository.Table,
                         m => m.VendorId,
                         v => v.Id,
                         (message, vendor) => new
                         {
                             Id = message.Id,
                             DateCreated = message.DateCreated,
                             Text = message.Text,
                             VendorId = message.VendorId,
                             VendorName = vendor.Name,
                             CustomerId = message.CustomerId,
                             VendorCustomerId = message.VendorCustomerId,
                             IsVendorResponse = message.IsVendorResponse,
                             IsChecked = message.IsChecked
                         }
                         )
                        .Join(_customerRepository.Table,
                         m => m.CustomerId,
                         c => c.Id,
                         (message, customer) => new
                         {
                             Id = message.Id,
                             DateCreated = message.DateCreated,
                             Text = message.Text,
                             VendorId = message.VendorId,
                             VendorName = message.VendorName,
                             CustomerId = message.CustomerId,
                             CustomerName = customer.Username,
                             VendorCustomerId = message.VendorCustomerId,
                             IsVendorResponse = message.IsVendorResponse,
                             IsChecked = message.IsChecked
                         }
                         )
                        .GroupJoin(_customerRepository.Table,
                         m => m.VendorCustomerId,
                         vc => vc.Id,
                         (message, vendorCustomer) => new
                         {
                             Id = message.Id,
                             DateCreated = message.DateCreated,
                             Text = message.Text,
                             VendorId = message.VendorId,
                             VendorName = message.VendorName,
                             CustomerId = message.CustomerId,
                             CustomerName = message.CustomerName,
                             VendorCustomerId = message.VendorCustomerId,
                             IsVendorResponse = message.IsVendorResponse,
                             IsChecked = message.IsChecked,
                             VendorCustomer = vendorCustomer
                         }
                         )
                        .OrderByDescending(r => r.Id)
                        .SelectMany(r => r.VendorCustomer.DefaultIfEmpty(),
                           (r, x) => new NopChatMessageModel
                           {
                               Id = r.Id,
                               DateCreated = r.DateCreated,
                               Text = r.Text,
                               VendorId = r.VendorId,
                               VendorName = r.VendorName,
                               CustomerId = r.CustomerId,
                               CustomerName = r.CustomerName,
                               VendorCustomerId = r.VendorCustomerId,
                               VendorCustomerName = x.Username,
                               IsVendorResponse = r.IsVendorResponse,
                               IsChecked = r.IsChecked,
                               VendorAvatar = GetVendorAvatar(r.VendorId).Result,
                               CustomerAvatar = GetCustomerAvatar(r.CustomerId).Result,
                           }
                        ).ToListAsync();

            var records = new PagedList<NopChatMessageModel>(result, pageIndex, pageSize);

            return records;
        }

        public virtual async Task<string> GetVendorNameByCustomerNameIfExxistAsync(string customerName)
        {
            string vendorName = "";

            vendorName = await _vendorRepository.Table
            .Join(_customerRepository.Table,
                v => v.Id,
                c => c.VendorId,
                (v, c) => new { vendor = v, customer = c }
            )
            .Where(r =>
                        r.vendor.Deleted == false
                        && r.vendor.Active == true
                        && r.customer.Deleted == false
                        && r.customer.Active == true
                        && r.customer.Username == customerName
                        )
            .Select(r => r.vendor.Name)
            .FirstOrDefaultAsync()
            ;
            if (vendorName == null)
                return "";

            return vendorName;
        }

        #endregion
    }
}
