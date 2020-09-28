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
using Nop.Plugin.Swift.Api.Domain.Shapes;
using Nop.Plugin.Swift.Api.DTOs.Shapes;
using System.Linq;

namespace Nop.Plugin.Api.AutoMapper
{
    public class ApiMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public ApiMapperConfiguration()
        {
            CreateMap<Language, LanguageDto>();

            CreateMap<CustomerRole, CustomerRoleDto>();


            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<ShapeAttributeDto, ShapeAttribute>().IgnoreAllNonExisting();
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<ShapeDto, Shape>().IgnoreAllNonExisting()
                .ForMember(d => d.Atttributes, o => o.MapFrom(s => s.Atttributes.ToArray()))
                .ForMember(d => d.SubCategories, o => o.MapFrom(s => s.SubCategories.ToArray()));

            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<ShapeAttribute, ShapeAttributeDto>().IgnoreAllNonExisting();
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<Shape, ShapeDto>().IgnoreAllNonExisting()
                .ForMember(d => d.Atttributes, o => o.MapFrom(s => s.Atttributes.ToArray()));

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
        }
    }
}
