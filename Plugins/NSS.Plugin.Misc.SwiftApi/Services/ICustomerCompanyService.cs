using NSS.Plugin.Misc.SwiftApi.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.Services
{
    public interface ICustomerCompanyService
    {
        void InsertCustomerCompany(CustomerCompany customerCompany);

        void DeleteCustomerCompany(CustomerCompany customerCompany);

        CustomerCompany GetCustomerCompany(int customerId, int companyId);
    }
}
