using Nop.Web.Framework.Mvc.ModelBinding;

namespace NSS.Plugin.Misc.SwiftApi.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        [NopResourceDisplayName("Plugins.Api.Admin.EnableApi")]
        public bool EnableApi { get; set; }

        public bool EnableApi_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Api.Admin.TokenExpiryInDays")]
        public int TokenExpiryInDays { get; set; }

        public bool TokenExpiryInDays_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
