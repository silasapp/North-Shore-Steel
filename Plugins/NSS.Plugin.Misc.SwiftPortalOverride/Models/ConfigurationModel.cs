using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration {get;set;}
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.UseSandBox")]
        public bool UseSandBox { get; set; }
        public bool UseSandBox_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.TestEmailAddress")]
        public string TestEmailAddress { get; set; }
        public bool TestEmailAddress_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.ApproverMailBox")]
        public string ApproverMailBox { get; set; }
        public bool ApproverMailBox_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.NSSApiBaseUrl")]
        public string NSSApiBaseUrl { get; set; }
        public bool NSSApiBaseUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.NSSApiAuthUsername")]
        public string NSSApiAuthUsername { get; set; }
        public bool NSSApiAuthUsername_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.NSSApiAuthPassword")]
        public string NSSApiAuthPassword { get; set; }
        public bool NSSApiAuthPassword_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.StorageAccountName")]
        public string StorageAccountName { get; set; }
        public bool StorageAccountName_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.StorageAccountKey")]
        public string StorageAccountKey { get; set; }
        public bool StorageAccountKey_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.StorageContainerName")]
        public string StorageContainerName { get; set; }
        public bool StorageContainerName_OverrideForStore { get; set; }


        // pay pal
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.PayPalUseSandbox")]
        public bool PayPalUseSandbox { get; set; }
        public bool PayPalUseSandBox_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.PayPalClientId")]
        public string PayPalClientId { get; set; }
        public bool PayPalClientId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.PayPalSecretKey")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string PayPalSecretKey { get; set; }
        public bool PayPalSecretKey_OverrideForStore { get; set; }
    }
}
