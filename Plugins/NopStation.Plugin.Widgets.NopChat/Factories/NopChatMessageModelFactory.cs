using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Vendors;
using NopStation.Plugin.Widgets.NopChat.Domains;
using NopStation.Plugin.Widgets.NopChat.Models;
using NopStation.Plugin.Widgets.NopChat.Services;

namespace NopStation.Plugin.Widgets.NopChat.Factories
{
    public class NopChatMessageModelFactory : INopChatMessageModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly INopChatMessageService _nopChatMessageService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public NopChatMessageModelFactory(ICustomerService customerService,
            IVendorService vendorService,
            INopChatMessageService nopChatMessageService,
            IRepository<Customer> customerRepository,
            IRepository<Vendor> vendorRepository,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings)
        {
            _customerService = customerService;
            _vendorService = vendorService;
            _nopChatMessageService = nopChatMessageService;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Methods

        public async Task<NopChatMessageModel> PrepareNopChatMessageModelAsync(NopChatMessage data)
        {
            NopChatMessageModel model = new NopChatMessageModel();
            model.Id = data.Id;
            model.Text = data.Text;
            model.DateCreated = data.DateCreated;
            model.VendorId = data.VendorId;
            var vendor = await _vendorService.GetVendorByIdAsync(data.VendorId);
            model.VendorName = vendor != null ? vendor.Name : "";
            model.CustomerId = data.CustomerId;
            model.CustomerName = (await _customerService.GetCustomerByIdAsync(data.CustomerId)).Username;
            model.VendorCustomerId = data.VendorCustomerId;
            if (data.VendorCustomerId > 0)
                model.VendorCustomerName = (await _customerService.GetCustomerByIdAsync(data.VendorCustomerId)).Username;

            model.IsVendorResponse = data.IsVendorResponse;
            model.IsChecked = data.IsChecked;

            model.CustomerAvatar = await GetCustomerAvatar(data.CustomerId);
            model.VendorAvatar = await GetVendorAvatar(data.VendorId);
            return model;
        }

        public async Task<IList<NopChatMessageModel>> PrepareNopChatMessageListAsync(int customerId, int vendorId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var data = await _nopChatMessageService.GetChatHistoryPagedAsync(customerId, vendorId, pageIndex, pageSize);
            var messages = await data.OrderBy(d => d.Id).ToListAsync();
            return messages;
        }

        #endregion

        #region Utils 

        public virtual async Task<string> GetVendorAvatar(int vendorId)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor != null)
            {
                var avatar = await _pictureService.GetPictureUrlAsync(vendor.PictureId);
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

        #endregion
    }
}
