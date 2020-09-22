using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.SwiftPortalOverride.Services;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<QueuedEmailServiceOverride>().As<IQueuedEmailService>().InstancePerLifetimeScope();
        }

        public int Order => 10;
    }
}
