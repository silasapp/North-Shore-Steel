using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;

namespace NSS.Plugin.Misc.SwiftApi.Factories
{
    public class AddressFactory : IFactory<Address>
    {
        public Task <Address> InitializeAsync()
        {
            var address = new Address
                          {
                              CreatedOnUtc = DateTime.UtcNow
                          };

            return Task.FromResult(address);
        }
    }
}
