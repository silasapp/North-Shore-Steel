using Nop.Core.Domain.Customers;
using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NSS.Plugin.Misc.SwiftApi.DTO.Customers;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class CustomerDtoMappings
    {
        public static CustomerDto ToDto(this Customer customer)
        {
            return customer.MapTo<Customer, CustomerDto>();
        }
    }
}
