using Nop.Plugin.Swift.Api.Domain.Customers;

namespace Nop.Plugin.Swift.Api.Services
{
    public interface ICustomerCompanyService
    {
        void InsertCustomerCompany(CustomerCompany customerCompany);

        void DeleteCustomerCompany(CustomerCompany customerCompany);

        CustomerCompany GetCustomerCompany(int customerId, int companyId);
    }
}
