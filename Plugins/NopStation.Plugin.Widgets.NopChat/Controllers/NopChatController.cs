using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Vendors;
using Nop.Services.Media;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.NopChat.Domains;
using NopStation.Plugin.Widgets.NopChat.Factories;
using NopStation.Plugin.Widgets.NopChat.Hubs;
using NopStation.Plugin.Widgets.NopChat.Services;

namespace NopStation.Plugin.Widgets.NopChat.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class NopChatController : NopStationPublicController
    {
        #region Fields 

        private readonly NopChatHub _nopChatHub;
        private readonly INopChatMessageService _nopChatMessageService;
        private readonly IVendorService _vendorService;
        private readonly INopChatMessageModelFactory _nopChatMessageModelFactory;
        private readonly IPictureService _pictureService;

        #endregion

        public NopChatController(NopChatHub nopChatHub,
            INopChatMessageService nopChatMessageService,
            IVendorService vendorService,
            INopChatMessageModelFactory nopChatMessageModelFactory,
            IPictureService pictureService)
        {
            _nopChatHub = nopChatHub;
            _nopChatMessageService = nopChatMessageService;
            _vendorService = vendorService;
            _nopChatMessageModelFactory = nopChatMessageModelFactory;
            _pictureService = pictureService;
        }

        #region Methods

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendMessage(NopChatMessage model)
        {
            model.DateCreated = DateTime.Now;
            model.IsChecked = false;
            try
            {
                await _nopChatMessageService.InsertNopChatMessageAsync(model);

                var newMessage = await _nopChatMessageModelFactory.PrepareNopChatMessageModelAsync(model);

                await _nopChatHub.SendNewMessage(newMessage);
                return Json(new { Result = true, Message = newMessage });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Result = false,
                    Error = ex
                });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetChatHistory(int customerId, int vendorId)
        {
            var data = await _nopChatMessageService.GetChatHistoryAsync(customerId, vendorId);

            if (data == null)
                return Json(new { Result = false });

            return Json(new { Result = data });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetChatHistoryPaged(int customerId, int vendorId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var data = await _nopChatMessageModelFactory.PrepareNopChatMessageListAsync(customerId, vendorId, pageIndex, pageSize);

            if (data == null)
                return Json(new { Result = false });

            return Json(new { Result = data });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetVendorById(int vendorId)
        {
            var data = new Vendor();
            try
            {
                data = await _vendorService.GetVendorByIdAsync(vendorId);
            }
            catch (Exception ex)
            {
                return Json(new { Result = false });
            }
            return Json(new
            {
                Result = new
                {
                    Id = data.Id,
                    Name = data.Name,
                    AvatarURL = await _pictureService.GetPictureUrlAsync(data.PictureId)
                }
            });
        }

        #endregion
    }
}