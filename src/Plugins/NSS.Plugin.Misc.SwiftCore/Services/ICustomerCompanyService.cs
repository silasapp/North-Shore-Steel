using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System.Collections.Generic;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface ICustomerCompanyService
    {
        Task InsertCustomerCompanyAsync(CustomerCompany customerCompany);

        Task DeleteCustomerCompanyAsync(CustomerCompany customerCompany);

        Task<CustomerCompany> GetCustomerCompanyAsync(int customerId, int companyId);

        Task<CustomerCompany> GetCustomerCompanyByErpCompIdAsync(int customerId, int erpCompId);

        Task<IEnumerable<CustomerCompany>> GetCustomerCompaniesAsync(int customerId);

        Task UpdateCustomerCompanyAsync(CustomerCompany customerCompany);

        Task<bool> AuthorizeAsync(int customerId, int erpCompanyId, ERPRole role);
    }
}
