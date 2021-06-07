using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Customers
{
    public class CustomerAttributeMappingDto
    {
        public Customer Customer { get; set; }
        public GenericAttribute Attribute { get; set; }
    }
}
