﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using System.Linq;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Infrastructure
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
               new { controller = "CustomerCustom", action = "Register" },
               new { },
               new[] { "Nop.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Index", "/",
               new { controller = "HomeCustom", action = "Index" },
               new { },
               new[] { "Nop.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

           
        }
    }
}