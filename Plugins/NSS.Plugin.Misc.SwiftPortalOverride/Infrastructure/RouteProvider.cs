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
               new { controller = "UserRegistration", action = "Register" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );


            // new customer change password
            endpointRouteBuilder.MapControllerRoute("NewCustomerChangePassword", "newcustomer/changepassword/",
               new { controller = "CustomerOverride", action = "NewCustomerChangePassword" },
                new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );


            // customer account
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Info", "customer/info/",
               new { controller = "CustomerOverride", action = "Info" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.ChangePassword", "customer/changepassword/",
               new { controller = "CustomerOverride", action = "ChangePassword" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Addresses", "customer/addresses/",
               new { controller = "CustomerOverride", action = "Addresses" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("CustomerAddressEdit", "customer/addressedit/{addressId:min(0)}/",
                new { controller = "CustomerOverride", action = "AddressEdit" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("CustomerAddressAdd", $"customer/addressadd/",
                new { controller = "CustomerOverride", action = "AddressAdd" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("CustomerNotifications", "customer/notifications/",
               new { controller = "CustomerOverride", action = "Notifications" },
              new { },
              new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );

            //confirm registration
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Index", "userregistration/{regId:int}/confirmregistration/",
             new { controller = "UserRegistration", action = "ConfirmRegistration" },
             new { },
             new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
             );


            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Index", "approveregistration/",
              new { controller = "UserRegistration", action = "Approve" },
              new { },
              new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Index", "rejectregistration/",
             new { controller = "UserRegistration", action = "Reject" },
             new { },
             new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
             );

            // dashboard
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Index", "/",
               new { controller = "HomeOverride", action = "Index" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            // catalog
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "catalog/",
               new { controller = "CatalogOverride", action = "Index" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            // resources
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Resources", "resources/",
               new { controller = "Resource", action = "Index" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            // cart
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "cart/",
               new { controller = "CartOverride", action = "Cart" },
               new { },
               new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
               );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "updatecart/",
                new { controller = "CartOverride", action = "UpdateCart" },
              new { },
              new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
              );


            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "wishlist/{customerGuid?}/",
            new { controller = "CartOverride", action = "Wishlist" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "checkout/",
                new { controller = "CartOverride", action = "StartCheckout" },
                new { },
                new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
                );


            // Checkout

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

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Catalog", "rejected/",
             new { controller = "CheckoutOverride", action = "Rejected" },
             new { },
             new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
             );

            // orders
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Orders", "orders/",
            new { controller = "OrderOverride", action = "CompanyOrders" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Orders", "orders/{orderId:min(0)}/",
            new { controller = "OrderOverride", action = "Details" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            // invoices
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.SwiftPortalOverride.Invoices", "invoices/",
            new { controller = "Invoice", action = "CompanyInvoices" },
            new { },
            new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
            );

            //topics
            endpointRouteBuilder.MapControllerRoute("TopicPopup", "t-popup/{SystemName}",
              new { controller = "TopicOverride", action = "TopicDetailsPopup" },
             new { },
             new[] { "NSS.Plugin.Misc.SwiftPortalOverride.Controllers" }
             );

        }
    }
}