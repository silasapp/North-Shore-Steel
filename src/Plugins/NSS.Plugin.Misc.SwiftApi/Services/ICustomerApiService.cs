using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NSS.Plugin.Misc.SwiftApi.DTO.Customers;
using NSS.Plugin.Misc.SwiftApi.Infrastructure;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface ICustomerApiService
    {
        Task<int> GetCustomersCountAsync();

        Task<CustomerDto> GetCustomerByIdAsync(int id, bool showDeleted = false);

        Task<Customer> GetCustomerEntityByIdAsync(int id);

        Task<IList<CustomerDto>> GetCustomersDtosAsync(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId);

        Task<IList<CustomerDto>> SearchAsync(
            string query = "", string order = Constants.Configurations.DefaultOrder,
            int page = Constants.Configurations.DefaultPageValue, int limit = Constants.Configurations.DefaultLimit);

        Task<Dictionary<string, string>> GetFirstAndLastNameByCustomerIdAsync(int customerId);
    }
}
