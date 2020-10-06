using Nop.Core.Configuration;

namespace NSS.Plugin.Misc.SwiftApi.Domain
{
    public class ApiSettings : ISettings
    {
        public bool EnableApi { get; set; } = true;

        public int TokenExpiryInDays { get; set; } = 0;
    }
}
