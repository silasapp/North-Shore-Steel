﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using System.Linq;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
       
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // register
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Register", "register/",
               new { controller = "RegisterPlugin", action = "Register" },
               new { },
               new[] { "Nop.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );


            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Homepage", "homepage/",
               new { controller = "RegisterPlugin", action = "Homepage" },
               new { },
               new[] { "Nop.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.TermsOfService", "termsofservice/",
              new { controller = "RegisterPlugin", action = "TermsOfService" },
              new { },
              new[] { "Nop.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );
        }
    }
}