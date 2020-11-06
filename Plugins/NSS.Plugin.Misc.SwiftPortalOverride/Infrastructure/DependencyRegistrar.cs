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

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            // self
            builder.RegisterType<NSSApiProvider>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<WorkFlowMessageServiceOverride>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PayPalServiceManager>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PayPalProcessor>().AsSelf().InstancePerLifetimeScope();

            // core
            builder.RegisterType<CustomerCompanyService>().As<ICustomerCompanyService>().InstancePerLifetimeScope();
            builder.RegisterType<ShapeService>().As<IShapeService>().InstancePerLifetimeScope();
            builder.RegisterType<AzureStorageService>().As<IStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerCompanyProductService>().As<ICustomerCompanyProductService>().InstancePerLifetimeScope();


            // overrides
            builder.RegisterType<QueuedEmailServiceOverride>().As<IQueuedEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkFlowMessageServiceOverride>().As<IWorkflowMessageService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductServiceOverride>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomShoppingCartService>().As<IShoppingCartService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomOrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();

            //  custom factories
            builder.RegisterType<CatalogModelFactory>().As<ICatalogModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProductModelFactory>().As<IProductModelFactory>().InstancePerLifetimeScope();

        }

        public int Order => 10;
    }
}
