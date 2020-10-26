using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
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
    }
}
