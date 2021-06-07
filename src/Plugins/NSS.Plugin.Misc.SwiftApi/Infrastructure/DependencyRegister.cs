using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Common;
using NSS.Plugin.Misc.SwiftApi.Converters;
using NSS.Plugin.Misc.SwiftApi.Factories;
using NSS.Plugin.Misc.SwiftApi.Helpers;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.Maps;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using NSS.Plugin.Misc.SwiftApi.Services;
using NSS.Plugin.Misc.SwiftApi.Validators;
using NSS.Plugin.Misc.SwiftApiApi.Services;
using NSS.Plugin.Misc.SwiftCore.Services;

namespace NSS.Plugin.Misc.SwiftApi.Infrastructure
{
    [UsedImplicitly]
    public class DependencyRegister : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            RegisterPluginServices(services);

            RegisterModelBinders(services);
        }

        public virtual int Order => short.MaxValue;

        private void RegisterModelBinders(IServiceCollection services)
        {
            services.AddScoped(typeof(ParametersModelBinder<>));
            services.AddScoped(typeof(JsonModelBinder<>));
        }

        private void RegisterPluginServices(IServiceCollection services)
        {
            services.AddScoped<CustomGenericAttributeService>();

            services.AddScoped<ISpecificationAttributeApiService, SpecificationAttributesApiService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICustomerCompanyService, CustomerCompanyService>();
            services.AddScoped<IShapeService, ShapeService>();
            services.AddScoped<IProductApiService, ProductApiService>();
            services.AddScoped<IStorageService, AzureStorageService>();
            services.AddScoped<IUserRegistrationService, UserRegistrationService>();
            services.AddScoped<ICustomerApiService, CustomerApiService>();
            //services.AddScoped<IGenericAttributeService, IGenericAttributeService>();

            services.AddScoped<IMappingHelper, MappingHelper>();
            services.AddScoped<ICustomerRolesHelper, CustomerRolesHelper>();
            services.AddScoped<IJsonHelper, JsonHelper>();
            services.AddScoped<IDTOHelper, DTOHelper>();

            services.AddScoped<IJsonFieldsSerializer, JsonFieldsSerializer>();

            services.AddScoped<IFieldsValidator, FieldsValidator>();

            services.AddScoped<IObjectConverter, ObjectConverter>();
            services.AddScoped<IApiTypeConverter, ApiTypeConverter>();

            services.AddScoped<IFactory<Customer>, CustomerFactory>();
            services.AddScoped<IFactory<Address>, AddressFactory>();
            services.AddScoped<IFactory<Product>, ProductFactory>();

            services.AddScoped<IJsonPropertyMapper, JsonPropertyMapper>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<Dictionary<string, object>>();


            // replace IAuthenticationService CookieAutheticationService (used in NopCommerce web) with BearerTokenOrCookieAuthenticationService that will combine Bearer token  (used in Nop api plugin) and Cookies authentication
            // // if token is not found to be compatible with web client, then IApiWorkContext is unnecessary
            services.Replace(ServiceDescriptor.Scoped<Nop.Services.Authentication.IAuthenticationService, BearerTokenOrCookieAuthenticationService>());
        }
    }
}
