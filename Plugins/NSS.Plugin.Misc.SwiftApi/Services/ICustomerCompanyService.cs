using NSS.Plugin.Misc.SwiftApi.Domain.Customers;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface ICustomerCompanyService
    {
        void InsertCustomerCompany(CustomerCompany customerCompany);

        void DeleteCustomerCompany(CustomerCompany customerCompany);

        CustomerCompany GetCustomerCompany(int customerId, int companyId);

        IEnumerable<CustomerCompany> GetCustomerCompanies(int customerId);
    }
}
