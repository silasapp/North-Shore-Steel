using Nop.Core.Domain.Customers;
using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NSS.Plugin.Misc.SwiftApi.DTO.CustomerRoles;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class CustomerRoleDtoMappings
    {
        public static CustomerRoleDto ToDto(this CustomerRole customerRole)
        {
            return customerRole.MapTo<CustomerRole, CustomerRoleDto>();
        }
    }
}
