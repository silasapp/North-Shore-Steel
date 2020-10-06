using Microsoft.AspNetCore.Authorization;
using Nop.Core.Infrastructure;
using NSS.Plugin.Misc.SwiftApi.Domain;

namespace NSS.Plugin.Misc.SwiftApi.Authorization.Requirements
{
    public class ActiveApiPluginRequirement : IAuthorizationRequirement
    {
        public bool IsActive()
        {
            var settings = EngineContext.Current.Resolve<ApiSettings>();

            if (settings.EnableApi)
            {
                return true;
            }

            return false;
        }
    }
}
