using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Factories;
using NopStation.Plugin.Widgets.Helpdesk.Models;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Controllers
{
    public class HelpdeskTicketController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ITicketService _ticketService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly HelpdeskSettings _helpdeskSettings;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IDepartmentService _departmentService;
        private readonly ITicketModelFactory _ticketModelFactory;
        private readonly INopFileProvider _fileProvider;
        private readonly OrderSettings _orderSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IWebHelper _webHelper;

        public HelpdeskTicketController(HelpdeskSettings ticketSettings,
            IOrderService orderService,
            IWorkContext workContext,
            ITicketService ticketService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            ITicketModelFactory ticketModelFactory,
            IDownloadService downloadService,
            HelpdeskSettings helpdeskSettings,
            IStoreContext storeContext,
            IProductService productService,
            IDepartmentService departmentService,
            INopFileProvider fileProvider,
            OrderSettings orderSettings,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            IQueuedEmailService queuedEmailService,
            IWebHelper webHelper)
        {
            _orderService = orderService;
            _workContext = workContext;
            _ticketService = ticketService;
            _localizationService = localizationService;
            _customerService = customerService;
            _downloadService = downloadService;
            _helpdeskSettings = helpdeskSettings;
            _storeContext = storeContext;
            _productService = productService;
            _departmentService = departmentService;
            _ticketModelFactory = ticketModelFactory;
            _fileProvider = fileProvider;
            _orderSettings = orderSettings;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _queuedEmailService = queuedEmailService;
            _webHelper = webHelper;
        }

        #region Utilities

        protected bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        protected virtual async Task<bool> IsMinimumTicketCreateIntervalValidAsync(Customer customer)
        {
            if (_helpdeskSettings.MinimumTicketCreateInterval == 0)
                return true;

            var lastTicket = (await _ticketService.GetAllTicketsAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: customer.Id, pageSize: 1))
                .FirstOrDefault();
            if (lastTicket == null)
                return true;

            var interval = DateTime.UtcNow - lastTicket.CreatedOnUtc;
            return interval.TotalSeconds > _helpdeskSettings.MinimumTicketCreateInterval;
        }

        protected virtual async Task<bool> IsMinimumResponseCreateIntervalValidAsync(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_helpdeskSettings.MinimumTicketCreateInterval == 0)
                return true;

            var lastResponse = (await _ticketService.GetTicketResponsesByCustomerIdAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: customer.Id, pageSize: 1))
                .FirstOrDefault();
            if (lastResponse == null)
                return true;

            var interval = DateTime.UtcNow - lastResponse.CreatedOnUtc;
            return interval.TotalSeconds > _helpdeskSettings.MinimumTicketCreateInterval;
        }

        #endregion

        #region Ticket list

        public IActionResult ReportBug()
        {
            return RedirectToRoute("HelpdeskAddNewTicket");
        }

        public async Task<IActionResult> List()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var tickets = await _ticketService.GetAllTicketsAsync(customerId: (await _workContext.GetCurrentCustomerAsync()).Id);
            var model = await _ticketModelFactory.PrepareTicketListModelAsync(tickets);

            return View(model);
        }

        public async Task<IActionResult> AddNew()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = await _ticketModelFactory.PrepareAddNewTicketModelAsync(null);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(TicketModel model)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (_helpdeskSettings.EnableTicketDepartment && _helpdeskSettings.TicketDepartmentRequired)
            {
                var depaerment = await _departmentService.GetHelpdeskDepartmentByIdAsync(model.DepartmentId);
                if (depaerment == null)
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Department.Required"));
            }

            if (_helpdeskSettings.EnableTicketCategory)
            {
                if (_helpdeskSettings.TicketCategoryRequired && !Enum.IsDefined(typeof(TicketCategory), model.CategoryId))
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Category.Required"));

                if (model.CategoryId == (int)TicketCategory.Order)
                {
                    var order = await _orderService.GetOrderByIdAsync(model.OrderId);
                    if (order == null || order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Order.Invalid"));
                }
                else if (model.CategoryId == (int)TicketCategory.Product)
                {
                    var product = await _productService.GetProductByIdAsync(model.ProductId);
                    if (product == null || product.Deleted)
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Product.Invalid"));
                }
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await IsMinimumTicketCreateIntervalValidAsync(customer))
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.MinTicketCreateInterval"));

            if (ModelState.IsValid)
            {
                var ticket = new Ticket()
                {
                    Body = model.Body,
                    CategoryId = model.CategoryId,
                    CreatedByCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Email = model.Email,
                    Name = model.Name,
                    OrderId = model.OrderId,
                    ProductId = model.ProductId,
                    TicketGuid = Guid.NewGuid(),
                    UpdatedOnUtc = DateTime.UtcNow,
                    PhoneNumber = model.PhoneNumber,
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    Subject = model.Subject,
                    Status = TicketStatus.Open
                };

                if (_helpdeskSettings.AllowCustomerToUploadAttachmentInTicket)
                {
                    var download = _downloadService.GetDownloadByGuidAsync(model.DownloadGuid);
                    ticket.DownloadId = download?.Id ?? 0;
                }
                if (_helpdeskSettings.AllowCustomerToSetPriority)
                    ticket.PriorityId = model.PriorityId;
                else
                    ticket.PriorityId = _helpdeskSettings.DefaultTicketPriorityId;

                if (_helpdeskSettings.EnableTicketDepartment)
                    ticket.DepartmentId = model.DepartmentId;

                await _ticketService.InsertTicketAsync(ticket);

                if (_helpdeskSettings.SendEmailOnNewTicket && !string.IsNullOrWhiteSpace(_helpdeskSettings.SendEmailsTo))
                {
                    var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(_helpdeskSettings.EmailAccountId) ??
                        await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                                       (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

                    var customerEmail = customer.Email;
                    var customerName = await _customerService.GetCustomerFullNameAsync(customer);
                    var adminLink = $"{_webHelper.GetStoreLocation()}Admin/Ticket/Edit/{ticket.Id}";

                    var emailAddresses = _helpdeskSettings.SendEmailsTo.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(x => IsValidEmail(x));

                    foreach (var emailAddress in emailAddresses)
                    {
                        var email = new QueuedEmail
                        {
                            Priority = QueuedEmailPriority.High,
                            From = emailAccount.Email,
                            FromName = emailAccount.DisplayName,
                            To = emailAddress,
                            ToName = null,
                            ReplyTo = customerEmail,
                            ReplyToName = customerName,
                            CC = string.Empty,
                            Bcc = null,
                            Subject = await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Email.Subject"),
                            Body = string.Format(await _localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Email.Body"), customerName, adminLink),
                            CreatedOnUtc = DateTime.UtcNow,
                            EmailAccountId = emailAccount.Id,
                            DontSendBeforeDateUtc = null
                        };

                        await _queuedEmailService.InsertQueuedEmailAsync(email);
                    }
                }

                return RedirectToRoute("HelpdeskTicketDetails", new { id = ticket.Id });
            }

            model = await _ticketModelFactory.PrepareAddNewTicketModelAsync(model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null || ticket.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return NotFound();

            var model = await _ticketModelFactory.PrepareTicketDetailsModelAsync(ticket);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddResponse(TicketResponseModel model)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var ticket = await _ticketService.GetTicketByIdAsync(model.TicketId);
            if (ticket == null || ticket.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return RedirectToRoute("HelpdeskTickets");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await IsMinimumResponseCreateIntervalValidAsync(customer))
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Helpdesk.TicketResponses.MinResponseCreateInterval"));

            ModelState.Remove("DownloadGuid");

            if (ModelState.IsValid)
            {
                var response = new TicketResponse()
                {
                    Body = model.Body,
                    CreatedByCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = true,
                    TicketId = model.TicketId
                };

                if (_helpdeskSettings.AllowCustomerToUploadAttachmentInResponse)
                {
                    var download = await _downloadService.GetDownloadByGuidAsync(model.DownloadGuid);
                    response.DownloadId = download?.Id ?? 0;
                }
                await _ticketService.InsertTicketResponseAsync(response);

                if (_helpdeskSettings.SendEmailOnNewResponse && !string.IsNullOrWhiteSpace(_helpdeskSettings.SendEmailsTo))
                {
                    var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(_helpdeskSettings.EmailAccountId) ??
                        await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                                       (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

                    var customerEmail = customer.Email;
                    var customerName = await _customerService.GetCustomerFullNameAsync(customer);
                    var adminLink = $"{_webHelper.GetStoreLocation()}Admin/Ticket/Edit/{ticket.Id}";

                    var emailAddresses = _helpdeskSettings.SendEmailsTo.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(x => IsValidEmail(x));

                    foreach (var emailAddress in emailAddresses)
                    {
                        var email = new QueuedEmail
                        {
                            Priority = QueuedEmailPriority.High,
                            From = emailAccount.Email,
                            FromName = emailAccount.DisplayName,
                            To = emailAddress,
                            ToName = null,
                            ReplyTo = customerEmail,
                            ReplyToName = customerName,
                            CC = string.Empty,
                            Bcc = null,
                            Subject = await _localizationService.GetResourceAsync("NopStation.Helpdesk.TicketResponses.Email.Subject"),
                            Body = string.Format(await _localizationService.GetResourceAsync("NopStation.Helpdesk.TicketResponses.Email.Body"), customerName, adminLink),
                            CreatedOnUtc = DateTime.UtcNow,
                            EmailAccountId = emailAccount.Id,
                            DontSendBeforeDateUtc = null
                        };

                        await _queuedEmailService.InsertQueuedEmailAsync(email);
                    }
                }

                return Json(new { Result = true });
            }

            return RedirectToRoute("HelpdeskTicketDetails", new { Id = model.TicketId });
        }

        public async Task<IActionResult> DownloadFile(Guid downloadGuid)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            var fileName = downloadGuid.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        [HttpPost]
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

            var validationFileMaximumSize = _orderSettings.ReturnRequestsFileMaximumSize;
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
                message = _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            });
        }

        #endregion
    }
}