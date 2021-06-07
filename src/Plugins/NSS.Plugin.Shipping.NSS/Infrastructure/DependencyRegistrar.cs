using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Shipping.NSS.Services;

namespace NSS.Plugin.Shipping.NSS.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IApiService, NSSApiService>();
            services.AddScoped<ShippingChargeService>();
        }

        public int Order => 1;
    }
}
