using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Quickstream.Domains;
using NopStation.Plugin.Payments.Quickstream.Areas.Admin.Factories;
using NopStation.Plugin.Payments.Quickstream.Areas.Admin.Models;
using NopStation.Plugin.Payments.Quickstream.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using System.Linq;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Payments.Quickstream.Areas.Admin.Controllers
{
    public class QuickStreamController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAcceptedCardModelFactory _acceptedCardModelFactory;
        private readonly IQuickStreamPaymentService _quickStreamPaymentService;
        private readonly IAcceptedCardService _acceptedCardService;

        #endregion

        #region Ctor

        public QuickStreamController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IAcceptedCardModelFactory acceptedCardModelFactory,
            IQuickStreamPaymentService quickStreamPaymentService,
            IAcceptedCardService acceptedCardService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _acceptedCardModelFactory = acceptedCardModelFactory;
            _quickStreamPaymentService = quickStreamPaymentService;
            _acceptedCardService = acceptedCardService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var quickStreamSettings = await _settingService.LoadSettingAsync<QuickstreamSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                UseSandbox = quickStreamSettings.UseSandbox,
                SupplierBusinessCode = quickStreamSettings.SupplierBusinessCode,
                CommunityCode = quickStreamSettings.CommunityCode,
                PublishableApiKey = quickStreamSettings.PublishableApiKey,
                SecretApiKey = quickStreamSettings.SecretApiKey,
                IpAddress = quickStreamSettings.IpAddress
            };

            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(quickStreamSettings, x => x.UseSandbox, storeScope);
                model.SupplierBusinessCode_OverrideForStore = await _settingService.SettingExistsAsync(quickStreamSettings, x => x.SupplierBusinessCode, storeScope);
                model.CommunityCode_OverrideForStore = await _settingService.SettingExistsAsync(quickStreamSettings, x => x.CommunityCode, storeScope);
                model.PublishableApiKey_OverrideForStore = await _settingService.SettingExistsAsync(quickStreamSettings, x => x.PublishableApiKey, storeScope);
                model.SecretApiKey_OverrideForStore = await _settingService.SettingExistsAsync(quickStreamSettings, x => x.SecretApiKey, storeScope);
                model.IpAddress_OverrideForStore = await _settingService.SettingExistsAsync(quickStreamSettings, x => x.IpAddress, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var quickStreamSettings = await _settingService.LoadSettingAsync<QuickstreamSettings>(storeScope);

            quickStreamSettings.UseSandbox = model.UseSandbox;
            quickStreamSettings.SupplierBusinessCode = model.SupplierBusinessCode;
            quickStreamSettings.CommunityCode = model.CommunityCode;
            quickStreamSettings.PublishableApiKey = model.PublishableApiKey;
            quickStreamSettings.SecretApiKey = model.SecretApiKey;
            quickStreamSettings.IpAddress = model.IpAddress;


            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(quickStreamSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(quickStreamSettings, x => x.SupplierBusinessCode, model.SupplierBusinessCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(quickStreamSettings, x => x.CommunityCode, model.CommunityCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(quickStreamSettings, x => x.PublishableApiKey, model.PublishableApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(quickStreamSettings, x => x.SecretApiKey, model.SecretApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(quickStreamSettings, x => x.IpAddress, model.IpAddress_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> AcceptCardList()
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var searchModel = new AcceptedCardSearchModel();
            searchModel.SetGridPageSize();

            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptCardList(AcceptedCardSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var model = await _acceptedCardModelFactory.PrepareAcceptedCardListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> Sync()
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var cards = await _quickStreamPaymentService.GetAcceptCardsWithSurChargeAsync(storeScope);

            var existingCards = await _acceptedCardService.GetAllAcceptedCardsAsync();
            foreach (var acceptedCard in existingCards)
                if (!cards.Any(x => x.CardScheme == acceptedCard.CardScheme && x.CardType == acceptedCard.CardType))
                    await _acceptedCardService.DeleteAcceptedCardAsync(acceptedCard);

            foreach (var card in cards)
            {
                if (existingCards.FirstOrDefault(x => x.CardScheme == card.CardScheme && x.CardType == card.CardType) is AcceptedCard acceptedCard)
                {
                    acceptedCard.Surcharge = double.Parse(card.SurchargePercentage);
                    acceptedCard.IsEnable = true;
                    await _acceptedCardService.UpdateAcceptedCardAsync(acceptedCard);
                }
                else
                {
                    var newAcceptedCard = new AcceptedCard
                    {
                        CardScheme = card.CardScheme,
                        CardType = card.CardType,
                        IsEnable = true,
                        Surcharge = double.Parse(card.SurchargePercentage)
                    };
                    await _acceptedCardService.InsertAcceptedCardAsync(newAcceptedCard);
                }
            }
            return Json(new { });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var acceptedCard = await _acceptedCardService.GetAcceptedCardByIdAsync(id);
            if (acceptedCard == null)
                return RedirectToAction("AcceptCardList");

            var model = await _acceptedCardModelFactory.PrepareAcceptedCardModelAsync(null, acceptedCard);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(AcceptedCardModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var acceptedCard = await _acceptedCardService.GetAcceptedCardByIdAsync(model.Id);
            if (acceptedCard == null)
                return RedirectToAction("AcceptCardList");

            if (ModelState.IsValid)
            {
                acceptedCard.IsEnable = model.IsEnable;
                acceptedCard.PictureId = model.PictureId;
                await _acceptedCardService.UpdateAcceptedCardAsync(acceptedCard);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.QuickStream.AcceptedCards.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = acceptedCard.Id });
            }

            model = await _acceptedCardModelFactory.PrepareAcceptedCardModelAsync(null, acceptedCard);

            return View(model);
        }

        #endregion
    }
}
