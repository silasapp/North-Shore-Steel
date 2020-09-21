using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Nop.Core.Configuration;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Api.Converters;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Maps;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Services;
using Nop.Plugin.Api.Validators;

namespace Nop.Plugin.Api.Infrastructure
{
    [UsedImplicitly]
    public class DependencyRegister : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            RegisterPluginServices(builder);

            RegisterModelBinders(builder);
        }

        public virtual int Order => short.MaxValue;

        private void RegisterModelBinders(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(ParametersModelBinder<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(JsonModelBinder<>)).InstancePerLifetimeScope();
        }

        private void RegisterPluginServices(ContainerBuilder builder)
        {
            //builder.RegisterType<ClientService>().As<IClientService>().InstancePerLifetimeScope();
            //builder.RegisterType<CustomerApiService>().As<ICustomerApiService>().InstancePerLifetimeScope();
            builder.RegisterType<SpecificationAttributesApiService>().As<ISpecificationAttributeApiService>().InstancePerLifetimeScope();

            builder.RegisterType<MappingHelper>().As<IMappingHelper>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRolesHelper>().As<ICustomerRolesHelper>().InstancePerLifetimeScope();
            builder.RegisterType<JsonHelper>().As<IJsonHelper>().InstancePerLifetimeScope();
            builder.RegisterType<DTOHelper>().As<IDTOHelper>().InstancePerLifetimeScope();

            builder.RegisterType<JsonFieldsSerializer>().As<IJsonFieldsSerializer>().InstancePerLifetimeScope();

            builder.RegisterType<FieldsValidator>().As<IFieldsValidator>().InstancePerLifetimeScope();

            builder.RegisterType<ObjectConverter>().As<IObjectConverter>().InstancePerLifetimeScope();
            builder.RegisterType<ApiTypeConverter>().As<IApiTypeConverter>().InstancePerLifetimeScope();

            builder.RegisterType<CustomerFactory>().As<IFactory<Customer>>().InstancePerLifetimeScope();
            builder.RegisterType<AddressFactory>().As<IFactory<Address>>().InstancePerLifetimeScope();

            builder.RegisterType<JsonPropertyMapper>().As<IJsonPropertyMapper>().InstancePerLifetimeScope();

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();

            builder.RegisterType<Dictionary<string, object>>().SingleInstance();
        }
    }
}
