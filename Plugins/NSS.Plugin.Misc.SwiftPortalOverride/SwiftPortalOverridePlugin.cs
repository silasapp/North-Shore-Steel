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

        #endregion

        #region Ctor

        public SwiftPortalOverridePlugin(IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreService storeService,
            IWebHelper webHelper,
            IMessageTemplateService messageTemplateService)
        {
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeService = storeService;
            _webHelper = webHelper;
            _messageTemplateService = messageTemplateService;
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
            _settingService.SaveSetting(
                new SwiftCoreSettings
                {
                    StorageContainerName = "swiftportal-container"
                }
            );

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
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
            });

            // email template
            //ConfigureMessageTemplates();

            // create proc
            var settings = DataSettingsManager.LoadSettings();
            var dataProvider = DataProviderManager.GetDataProvider(settings.DataProvider);

            dataProvider.ExecuteNonQuery(@"IF EXISTS (
                SELECT type_desc, type
                FROM sys.procedures WITH(NOLOCK)
                WHERE NAME = 'ProductLoadAllPagedSwiftPortal'
                    AND type = 'P'
              )
             DROP PROCEDURE dbo.ProductLoadAllPagedSwiftPortal");

            var sql = GetSQL("ProductLoadAllPagedSwiftPortal");
            dataProvider.ExecuteNonQuery(sql);

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SwiftCoreSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Misc.SwiftPortalOverride");

            //// email template
            //var approvalMail = _messageTemplateService.GetMessageTemplatesByName(SwiftPortalOverrideDefaults.ApprovalMessageTemplateName).FirstOrDefault();
            //if (approvalMail != null)
            //    _messageTemplateService.DeleteMessageTemplate(approvalMail);

            base.Uninstall();
        }

        //void ConfigureMessageTemplates()
        //{
        //    // approval email
        //    var approvalTemplate = _messageTemplateService.GetMessageTemplatesByName(SwiftPortalOverrideDefaults.ApprovalMessageTemplateName).FirstOrDefault();
        //    if (approvalTemplate == null)
        //    {
        //        var newCustomerMail = _messageTemplateService.GetMessageTemplatesByName("NewCustomer.Notification").FirstOrDefault();
        //        approvalTemplate = _messageTemplateService.CopyMessageTemplate(newCustomerMail);
        //    }
        //    // change body
        //    //approvalMail.Body =
        //    approvalTemplate.Name = SwiftPortalOverrideDefaults.ApprovalMessageTemplateName;
        //    approvalTemplate.Body = $@"<p>  <a href='%Store.URL%'>%Store.Name%</a>  <br />  <br />  A new customer registered with your store. Below are the customer's details:  <br />  Full name: %Customer.FullName%  <br />  Email: %Customer.Email% <br />  Erp Id: %Customer.ErpId%  </p>  ";
        //    _messageTemplateService.UpdateMessageTemplate(approvalTemplate);
        //}

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
