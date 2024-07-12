using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Controllers
{
    public class TicketController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ITicketModelFactory _ticketModelFactory;
        private readonly ITicketService _ticketService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly ICustomerService _customerService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly HelpdeskSettings _ticketSettings;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public TicketController(ILocalizationService localizationService,
            INotificationService notificationService,
            ITicketModelFactory ticketModelFactory,
            ITicketService ticketService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            ICustomerService customerService,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            IQueuedEmailService queuedEmailService,
            HelpdeskSettings ticketSettings,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _ticketModelFactory = ticketModelFactory;
            _ticketService = ticketService;
            _permissionService = permissionService;
            _workContext = workContext;
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _customerService = customerService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _queuedEmailService = queuedEmailService;
            _ticketSettings = ticketSettings;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        #region Tickets

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            var searchModel = await _ticketModelFactory.PrepareTicketSearchModel(new TicketSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<JsonResult> List(TicketSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return await AccessDeniedDataTablesJson();

            var model = await _ticketModelFactory.PrepareTicketListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            var model = await _ticketModelFactory.PrepareTicketModelAsync(new TicketModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(TicketModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var ticket = model.ToEntity<Ticket>();
                ticket.CreatedByCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                ticket.CategoryId = model.CategoryId;
                ticket.StatusId = model.StatusId;
                ticket.PriorityId = model.PriorityId;
                ticket.UpdatedOnUtc = DateTime.UtcNow;
                ticket.CreatedOnUtc = DateTime.UtcNow;
                ticket.TicketGuid = Guid.NewGuid();
                await _ticketService.InsertTicketAsync(ticket);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = ticket.Id });
            }

            model = await _ticketModelFactory.PrepareTicketModelAsync(new TicketModel(), null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return RedirectToAction("List");

            var model = await _ticketModelFactory.PrepareTicketModelAsync(null, ticket);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(TicketModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            var ticket = await _ticketService.GetTicketByIdAsync(model.Id);
            if (ticket == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                ticket = model.ToEntity(ticket);
                ticket.UpdatedOnUtc = DateTime.UtcNow;
                ticket.StatusId = model.StatusId;
                ticket.CategoryId = model.CategoryId;
                ticket.PriorityId = model.PriorityId;
                await _ticketService.UpdateTicketAsync(ticket);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = ticket.Id });
            }
            model = await _ticketModelFactory.PrepareTicketModelAsync(model, ticket);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return AccessDeniedView();

            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return RedirectToAction("List");

            await _ticketService.DeleteTicketAsync(ticket);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Responses

        [HttpPost]
        public virtual async Task<JsonResult> ResponseList(ResponseSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return await AccessDeniedDataTablesJson();

            //try to get a ticket with the specified id
            var ticket = await _ticketService.GetTicketByIdAsync(searchModel.Ticketid)
                ?? throw new ArgumentException("No ticket found with the specified id");

            //prepare model
            var model = await _ticketModelFactory.PrepareResponseListModelAsync(searchModel, ticket);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> ResponseAdd(ResponseModel model)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return await AccessDeniedDataTablesJson();

            var ticket = await _ticketService.GetTicketByIdAsync(model.TicketId);
            if (ticket == null)
                return ErrorJson("Ticket cannot be loaded");

            if (ModelState.IsValid)
            {
                var response = new TicketResponse()
                {
                    Body = model.Body,
                    CreatedByCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = model.DisplayToCustomer,
                    DownloadId = model.DownloadId,
                    TicketId = ticket.Id
                };
                await _ticketService.InsertTicketResponseAsync(response);

                if (_ticketSettings.SendEmailOnNewResponse)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(ticket.CustomerId);
                    var customerName = await _customerService.GetCustomerFullNameAsync(customer);

                    var adminCustomerName = await _customerService.GetCustomerFullNameAsync(await _workContext.GetCurrentCustomerAsync());
                    var ticketLink = $"{_webHelper.GetStoreLocation()}customer/ticket/{ticket.Id}";

                    var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(_ticketSettings.EmailAccountId) ??
                            await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                                           (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

                    var email = new QueuedEmail()
                    {
                        //Priority = 5,
                        From = emailAccount.Email,
                        FromName = emailAccount.DisplayName,
                        To = ticket.Email ?? customer.Email,
                        ToName = customerName,
                        CC = string.Empty,
                        Bcc = null,
                        Subject = await _localizationService.GetResourceAsync("NopStation.Helpdesk.TicketResponses.Email.Subject"),
                        Body = string.Format(await _localizationService.GetResourceAsync("NopStation.Helpdesk.TicketResponses.Email.Body"), adminCustomerName, ticketLink),
                        CreatedOnUtc = DateTime.UtcNow,
                        EmailAccountId = emailAccount.Id
                    };

                    await _queuedEmailService.InsertQueuedEmailAsync(email);
                }
            }

            return Json(new { Result = true });
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> ResponseDelete(ResponseModel model)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
                return await AccessDeniedDataTablesJson();

            var response = await _ticketService.GetTicketResponseByIdAsync(model.Id);
            if (response == null)
                return ErrorJson("Response cannot be loaded");

            await _ticketService.DeleteTicketResponseAsync(response);

            return Json(new { Result = true });
        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var download = await _downloadService.GetDownloadByIdAsync(id);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            var fileName = download.DownloadGuid.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        [EditAccessAjax, HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> UploadFile()
        {
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty,
                });
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var validationFileMaximumSize = 2048;
            if (validationFileMaximumSize > 0)
            {
                //compare in bytes
                var maxFileSizeBytes = validationFileMaximumSize * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), validationFileMaximumSize),
                        downloadGuid = Guid.Empty,
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            });
        }

        #endregion

        #endregion
    }
}
