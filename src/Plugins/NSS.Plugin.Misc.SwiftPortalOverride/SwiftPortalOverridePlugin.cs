using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride
{
    public class SwiftPortalOverridePlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly EmailAccountSettings _emailAccountSettings;

        #endregion

        #region Ctor

        public SwiftPortalOverridePlugin(IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreService storeService,
            IWebHelper webHelper,
            IMessageTemplateService messageTemplateService,
            EmailAccountSettings emailAccountSettings)
        {
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeService = storeService;
            _webHelper = webHelper;
            _messageTemplateService = messageTemplateService;
            _emailAccountSettings = emailAccountSettings;
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
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(
                new SwiftCoreSettings
                {
                    StorageContainerName = "swiftportal-container",
                    ApplyForCreditUrl = "https://www.nssco.com/assets/files/newaccountform.pdf"
                }
            );

            //locales
            /* _localizationService.AddPluginLocaleResource() changed to _localizationService.AddLocaleResourceAsync() */
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                // config fields
                ["Plugins.Misc.SwiftPortalOverride.Fields.UseSandBox"] = "Use SandBox",
                ["Plugins.Misc.SwiftPortalOverride.Fields.UseSandBox.Hint"] = "Enable sandbox mode.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.TestEmailAddress"] = "Test Email Address",
                ["Plugins.Misc.SwiftPortalOverride.Fields.TestEmailAddress.Hint"] = "Enter email address to be used in sandbox mode. You can use ';' as seperator for multiple email addresses.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.ApproverMailBox"] = "Approver Mail Box",
                ["Plugins.Misc.SwiftPortalOverride.Fields.ApproverMailBox.Hint"] = "Enter NSS approver mail box.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NSSApiBaseUrl"] = "NSS API BaseUrl",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NSSApiBaseUrl.Hint"] = "Enter NSS API base url.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NSSApiAuthUsername"] = "NSS Api Username",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NSSApiAuthUsername.Hint"] = "Enter NSS API authentication username.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NSSApiAuthPassword"] = "NSS API Password",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NSSApiAuthPassword.Hint"] = "Enter NSS API authentication password.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.StorageAccountName"] = "Storage Account Name",
                ["Plugins.Misc.SwiftPortalOverride.Fields.StorageAccountName.Hint"] = "Enter Storage Account Name.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.StorageAccountKey"] = "Storage Account Key",
                ["Plugins.Misc.SwiftPortalOverride.Fields.StorageAccountKey.Hint"] = "Enter Storage Account key.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.StorageContainerName"] = "Storage Container Name",
                ["Plugins.Misc.SwiftPortalOverride.Fields.StorageContainerName.Hint"] = "Enter Storage Container Name.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.PayPalUseSandbox"] = "PayPal Use Sandbox",
                ["Plugins.Misc.SwiftPortalOverride.Fields.PayPalUseSandbox.Hint"] = "Enable PayPal sandbox environment.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.PayPalClientId"] = "PayPal ClientID",
                ["Plugins.Misc.SwiftPortalOverride.Fields.PayPalClientId.Hint"] = "Enter PayPal ClientID.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.PayPalSecretKey"] = "PayPal Secret Key",
                ["Plugins.Misc.SwiftPortalOverride.Fields.PayPalSecretKey.Hint"] = "Enter PayPal Secret Key.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.MarketingVideoUrl"] = "Marketing Video Url",
                ["Plugins.Misc.SwiftPortalOverride.Fields.MarketingVideoUrl.Hint"] = "Enter marketing video url that will be embedded when a customer signs up.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.ApplyForCreditUrl"] = "Apply For Credit Url",
                ["Plugins.Misc.SwiftPortalOverride.Fields.ApplyForCreditUrl.Hint"] = "Enter apply for credit link url to be used on invoice screen.",
                ["Plugins.Misc.SwiftPortalOverride.Fields.NewCustomerWelcomeMessage.Text"] = @"
                    <p>
                        Thank you for registering as a new customer.
                    </p>
                    <p>
                        Our goal is to have approval within one business hour.
                    </p>
                    <p>
                        If you need immediate assistance, call Marcelo at 713-980-5879.
                    </p>",
                ["Plugins.Misc.SwiftPortalOverride.Fields.OldCustomerWelcomeMessage.Text"] = @"
                    <p>
                        Thank you for registering as an existing customer.
                    </p>
                    <p>
                        It is important that we properly synchronize you with our current database.
                    </p>
                    <p>
                        Our goal is to have approval within four business hour.
                    </p>
                    <p>
                        If you need immediate assistance, call Marcelo at 713-980-5879.
                    </p>",

                // add other text
    });

            // create proc
            var settings = DataSettingsManager.LoadSettings();
            var dataProvider = DataProviderManager.GetDataProvider(settings.DataProvider);

            await dataProvider.ExecuteNonQueryAsync(@"IF EXISTS (
                SELECT type_desc, type
                FROM sys.procedures WITH(NOLOCK)
                WHERE NAME = 'ProductLoadAllPagedSwiftPortal'
                    AND type = 'P'
              )
             DROP PROCEDURE dbo.ProductLoadAllPagedSwiftPortal");

            var sql = GetSQL("ProductLoadAllPagedSwiftPortal");
            await dataProvider.ExecuteNonQueryAsync(sql);

            // email template
            await ConfigureMessageTemplatesAsync();

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<SwiftCoreSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.SwiftPortalOverride");

            //// email template
            var changePasswordTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(SwiftPortalOverrideDefaults.ChangePasswordMessageTemplateName))?.FirstOrDefault();
            if (changePasswordTemplate != null)
                await _messageTemplateService.DeleteMessageTemplateAsync(changePasswordTemplate);

            var pendingApprovalTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(SwiftPortalOverrideDefaults.NewCustomerPendingApprovalMessageTemplateName))?.FirstOrDefault();
            if (pendingApprovalTemplate != null)
                await _messageTemplateService.DeleteMessageTemplateAsync(changePasswordTemplate);

            var denialTemplate =  (await _messageTemplateService.GetMessageTemplatesByNameAsync(SwiftPortalOverrideDefaults.NewCustomerRejectionMessageTemplateName))?.FirstOrDefault();
            if (denialTemplate != null)
                await _messageTemplateService.DeleteMessageTemplateAsync(changePasswordTemplate);

            await base.UninstallAsync();
        }

        async Task ConfigureMessageTemplatesAsync()
        {
            // change password email
            var changePasswordTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(SwiftPortalOverrideDefaults.ChangePasswordMessageTemplateName))?.FirstOrDefault();
            if (changePasswordTemplate == null)
            {
                changePasswordTemplate = new MessageTemplate
                {
                    Name = SwiftPortalOverrideDefaults.ChangePasswordMessageTemplateName,
                    Subject = "%Store.Name%. Change Password",
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId,
                    Body = $"<a href={"\"%Store.URL%\""}>%Store.Name%</a>  <br />  <br />   Your Password was changed successfully.  <br />  <br />  %Store.Name%  ",
                    IsActive = true,
                };

                await _messageTemplateService.InsertMessageTemplateAsync(changePasswordTemplate);
            }


            // pennding aproval email
            var pendingApprovalTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(SwiftPortalOverrideDefaults.NewCustomerPendingApprovalMessageTemplateName))?.FirstOrDefault();
            if (pendingApprovalTemplate == null)
            {
                pendingApprovalTemplate = new MessageTemplate
                {
                    Name = SwiftPortalOverrideDefaults.NewCustomerPendingApprovalMessageTemplateName,
                    Subject = "%Store.Name%. Registration Pending Approval",
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId,
                    Body = $"<a href={"\"%Store.URL%\""}>%Store.Name%</a>  <br />  <br />   Your registration is pending approval.  <br />  <br />  %Store.Name%  ",
                    IsActive = true,
                };

                await _messageTemplateService.InsertMessageTemplateAsync(pendingApprovalTemplate);
            }

            // reject email
            var denialTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(SwiftPortalOverrideDefaults.NewCustomerRejectionMessageTemplateName))?.FirstOrDefault();
            if (denialTemplate == null)
            {
                denialTemplate = new MessageTemplate
                {
                    Name = SwiftPortalOverrideDefaults.NewCustomerRejectionMessageTemplateName,
                    Subject = "%Store.Name%. Registration Denial",
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId,
                    Body = $"<a href={"\"%Store.URL%\""}>%Store.Name%</a>  <br />  <br />   Your registration has been denied.  <br />  <br />  %Store.Name%  ",
                    IsActive = true,
                };

                await _messageTemplateService.InsertMessageTemplateAsync(denialTemplate);
            }
        }

        static string GetSQL(string file)
        {
            string sql = null;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"NSS.Plugin.Misc.SwiftPortalOverride.Domains.SQL.{file}.sql";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                sql = reader.ReadToEnd();
            }

            return sql;
        }

        #endregion
    }
}
