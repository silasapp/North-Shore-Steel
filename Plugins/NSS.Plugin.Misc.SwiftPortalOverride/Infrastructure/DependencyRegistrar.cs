using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Messages;
using NSS.Plugin.Misc.SwiftCore.Services;
using Nop.Services.Catalog;
using Nop.Web.Factories;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            // self
            builder.RegisterType<NSSApiProvider>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<WorkFlowMessageServiceOverride>().AsSelf().InstancePerLifetimeScope();

            // core
            builder.RegisterType<CustomerCompanyService>().As<ICustomerCompanyService>().InstancePerLifetimeScope();
            builder.RegisterType<ShapeService>().As<IShapeService>().InstancePerLifetimeScope();
            // overrides
            builder.RegisterType<QueuedEmailServiceOverride>().As<IQueuedEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkFlowMessageServiceOverride>().As<IWorkflowMessageService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductServiceOverride>().As<IProductService>().InstancePerLifetimeScope();

            // factories
            builder.RegisterType<CatalogModelFactory>().As<ICatalogModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProductModelFactory>().As<IProductModelFactory>().InstancePerLifetimeScope();


        }

        public int Order => 10;
    }
}
