using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride
{
    public class SwiftPortalOverridePlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public SwiftPortalOverridePlugin(IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreService storeService,
            IWebHelper webHelper)
        {
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeService = storeService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/SwiftPortalOverride/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new SwiftPortalOverrideSettings());

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Misc.SwiftPortalOverride.Fields.UseSandBox"] = "Use SandBox",
                ["Plugins.Misc.SwiftPortalOverride.Fields.UseSandBox.Hint"] = "Enable sandbox mode.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.TestEmailAddress"] = "Test Email Address",
                ["Plugins.Misc.SwiftPortalOverride.Fields.TestEmailAddress.Hint"] = "Enter email address to be used in sandbox mode. You can use ';' as seperator for multiple email addresses.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.ApproverMailBox"] = "Approver Mail Box ",
                ["Plugins.Misc.SwiftPortalOverride.Fields.ApproverMailBox.Hint"] = "Enter swift approver mail box.",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SwiftPortalOverrideSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Misc.SwiftPortalOverride");

            base.Uninstall();
        }

        #endregion
    }
}
