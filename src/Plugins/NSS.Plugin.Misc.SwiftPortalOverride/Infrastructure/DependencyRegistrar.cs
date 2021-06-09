using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Messages;
using NSS.Plugin.Misc.SwiftCore.Services;
using Nop.Services.Catalog;
using NSS.Plugin.Misc.SwiftPortalOverride.Factories;
using Nop.Services.Orders;
using Nop.Services.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //Self
            services.AddSingleton<WorkFlowMessageServiceOverride>();
            services.AddSingleton<PayPalServiceManager>();
            services.AddSingleton<PayPalProcessor>();

            //Core
            services.AddScoped<ICustomerCompanyService, CustomerCompanyService>();
            services.AddScoped<IShapeService, ShapeService>();
            services.AddScoped<IStorageService, AzureStorageService>();
            services.AddScoped<ICustomerCompanyProductService, CustomerCompanyProductService>();
            services.AddScoped<IApiService, NSSApiService>();

            // Override
            services.AddScoped<IQueuedEmailService, QueuedEmailServiceOverride>();
            services.AddScoped<IWorkflowMessageService, WorkFlowMessageServiceOverride>();
            services.AddScoped<IProductService, ProductServiceOverride>();
            services.AddScoped<IShoppingCartService, CustomShoppingCartService>();
            services.AddScoped<ICustomerService, CustomCustomerService>();
            services.AddScoped<IOrderProcessingService, CustomOrderProcessingService>();

            //  factories
            services.AddScoped<ICatalogModelFactory, CatalogModelFactory>();
            services.AddScoped<ICheckoutModelFactory, CheckoutModelFactory>();
            services.AddScoped<IProductModelFactory, ProductModelFactory>();
            services.AddScoped<IOrderModelFactory, OrderModelFactory>();
            services.AddScoped<IInvoiceModelFactory, InvoiceModelFactory>();
            services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
            services.AddScoped<Nop.Web.Factories.ICommonModelFactory, CustomCommonModelFactory>();
        }

        public int Order => 10;
    }
}
