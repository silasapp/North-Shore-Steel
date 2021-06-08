using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NSS.Plugin.Misc.SwiftPortalOverride.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class SwiftPortalOverrideController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public SwiftPortalOverrideController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope =await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var swiftPortalOverrideSettings = await _settingService.LoadSettingAsync<SwiftCoreSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandBox = swiftPortalOverrideSettings.UseSandBox,
                TestEmailAddress = swiftPortalOverrideSettings.TestEmailAddress,
                ApproverMailBox = swiftPortalOverrideSettings.ApproverMailBox,
                NSSApiBaseUrl = swiftPortalOverrideSettings.NSSApiBaseUrl,
                NSSApiAuthUsername = swiftPortalOverrideSettings.NSSApiAuthUsername,
                NSSApiAuthPassword = swiftPortalOverrideSettings.NSSApiAuthPassword,
                StorageAccountKey = swiftPortalOverrideSettings.StorageAccountKey,
                StorageAccountName = swiftPortalOverrideSettings.StorageAccountName,
                StorageContainerName = swiftPortalOverrideSettings.StorageContainerName,
                PayPalUseSandbox = swiftPortalOverrideSettings.PayPalUseSandbox,
                PayPalClientId = swiftPortalOverrideSettings.PayPalClientId,
                PayPalSecretKey = swiftPortalOverrideSettings.PayPalSecretKey,
                MarketingVideoUrl = swiftPortalOverrideSettings.MarketingVideoUrl,
                ApplyForCreditUrl = swiftPortalOverrideSettings.ApplyForCreditUrl,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.UseSandBox_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.UseSandBox, storeScope);
                model.TestEmailAddress_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.TestEmailAddress, storeScope);
                model.ApproverMailBox_OverrideForStore =await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.ApproverMailBox, storeScope);
                model.NSSApiBaseUrl_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.NSSApiBaseUrl, storeScope);
                model.NSSApiAuthUsername_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.NSSApiAuthUsername, storeScope);
                model.NSSApiAuthPassword_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.NSSApiAuthPassword, storeScope);
                model.StorageAccountKey_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.StorageAccountKey, storeScope);
                model.StorageAccountName_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.StorageAccountName, storeScope);
                model.StorageContainerName_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.StorageContainerName, storeScope);
                model.PayPalUseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.PayPalUseSandbox, storeScope);
                model.PayPalClientId_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.PayPalClientId, storeScope);
                model.PayPalSecretKey_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.PayPalSecretKey, storeScope);
                model.MarketingVideoUrl_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.MarketingVideoUrl, storeScope);
                model.ApplyForCreditUrl_OverrideForStore = await _settingService.SettingExistsAsync(swiftPortalOverrideSettings, x => x.ApplyForCreditUrl, storeScope);
            }

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope =await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var swiftPortalOverrideSettings = await _settingService.LoadSettingAsync<SwiftCoreSettings>(storeScope);

            //save settings
            swiftPortalOverrideSettings.UseSandBox = model.UseSandBox;
            swiftPortalOverrideSettings.TestEmailAddress = model.TestEmailAddress;
            swiftPortalOverrideSettings.ApproverMailBox = model.ApproverMailBox;
            swiftPortalOverrideSettings.NSSApiBaseUrl = model.NSSApiBaseUrl;
            swiftPortalOverrideSettings.NSSApiAuthUsername = model.NSSApiAuthUsername;
            swiftPortalOverrideSettings.NSSApiAuthPassword = model.NSSApiAuthPassword;
            swiftPortalOverrideSettings.StorageAccountKey = model.StorageAccountKey;
            swiftPortalOverrideSettings.StorageAccountName = model.StorageAccountName;
            swiftPortalOverrideSettings.StorageContainerName = model.StorageContainerName;
            swiftPortalOverrideSettings.PayPalUseSandbox = model.PayPalUseSandbox;
            swiftPortalOverrideSettings.PayPalClientId = model.PayPalClientId;
            swiftPortalOverrideSettings.PayPalSecretKey = model.PayPalSecretKey;
            swiftPortalOverrideSettings.MarketingVideoUrl = model.MarketingVideoUrl;
            swiftPortalOverrideSettings.ApplyForCreditUrl = model.ApplyForCreditUrl;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.UseSandBox, model.UseSandBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.TestEmailAddress, model.TestEmailAddress_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.ApproverMailBox, model.ApproverMailBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.NSSApiBaseUrl, model.NSSApiBaseUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.NSSApiAuthUsername, model.NSSApiAuthUsername_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.NSSApiAuthPassword, model.NSSApiAuthPassword_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.StorageAccountKey, model.StorageAccountKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.StorageAccountName, model.StorageAccountName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.StorageContainerName, model.StorageContainerName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.PayPalUseSandbox, model.PayPalUseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.PayPalClientId, model.PayPalClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.PayPalSecretKey, model.PayPalSecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.MarketingVideoUrl, model.MarketingVideoUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swiftPortalOverrideSettings, x => x.ApplyForCreditUrl, model.ApplyForCreditUrl_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

    }
}
