using Nop.Core.Domain.Common;
using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NSS.Plugin.Misc.SwiftApi.DTO;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class AddressDtoMappings
    {
        public static AddressDto ToDto(this Address address)
        {
            return address.MapTo<Address, AddressDto>();
        }

        public static Address ToEntity(this AddressDto addressDto)
        {
            return addressDto.MapTo<AddressDto, Address>();
        }
    }
}
