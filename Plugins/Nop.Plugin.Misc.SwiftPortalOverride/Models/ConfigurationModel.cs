using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration {get;set;}
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.UseSandBox")]
        public bool UseSandBox { get; set; }
        public bool UseSandBox__OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.TestEmailAddress")]
        public string TestEmailAddress { get; set; }
        public bool TestEmailAddress__OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Misc.SwiftPortalOverride.Fields.ApproverMailBox")]
        public string ApproverMailBox { get; set; }
        public bool ApproverEmailBox__OverrideForStore { get; set; }
    }
}
