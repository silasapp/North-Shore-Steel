using System;
using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using NSS.Plugin.Misc.SwiftApi.DTO.Customers;
using NSS.Plugin.Misc.SwiftApi.Infrastructure;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface ICustomerApiService
    {
        int GetCustomersCount();

        CustomerDto GetCustomerById(int id, bool showDeleted = false);

        Customer GetCustomerEntityById(int id);

        IList<CustomerDto> GetCustomersDtos(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId);

        IList<CustomerDto> Search(
            string query = "", string order = Constants.Configurations.DefaultOrder,
            int page = Constants.Configurations.DefaultPageValue, int limit = Constants.Configurations.DefaultLimit);

        Dictionary<string, string> GetFirstAndLastNameByCustomerId(int customerId);
    }
}
