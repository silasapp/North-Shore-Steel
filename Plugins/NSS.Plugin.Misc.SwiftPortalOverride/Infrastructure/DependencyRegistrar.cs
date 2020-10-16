using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using NSS.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Messages;
using NSS.Plugin.Misc.SwiftCore.Services;
using Nop.Services.Catalog;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<NSSApiProvider>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<WorkFlowMessageServiceOverride>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<CustomerCompanyService>().As<ICustomerCompanyService>().InstancePerLifetimeScope();

            builder.RegisterType<QueuedEmailServiceOverride>().As<IQueuedEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkFlowMessageServiceOverride>().As<IWorkflowMessageService>().InstancePerLifetimeScope();

            builder.RegisterType<ProductServiceOverride>().As<IProductService>().InstancePerLifetimeScope();

        }

        public int Order => 10;
    }
}
