using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.SwiftPortalOverride.Models;
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

namespace Nop.Plugin.Misc.SwiftPortalOverride.Controllers
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

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftPortalOverrideSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandBox = swiftPortalOverrideSettings.UseSandBox,
                TestEmailAddress = swiftPortalOverrideSettings.TestEmailAddress,
                ApproverMailBox = swiftPortalOverrideSettings.ApproverMailBox,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.UseSandBox__OverrideForStore = _settingService.SettingExists(swiftPortalOverrideSettings, x => x.UseSandBox, storeScope);
                model.TestEmailAddress__OverrideForStore = _settingService.SettingExists(swiftPortalOverrideSettings, x => x.TestEmailAddress, storeScope);
                model.ApproverEmailBox__OverrideForStore = _settingService.SettingExists(swiftPortalOverrideSettings, x => x.ApproverMailBox, storeScope);
            }

            return View("~/Plugins/Misc.SwiftPortalOverride/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftPortalOverrideSettings>(storeScope);

            //save settings
            swiftPortalOverrideSettings.UseSandBox = model.UseSandBox;
            swiftPortalOverrideSettings.TestEmailAddress = model.TestEmailAddress;
            swiftPortalOverrideSettings.ApproverMailBox = model.ApproverMailBox;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            _settingService.SaveSettingOverridablePerStore(swiftPortalOverrideSettings, x => x.UseSandBox, model.UseSandBox__OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(swiftPortalOverrideSettings, x => x.TestEmailAddress, model.TestEmailAddress__OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(swiftPortalOverrideSettings, x => x.ApproverMailBox, model.ApproverEmailBox__OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion

    }
}
