using Microsoft.AspNetCore.Builder;
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

            //login
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Login", "login/",
              new { controller = "CustomerOverride", action = "Login" },
              new { },
              new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );


            // register
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Register", "register/",
               new { controller = "CustomerOverride", action = "Register" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Index", "/",
               new { controller = "HomeOverride", action = "Index" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "catalog/",
               new { controller = "CatalogOverride", action = "Index" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "cart/",
               new { controller = "CartOverride", action = "Cart" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "checkout/",
              new { controller = "CheckoutOverride", action = "Index" },
              new { },
              new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "onepagecheckout/",
              new { controller = "CheckoutOverride", action = "OnePageCheckout" },
              new { },
              new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "completed/",
             new { controller = "CheckoutOverride", action = "Completed" },
             new { },
             new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
             );


            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "updatecart/",
            new { controller = "CartOverride", action = "UpdateCart" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            // orders
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Orders", "orders/",
            new { controller = "OrderOverride", action = "CustomerOrders" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Orders", "orders/{orderId:min(0)}/",
            new { controller = "OrderOverride", action = "Details" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Orders", "open-orders/",
            new { controller = "OrderOverride", action = "CompanyOpenOrders" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Orders", "closed-orders/",
            new { controller = "OrderOverride", action = "CompanyClosedOrders" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Invoices", "open-invoices/",
            new { controller = "Invoice", action = "CompanyOpenInvoices" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Invoices", "closed-invoices/",
            new { controller = "Invoice", action = "CompanyClosedInvoices" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );
        }
    }
}