using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Shipping.NSS.Services;

namespace NSS.Plugin.Shipping.NSS.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<NSSApiService>().As<IApiService>().InstancePerLifetimeScope();

            builder.RegisterType<ShippingChargeService>().AsSelf().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}
