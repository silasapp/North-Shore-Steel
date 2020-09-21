using AutoMapper;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.CustomerRoles;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.MappingExtensions;

namespace Nop.Plugin.Api.AutoMapper
{
    public class ApiMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public ApiMapperConfiguration()
        {
            CreateMap<Language, LanguageDto>();

            CreateMap<CustomerRole, CustomerRoleDto>();

            CreateAddressMap();
            CreateAddressDtoToEntityMap();

            CreateCustomerToDTOMap();
        }

        public int Order => 0;

        private new static void CreateMap<TSource, TDestination>()
        {
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<TSource, TDestination>()
                                      .IgnoreAllNonExisting();
        }

        private void CreateAddressMap()
        {
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<Address, AddressDto>()
                                      .IgnoreAllNonExisting()
                                      .ForMember(x => x.Id, y => y.MapFrom(src => src.Id));
                                      //.ForMember(x => x.CountryName,
                                      //           y => y.MapFrom(src => src.Country.GetWithDefault(x => x, new Country()).Name))
                                      //.ForMember(x => x.StateProvinceName,
                                      //           y => y.MapFrom(src => src.StateProvince.GetWithDefault(x => x, new StateProvince()).Name));
        }

        private void CreateAddressDtoToEntityMap()
        {
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<AddressDto, Address>()
                                      .IgnoreAllNonExisting()
                                      .ForMember(x => x.Id, y => y.MapFrom(src => src.Id));
        }

        private void CreateCustomerToDTOMap()
        {
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<Customer, CustomerDto>()
                                      .IgnoreAllNonExisting()
                                      .ForMember(x => x.Id, y => y.MapFrom(src => src.Id));
                                      //.ForMember(x => x.BillingAddress,
                                      //           y => y.MapFrom(src => src.BillingAddress.GetWithDefault(x => x, new Address()).ToDto()))
                                      //.ForMember(x => x.ShippingAddress,
                                      //           y => y.MapFrom(src => src.ShippingAddress.GetWithDefault(x => x, new Address()).ToDto()))
                                      //.ForMember(x => x.Addresses,
                                      //           y =>
                                      //               y.MapFrom(
                                      //                         src =>
                                      //                             src.Addresses.GetWithDefault(x => x, new List<Address>())
                                      //                                .Select(address => address.ToDto())))
                                      //.ForMember(x => x.ShoppingCartItems,
                                      //           y =>
                                      //               y.MapFrom(
                                      //                         src =>
                                      //                             src.ShoppingCartItems.GetWithDefault(x => x, new List<ShoppingCartItem>())
                                      //                                .Select(item => item.ToDto())))
                                      //.ForMember(x => x.RoleIds, y => y.MapFrom(src => src.CustomerRoles.Select(z => z.Id)));
        }
    }
}
